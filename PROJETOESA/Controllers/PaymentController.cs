using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROJETOESA.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using PROJETOESA.Services;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Data;
using System.Diagnostics;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Localization;
using PROJETOESA.Services.EasyPay;

namespace PROJETOESA.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class PaymentController : Controller
    {
        private readonly AeroHelperContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<PaymentController> _stringLocalizer;
        private readonly IEmailSender _emailSender;
        private IEasyPayService _easyPayService;

        private readonly int _pageSize = 5;

        public PaymentController(AeroHelperContext context, UserManager<ApplicationUser> userManager, 
            IStringLocalizer<PaymentController> stringLocalizer, IEmailSender emailSender, 
            IEasyPayService easyPayService)
        {
            _context = context;
            _userManager = userManager;
            _stringLocalizer = stringLocalizer;
            _emailSender = emailSender;
            _easyPayService = easyPayService;
        }

        //public async Task<IActionResult> PaymentsHistory(int? page, string order) { }

        //public async Task<IActionResult> PaymentDetails(Guid id)
        //{
        //    if(User.Identity != null && User.Identity)
        //}
    }
}