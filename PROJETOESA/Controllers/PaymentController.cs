using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Square.Models;
using PROJETOESA.Models.SquareModels;
using Castle.Core.Smtp;

namespace PROJETOESA.Controllers
{
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly int _pageSize = 5;
        private readonly UserManager<ApplicationUser> _userManager;
        //private IEasyPayService _easyPayService;


        private readonly SquareService _squareService;
        private readonly AeroHelperContext _context;
        public PaymentController(SquareService squareService, AeroHelperContext context, UserManager<ApplicationUser> userManager,
            IStringLocalizer<PaymentController> stringLocalizer)
        {
            _userManager = userManager;

            _context = context;
            _squareService = squareService;
        }



        [HttpPost]
        [Route("api/payment/purchase-ticket")]
        public async Task<IActionResult> PurchaseTicket([FromBody] PaymentModel payment)
        {

            try
            {
                // Check if required properties are present
                if (payment == null)
                {
                    return BadRequest(new { Message = "Invalid request format" });
                }

                Task<ApplicationUser?> user = _userManager.FindByEmailAsync(payment.Email);

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

                var res = await _squareService.PayAsync(paymentModel, _userManager);

                await _context.SaveChangesAsync();

                // Return a success response
                return Ok(res);


            }
            catch (Exception ex)
            {
                // Return an error response with details
                return BadRequest(new { Message = "Failed to purchase ticket", Error = ex.Message });
            }
        }

        [HttpGet("api/payment/get-cards/{customerEmail}")]
        public async Task<ActionResult<IEnumerable<SquareCard>>> GetCustomerCards(string customerEmail)
        {
            var cards = await _squareService.GetSquareCardAsync(customerEmail, _userManager);

            return Ok(cards);
        }

        [HttpPost]
        [Route("api/payment/pay-now")]
        public async Task<IActionResult> PayNow([FromBody] CustomerCardID custCard)
        {
            var res = await _squareService.CompleteTicketPayment(custCard.customerCardID);

            return Ok(res);
        }

        [HttpPost]
        [Route("api/payment/premium-subscription")]
        public async Task<IActionResult> PayNowPremium([FromBody] CustomerCardID custCard)
        {
            var res = await _squareService.SubscribePremium(custCard.customerCardID);

            return Ok(res);
        }

        [HttpPost]
        [Route("api/payment/cancel-subscription")]
        public async Task<IActionResult> CancelPremiumNow([FromBody] CustomerCardID custCard)
        {
            var res = await _squareService.CancelSubscription(custCard.customerCardID);

            return Ok(res);
        }

        [HttpGet("api/payment/get-user-payments/{userEmail}")]
        public async Task<ActionResult<IEnumerable<PaymentHistoryModel>>> GetCustomerPayments(string userEmail)
        {
            List<PaymentHistoryModel> model = await _squareService.GetSquareCustomerPayments(userEmail, _userManager);

            return Ok(model);
        }
    }

    public class CustomerCardID
    {
        public string customerCardID { get; set; }
    }
}
