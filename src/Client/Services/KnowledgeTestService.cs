using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImmunizNation.Client.Data;
using ImmunizNation.Client.Models;
using ImmunizNation.Client.ViewModels.KnowledgeTest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImmunizNation.Client.Services
{
    public interface IKnowledgeTestService
    {
        Task<IList<KnowledgetTestQuestionViewModel>> GetKnowledgetTestQuestions();
        Task<IList<KnowledgeTest>> GetExamsByUser(ApplicationUser user);
        Task AddKnowledgeTest(ApplicationUser user, IList<KnowledgetTestQuestionViewModel> questions);
        Task<KnowledgeTest> GetPassingKnowledgeTest(ApplicationUser user);
    }

    public class KnowledgeTestService : IKnowledgeTestService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KnowledgeTestService> _logger;
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _configuration;
        private readonly ICertificateService _certificateService;

        public KnowledgeTestService(
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<KnowledgeTestService> logger,
            IOptions<AppSettings> appSettings,
            ICertificateService certificateService)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _logger = logger;
            _configuration = configuration;
            _certificateService = certificateService;
        }

        public async Task<IList<KnowledgetTestQuestionViewModel>> GetKnowledgetTestQuestions()
        {
            var questions = await _context.KnowledgeTestsQuestions
                .OrderBy(m=>m.Order)
                .Select(c => new KnowledgetTestQuestionViewModel
                {
                    Id = c.Id,
                    Question = c.Question,
                    LessonDescription = c.LessonDescription,
                    Answer = c.Answer,
                    Answers = _context.KnowledgeTestAnswers
                        .Where(a => a.KnowledgeTestQuestionId == c.Id)
                        .OrderBy(a => a.Order)
                        .Select(m => new KnowledgeTestAnswerViewModel
                        {
                            Id = m.Id,
                            Description = m.Description,
                            Value = m.Value
                        }).ToList()
                }).ToListAsync();

            return questions;
        }

        public async Task<IList<KnowledgeTest>> GetExamsByUser(ApplicationUser user)
        {
            return await _context.KnowledgeTests
                .Where(m => m.UserId == user.Id)
                .Include(m => m.Answers)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new knowledge test unless the exam already exists
        /// </summary>
        /// <param name="user">Which user submitted the exam</param>
        /// <param name="questions">Questions including User Answers</param>
        /// <returns>Added or updated knowledgeTest</returns>
        public async Task AddKnowledgeTest(ApplicationUser user, IList<KnowledgetTestQuestionViewModel> questions)
        {
            if (user == null || questions.Count == 0)
            {
                _logger.LogError("{Application User} was null or has not completed the Leaner Assessment Test", nameof(ApplicationUser));
                return;
            }

            try
            {
                var exam = new KnowledgeTest
                {
                    User = user,
                    Answers = questions.Select(m => new KnowledgeTestUserAnswer
                    {
                        KnowledgeTestQuestionId = m.Id,
                        Value = m.UserAnswer
                    }).ToList(),
                    CreatedDate = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                };

                exam.Score = CalculateScore(exam, questions);

                var scoreString = exam.Score * 100 + "%";
                _logger.LogInformation("Adding knowledge test for user {Email} with a score of: {Score}", user.Email, scoreString);

                _context.KnowledgeTests.Add(exam);
                await _context.SaveChangesAsync();

                if(exam.Score > _appSettings.RequiredKnowledgeTestScore && user.SubscriptionEmail)
                {
                    if(user.AccountType == AccountTypes.Pharmacist)
                    {
                        await _certificateService.SendCertificateEmail(user);
                    }
                    else if(user.AccountType == AccountTypes.GeneralPractitioner)
                    {
                        // see if the reflective questions are complete.
                        var reflectiveQuestions = await _context.ReflectiveQuestionExam.Where(d => !d.IsDraft && d.UserId == user.Id).Include(d => d.Answers).FirstOrDefaultAsync();
                   
                        if(reflectiveQuestions != null)
                        {
                            var completed = true;
                            foreach (var answer in reflectiveQuestions.Answers)
                            {
                                if (String.IsNullOrWhiteSpace(answer.Answer))
                                {
                                    completed = false;
                                    break;
                                }
                            }

                            if (completed)
                            {
                                await _certificateService.SendCertificateEmail(user);
                            }

                        }
                    }
                   
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to create Knowledge Test for user: {UserId}", user.Id);
            }
        }

        private double CalculateScore(KnowledgeTest exam, IList<KnowledgetTestQuestionViewModel> questions)
        {
            double correct = 0;
            double numOfQuestions = questions.Count;

            foreach (var answer in exam.Answers)
            {
                foreach (var question in questions)
                {
                    if (answer.KnowledgeTestQuestionId == question.Id && answer.Value == question.Answer)
                    {
                        correct++;
                    }
                }
            }

            return correct == 0 ? correct : (correct / numOfQuestions);
        }

        public async Task<KnowledgeTest> GetPassingKnowledgeTest(ApplicationUser user)
        {
            _logger.LogInformation("Retrieving any learner assessment tests with a passing grade for user {Email}.", user.Email);

            return await _context.KnowledgeTests
                .Where(k => k.UserId == user.Id && k.Score >= _appSettings.RequiredKnowledgeTestScore)
                .OrderBy(k => k.Id)
                .FirstOrDefaultAsync();
        }
    }
}
