using ImmunizNation.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImmunizNation.Client.ViewModels.EvaluationSurvey;
using Microsoft.EntityFrameworkCore;
using ImmunizNation.Client.Models.EvaluationSurvey;

namespace ImmunizNation.Client.Services
{
    public interface IEvaluationSurveyService
    {
        Task<IList<EvaluationSurveyGroupsViewModel>> GetSurveyAsync();
        Task AddSurveyAsync(EvaluationSurvey survey);
    }

    public class EvaluationSurveyService : IEvaluationSurveyService
    {
        private readonly ApplicationDbContext _context;

        public EvaluationSurveyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<EvaluationSurveyGroupsViewModel>> GetSurveyAsync()
        {
            return  await _context.EvaluationSurveyGroups.OrderBy(m=>m.Order).Select(group => new EvaluationSurveyGroupsViewModel
            {
                Id = group.Id,
                Title = group.Title,
                Description = group.Description,
                Questions = _context.EvaluationSurveyQuestions
                    .Where(m=>m.GroupId == group.Id)
                    .OrderBy(m=>m.Order)
                    .Select(question => new EvaluationSurveyQuestionsViewModel
                    {
                        Id = question.Id,
                        GroupId = question.GroupId,
                        Description = question.Description,
                        InputType = question.InputType,
                        Options = _context.EvaluationSurveyOptions
                            .Where(e=>e.QuestionId == question.Id)
                            .OrderBy(e=>e.Order)
                            .ToList()
                    }).ToList()
            }).ToListAsync();
        }

        public async Task AddSurveyAsync(EvaluationSurvey survey)
        {
            _context.Add(survey);
            await _context.SaveChangesAsync();
        }
    }
}
