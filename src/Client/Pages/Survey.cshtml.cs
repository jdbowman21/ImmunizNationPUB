using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImmunizNation.Client.Data;
using ImmunizNation.Client.Models;
using ImmunizNation.Client.Models.EvaluationSurvey;
using ImmunizNation.Client.Services;
using ImmunizNation.Client.ViewModels.EvaluationSurvey;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ImmunizNation.Client.Pages
{
    public class SurveyModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEvaluationSurveyService _service;

        [TempData]
        public bool ShowSubmitModal { get; set; }

        [BindProperty]
        public IList<EvaluationSurveyGroupsViewModel> SurveyGroups { get; set; }

        public SurveyModel(
            UserManager<ApplicationUser> userManager,
            IEvaluationSurveyService service)
        {
            _userManager = userManager;
            _service = service;
        }


        public async Task OnGetAsync()
        {
            SurveyGroups = await _service.GetSurveyAsync();

            //EvaluationSurveyViewModel survey = await _service.GetSurveyAsync();
            //Questions = survey.Questions.ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // check if atleast one qustion is answered so completely empty surveys cannot be submitted.

            int answered = 0;

            EvaluationSurvey survey = new EvaluationSurvey
            {
                CreatedDate = DateTime.UtcNow,
                UserAnswers = new List<EvaluationSurveyUserAnswer>()
            };

            foreach (var group in SurveyGroups)
            {
                foreach (var question in group.Questions)
                {
                    if (group.Questions == null)
                        continue;

                    if (!String.IsNullOrEmpty(question.UserAnswer))
                    {
                        survey.UserAnswers.Add(new EvaluationSurveyUserAnswer
                        {
                            QuestionId = question.Id,
                            Value = question.UserAnswer
                        });
                        answered++;
                    }
                }
            }

            await _service.AddSurveyAsync(survey);

            if(answered == 0)
            {
                ModelState.AddModelError(String.Empty, "You must answer atleast one question to complete the Evaluation Survey.");
            }

            if(!ModelState.IsValid)
            {
                return Page();
            }

            return RedirectToPage("./Resources");
        }
    }
}
