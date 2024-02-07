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


//public static void SeedReflectiveQuestions(ModelBuilder builder)
//{
//    builder.Entity<ReflectiveQuestion>().HasData(
//        new ReflectiveQuestion
//        {
//            Id = 1,
//            Description = "Integer cursus aliquet malesuada."
//        },
//        new ReflectiveQuestion
//        {
//            Id = 2,
//            Description = "Fusce sollicitudin, urna vitae tincidunt maximus, turpis lorem pulvinar justo, et facilisis ex odio in erat."
//        },
//        new ReflectiveQuestion
//        {
//            Id = 3,
//            Description = "Aliquam id nisi ut arcu iaculis sodales."
//        }
//    );
//}

//public static void SeedKnowledgeTest(ModelBuilder builder)
//{
//    //builder.Entity<KnowledgeTest>().HasData(
//    //        new KnowledgeTest
//    //        {
//    //            Id = 1
//    //        }
//    //    );

//    builder.Entity<KnowledgeTestQuestion>().HasData(
//            new KnowledgeTestQuestion
//            {
//                Id = 1,
//                Question = "Lorem ipsum dolor sit amet.",
//                Answer = "1",
//            },
//            new KnowledgeTestQuestion
//            {
//                Id = 2,
//                Question = "Nullam turpis arcu, efficitur et turpis at, tempus egestas dolor.",
//                Answer = "2",
//            },
//            new KnowledgeTestQuestion
//            {
//                Id = 3,
//                Question = "Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.",
//                Answer = "3",
//            },
//            new KnowledgeTestQuestion
//            {
//                Id = 4,
//                Question = "Lorem ipsum dolor sit amet.",
//                Answer = "3",
//            },
//            new KnowledgeTestQuestion
//            {
//                Id = 5,
//                Question = "Nullam turpis arcu, efficitur et turpis at, tempus egestas dolor.",
//                Answer = "3",
//            }
//        );

//    builder.Entity<KnowledgeTestAnswer>().HasData(
//            // Question 1
//            new KnowledgeTestAnswer
//            {
//                Id = 1,
//                Description = "Integer nec urna id velit aliquam",
//                Value = "1",
//                KnowledgeTestQuestionId = 1
//            },
//            new KnowledgeTestAnswer
//            {
//                Id = 2,
//                Description = "Mauris et urna eu est interdum pellentesque.",
//                Value = "2",
//                KnowledgeTestQuestionId = 1
//            },
//            new KnowledgeTestAnswer
//            {
//                Id = 3,
//                Description = "Mauris et urna eu est interdum pellentesque.",
//                Value = "3",
//                KnowledgeTestQuestionId = 1
//            },
//            // Question 2
//            new KnowledgeTestAnswer
//            {
//                Id = 4,
//                Description = "Maecenas mollis lobortis porta.",
//                Value = "1",
//                KnowledgeTestQuestionId = 2
//            },
//            new KnowledgeTestAnswer
//            {
//                Id = 5,
//                Description = "Duis vulputate est eu metus sagittis aliquet.",
//                Value = "2",
//                KnowledgeTestQuestionId = 2
//            },
//            new KnowledgeTestAnswer
//            {
//                Id = 6,
//                Description = "Suspendisse potenti.",
//                Value = "3",
//                KnowledgeTestQuestionId = 2
//            },
//            // Question 3
//            new KnowledgeTestAnswer
//            {
//                Id = 7,
//                Description = "lacus nulla elementum magna, vitae ultrices sapien lectus in nunc.",
//                Value = "1",
//                KnowledgeTestQuestionId = 3
//            },
//            new KnowledgeTestAnswer
//            {
//                Id = 8,
//                Description = "Duis tempus id ligula quis lobortis.",
//                Value = "2",
//                KnowledgeTestQuestionId = 3
//            },
//            new KnowledgeTestAnswer
//            {
//                Id = 9,
//                Description = "Vestibulum laoreet massa rutrum nibh laoreet laoreet.",
//                Value = "3",
//                KnowledgeTestQuestionId = 3
//            },
//            // Question 4
//            new KnowledgeTestAnswer
//            {
//                Id = 10,
//                Description = "lacus nulla elementum magna, vitae ultrices sapien lectus in nunc.",
//                Value = "1",
//                KnowledgeTestQuestionId = 4
//            },
//            new KnowledgeTestAnswer
//            {
//                Id = 11,
//                Description = "Duis tempus id ligula quis lobortis.",
//                Value = "2",
//                KnowledgeTestQuestionId = 4
//            },
//            new KnowledgeTestAnswer
//            {
//                Id = 12,
//                Description = "Vestibulum laoreet massa rutrum nibh laoreet laoreet.",
//                Value = "3",
//                KnowledgeTestQuestionId = 4
//            },
//            // Question 5
//            new KnowledgeTestAnswer
//            {
//                Id = 13,
//                Description = "Maecenas mollis lobortis porta.",
//                Value = "1",
//                KnowledgeTestQuestionId = 5
//            },
//            new KnowledgeTestAnswer
//            {
//                Id = 14,
//                Description = "Duis vulputate est eu metus sagittis aliquet.",
//                Value = "2",
//                KnowledgeTestQuestionId = 5
//            },
//            new KnowledgeTestAnswer
//            {
//                Id = 15,
//                Description = "Suspendisse potenti.",
//                Value = "3",
//                KnowledgeTestQuestionId = 5
//            }
//        );
//}

