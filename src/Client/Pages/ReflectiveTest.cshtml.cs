using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ImmunizNation.Client.Data;
using ImmunizNation.Client.Models;
using ImmunizNation.Client.ViewModels.ReflectiveQuestions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ImmunizNation.Client.Services;
using Microsoft.AspNetCore.Authorization;

namespace ImmunizNation.Client.Pages
{
    [Authorize]
    public class ReflectiveTestModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReflectiveQuestionsService _service;

        [BindProperty]
        public IList<ReflectiveQuestionViewModel> ReflectiveQuestions { get; set; }

        [BindProperty]
        public bool IsDraft { get; set; }

        [TempData]
        public bool ShowModal { get; set; }

        [TempData]
        public bool ShowSaveModal { get; set; }
        

        public ReflectiveTestModel(
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context,
            IReflectiveQuestionsService service)
        {
            _userManager = userManager;
            _context = context;
            _service = service;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if(user.AccountType != AccountTypes.GeneralPractitioner)
            {
                return RedirectToPage("./Index");
            }

            ReflectiveQuestions = await _service.GetReflectiveQuestions();
            ReflectiveQuestionExam exam = await _service.GetExamByUserAsync(user);

            IsDraft = true;
            ShowModal = false;
            ShowSaveModal = false;

            if (exam != null)
            {
                IsDraft = exam.IsDraft;

                for (int i = 0; i < exam.Answers.Count; i++)
                {
                    ReflectiveQuestions[i].Answer = exam.Answers.ElementAt(i).Answer;
                }
            }

            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            await _service.AddOrUpdate(user, ReflectiveQuestions, IsDraft);

            ShowModal = !IsDraft;
            ShowSaveModal = IsDraft;

            return Page();
        }
    }
}
