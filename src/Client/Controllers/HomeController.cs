using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ImmunizNation.Models;
using ImmunizNation.Client.ViewModels.KnowledgeTest;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using System.IO;
using ImmunizNation.Client.Services;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Identity;
using ImmunizNation.Client.Models;
using MimeKit;

namespace ImmunizNation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICertificateService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public HomeController(
            ILogger<HomeController> logger, 
            ICertificateService service, 
            IEmailService mailService,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
            _service = service;
            _emailService = mailService;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Gets the certificate for the authenticated user.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetCertificate()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error");
            }

            var pdf = await _service.CertificateOfAttendanceAsync(user);

            if (pdf != null)
            {
                using (var stream = new MemoryStream())
                {
                    pdf.Save(stream, false);

                    return File(
                        stream.ToArray(),
                        "application/pdf",
                        _service.GenerateCertificateName(user)
                    );
                }
            }

            return RedirectToAction("Error");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendCertificateEmail()
        {
            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);

                await _service.SendCertificateEmail(user);

            } catch (Exception ex)
            {
                _logger.LogError(ex, "unable to send email certificate");
                return RedirectToAction("Error");
            }
            return RedirectToPage("/Index");

            //var pdf = await _service.CertificateOfAttendanceAsync(user);
            //if (pdf != null)
            //{
            //    using (var stream = new MemoryStream())
            //    {
            //        pdf.Save(stream, false);

            //        string certificateName = user.AccountType == AccountTypes.GeneralPractitioner
            //            ? "Certificate of Completion"
            //            : "Statement of Attendance";

            //        var attachments = new List<EmailAttachment>
            //        {
            //            new EmailAttachment
            //            {
            //                Name = _service.GenerateCertificateName(user),
            //                ByteArray = stream.ToArray(),
            //                ContentType = new ContentType("application", "pdf")
            //            }
            //        };

            //        string htmlMessage = $"<p>Congratulations {user.FirstName} {user.LastName}</p><p>Your { certificateName } is attached.</p>";

            //        _emailService.Send(user.Email, $"ImmunizNation - {certificateName}", htmlMessage, attachments);

            //        return RedirectToPage("/Index");

            //    }
            //}

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        

    }
}
