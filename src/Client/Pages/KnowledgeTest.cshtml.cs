using ImmunizNation.Client.Data;
using ImmunizNation.Client.Models;
using ImmunizNation.Client.Services;
using ImmunizNation.Client.ViewModels.KnowledgeTest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
/**
* 1. pull the questions and answers from the database and build the viewmodel onGet
* 2. Need to pull the question answer from the database onPost.
*/

namespace ImmunizNation.Client.Pages
{
    [Authorize]
    public class KnowledgeTestModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<KnowledgeTestModel> _logger;
        private readonly IKnowledgeTestService _service;
        private readonly IOptions<AppSettings> _options;

        public KnowledgeTestModel(
            UserManager<ApplicationUser> userManager,
            IKnowledgeTestService service,
            ILogger<KnowledgeTestModel> logger,
            ApplicationDbContext context,
            IOptions<AppSettings> settings)
        {
            _context = context;
            _service = service;
            _logger = logger;
            _userManager = userManager;
            _options = settings;
        }

        public string ReturnUrl { get; set; }

        [BindProperty]
        public double PassingScore { get; set; }

        /// <summary>
        /// Represents the questions for the knowledge test.
        /// </summary>
        [BindProperty]
        public IList<KnowledgetTestQuestionViewModel> Questions { get; set; }

        public KnowledgetTestResultsViewModel Results { get; set; }

        public double Score { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var exams = await _service.GetExamsByUser(user);

            KnowledgeTest exam = null;
            foreach (var item in exams)
            {
                if (item.Score.HasValue && item.Score >= _options.Value.RequiredKnowledgeTestScore)
                {
                    exam = item;
                    Score = item.Score.Value;
                    break;
                }
            }
            
            Questions = await _service.GetKnowledgetTestQuestions();

            if (exam != null)
            {
                foreach (var question in Questions)
                {
                    var answer = GetAnswerByQuestion(question, exam);
                    if (answer != null)
                    {
                        question.UserAnswer = answer.Value;
                    }
                }
            }
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(IList<KnowledgetTestQuestionViewModel> questions)
        {
            // Users are required to fill
            foreach (var question in Questions)
            {
                if (String.IsNullOrEmpty(question.UserAnswer))
                {
                    ModelState.AddModelError(String.Empty, "You must answer all the questions to complete the Learner Assessment Test.");
                    break;
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            double numOfQuestions = Questions.Count;
            double correctAnswers = 0;

            foreach (var question in Questions)
            {
                if (question.UserAnswer == question.Answer)
                {
                    correctAnswers++;
                }
            }


            // don't divide by zero.
            double result = correctAnswers == 0 ? correctAnswers : (correctAnswers / numOfQuestions);

            Results = new KnowledgetTestResultsViewModel(result)
            {
                AccountType = user.AccountType
            };

            await _service.AddKnowledgeTest(user, questions);
            return Page();
        }

        private KnowledgeTestUserAnswer GetAnswerByQuestion(KnowledgetTestQuestionViewModel vm, KnowledgeTest exam)
        {
            foreach (var answer in exam.Answers)
            {
                if (vm.Id == answer.KnowledgeTestQuestionId)
                {
                    return answer;
                }
            }
            return null;
        }

        public bool HasPassedTest()
        {
            if (Score != 0 && Score >= _options.Value.RequiredKnowledgeTestScore) 
                return true;
 
            return false;
        }
    }
}