//public static void SeedEvaluationSurvey(ModelBuilder builder)
//{
//    builder.Entity<EvaluationSurveyQuestionGroup>().HasData(
//        new EvaluationSurveyQuestionGroup { Id = 1, Order = 1, Title = "", Description = "" },
//        new EvaluationSurveyQuestionGroup { Id = 2, Order = 2, Title = "Educational Objectives", Description = "Please rate whether this program met the following learning objective." },
//        new EvaluationSurveyQuestionGroup { Id = 3, Order = 3, Title = "Program Content and Delivery", Description = "Please rate your agreement with the following statement." }
//    );

//    builder.Entity<EvaluationSurveyOption>().HasData(
//        new EvaluationSurveyOption
//        {
//            Id = 1,
//            QuestionId = 2,
//            Description = "Raveena Gamble",
//            Value = "Raveena Gamble"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 2,
//            QuestionId = 2,
//            Description = "Jaiden Powers",
//            Value = "Jaiden Powers"
//        },
//        // question 3
//        new EvaluationSurveyOption
//        {
//            Id = 3,
//            QuestionId = 3,
//            Description = "Strongly Agree",
//            Value = "1"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 4,
//            QuestionId = 3,
//            Description = "Agree",
//            Value = "2"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 5,
//            QuestionId = 3,
//            Description = "Neutral",
//            Value = "3"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 6,
//            QuestionId = 3,
//            Description = "Disagree",
//            Value = "4"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 7,
//            QuestionId = 3,
//            Description = "Strongly Disagree",
//            Value = "5"
//        },
//        // question 4
//        new EvaluationSurveyOption
//        {
//            Id = 8,
//            QuestionId = 4,
//            Description = "Strongly Agree",
//            Value = "1"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 9,
//            QuestionId = 4,
//            Description = "Agree",
//            Value = "2"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 10,
//            QuestionId = 4,
//            Description = "Neutral",
//            Value = "3"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 11,
//            QuestionId = 4,
//            Description = "Disagree",
//            Value = "4"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 12,
//            QuestionId = 4,
//            Description = "Strongly Disagree",
//            Value = "5"
//        },
//        // question 5
//        new EvaluationSurveyOption
//        {
//            Id = 13,
//            QuestionId = 5,
//            Description = "Strongly Agree",
//            Value = "1"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 14,
//            QuestionId = 5,
//            Description = "Agree",
//            Value = "2"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 15,
//            QuestionId = 5,
//            Description = "Neutral",
//            Value = "3"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 16,
//            QuestionId = 5,
//            Description = "Disagree",
//            Value = "4"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 17,
//            QuestionId = 5,
//            Description = "Strongly Disagree",
//            Value = "5"
//        },
//        // question 6
//        new EvaluationSurveyOption
//        {
//            Id = 18,
//            QuestionId = 6,
//            Description = "Strongly Agree",
//            Value = "1"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 19,
//            QuestionId = 6,
//            Description = "Agree",
//            Value = "2"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 20,
//            QuestionId = 6,
//            Description = "Neutral",
//            Value = "3"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 21,
//            QuestionId = 6,
//            Description = "Disagree",
//            Value = "4"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 22,
//            QuestionId = 6,
//            Description = "Strongly Disagree",
//            Value = "5"
//        },

