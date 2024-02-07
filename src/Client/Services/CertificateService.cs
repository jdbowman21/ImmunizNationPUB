using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ImmunizNation.Client.Data;
using ImmunizNation.Client.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using System.Text.RegularExpressions;
using MimeKit;

namespace ImmunizNation.Client.Services
{
    public interface ICertificateService
    {
        Task<PdfDocument> CertificateOfAttendanceAsync(ApplicationUser user);
        Task<PdfDocument> CertificateOfAttendanceAsync(string userId);
        Task SendCertificateEmail(ApplicationUser user);

        Task SendCertificateEmail(string userId);
    }

    /// <summary>
    /// Service for generating Certificates
    /// </summary>
    public class CertificateService : ICertificateService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CertificateService> _logger;
        private readonly IEmailService _emailService;

        public CertificateService(IServiceProvider services, 
            IConfiguration configuration, 
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CertificateService> logger,
            IEmailService emailService)
        {
            _services = services;
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _emailService = emailService;
        }

        /// <summary>
        /// Gets the certificate for the user when they have completed their examinations.
        /// </summary>
        /// <exception cref="System.ArgumentException">Thrown when the userId is not found.</exception>
        /// <param name="userId">Database user id</param>
        /// <returns>Bitmap image of the certificate based on the users Account Type.</returns>
        public async Task<PdfDocument> CertificateOfAttendanceAsync(string userId)
        {
            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();

            if(user == null)
            {
                throw new ArgumentException("Invalid user Id: ", userId);
            }

            return await CertificateOfAttendanceAsync(user);
        }

        /// <summary>
        /// Gets the user certificate.
        /// </summary>
        /// <exception cref="System.ArgumentException">Thrown when user is null</exception>
        /// <param name="user">User the certificate is for</param>
        /// <returns>PDF of the certificate based on the users Account Type.</returns>
        public async Task<PdfDocument> CertificateOfAttendanceAsync(ApplicationUser user)
        {
            if(user != null)
            {
                try
                {
                    if(user.AccountType == AccountTypes.GeneralPractitioner) 
                    {
                        return GeneratePDFCertificateCFPC(user);
                    } 
                    else if (user.AccountType == AccountTypes.Pharmacist)
                    {
                        return GeneratePDFCertificateCCCEP(user);
                    }
                } 
                catch(Exception ex)
                {
                    _logger.LogError("Unable to generate generated certificate of attendance.");
                    throw new Exception("Unable to generate certificate", ex);
                }
            } 
            else
            {
                _logger.LogError("Invalid user");
            }

            // user was either not found.
            return null;
        }

        private PdfDocument GeneratePDFCertificateCFPC(ApplicationUser user)
        {
            // get the path for the certificate of attendance CFPC PDF file
            var env = _services.GetService<IWebHostEnvironment>();
            var path = env.ContentRootPath + "\\" + _configuration["GPCertificatePath"];

            PdfDocument document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
            var test = PdfReader.TestPdfFile(path);
            PdfAcroForm form = document.AcroForm;

            for (int i = 0; i < form.Fields.Count; i++)
            {
                if (form.Fields[i].Name == "On")
                {
                    form.Fields[i].Value = new PdfString(user.DateOfSession.ToString("MM/dd/yyyy"));
                }
                else if(form.Fields[i].Name == "Name")
                {
                    form.Fields[i].Value = new PdfString(user.FirstName + " " + user.LastName);
                }
                else if(form.Fields[i].Name == "At")
                {
                    form.Fields[i].Value = new PdfString(user.LocationOfSession);
                }
                else if (form.Fields[i].Name == "Cert")
                {
                    form.Fields[i].Value = new PdfString(user.CertSessionId);
                }
                else if(form.Fields[i].Name == "Province")
                {
                    form.Fields[i].Value = new PdfString(Regex.Replace(user.Province.ToString(), "([A-Z])", " $1").Trim() + " Chapter");
                }
                form.Fields[i].ReadOnly = true;
            }

            if (form.Elements.ContainsKey("/NeedAppearances"))
                form.Elements["/NeedAppearances"] = new PdfBoolean(true);
            else
                form.Elements.Add("/NeedAppearances", new PdfBoolean(true));

            return document;
        }

        private PdfDocument GeneratePDFCertificateCCCEP(ApplicationUser user)
        {
            var env = _services.GetService<IWebHostEnvironment>();

            var path = user.DateOfSession >= new DateTime(2023, 2, 7)
                ? env.ContentRootPath + "\\" + _configuration["PharmCertificatePathv2"]
                : env.ContentRootPath + "\\" + _configuration["PharmCertificatePath"];

            
            //var path = env.ContentRootPath + "\\" + _configuration["PharmCertificatePath"];
            PdfDocument document = PdfReader.Open(path, PdfDocumentOpenMode.Modify);
            PdfAcroForm form = document.AcroForm;

            for (int i = 0; i < form.Fields.Count; i++)
            {
                if (form.Fields[i].Name == "license")
                {
                    form.Fields[i].Value = new PdfString(user.LicenseNumber);
                }
                else if (form.Fields[i].Name == "name")
                {
                    form.Fields[i].Value = new PdfString(user.FirstName + " " + user.LastName + ",");
                }
                else if (form.Fields[i].Name == "date")
                {
                    form.Fields[i].Value = new PdfString(user.DateOfSession.ToString("MM/dd/yyyy"));
                }
                form.Fields[i].ReadOnly = true;
            }

            if (form.Elements.ContainsKey("/NeedAppearances"))
                form.Elements["/NeedAppearances"] = new PdfBoolean(true);
            else
                form.Elements.Add("/NeedAppearances", new PdfBoolean(true));

            return document;
        }

        public async Task SendCertificateEmail(ApplicationUser user)
        {
            var pdf = await CertificateOfAttendanceAsync(user);
            if (pdf != null)
            {
                using (var stream = new MemoryStream())
                {
                    pdf.Save(stream, false);

                    var attachments = new List<EmailAttachment>
                    {
                        new EmailAttachment
                        {
                            Name = this.GenerateCertificateName(user),
                            ByteArray = stream.ToArray(),
                            ContentType = new ContentType("application", "pdf")
                        }
                    };

                    var env = _services.GetService<IWebHostEnvironment>();
                    var templatePath = user.AccountType == AccountTypes.Pharmacist ? _configuration["CompletedPharmEmail"] : _configuration["CompletedGPEmail"];
                    var path = env.ContentRootPath + "\\" + templatePath;

                    using (StreamReader sr = new StreamReader(path))
                    {
                        var htmlMessage = sr.ReadToEnd();
                        _emailService.Send(user.Email, "Congratulations! ImmunizNation Certificate of Completion", htmlMessage, attachments);
                    }
                }
            }
        }

        public async Task SendCertificateEmail(string userId)
        {
            var user = await _context.Users.Where(u=>u.Id == userId).FirstOrDefaultAsync();
            if (user != null)
            {
                await SendCertificateEmail(user);
            }
        }
    }

    public static class CertificateServiceExtensions
    {
        public static string GenerateCertificateName(this ICertificateService service, ApplicationUser user)
        {
            var filename = new StringBuilder();
            //filename.Append(user.FirstName);
            //filename.Append(" ");
            //filename.Append(user.LastName);
            //filename.Append(" - ");
            filename.Append(
                user.AccountType == AccountTypes.GeneralPractitioner
                ? "Certificate of Completion"
                : "Statement of Attendance"
            );
            filename.Append(".pdf");

            return filename.ToString().ToUpper();
        }
    }

}
