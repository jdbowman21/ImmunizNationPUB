using ImmunizNation.Client.Data;
using ImmunizNation.Client.Models;
using ImmunizNation.Client.ViewModels.ReflectiveQuestions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImmunizNation.Client.Services
{
    public interface IReflectiveQuestionsService
    {
        Task AddOrUpdate(ApplicationUser user, IList<ReflectiveQuestionViewModel> reflectiveQuestions, bool isDraft);
        Task<ReflectiveQuestionExam> GetExamByUserAsync(ApplicationUser user);
        Task<IList<ReflectiveQuestionViewModel>> GetReflectiveQuestions();
    }

    public class ReflectiveQuestionsService : IReflectiveQuestionsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICertificateService _certificateService;
        private readonly AppSettings _appSettings;

        public ReflectiveQuestionsService(
            ApplicationDbContext context, 
            ICertificateService certificateService,
            IOptions<AppSettings> settings)
        {
            _context = context;
            _certificateService = certificateService;
            _appSettings = settings.Value;
        }

        public async Task<IList<ReflectiveQuestionViewModel>> GetReflectiveQuestions()
        {
            return await _context.ReflectiveQuestions.Select(c => new ReflectiveQuestionViewModel
            {
                Id = c.Id,
                Description = c.Description
            }).ToListAsync();
        }

        public async Task<ReflectiveQuestionExam> GetExamByUserAsync(ApplicationUser user)
        {
            var exam = await _context.ReflectiveQuestionExam
                .Where(m => m.UserId == user.Id)
                .Include(m => m.Answers)
                .FirstOrDefaultAsync();

            return exam;
        }

        public async Task AddOrUpdate(ApplicationUser user, IList<ReflectiveQuestionViewModel> reflectiveQuestions, bool isDraft)
        {
            // Check to see if all the questions have been answered.
            bool isComplete = isDraft ? true : IsQuestionaireComplete(reflectiveQuestions);

            if(isComplete)
            {
                ReflectiveQuestionExam exam = await _context.ReflectiveQuestionExam
                  .Include(m => m.Answers)
                  .Where(m => m.UserId == user.Id)
                  .FirstOrDefaultAsync();

                // if we have an exam, but it was saved by the user, we need to complete the exam if all questions are answered.
                if (exam != null)
                {
                    for (int i = 0; i < exam.Answers.Count; i++)
                    {
                        var answer = exam.Answers.ElementAt(i);
                        answer.Answer = reflectiveQuestions[i].Answer;
                    }
                    exam.IsDraft = isDraft;
                    exam.LastUpdated = DateTime.UtcNow;

                    _context.ReflectiveQuestionExam.Update(exam);
                }
                else if (exam == null)
                {
                    exam = new ReflectiveQuestionExam
                    {
                        UserId = user.Id,
                        Answers = new List<ReflectiveQuestionUserAnswer>(),
                        IsDraft = isDraft,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow,
                    };

                    foreach (var question in reflectiveQuestions)
                    {
                        exam.Answers.Add(new ReflectiveQuestionUserAnswer
                        {
                            Answer = question.Answer,
                            ReflectiveQuestionId = question.Id
                        });
                    }
                    _context.ReflectiveQuestionExam.Add(exam);
                }

                await _context.SaveChangesAsync();

                if(!isDraft && user.SubscriptionEmail)
                {
                    // check the knowledge test has been passed.
                    var result = await _context.KnowledgeTests.Where(d => d.UserId == user.Id && d.Score >= _appSettings.RequiredKnowledgeTestScore).FirstOrDefaultAsync();

                    if (result != null)
                    {
                        await _certificateService.SendCertificateEmail(user);
                    }
                }
            }
        }

        protected bool IsQuestionaireComplete(IList<ReflectiveQuestionViewModel> reflectiveQuestions)
        {
            bool complete = true;
            foreach (var question in reflectiveQuestions)
            {
                if (String.IsNullOrEmpty(question.Answer))
                {
                    complete = false;
                    break;
                }
            }
            return complete;
        }
    }
}