//        // question 7
//        new EvaluationSurveyOption
//        {
//            Id = 23,
//            QuestionId = 7,
//            Description = "Strongly Agree",
//            Value = "1"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 24,
//            QuestionId = 7,
//            Description = "Agree",
//            Value = "2"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 25,
//            QuestionId = 7,
//            Description = "Neutral",
//            Value = "3"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 26,
//            QuestionId = 7,
//            Description = "Disagree",
//            Value = "4"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 27,
//            QuestionId = 7,
//            Description = "Strongly Disagree",
//            Value = "5"
//        },
//        // question 8
//        new EvaluationSurveyOption
//        {
//            Id = 28,
//            QuestionId = 8,
//            Description = "Strongly Agree",
//            Value = "1"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 29,
//            QuestionId = 8,
//            Description = "Agree",
//            Value = "2"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 30,
//            QuestionId = 8,
//            Description = "Neutral",
//            Value = "3"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 31,
//            QuestionId = 8,
//            Description = "Disagree",
//            Value = "4"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 32,
//            QuestionId = 8,
//            Description = "Strongly Disagree",
//            Value = "5"
//        },
//        // question 9
//        new EvaluationSurveyOption
//        {
//            Id = 33,
//            QuestionId = 9,
//            Description = "Strongly Agree",
//            Value = "1"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 34,
//            QuestionId = 9,
//            Description = "Agree",
//            Value = "2"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 35,
//            QuestionId = 9,
//            Description = "Neutral",
//            Value = "3"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 36,
//            QuestionId = 9,
//            Description = "Disagree",
//            Value = "4"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 37,
//            QuestionId = 9,
//            Description = "Strongly Disagree",
//            Value = "5"
//        },
//        // question 10
//        new EvaluationSurveyOption
//        {
//            Id = 38,
//            QuestionId = 10,
//            Description = "Strongly Agree",
//            Value = "1"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 39,
//            QuestionId = 10,
//            Description = "Agree",
//            Value = "2"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 40,
//            QuestionId = 10,
//            Description = "Neutral",
//            Value = "3"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 41,
//            QuestionId = 10,
//            Description = "Disagree",
//            Value = "4"
//        },
//        new EvaluationSurveyOption
//        {
//            Id = 42,
//            QuestionId = 10,
//            Description = "Strongly Disagree",
//            Value = "5"
//        }
//    );

//    builder.Entity<EvaluationSurveyQuestion>().HasData(
//        new EvaluationSurveyQuestion
//        {
//            Id = 1,
//            GroupId = 1,
//            Description = "Please enter the date of the event you attended:",
//            InputType = InputType.DateTime,
//        },
//        new EvaluationSurveyQuestion
//        {
//            Id = 2,
//            GroupId = 1,
//            Description = "Please enter the name of the event speaker:",
//            InputType = InputType.Select,
//        },
//        new EvaluationSurveyQuestion
//        {
//            Id = 3,
//            GroupId = 2,
//            Description = "Describe the burden of disease and risk factors associated with herpes zoster (HZ) in adults and the need for improved adult ImmunizNation rates in a post-pandemic environment.",
//            InputType = InputType.Radio
//        },
//        new EvaluationSurveyQuestion
//        {
//            Id = 4,
//            GroupId = 2,
//            Description = "Discuss new clinical evidence available for HZ vaccines in Canada and current national, provincial, and professional association guidelines related to HZ ImmunizNation.",
//            InputType = InputType.Radio
//        },
//        new EvaluationSurveyQuestion
//        {
//            Id = 5,
//            GroupId = 2,
//            Description = "Evaluate best practices, strategies, and tools for applying recommendations into practice and optimizing adult ImmunizNation across HCP/AHCP networks.",
//            InputType = InputType.Radio
//        },
//        // --------------------
//        new EvaluationSurveyQuestion
//        {
//            Id = 6,
//            GroupId = 3,
//            Description = "The program content was relevant to my practice.",
//            InputType = InputType.Radio
//        },
//        new EvaluationSurveyQuestion
//        {
//            Id = 7,
//            GroupId = 3,
//            Description = "The program content enhanced my knowledge.",
//            InputType = InputType.Radio
//        },
//        new EvaluationSurveyQuestion
//        {
//            Id = 8,
//            GroupId = 3,
//            Description = "The program met my expectations.",
//            InputType = InputType.Radio
//        },
//        new EvaluationSurveyQuestion
//        {
//            Id = 9,
//            GroupId = 3,
//            Description = "The program was well organized.",
//            InputType = InputType.Radio
//        },
//        new EvaluationSurveyQuestion
//        {
//            Id = 10,
//            GroupId = 3,
//            Description = "The program was well organized.",
//            InputType = InputType.Radio
//        },
//        new EvaluationSurveyQuestion
//        {
//            Id = 11,
//            GroupId = 3,
//            Description = "What information did you learn from this program that was new and valuable to your practice.",
//            InputType = InputType.TextArea
//        }
//    );
//}