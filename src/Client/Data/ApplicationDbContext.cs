using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ImmunizNation.Client.Models;
using ImmunizNation.Client.Models.EvaluationSurvey;
using Microsoft.AspNetCore.Identity;

namespace ImmunizNation.Client.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        #region KnowledgeTest
        /// <summary>
        /// Linking table for knowledge tests. 
        /// </summary>
        public DbSet<KnowledgeTest> KnowledgeTests { get; set; }

        /// <summary>
        /// Questions for a knowledge test
        /// </summary>
        public DbSet<KnowledgeTestQuestion> KnowledgeTestsQuestions { get; set; }

        /// <summary>
        /// Knowledget test question answers.
        /// </summary>
        public DbSet<KnowledgeTestAnswer> KnowledgeTestAnswers { get; set; }

        /// <summary>
        /// User submitted Knowledget test answers
        /// </summary>
        public DbSet<KnowledgeTestUserAnswer> KnowledgeTestUserAnswers { get; set; }
        #endregion

        #region ReflectiveQuestions
        public DbSet<ReflectiveQuestion> ReflectiveQuestions { get; set; }
        public DbSet<ReflectiveQuestionExam> ReflectiveQuestionExam { get; set; }
        public DbSet<ReflectiveQuestionUserAnswer> ReflectiveQuestionUserAnswers { get; set; }
        #endregion

        #region EvaluationSurvey
        public DbSet<EvaluationSurveyQuestion> EvaluationSurveyQuestions { get; set; }
        public DbSet<EvaluationSurveyQuestionGroup> EvaluationSurveyGroups { get; set; }
        public DbSet<EvaluationSurveyOption> EvaluationSurveyOptions { get; set; }
        public DbSet<EvaluationSurvey> EvaluationSurveys { get; set; }
        public DbSet<EvaluationSurveyUserAnswer> EvaluationSurveyUserAnswers { get; set; }
        #endregion

        public DbSet<Resource> Resources { get; set; }
        public DbSet<ResourceDownload> ResourceDownloads { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Core Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Core Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // ...

            // DO NOT USE THIS METHOD FOR SEEINDING THIS DATABASE!
            // see ApplicationDbContextSeed for database seeding.
        }
    }

}
