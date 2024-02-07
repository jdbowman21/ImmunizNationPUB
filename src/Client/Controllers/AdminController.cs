using ClosedXML.Excel;
using ImmunizNation.Client.Models;
using ImmunizNation.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImmunizNation.Client.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdminService _service;
        private readonly ICertificateService _certificateService;
        private readonly IResourceService _resourceService;

        private readonly string[] columns =
        {
            "First name", "Last name", "Email", "City", "Province",  "Account Type", "Cert + Session ID #", "Location of Session", "Date of Session", "Score", "RA Completed", "Allow Email Notifications", "Registered date"
        };

        public AdminController(
            UserManager<ApplicationUser> userManager,
            IAdminService service,
            ICertificateService certificateService,
            IResourceService resourceService)
        {
            _userManager = userManager;
            _service = service;
            _certificateService = certificateService;
            _resourceService = resourceService;
        }

        [HttpPost]
        public async Task<IActionResult> ExportAllUsers(string returnUrl)
        {
            using (var workbook = new XLWorkbook())
            {
                await CreateUserWorksheet(workbook);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ImmunizNation-Users.xlsx");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportReflectiveActivity(string returnUrl)
        {
            using (var workbook = new XLWorkbook())
            {
                await CreateReflectiveActivityWorksheet(workbook);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ImmunizNation-Reflective-Activity.xlsx");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportEvaluationSurvey()
        {
            using (var workbook = new XLWorkbook()) 
            {
                await CreateEvaluationSurveyWorksheet(workbook);

                using(var stream  = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ImmunizNation-Reflective-Activity.xlsx");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportWorkbook(string returnUrl)
        {
            using (var workbook = new XLWorkbook())
            {
                await CreateUserWorksheet(workbook);
                await CreateReflectiveActivityWorksheet(workbook);
                await CreateEvaluationSurveyWorksheet(workbook);
                await GetResourceWorksheetAsync(workbook);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ImmunizNation-Workbook.xlsx");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportResourceSpreadsheet()
        {
            using(var workbook = new XLWorkbook())
            {
                await GetResourceWorksheetAsync(workbook);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ImmunizNation-Resource-Downloads.xlsx");
                }
            }
        }

        public async Task<IActionResult> SendCertificateByUserId(string userId)
        {
            await _certificateService.SendCertificateEmail(userId);

            return RedirectToPage("/Admin");
        }

        private async Task<IXLWorksheet> CreateUserWorksheet(IXLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Users");
            var currentRow = 1;
            var currentColumn = 1;

            // Create the excell headers.
            foreach (var column in columns)
            {
                IXLCell cell = worksheet.Cell(currentRow, currentColumn);
                cell.Value = column;
                cell.Style.Font.SetBold(true);
                currentColumn++;
            }

            var users = await _service.GetAllUsersAsync();

            foreach (var user in users)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = user.FirstName;
                worksheet.Cell(currentRow, 2).Value = user.LastName;
                worksheet.Cell(currentRow, 3).Value = user.Email;
                worksheet.Cell(currentRow, 4).Value = user.City;
                worksheet.Cell(currentRow, 5).Value = user.Province;
                worksheet.Cell(currentRow, 6).Value = Regex.Replace(user.AccountType.ToString(), "([a-z])([A-Z])", "$1 $2");
                worksheet.Cell(currentRow, 7).Value = user.AccountType == AccountTypes.Pharmacist ? "N/A" : user.CertSessionId;
                worksheet.Cell(currentRow, 8).Value = user.LocationOfSession;
                worksheet.Cell(currentRow, 9).Value = user.DateOfSession;
                worksheet.Cell(currentRow, 10).Value = user.ScoreAsPercent;
                worksheet.Cell(currentRow, 11).Value = user.RACompleted ? "Complete" : "Incomplete";
                worksheet.Cell(currentRow, 12).Value = user.SubscriptionEmail;
                worksheet.Cell(currentRow, 13).Value = user.CreatedDate.ToLongDateString();
            }

            worksheet.Columns().AdjustToContents();

            return worksheet;
        }

        private async Task<IXLWorksheet> CreateReflectiveActivityWorksheet(IXLWorkbook workbook)
        {
            var results = await _service.GetReflectiveActivityAsync();
            var worksheet = workbook.Worksheets.Add("Reflective Activity");
            var currentRow = 1;
            var currentColumn = 1;

            // Create the excell headers.
            var columns = new List<string> { "Name" };
            columns.AddRange(results.Columns);

            foreach (var column in columns)
            {
                IXLCell cell = worksheet.Cell(currentRow, currentColumn);
                cell.Value = column;
                cell.Style.Font.SetBold(true);
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;

                worksheet.Column(currentColumn).Width = 75;

                currentColumn++;
            }

            foreach (var result in results.Results)
            {
                currentColumn = 1;
                currentRow++;

                // Enter the users name.
                IXLCell cell = worksheet.Cell(currentRow, currentColumn);
                cell.Value = result.Name;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                
                foreach (var answer in result.Answers)
                {
                    currentColumn++;

                    cell = worksheet.Cell(currentRow, currentColumn);

                    cell.Value = answer;
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                }
            }

            // adjust the name column to fit to contents only.
            worksheet.Column(1).AdjustToContents();

            return worksheet;
        }

        private async Task<IXLWorksheet> CreateEvaluationSurveyWorksheet(IXLWorkbook workbook)
        {
            var results = await _service.GetEvaluationSurveyAsync();
            var worksheet = workbook.Worksheets.Add("Evaluation Survey");
            var currentRow = 1;
            var currentColumn = 1;


            // build the spreadsheet column by column.
            foreach (var column in results)
            {
                // add the columns header
                IXLCell cell = worksheet.Cell(currentRow, currentColumn);
                cell.Value = column.Header;
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;

                foreach (var row in column.Rows)
                {
                    currentRow++;
                    cell = worksheet.Cell(currentRow, currentColumn);
                    cell.Value = row;
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                }

                if(column.InputType == Models.EvaluationSurvey.InputType.DateTime)
                {
                    worksheet.Column(currentColumn).Width = 20;
                }
                else
                {
                    worksheet.Column(currentColumn).Width = 50;
                }

                currentColumn++;
                currentRow = 1;
            }

            return worksheet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> GetCertificateByUserId(string userId)
        {
            var pdf = await _certificateService.CertificateOfAttendanceAsync(userId);
            
            var user = await _userManager.GetUserAsync(User);

            if (pdf != null)
            {
                using (var stream = new MemoryStream())
                {
                    pdf.Save(stream, false);

                    return File(
                        stream.ToArray(),
                        "application/pdf",
                        _certificateService.GenerateCertificateName(user)
                    );
                }
            }

            return RedirectToAction("Error");
        }

        private async Task<IXLWorksheet> GetResourceWorksheetAsync(IXLWorkbook workbook)
        {
            var resourceDownloads = await _resourceService.GetResourceDownloads();
            var worksheet = workbook.Worksheets.Add("Resources");
            var currentRow = 1;
            var currentColumn = 1;

            IXLCell cell = worksheet.Cell(currentRow, currentColumn);
            cell.Value = "Resource";
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;

            worksheet.Column(currentColumn).Width = 50;

            currentColumn++;

            cell = worksheet.Cell(currentRow, currentColumn);
            cell.Value = "Username";
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;

            worksheet.Column(currentColumn).Width = 40;

            currentColumn++;

            cell = worksheet.Cell(currentRow, currentColumn);
            cell.Value = "First name";
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;

            worksheet.Column(currentColumn).Width = 25;

            currentColumn++;

            cell = worksheet.Cell(currentRow, currentColumn);
            cell.Value = "Last name";
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;

            worksheet.Column(currentColumn).Width = 25;

            currentColumn++;

            cell = worksheet.Cell(currentRow, currentColumn);
            cell.Value = "Profession";
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;

            worksheet.Column(currentColumn).Width = 20;

            currentColumn++;

            cell = worksheet.Cell(currentRow, currentColumn);
            cell.Value = "Downloads";
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Column(currentColumn).Width = 15;

            currentColumn = 1;

            foreach(var resourceDownload in resourceDownloads)
            {
                currentRow++;
                cell = worksheet.Cell(currentRow, currentColumn);
                cell.Value = resourceDownload.Resource.Title;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                currentColumn++;

                cell = worksheet.Cell(currentRow, currentColumn);
                cell.Value = resourceDownload.User.UserName;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                currentColumn++;

                cell = worksheet.Cell(currentRow, currentColumn);
                cell.Value = resourceDownload.User.FirstName;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                currentColumn++;

                cell = worksheet.Cell(currentRow, currentColumn);
                cell.Value = resourceDownload.User.LastName;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                currentColumn++;

                cell = worksheet.Cell(currentRow, currentColumn);
                cell.Value = (resourceDownload.User.AccountType == AccountTypes.Pharmacist ? "Pharmacist" : "General Practitioner");
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                currentColumn++;

                cell = worksheet.Cell(currentRow, currentColumn);
                cell.Value = resourceDownload.Count;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                currentColumn = 1;
            }

            //foreach (var resource in resourceDownloads)
            //{
            //    // add the columns header
            //    currentRow++;
            //    cell = worksheet.Cell(currentRow, currentColumn);
            //    cell.Value = resource;
            //    cell.Style.Alignment.WrapText = true;
            //    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            //    currentColumn++;

            //    cell = worksheet.Cell(currentRow, currentColumn);
            //    //cell.Value = resource.DownloadCount;
            //    cell.Style.Alignment.WrapText = true;
            //    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            //    currentColumn = 1;
            //}

            return worksheet;
        }
    }
}
