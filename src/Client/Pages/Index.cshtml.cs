using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImmunizNation.Client.Data;
using ImmunizNation.Client.Models;
using ImmunizNation.Client.Services;
using ImmunizNation.Client.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ImmunizNation.Client.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReflectiveQuestionsService _reflectiveQuestionsService;
        private readonly IKnowledgeTestService _knowledgeTestService;

        public string UserName { get; set; }

        public double PassingGrade { get; set; }

        public ApplicationUser AppUser { get; set; }

        [BindProperty]
        public ReflectiveQuestionExam ReflectiveExamResult { get; set; }

        [TempData]
        public AccountTypes AccountType { get; set; }

        [TempData]
        bool IsCertificateComplete { get; set; }

        public KnowledgeTest HighestScoreExam { get; set; }
        public KnowledgeTest LastExam { get; set; }
        public IList<KnowledgeTest> KnowledgetTests { get; set; }

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IReflectiveQuestionsService reflectiveQuestionsService,
            IKnowledgeTestService knowlegetTestService
        )
        {
            _context = context;
            _userManager = userManager;
            _reflectiveQuestionsService = reflectiveQuestionsService;
            _knowledgeTestService = knowlegetTestService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // admins on login will be redirected to the admin dashbaord.
            if(User.IsInRole("Admin"))
            {
                return RedirectToPage("./Admin");
            }

            PassingGrade = 0.7;

            AppUser = await _userManager.GetUserAsync(User);

            if(AppUser != null)
            {
                AccountType = AppUser.AccountType;
                UserName = AppUser.FirstName + " " + AppUser.LastName;

                KnowledgetTests = await _knowledgeTestService.GetExamsByUser(AppUser);
                ReflectiveExamResult = await _reflectiveQuestionsService.GetExamByUserAsync(AppUser);

                // get the best attempt.
                HighestScoreExam = GetHighscoreExam(KnowledgetTests);

                // evaluate the knowlege test
                var questions = await _context.KnowledgeTestsQuestions.FirstOrDefaultAsync();

                LastExam = GetLastKnowledgeTest(KnowledgetTests);
            }

            return Page();
        }

        private KnowledgeTest GetHighscoreExam(IList<KnowledgeTest> exams)
        {
            return exams.OrderByDescending(m=>m.Score).FirstOrDefault();
        }

        private KnowledgeTest GetLastKnowledgeTest(IList<KnowledgeTest> exams)
        {
            return exams.OrderByDescending(m => m.CreatedDate).FirstOrDefault();
        } 

        public bool CompletedReflectiveQuestions()
        {
            if(ReflectiveExamResult == null || ReflectiveExamResult.IsDraft)
            {
                return false;
            }

            int answered = 0;
            int questions = 0;
            foreach (var answer in ReflectiveExamResult.Answers)
            {
                if(!String.IsNullOrEmpty(answer.Answer))
                {
                    answered++;
                }
                questions++;
            }

            return answered == questions;
        }
    }
}
