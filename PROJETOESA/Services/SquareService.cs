using Azure;
using Square.Exceptions;
using Square;
using Square.Http.Client;
using Square.Models;
using PROJETOESA.Data;
using PROJETOESA.Models.SquareModels;
using PROJETOESA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using System.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Castle.Core.Resource;
using PROJETOESA.Controllers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PROJETOESA.Services
{
    public class SquareService
    {
        private SquareClient _client;
        private AeroHelperContext _context;
        private ApplicationUser _user;
        private Models.Payment _payment;
        private string squareId;
        private string squareLocation;
        private string squareOrderID;
        private string squareSourceID = "cnon:card-nonce-ok";
        private readonly IConfiguration _configuration;
        private const string subscriptionPlanID = "TAWJ57V3HV7376F4DSDG2ZED";
        private const long subscriptionPrice = 15000;


        public SquareService(SquareClient client, AeroHelperContext context, IConfiguration configuration)
        {
            _client = client ?? throw new ArgumentNullException(nameof(_client));
            _context = context;
            _configuration = configuration;
            squareLocation = _configuration["SquareSettings:LOCATION_ID"];
        }

        public async Task<bool> PayAsync(PaymentModel payment, UserManager<ApplicationUser> userManager)

        {
            _user = await userManager.FindByEmailAsync(payment.Email);

            var checkingPayment = await _context.Payment
                .Where(c => c.CustomerId == _user.CustomerID)
                .OrderByDescending(c => c.date) // Assuming 'Date' is the property representing the date
                .FirstOrDefaultAsync();

            if (checkingPayment == null || checkingPayment.paymentState.Equals("COMPLETED") || checkingPayment.paymentState.Equals("CANCELED"))
            {

                squareLocation = _configuration["SquareSettings:LOCATION_ID"];


                long lPrice = (long)payment.price;
                var amountMoney = new Money.Builder()
                    .Amount(lPrice)
                    .Currency("USD")
                    .Build();



                try
                {
                    // ADICIONAR DADOS NA BD PARA HISTORICO
                    //Criar Customer, criar Order e no fim Criar pagamento.

                    squareId = CreateSquareCustomer();
                    squareOrderID = CreateSquareOrder(payment);
                    var idempotencyKey = Guid.NewGuid().ToString();
                    CreatePaymentRequest paymentRequest = new CreatePaymentRequest.Builder(squareSourceID, idempotencyKey, amountMoney)
                        .AcceptPartialAuthorization(false)
                        .Autocomplete(false)
                        .BuyerEmailAddress(payment.Email)
                        .CustomerId(squareId)
                        .LocationId(squareLocation)
                        .OrderId(squareOrderID)
                        .Build();

                    var result = await _client.PaymentsApi.CreatePaymentAsync(paymentRequest);
                    if (!String.IsNullOrEmpty(result.Payment.Id))
                    {
                        Models.Payment p = new Models.Payment();
                        p.PaymentId = result.Payment.Id;
                        p.CustomerId = result.Payment.CustomerId;
                        p.paymentState = result.Payment.Status;
                        p.date = DateTime.Now;
                        _context.Payment.Add(p);
                        _context.SaveChanges();
                    }

                    if (CustomerCreateCard())
                    {
                        Debug.WriteLine("Card Created");
                    }


                }
                catch (ApiException e)
                {
                    Console.WriteLine("Failed to make the request");
                    Console.WriteLine($"Response Code: {e.ResponseCode}");
                    Console.WriteLine($"Exception: {e.Message}");
                }

                return true;
            }
            else 
            {
                return false;
            }

            
            
        }


        public async Task<bool> CompleteTicketPayment(string customerCardID)
        { 
            var card = _client.CardsApi.RetrieveCard(customerCardID);

            var customerID = card.Card.CustomerId;

            var payment = await _context.Payment
                .Where(c => c.CustomerId == customerID)
                .OrderByDescending(c => c.date) // Assuming 'Date' is the property representing the date
                .FirstOrDefaultAsync();

            if (payment != null) 
            {
                var res = _client.PaymentsApi.CompletePayment(payment.PaymentId);
                payment.paymentState = res.Payment.Status;
                _context.SaveChanges();
                return res.Payment.Status.Equals("COMPLETED") ? true : false;
            }

            return false;
        }

        public async Task<bool> SubscribePremium(string customerCardID)
        {
            var card = _client.CardsApi.RetrieveCard(customerCardID);

            var customerID = card.Card.CustomerId;

            var customerToPremium = await _context.Users
                .Where(c => c.CustomerID == customerID)
                .FirstOrDefaultAsync();


            if (customerToPremium != null)
            {
                CreateSubscriptionRequest subscription = CreateSquareSubscription(customerCardID);

                var res = _client.SubscriptionsApi.CreateSubscription(subscription);

                var subscribeID = _client.SubscriptionsApi.RetrieveSubscription(res.Subscription.Id);

                var orderID = GetOrderId(customerID);

                var gotOrder = _client.OrdersApi.RetrieveOrder(orderID);

                var paymentID = gotOrder.Order.Tenders[0].PaymentId;

                if (!String.IsNullOrEmpty(paymentID))
                {
                    Models.Payment p = new Models.Payment();
                    p.PaymentId = paymentID;
                    p.CustomerId = customerID;
                    p.paymentState = "COMPLETED";
                    p.date = DateTime.Now;
                    _context.Payment.Add(p);
                    _context.SaveChanges();
                }

                if (res.Subscription.Id != "")
                {
                    customerToPremium.Role = TipoConta.ClientePremium;
                    customerToPremium.subscriptionID = res.Subscription.Id;
                    _context.SaveChanges();
                    return true; 
                } 
                else 
                { return false; }
            }

            return false;
        }

        private string GetOrderId(string customerID)
        {
            List<string> locationIds = new List<string>();
            locationIds.Add(squareLocation);
            List<string> customerIds = new List<string>();
            customerIds.Add(customerID);

            InvoiceFilter invoiceFilter = new InvoiceFilter.Builder(locationIds)
                .CustomerIds(customerIds)
                .Build();

            InvoiceSort invoiceSort = new InvoiceSort.Builder("INVOICE_SORT_DATE")
                .Order("DESC")
                .Build();



            InvoiceQuery invoiceQuery = new InvoiceQuery.Builder(invoiceFilter)
                .Sort(invoiceSort)
                .Build();  

            SearchInvoicesRequest search = new SearchInvoicesRequest.Builder(invoiceQuery).Build();

            var invoiceId = _client.InvoicesApi.SearchInvoices(search);

            string orderId = invoiceId.Invoices[0].OrderId;

            return orderId;
        }

        public async Task<bool> CancelSubscription(string customerCardID)
        {
            var card = _client.CardsApi.RetrieveCard(customerCardID);

            var customerID = card.Card.CustomerId;

            var customerRemovePremium = await _context.Users
                .Where(c => c.CustomerID == customerID)
                .FirstOrDefaultAsync();


            if (customerRemovePremium != null)
            {
                var res = _client.SubscriptionsApi.CancelSubscription(customerRemovePremium.subscriptionID);

                customerRemovePremium.Role = TipoConta.ClienteNormal;
                customerRemovePremium.subscriptionID = "";
                _context.SaveChanges();

                return true;
            }

            return false;

        }


        public CreateSubscriptionRequest CreateSquareSubscription(string customerCardID)
        {
            var card = _client.CardsApi.RetrieveCard(customerCardID);
            var customerID = card.Card.CustomerId;

            var day = DateTime.Now.Day;
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;
            var dateForReq = year + "-" + month + "-" + day;
            CreateSubscriptionRequest createSubscription = new CreateSubscriptionRequest.Builder(squareLocation, subscriptionPlanID, customerID)
                    .CardId(customerCardID)
                    .IdempotencyKey(Guid.NewGuid().ToString())
                    .StartDate(dateForReq)
                    .Build();

            return createSubscription;
        }

        public string CreateSquareCustomer()
        {


            if (_user.CustomerID == null || _user.CustomerID == "")
            {
                CreateCustomerRequest createCustomerRequest = new CreateCustomerRequest.Builder()
                    .GivenName(_user.Name)
                    .IdempotencyKey(Guid.NewGuid().ToString())
                    .Birthday(_user.BirthDate.ToString())
                    .EmailAddress(_user.Email)
                    .Build();
                var customer = _client.CustomersApi.CreateCustomer(createCustomerRequest);
                var user = _context.Users.FirstOrDefault(x => x.Email == _user.Email);
                user.CustomerID = customer.Customer.Id;
                _context.SaveChanges();
                return customer.Customer.Id;
            }
            else
            {

                var oldCustomer = _client.CustomersApi.RetrieveCustomer(_user.CustomerID);
                return oldCustomer.Customer.Id;
            }

        }

        public string CreateSquareOrder(PaymentModel model)
        {
            var amountMoney = new Money.Builder()
                .Amount((long)model.price)
                .Currency("USD")
                .Build();

            var taxMoney = new Money.Builder()
                .Amount(0)
                .Currency("USD")
                .Build();

            OrderServiceCharge orderServiceCharge = new OrderServiceCharge.Builder()
                .AmountMoney(amountMoney)
                .CalculationPhase("TOTAL_PHASE")
                .Name(model.ShippingAddress)
                .Uid(Guid.NewGuid().ToString())
                .Build();


            IList<OrderServiceCharge> orders = new List<OrderServiceCharge>();
            orders?.Add(orderServiceCharge);


            Order newOrder = new Order.Builder(squareLocation)
                .CustomerId(_user.CustomerID)
                .ServiceCharges(orders)
                .Build();


            CreateOrderRequest createOrderRequest = new CreateOrderRequest.Builder()
                .IdempotencyKey(Guid.NewGuid().ToString())
                .Order(newOrder)
                .Build();

            CreateOrderResponse createdOrder = _client.OrdersApi.CreateOrder(createOrderRequest);


            return createdOrder.Order.Id;
        }


        private Boolean CustomerCreateCard()
        {
            var customer = _client.CustomersApi.RetrieveCustomer(_user.CustomerID);
            if (customer.Customer.Cards == null)
            {   
                var yearOfExpiration = DateTime.Now.Year + 4;
                Card newCard = new Card.Builder()
                    .CustomerId(_user.CustomerID)
                    .CardholderName(_user.Name)
                    .ExpMonth(DateTime.Now.Month)
                    .ExpYear(yearOfExpiration)
                    .Version(1)
                    .Build();

                CreateCardRequest newCardRequest = new CreateCardRequest.Builder(Guid.NewGuid().ToString(), squareSourceID, newCard)
                    .Build();

                _client.CardsApi.CreateCard(newCardRequest);

                return true;
            }

            return true;
        }

        public async Task<SquareCard> GetSquareCardAsync(string customerEmail, UserManager<ApplicationUser> userManager)
        {
            _user = await userManager.FindByEmailAsync(customerEmail);

            try {
                if (_user.CustomerID == null)
                {
                    CreateSquareCustomer();
                }


                if (CustomerCreateCard())
                {
                    var cardId = _client.CustomersApi.RetrieveCustomer(_user.CustomerID);
                    var cardToModel = _client.CardsApi.RetrieveCard(cardId.Customer.Cards[0].Id);
                    SquareCard squareCard = new SquareCard
                    {
                        Id = cardToModel.Card.Id,
                        CardBrand = cardToModel.Card.CardBrand,
                        Last4 = cardToModel.Card.Last4,
                        ExpMonth = (long)cardToModel.Card.ExpMonth,
                        ExpYear = (long)cardToModel?.Card.ExpYear,
                        CardholderName = cardToModel.Card.CardholderName,
                        BillingAddress = new BillingAddress { PostalCode = cardToModel.Card.BillingAddress.PostalCode },
                        Fingerprint = cardToModel.Card.Fingerprint

                    };

                    return squareCard;
                }

                return null;
                
            }
            catch (ApiException e)
            {
                Console.WriteLine("Failed to make the request");
                Console.WriteLine($"Response Code: {e.ResponseCode}");
                Console.WriteLine($"Exception: {e.Message}");
            }



            return null;
        }

        public async Task<List<PaymentHistoryModel>> GetSquareCustomerPayments(string userEmail, UserManager<ApplicationUser> userManager)
        {
            _user = await userManager.FindByEmailAsync(userEmail);
            List<PaymentHistoryModel> allPayments = new List<PaymentHistoryModel>();
            if (_user.CustomerID != null) 
            {
                var customerID = _user.CustomerID;
                var payment = await _context.Payment
                .Where(c => c.CustomerId == customerID)
                .OrderByDescending(c => c.date) // Assuming 'Date' is the property representing the date
                .ToListAsync();

               
                if (payment.Count != 0) 
                {
                    foreach (var p in payment)
                    {
                        var getAPayment = _client.PaymentsApi.GetPayment(p.PaymentId);
                        var cust = _client.CustomersApi.RetrieveCustomer(customerID);

                        if (getAPayment != null && getAPayment.Payment.AmountMoney.Amount == subscriptionPrice && cust != null)
                        {
                            PaymentHistoryModel subscriptionModel = new PaymentHistoryModel
                            {
                                price = subscriptionPrice/1000,
                                currency = "EUR",
                                Email = userEmail,
                                FirstName = _user.Name,
                                CreditCard = cust.Customer.Cards[0].Last4,
                                LastName = "",
                                ShippingAddress = "",
                                Status = getAPayment.Payment.Status
                            };
                            allPayments.Add(subscriptionModel);
                        }
                        else if (getAPayment != null && getAPayment.Payment.AmountMoney.Amount != subscriptionPrice && cust != null)
                        {
                            PaymentHistoryModel paymentModel = new PaymentHistoryModel
                            {
                                price = (decimal)getAPayment.Payment.AmountMoney.Amount,
                                currency = "EUR",
                                Email = userEmail,
                                FirstName = _user.Name,
                                CreditCard = cust.Customer.Cards[0].Last4,
                                LastName = "",
                                ShippingAddress = "",
                                Status = getAPayment.Payment.Status
                            };
                            allPayments.Add(paymentModel);
                        }
                        
                    }
                }
            }

            return allPayments;
            
        }


    }

}


