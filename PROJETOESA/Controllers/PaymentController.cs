using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PROJETOESA.Services.EasyPay;
using System.Globalization;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly int _pageSize = 5;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private IEasyPayService _easyPayService;


        private readonly SquareService _squareService;
        private readonly AeroHelperContext _context;
        public PaymentController(SquareService squareService, AeroHelperContext context, UserManager<ApplicationUser> userManager,
            IStringLocalizer<PaymentController> stringLocalizer, IEmailSender emailSender,
            IEasyPayService easyPayService, IOptions<EmailSender> emailSettings) 
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _easyPayService = easyPayService;
            _context = context;
            _squareService = squareService;
        }



        [HttpPost]
        [Route("api/payment/purchase-ticket")]
        public async Task<IActionResult> PurchaseTicket([FromBody] PaymentModel payment)
        {
            Debug.WriteLine("AQUI Servidor");
            Debug.WriteLine(payment.price);

            try
            {
                // Check if required properties are present
                if (payment == null)
                {
                    return BadRequest(new { Message = "Invalid request format" });
                }

                PaymentModel paymentModel = new PaymentModel
                {
                    // Set properties based on userInput or other logic
                    price = payment.price, // Assuming price is a property in userInput
                    currency = payment.currency,
                    Email = payment.Email,
                    CreditCard = payment.CreditCard,
                    FirstName = payment.FirstName,
                    LastName = payment.LastName,
                    ShippingAddress = payment.ShippingAddress,
                };

                _squareService.PayAsync(paymentModel);

                await _context.SaveChangesAsync();

                // Return a success response
                return Ok(new { Message = "Ticket purchased successfully" });


            }
            catch (Exception ex)
            {
                // Return an error response with details
                return BadRequest(new { Message = "Failed to purchase ticket", Error = ex.Message });
            }
        }

        [HttpGet("history")]
        [Authorize(Roles = "ClienteNormal, ClientePremium")]
        public async Task<IActionResult> GetPaymentsHistory(int? page, string order)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.FindByEmailAsync(User.Identity.Name);

                if (user.Role.Equals(TipoConta.ClienteNormal) || user.Role.Equals(TipoConta.ClientePremium))
                {
                    var userPayments = await _context.Payment.ToListAsync();

                    int pageNumber = (page ?? 1);
                    var totalPages = Math.Ceiling((double)userPayments.Count() / _pageSize);

                    switch (order)
                    {
                        case "PAsc":
                            userPayments = userPayments.OrderBy(b => b.Price).ToList();
                            break;
                        case "PDesc":
                            userPayments = userPayments.OrderByDescending(b => b.Price).ToList();
                            break;
                        case "SAsc":
                            userPayments = userPayments.OrderBy(b => b.paymentStatus).ToList();
                            break;
                        case "SDesc":
                            userPayments = userPayments.OrderByDescending(b => b.paymentStatus).ToList();
                            break;
                        default:
                            userPayments = userPayments.OrderBy(b => b.paymentStatus).ToList();
                            break;
                    }

                    var paginatedData = userPayments.Skip((pageNumber - 1) * _pageSize).Take(_pageSize).ToList();

                    return Ok(paginatedData);
                }
            }

            return Unauthorized(new { error = "Unauthorized" });
        }

        [HttpGet("details/{id}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentDetails(Guid id)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var payments = await _context.Payment
                    .ToListAsync();

                var payment = payments.Find(p => p.PaymentId == id);

                if (payment == null)
                {
                    return NotFound(new { error = "Payment not found" });
                }
            }

            return Unauthorized(new { error = "Unauthorized" });
        }

        [HttpPost("/generateTemporaryPayment")]
        public async Task<IActionResult> GenerateTemporaryPayment(double paymentValue, string paymentValueString)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            bool isPaymentValueValid = false;
            if (paymentValueString != null)
            {
                isPaymentValueValid = double.TryParse(paymentValueString.Replace(',', '.').Replace("-", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out _);
            }

            if (!isPaymentValueValid)
            {
                return BadRequest("Invalid payment value");
            }

            if (paymentValue == 0.0)
            {
                Payment zeroPayment = new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    Price = paymentValue,
                    Entity = 00000,
                    Reference = 000000000,
                    LimitDate = DateTime.Now,
                    paymentStatus = PaymentStatus.Paid,
                    PaymentMethod = "NA"
                };

                _context.Payment.Add(zeroPayment);
            }
            else
            {
                Payment tempPayment = new Payment
                {
                    PaymentId = Guid.NewGuid(),
                    Price = paymentValue,
                    Entity = null,
                    Reference = null,
                    LimitDate = DateTime.Now.AddDays(2.0),
                    paymentStatus = PaymentStatus.WaitingApproval,
                    PaymentMethod = null
                };

                _context.Payment.Add(tempPayment);
                
                await _emailSender.SendEmailAsync(user.Email, "Pagamento Temporário", "{_stringLocalizer[\"payments.tempPaymentDescription\"].Value}");
            }
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("/createPayment")]
        public async Task<IActionResult> CreatePayment(Guid id, string method)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            Payment tempPayment = await _context.Payment.Where(p => p.PaymentId == id).FirstAsync();

            DateTime dateLimit = DateTime.Now;

            if (method.Equals("MB WAY"))
            {
                dateLimit = dateLimit.AddMinutes(5);
                method = "mbw";
            }
            else
            {
                dateLimit = dateLimit.AddDays(2);
                method = "mb";
            }

            Payment defPayment = await _easyPayService.CreateSinglePayment(dateLimit, user, tempPayment.Price, method);

            if (defPayment.PaymentId == Guid.Empty)
            {
                return Ok("sucesso");
            }

            _context.Payment.Remove(tempPayment);
            _context.Payment.Add(defPayment);

            if (method.Equals("mb"))
            {
                await _emailSender.SendEmailAsync(user.Email, "Pagamento Criado", "{_stringLocalizer[\"payments.emailEntityText\"].Value}: " +
                    "{defPayment.Entity}{_stringLocalizer[\"payments.emailReferenceText\"].Value}: {defPayment.Reference}" +
                    "{_stringLocalizer[\"payments.emailPriceText\"].Value}: {defPayment.Price}€" +
                    "{_stringLocalizer[\"payments.emailLimitDateText\"].Value}: {defPayment.LimitDate}");
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        
    }
}
