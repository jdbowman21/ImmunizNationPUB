using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ImmunizNation.Client.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ImmunizNation.Client.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace ImmunizNation.Client.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        public RegisterModel(
            IServiceProvider services,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _services = services;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Display(Name = "Account Type")]
            public AccountTypes AccountType { get; set; }

            [Display(Name = "Select a Province")]
            public Provinces Province { get; set; }

            [Required]
            [StringLength(255, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 1)]
            [Display(Name ="First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(255, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            public string City { get; set; }

            [StringLength(255, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "License Number")]
            public string LicenseNumber { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            
            public bool SubscriptionEmail { get; set; }

            [Required]
            [Display(Name = "Date of Session")]
            [DataType(DataType.Date)]
            public DateTime DateOfSession { get; set; }

            [Display(Name = "Location of Session")]
            public string LocationOfSession { get; set; }

            [Display(Name = "CERT + Session ID #")]
            public string CertSessionId { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Manually validate the license number because it is being validated for GeneralPractitioners.
            if(Input.AccountType == AccountTypes.Pharmacist && String.IsNullOrEmpty(Input.LicenseNumber))
            {
                ModelState.AddModelError("Input.LicenseNumber", "The Licence Number field is required.");
            }

            if(Input.AccountType == AccountTypes.GeneralPractitioner)
            {
                if (String.IsNullOrEmpty(Input.CertSessionId))
                {
                    ModelState.AddModelError("Input.CertSessionId", "The Cert + Session ID # is required.");
                }
                if(String.IsNullOrEmpty(Input.LocationOfSession))
                {
                    ModelState.AddModelError("Input.LocationOfSession", "The Location of Session is required.");
                }
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser {
                    UserName = Input.Email,
                    Email = Input.Email,
                    AccountType = (AccountTypes)Input.AccountType,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Province = Input.Province,
                    City = Input.City,
                    SubscriptionEmail = Input.SubscriptionEmail,
                    LicenseNumber = Input.LicenseNumber,
                    CreatedDate = DateTime.UtcNow,
                    LocationOfSession = Input.LocationOfSession,
                    CertSessionId = Input.CertSessionId,
                    DateOfSession = Input.DateOfSession
                };
               
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    //var callbackUrl = Url.Page(
                    //    "/Account/ConfirmEmail",
                    //    pageHandler: null,
                    //    values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                    //    protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    var env = _services.GetService<IWebHostEnvironment>();
                    var templatePath = user.AccountType == AccountTypes.Pharmacist ? _configuration["RegisterdPharmEmail"] : _configuration["RegisterdGpEmail"];
                    var path = env.ContentRootPath + "\\" + templatePath;

                    using (StreamReader sr = new StreamReader(path))
                    {
                        var htmlMessage = sr.ReadToEnd();
                        await _emailSender.SendEmailAsync(Input.Email, "Thanks for registering for ImmunizNation", htmlMessage);
                    }

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
