using ImmunizNation.Client.Data;
using ImmunizNation.Client.Models;
using ImmunizNation.Client.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ImmunizNation.Client.ViewModels.ReflectiveQuestions;
using System;
using ImmunizNation.Client.ViewModels.EvaluationSurvey;
using ImmunizNation.Client.Models.EvaluationSurvey;

namespace ImmunizNation.Client.Services
{
    public interface IAdminService
    {
        /// <summary>
        /// Returns the queryable version for getting users.
        /// </summary>
        /// <returns></returns>
        IQueryable<ApplicationUserViewModel> GetQueyableUsers();

        /// <summary>
        /// Returns a list of all the users.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ApplicationUserViewModel>> GetAllUsersAsync();

        /// <summary>
        /// Returns a list of all the reflective activity for excel exports.
        /// </summary>
        /// <returns></returns>
        Task<ReflectiveActivityViewModel> GetReflectiveActivityAsync();

        Task<ICollection<EvaluationSurveyReportColumnViewModel>> GetEvaluationSurveyAsync();
    }

    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUserViewModel>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u=>u.AccountType != AccountTypes.None)
                .Select(u=> new ApplicationUserViewModel { 
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    AccountType = u.AccountType,
                    City = u.City,
                    Province = u.Province,
                    CertSessionId = u.CertSessionId,
                    LocationOfSession = u.LocationOfSession,
                    DateOfSession = u.DateOfSession,
                    SubscriptionEmail = u.SubscriptionEmail,
                    Score = _context.KnowledgeTests.Where(t=>t.UserId == u.Id).OrderBy(t=>t.Score).Select(t=> t.Score).FirstOrDefault(),
                    RACompleted = HasUserCompletedRA(_context, u.Id),
                    CreatedDate = u.CreatedDate
                }).ToListAsync();
        }

        private static bool HasUserCompletedRA(ApplicationDbContext context, string userId)
        {
            // make sure the user as entered all the questions.
            var questionCount = context.ReflectiveQuestions.Count();
            var answers = context.ReflectiveQuestionExam
                .Where(m => m.IsDraft == false && m.UserId == userId)
                .Include(m => m.Answers)
                .FirstOrDefault();

            // No Reflective Answers have been entered.
            if (answers == null) return false;

            return questionCount == answers.Answers.Count;
        }

        public IQueryable<ApplicationUserViewModel> GetQueyableUsers()
        {
            return _context.Users.Where(u=>u.AccountType != AccountTypes.None).Select(u => new ApplicationUserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                AccountType = u.AccountType,
                City = u.City,
                Province = u.Province,
                CertSessionId = u.CertSessionId,
                LocationOfSession = u.LocationOfSession,
                DateOfSession = u.DateOfSession,
                SubscriptionEmail = u.SubscriptionEmail,
                Score = _context.KnowledgeTests.Where(t => t.UserId == u.Id).OrderByDescending(t => t.Score).Select(t => t.Score).FirstOrDefault(),
                RACompleted = HasUserCompletedRA(_context, u.Id),
                CreatedDate = u.CreatedDate
            });
        }

        public async Task<ReflectiveActivityViewModel> GetReflectiveActivityAsync()
        {
            ReflectiveActivityViewModel vm = new ReflectiveActivityViewModel();
            Dictionary<string, string> userCache = new Dictionary<string, string>();

            vm.Columns = await _context.ReflectiveQuestions.Select(m=> m.Description).ToListAsync();
            vm.Results = new List<ReflectiveActivityResultViewModel>();

            var questions = await _context.ReflectiveQuestionExam
                .OrderBy(m=>m.UserId)
                .Include(m=>m.Answers)
                .ToListAsync();

            foreach (var question in questions)
            {
                if(!userCache.ContainsKey(question.UserId))
                {
                    var user = await _context.Users.Where(m => m.Id == question.UserId).FirstOrDefaultAsync();
                    userCache.Add(user.Id, user.FirstName + " " + user.LastName);
                }

                ReflectiveActivityResultViewModel result = new ReflectiveActivityResultViewModel();
                result.Name = userCache[question.UserId];
                result.Answers = question.Answers.Select(m => m.Answer).ToList();

                vm.Results.Add(result);
            }

            return vm;
        }

        public async Task<ICollection<EvaluationSurveyReportColumnViewModel>> GetEvaluationSurveyAsync()
        {
            var vm = new EvaluationSurveyReportViewModel();

            var surveys = await _context.EvaluationSurveys.OrderBy(m=>m.Id).Include(m=>m.UserAnswers).ToListAsync();
            var groups = await _context.EvaluationSurveyGroups.OrderBy(m => m.Order).Include(m => m.Questions).ToListAsync();

            List<EvaluationSurveyReportColumnViewModel> columns = new List< EvaluationSurveyReportColumnViewModel>();

            columns.Add(new EvaluationSurveyReportColumnViewModel 
            {
                Id = 0,
                Header = "Date Submitted",
                InputType = InputType.DateTime,
                Rows = surveys.Select(survey=> survey.CreatedDate.ToString("MM/dd/yyyy")).ToList()
            });


            // configure the column headers before looping through each survey and getting the user answers.
            foreach (var group in groups)
            {
                // sort each groups question manually since we cannot achieve this with the .Include() through EF.
                var questions = group.Questions.OrderBy(m => m.Order).ToList();
                foreach (var question in questions)
                {
                    columns.Add(new EvaluationSurveyReportColumnViewModel
                    {
                        Id = question.Id,
                        Header = question.Description,
                        InputType = question.InputType
                    });
                }
            }

            foreach (var survey in surveys)
            {
                foreach (var column in columns)
                {
                    bool bFound = false;

                    foreach (var answer in survey.UserAnswers)
                    {
                        if (answer.QuestionId == column.Id)
                        {
                            column.Rows.Add(answer.Value);
                            bFound = true;
                        }
                    }

                    if (!bFound)
                    {
                        column.Rows.Add("");
                    }
                }
            }

            return columns;
        }
    }
}
