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


        public SquareService(SquareClient client, AeroHelperContext context, IConfiguration configuration)
        {
            _client = client ?? throw new ArgumentNullException(nameof(_client));
            _context = context;
            _configuration = configuration;
        }

        public async Task PayAsync(PaymentModel payment, UserManager<ApplicationUser> userManager)

        {
            _user = await userManager.FindByEmailAsync(payment.Email);
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
                }

                //COMPLETAR PAGAMENTO (criar cartao????)
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
        }


        public async Task<bool> CompleteTicketPayment(string cardID)
        { 
            var card = _client.CardsApi.RetrieveCard(cardID);

            var customerID = card.Card.CustomerId;

            var payment = await _context.Payment
                .Where(c => c.CustomerId == customerID)
                .OrderByDescending(c => c.date) // Assuming 'Date' is the property representing the date
                .FirstOrDefaultAsync();

            if (payment != null) 
            {
                //_client.PaymentsApi.CompletePayment(payment.PaymentId);
                return true;
            }

            return false;
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
                _user.CustomerID = customer.Customer.Id;
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


    }

}


