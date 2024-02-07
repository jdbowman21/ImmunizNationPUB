using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ImmunizNation.Client.Models;
using ImmunizNation.Client.Services;
using ImmunizNation.Client.Utilities;
using ImmunizNation.Client.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImmunizNation.Client.Pages
{
    public enum FilterOptions
    {
        Name,
        Email,
        AccountType
    }

    [Authorize(Roles = "Admin")]
    public class AdminModel : PageModel
    {
        private readonly IAdminService _service;
        private readonly ILogger<AdminModel> _logger;
        private readonly IOptions<AppSettings> _options;

        // sorting
        public string NameSort { get; set; }
        public string EmailSort { get; set; }
        public string AccountTypeSort { get; set; }
        public string CurrentSort { get; set; }
        public string ScoreSort { get; set; }
        public string RACompleted { get; set; }
        public string DateOfSessionSort { get; set; }
        // --------

        [BindProperty]
        public string CurrentFilter { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime? DateFilter { get; set; }

        [Display(Name= "Users")]
        public int TotalUsers { get; set; }
        [Display(Name = "Pharmacists ")]
        public int TotalPharmacists { get; set; }
        [Display(Name = "General Practitioners")]
        public int TotalGP { get; set; }

        public PaginatedList<ApplicationUserViewModel> Users { get; set; }

        public AdminModel(
            IAdminService service,
            ILogger<AdminModel> logger,
            IOptions<AppSettings> options)
        {
            _service = service;
            _logger = logger;
            _options = options;
        }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, DateTime? dateFilter, int? pageIndex)
        {
            CurrentSort = sortOrder;

            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            EmailSort = String.IsNullOrEmpty(sortOrder) ? "email_desc" : "";
            AccountTypeSort = String.IsNullOrEmpty(sortOrder) ? "accountType_desc" : "";
            ScoreSort = String.IsNullOrEmpty(sortOrder) ? "score_desc" : "";
            RACompleted = String.IsNullOrEmpty(sortOrder) ? "ra_desc" : "";
            DateOfSessionSort = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";


            DateFilter = dateFilter.HasValue ? dateFilter.Value : null;

            if (!String.IsNullOrEmpty(searchString))
            {
                pageIndex = 1;
            } 
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            var usersQuery = _service.GetQueyableUsers();
            TotalUsers = await usersQuery.CountAsync();
            TotalPharmacists = await usersQuery.Where(c => c.AccountType == AccountTypes.Pharmacist).CountAsync();
            TotalGP = await usersQuery.Where(c => c.AccountType == AccountTypes.GeneralPractitioner).CountAsync();

            if (!String.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(s => s.LastName.Contains(searchString) 
                            || s.FirstName.Contains(searchString)
                            || s.Email.Contains(searchString));
            }

            if(dateFilter.HasValue)
            {
                usersQuery = usersQuery.Where(s=>s.DateOfSession.HasValue && s.DateOfSession.Value == dateFilter.Value);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    usersQuery = usersQuery.OrderByDescending(s => s.LastName);
                    break;
                case "email_desc":
                    usersQuery = usersQuery.OrderByDescending(s => s.Email);
                    break;
                case "accountType_desc":
                    usersQuery = usersQuery.OrderByDescending(s => s.AccountType);
                    break;
                case "score_desc":
                    usersQuery = usersQuery.OrderByDescending(s => s.Score);
                    break;
                case "ra_desc":
                    usersQuery = usersQuery.OrderByDescending(s => s.AccountType);
                    break;
                case "date_desc":
                    usersQuery = usersQuery.OrderByDescending(s => s.DateOfSession);
                    break;
                default:
                    usersQuery = usersQuery.OrderBy(s => s.FirstName);
                    break;
            }

            _logger.LogInformation("Creating Application User Paginated List...");

            Users = await PaginatedList<ApplicationUserViewModel>.CreateAsync(
                usersQuery.AsNoTracking(), pageIndex ?? 1, _options.Value.PageSize);
        }
    }
}
