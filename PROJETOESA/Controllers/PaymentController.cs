using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Data;
using PROJETOESA.Models;
using PROJETOESA.Services;
using System.Text.Json;

namespace PROJETOESA.Controllers
{
    public class PaymentController : Controller
    {

        private readonly SquareService _squareService;
        private readonly AeroHelperContext _context;
        public PaymentController(SquareService squareService, AeroHelperContext context) 
        {
            _context = context;
            _squareService = squareService;
        }



        [HttpPost]
        [Route("api/trip-details/purchase-ticket")]
        //[Authorize]
        public async Task<IActionResult> PurchaseTicket([FromBody] PaymentModel payment)
        {
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
    }
}
