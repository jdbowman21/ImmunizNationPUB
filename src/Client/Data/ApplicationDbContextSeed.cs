using ImmunizNation.Client.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ImmunizNation.Client.Models.EvaluationSurvey;
using System.Security.Claims;

namespace ImmunizNation.Client.Data
{
    public interface IMigrationSeeder
    {
        /// <summary>
        /// Seeds the database with data required for the application.
        /// @note - this method was chosen over using the ApplicationDbContext.OnModelCreating seeding option because it allowed for a more organized method of seeding,
        /// and included automatic foreign key linking and auto database generated Id's.
        /// </summary>
        /// <typeparam name="TContext">Database Context</typeparam>
        /// <param name="context">Reference of the database context</param>
        /// <param name="env">Hosting Environment</param>
        /// <param name="logger">For warning and error logging during seeding.</param>
        /// <param name="settings">Application settings.</param>
        /// <returns>An awaiter for the async task for seeding a database context.</returns>
        Task SeedAsync<TContext>(TContext context, IHostEnvironment env,
            ILogger<IMigrationSeeder> logger, IOptions<AppSettings> settings) where TContext : DbContext;
    }

    public class ApplicationDbContextSeed : IMigrationSeeder
    {
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();
        private readonly IServiceProvider _services;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _adminRoleName = "Admin";
        private readonly string[] defaultRadioOptions = new string[] { "Strongly Agree", "Agree", "Neutral", "Disagree", "Strongly Disagree" };

        /// <summary>
        /// Creates a new instance of the ApplicationDbContextSeed
        /// </summary>
        /// <param name="serviceProvider">service provider for the application</param>
        public ApplicationDbContextSeed(IServiceProvider serviceProvider)
        {
            _services = serviceProvider;
            _userManager = _services.GetService<UserManager<ApplicationUser>>();
        }

        
        public async Task SeedAsync<TContext>(TContext context, IHostEnvironment env,
            ILogger<IMigrationSeeder> logger, IOptions<AppSettings> settings) where TContext : DbContext
        {
            ApplicationDbContext dbContext = context as ApplicationDbContext;

            try
            {
                // First add the users and the roles for an administrator account, than seed database with the examinations and survey. 

                var contentRootPath = env.ContentRootPath;
                var webRoot = env.ContentRootPath;

                var users = GetAdminUsers();

                // add the default users.
                var adminRole = new IdentityRole { Name = _adminRoleName, NormalizedName = _adminRoleName };
                var claim = new Claim(adminRole.Id, adminRole.Id.ToString());

                if (!dbContext.Roles.Any(r=>r.Name == _adminRoleName))
                {
                    var roleStore = new RoleStore<IdentityRole>(dbContext);
                    await roleStore.CreateAsync(adminRole);
                    await roleStore.AddClaimAsync(adminRole, claim);
                }

                foreach (var user in users)
                {
                    if(!dbContext.Users.Any(u=>u.NormalizedEmail == user.NormalizedEmail))
                    {
                        var userStore = new UserStore<ApplicationUser>(dbContext);
                        var result = await userStore.CreateAsync(user);

                        if (result.Succeeded && dbContext.Roles.Any(r => r.Name == _adminRoleName))
                        {
                            await AssignRolesAsync(_services, user.Email, new string[] { _adminRoleName });                            
                            await _userManager.AddClaimAsync(user, claim);
                        }
                    }
                }

                // Seed the knowledge Test
                var knowledgetTest = GetKnowlegeTest();

                foreach (var test in knowledgetTest)
                {
                    if(!dbContext.KnowledgeTestsQuestions.Any(k=>k.ConcurrencyStamp == test.ConcurrencyStamp))
                    {
                        dbContext.KnowledgeTestsQuestions.Add(test);
                    }
                }

                // Seed the ReflectiveQuestions.
                var reflectiveQuestions = GetReflectiveQuestions();

                foreach (var question in reflectiveQuestions)
                {
                    if (!dbContext.ReflectiveQuestions.Any(q => q.Description == question.Description))
                    {
                        dbContext.ReflectiveQuestions.Add(question);
                    }
                }

                // Seed the Evaluation Survey.
                var surveyGroups = GetEvaluationSurveyGroups();

                foreach (var group in surveyGroups)
                {
                    if (!dbContext.EvaluationSurveyGroups.Any(g => g.Order == group.Order))
                    {
                        dbContext.EvaluationSurveyGroups.Add(group);
                    }
                }

                var resources = GetResourceData();

                foreach(var resource in resources)
                {
                    if(!dbContext.Resources.Any(r=> r.Id == resource.Id))
                    {
                        dbContext.Resources.Add(resource);
                    }
                }

                await dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "EXCEPTION ERROR: {Message}", ex.Message);
            }

        }

        /// <summary>
        /// Returns a list of pre-defined administrators for the application 
        /// </summary>
        /// <returns>Collection of pre-defined adminstrators</returns>
        private IEnumerable<ApplicationUser> GetAdminUsers()
        {
            var email = "admin@ImmunizNation.ca";

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString("D"),
                FirstName = "Justin",
                LastName = "Bowman",
                UserName = "Justin Bowman",
                City = "Guelph",
                Province = Provinces.Ontario,
                AccountType = AccountTypes.None,
                Email = email,
                NormalizedEmail = email.ToUpper(),
                NormalizedUserName = email.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                DateOfSession = DateTime.MinValue,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, "Pass@Word1");

            return new List<ApplicationUser>
            {
                user
            };
        }

        /// <summary>
        /// Assigns a user their roles.
        /// </summary>
        /// <param name="services">Service provider</param>
        /// <param name="email">Users email (unique)</param>
        /// <param name="roles">Collection of roles to add.</param>
        /// <returns>Result from adding the roles.</returns>
        private async Task<IdentityResult> AssignRolesAsync(IServiceProvider services, string email, string[] roles)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(email);
            var result = await _userManager.AddToRolesAsync(user, roles);

            return result;
        }

        /// <summary>
        /// Database model for Reflective questions.
        /// </summary>
        /// <returns>Returns an array of questions.</returns>
        private ICollection<ReflectiveQuestion> GetReflectiveQuestions()
        {
            var reflectiveQuestions = new List<ReflectiveQuestion>
            {
                new ReflectiveQuestion
                {
                    Description = "What information did you learn from this program that was new and valuable to your practice? ",
                },
                new ReflectiveQuestion
                {
                    Description = "What are the top two barriers to HZ immunization in your practice? What learnings from this program can you apply to help overcome these challenges?"
                },
                new ReflectiveQuestion
                {
                    Description = "Describe two ways in which you will change your approach to initiating HZ immunization discussions with your patients as a result of attending this program? "
                },
                new ReflectiveQuestion
                {
                    Description = "What are the most valuable strategies learned from this program to facilitate increased patient uptake of HZ vaccination?"
                },
                new ReflectiveQuestion
                {
                    Description = "After attending this program, how will you involve pharmacists in your HZ immunization plans?"
                }
            };

            return reflectiveQuestions;
        }

        /// <summary>
        /// Database model for knowledge test questions and answer options.
        /// </summary>
        /// <returns>Returns a collection of question and answer options.</returns>
        private ICollection<KnowledgeTestQuestion> GetKnowlegeTest()
        {
            var knowledgeTest = new List<KnowledgeTestQuestion>
            {
                // question 1
                new KnowledgeTestQuestion
                {
                    Answer = "3",
                    Order= 1,
                    Question = "Which of the following statements is <u>false</u> regarding the risk and burden of HZ?",
                    LessonDescription = "Slide 12 outlines each 10-year increase in age is associated with a mean 1.2-3.1% increase in relative odds of developing PHN. Slide 15 shows the rates of hospitalization for HZ also increase with age. ",
                    ConcurrencyStamp = "8601bfdb-b620-4fb4-8aed-b022c8a39ec0",
                    Answers = new List<KnowledgeTestAnswer>
                    {
                        new KnowledgeTestAnswer
                        {
                            Order = 1,
                            Description = "HZ infection can lead to debilitating complications such as post-herpetic neuralgia or HZ ophthalmicus.",
                            Value = "1",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 2,
                            Description = "HZ is more prevalent in individuals ≥50 years of age due to declining VZV-specific immunity and immunosuppression.",
                            Value = "2",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 3,
                            Description = "Advanced age does not increase the risk of developing HZ-related complications.",
                            Value = "3",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 4,
                            Description = "Female sex, Caucasian ethnicity, family history of HZ and cancer are some of the factors which can contribute towards increased risk of HZ infection.",
                            Value = "4",
                        }
                    }
                },
                // question 2
                new KnowledgeTestQuestion
                {
                    Answer = "3",
                    Order= 2,
                    Question = "What is the lifetime risk of HZ in Canada for patients ≥50 years of age and ≥80 years of age, respectively?",
                    LessonDescription = "This information can be found on slide 12.",
                    ConcurrencyStamp = "8ce41a11-c57d-4d49-a28f-dea8da254822",
                    Answers = new List<KnowledgeTestAnswer>
                    {
                        new KnowledgeTestAnswer
                        {
                            Order = 1,
                            Description = "17% and 28%",
                            Value = "1",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 2,
                            Description = "20% and 40%",
                            Value = "2",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 3,
                            Description = "28% and 50%",
                            Value = "3",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 4,
                            Description = "42% and 61%",
                            Value = "4",
                        }
                    }
                },
                // question 3
                new KnowledgeTestQuestion
                {
                    Answer = "1",
                    Order=3,
                    Question = "True of false: Immunocompromised patients have been shown to be >5 times more likely to develop HZ complications and 2x more likely to be hospitalized due to HZ.",
                    LessonDescription = "Slide 15 outlines immunocompromised patients are 5.4 times more likely to develop HZ complication and had double the risk of hospitalization.",
                    ConcurrencyStamp = "12d8cffc-f78a-4f24-897b-d9f8464cf53d",
                    Answers = new List<KnowledgeTestAnswer>
                    {
                        new KnowledgeTestAnswer
                        {
                            Order = 1,
                            Description = "True",
                            Value = "1",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 2,
                            Description = "False",
                            Value = "2",
                        }
                    }
                },
                // question 4
                new KnowledgeTestQuestion
                {
                    Answer = "2",
                    Order = 4,
                    Question = "During the 2020-2021 Vaccination Coverage Survey conducted by the Public Health Agency of Canada, what percentage of Canadian adults self-reported being up to date on HZ immunization?",
                    LessonDescription = "Slide 17 outlines HZ vaccination had the lowest rates (27%) compared to adult immunization against tetanus and pneumococci.",
                    
                    ConcurrencyStamp = "8fb9726d-4051-42c3-9cec-c4932b4edfcf",
                    Answers = new List<KnowledgeTestAnswer>
                    {
                        new KnowledgeTestAnswer
                        {
                            Order = 1,
                            Description = "19%",
                            Value = "1",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 2,
                            Description = "27%",
                            Value = "2",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 3,
                            Description = "34%",
                            Value = "3",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 3,
                            Description = "43%",
                            Value = "4",
                        },
                    }
                },
                // question 5
                new KnowledgeTestQuestion
                {
                    Answer = "2",
                    Order  = 5,
                    Question = "True or false: Market research from 2021 revealed the top reason for patients not receiving an HZ vaccine was because they did not understand they were at risk.",
                    LessonDescription = "Slide 19 notes the top reason for not receiving an HZ vaccine was because the individual’s healthcare professional had not recommended vaccination.",
                    ConcurrencyStamp = "504e4d35-48fa-4a9b-b2ee-0f0da72e181d",
                    Answers = new List<KnowledgeTestAnswer>
                    {
                        new KnowledgeTestAnswer
                        {
                            Order = 1,
                            Description = "True",
                            Value = "1",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 2,
                            Description = "False",
                            Value = "2",
                        }
                    }
                },
                // question 6
                new KnowledgeTestQuestion
                {
                    Answer = "5",
                    Order = 6,
                    Question = "Which of the following statements is <u>false</u> regarding the clinical evidence supported RZV and LZV?",
                    LessonDescription = "Slide 25 outlines the indications of RZV and LZV. Slides 26 and 29 discuss the RZV clinical trial data and real-world evidence for immunocompromised individuals, respectively. Slide 30 reviews the clinical trial data for LZV. Slide 31 outlines the number needed to treat, showing greater values for LZV compared to RZV.",
                    ConcurrencyStamp = "4309f996-435e-499d-b6ef-1384cf66a4f4",
                    Answers = new List<KnowledgeTestAnswer>
                    {
                        new KnowledgeTestAnswer
                        {
                            Order = 1,
                            Description = "Both RZV and LZV are indicated for individuals ≥50 years of age, while only RZV is indicated for adults ≥18 years of age who are or will be at increased risk of HZ due to immunodeficiency or immunosuppression caused by known disease or therapy.",
                            Value = "1",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 2,
                            Description = "The pivotal trials for RZV demonstrated a vaccine efficacy of 91.3-97.2%, with interim long-term assessment showing a sustained benefit to year 8.",
                            Value = "2",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 3,
                            Description = "Real world evidence for RZV demonstrated vaccine effectiveness of 86.8% in patients 50-79 years of age and 64.1% in immunocompromised individuals.",
                            Value = "3",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 4,
                            Description = "LZV trials demonstrated a 51.3% reduction in the incidence of HZ, with declining but statistically significant protection up to year 8.",
                            Value = "4",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 5,
                            Description = "The number needed to treat for RZV and LZV is the same.",
                            Value = "5",
                        }
                    }
                },
                // question 7
                new KnowledgeTestQuestion
                {
                    Answer = "1",
                    Order = 7,
                    Question = "True or false: NACI recommends COVID-19 vaccines may be given concomitantly with other vaccines, including HZ vaccines.",
                    LessonDescription = "Slide 37 reviews the NACI guidance for coadministration of vaccines with COVID-19 vaccines.",
                    
                    ConcurrencyStamp = "cedff6e9-2dd6-4866-a469-eb211df8352b",
                    Answers = new List<KnowledgeTestAnswer>
                    {
                        new KnowledgeTestAnswer
                        {
                            Order = 1,
                            Description = "True",
                            Value = "1",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 2,
                            Description = "False",
                            Value = "2",
                        }
                    }
                },
                // question 8
                new KnowledgeTestQuestion
                {
                    Answer = "6",
                    Order = 8,
                    Question = "Which of the following are best practices which can be employed to initiate HZ conversations in eligible patients?",
                    LessonDescription = "These strategies are discussed throughout the case studies. Slide 42 discusses incorporation into the >50 year old checklist, leveraging influenza vaccination, and using family history of HZ, recent prescription of antivirals/antibiotics and diagnosis of an immunocompromising condition as prompts. Leveraging diagnosis or treatment initiation for an immunocompromising condition is further discussed on slide 55. The role of pharmacists is outlined on slides 43 and 51.",
                    ConcurrencyStamp = "569e1c81-9e65-4798-a93f-4b1cd19d5a06",
                    Answers = new List<KnowledgeTestAnswer>
                    {
                        new KnowledgeTestAnswer
                        {
                            Order = 1,
                            Description = "Incorporate HZ immunization discussion into the standard ≥ 50-year-old checklist.",
                            Value = "1",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 2,
                            Description = "Leverage routine yearly influenza vaccination discussions or co-administration with other age-appropriate vaccines.",
                            Value = "2",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 3,
                            Description = "If a patient is diagnosed with an immunocompromising condition or prescribed an immunosuppressing treatment, leverage these events to ensure vaccination is up to date.",
                            Value = "3",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 4,
                            Description = "Establish partnerships between family practitioners and pharmacists to ensure vaccination status is being discussed at the time of new prescription dispensing.",
                            Value = "4",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 5,
                            Description = "Be aware of key conversation prompts, such as patients mentioning a family history of HZ or a recent prescription of antibiotics/antivirals for pneumonia or HZ.",
                            Value = "5",
                        },
                        new KnowledgeTestAnswer
                        {
                            Order = 6,
                            Description = "All of the above.",
                            Value = "6",
                        }
                    }
                }
            };

            return knowledgeTest;
        }

        /// <summary>
        /// Database model for evaluation survey groups including questions and answer options.
        /// Questions can specify their own Input type, but at the moment only one answer per question is allowed.
        /// </summary>
        /// <returns>Returns grouping of questions and answer options.</returns>
        private ICollection<EvaluationSurveyQuestionGroup> GetEvaluationSurveyGroups()
        {
            // @note - EF is not maintaining the order of the list, use order to define the order of display.
            // I am assuming it's a more complex data model to seed which internally EF is giving different ordering.
            var groups = new List<EvaluationSurveyQuestionGroup>
            {
                new EvaluationSurveyQuestionGroup
                {
                    Order = 1,
                    Title = "",
                    Description = "",
                    Questions = new List<EvaluationSurveyQuestion>
                    {
                        new EvaluationSurveyQuestion
                        {
                            Order = 1,
                            Description = "Please enter the date of the event you attended:",
                            InputType = InputType.DateTime
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 2,
                            Description = "Please enter the name of the event speaker:",
                            InputType = InputType.Text,
                        },
                    }
                },
                new EvaluationSurveyQuestionGroup
                {
                    Order = 2,
                    Title = "Educational Objectives",
                    Description = "Please rate whether this program met the following learning objectives.",
                    Questions = new List<EvaluationSurveyQuestion>
                    {
                        new EvaluationSurveyQuestion
                        {
                            Order = 1,
                            Description = "Describe the burden of disease and risk factors associated with herpes zoster (HZ) in adults and the need for improved adult ImmunizNation rates in a post-pandemic environment:",
                            InputType = InputType.Radio,
                            Options = GetDefaultRadioOptions()
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 2,
                            Description = "Discuss new clinical evidence available for HZ vaccines in Canada and current national, provincial, and professional association guidelines related to HZ ImmunizNation:",
                            InputType = InputType.Radio,
                            Options = GetDefaultRadioOptions()
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 3,
                            Description = "Evaluate best practices, strategies, and tools for applying recommendations into practice and optimizing adult ImmunizNation across HCP/AHCP networks:",
                            InputType = InputType.Radio,
                            Options = GetDefaultRadioOptions()
                        },
                    }
                },
                new EvaluationSurveyQuestionGroup
                {
                    Order = 3,
                    Title = "Program Content and Delivery",
                    Description = "Please rate your agreement with the following statements.",
                    Questions = new List<EvaluationSurveyQuestion>
                    {
                        new EvaluationSurveyQuestion
                        {
                            Order = 1,
                            Description = "The program content was relevant to my practice:",
                            InputType = InputType.Radio,
                            Options = GetDefaultRadioOptions()
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 2,
                            Description = "The program content enhanced my knowledge:",
                            InputType = InputType.Radio,
                            Options = GetDefaultRadioOptions()
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 3,
                            Description = "The program met my expectations:",
                            InputType = InputType.Radio,
                            Options = GetDefaultRadioOptions()
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 4,
                            Description = "The program was well organized:",
                            InputType = InputType.Radio,
                            Options = GetDefaultRadioOptions()
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 5,
                            Description = "The speaker was effective in facilitating the program:",
                            InputType = InputType.Radio,
                            Options = GetDefaultRadioOptions()
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 6,
                            Description = "What information did you learn from this program that was new and valuable to your practice:",
                            InputType = InputType.TextArea
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 7,
                            Description = "Describe two ways in which you will change your practice as a result of attending this program:",
                            InputType = InputType.TextArea
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 8,
                            Description = "Did you perceive any degree of bias in any part of the program?",
                            InputType = InputType.Radio,
                            Options = new List<EvaluationSurveyOption>
                            {
                                new EvaluationSurveyOption
                                {
                                    Order = 1,
                                    Description = "Yes",
                                    Value= "Yes"
                                },
                                new EvaluationSurveyOption
                                {
                                    Order = 2,
                                    Description = "No",
                                    Value = "No"
                                }
                            }
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 9,
                            Description = "If you selected yes to the above question, please describe:",
                            InputType = InputType.TextArea
                        },
                        new EvaluationSurveyQuestion
                        {
                            Order = 10,
                            Description = "Please list any suggestions regarding future learning needs related to HZ immunization:",
                            InputType = InputType.TextArea
                        },
                    }
                }

            };

            return groups;
        }

        /// <summary>
        /// Gets the default radio button options to solve and cleanup of repeating the data seeding for radio button question options.
        /// </summary>
        /// <returns>Collection of the default radio button options maintaining order.</returns>
        private ICollection<EvaluationSurveyOption> GetDefaultRadioOptions()
        {
            var questionOptions = new List<EvaluationSurveyOption>();

            for (int i = 0; i < defaultRadioOptions.Length; i++)
            {
                questionOptions.Add(new EvaluationSurveyOption
                {
                    Order = i + 1,
                    Description = defaultRadioOptions[i],
                    Value = defaultRadioOptions[i]
                });
            }

            return questionOptions;
        }

        public ICollection<Resource> GetResourceData()
        {
            List<Resource> Resources = new List<Resource>
            {
                new Resource
                {
                    Id= Guid.Parse("cdf90776-1594-45b8-ab2a-e3283f9d4df6"),
                    Path = "Resources\\FINAL_ImmunizNation_Reference_Guide_Aug_29.pdf",
                    MimeType= "application/pdf",
                    FileType= "pdf",
                    FileName= "Practical Counselling Guide for HZ Vaccination.pdf",
                    FileSize= "1.43MB",
                    Title= "Practical Counselling Guide for HZ Vaccination",
                    ThumbnailPath= "images/reference-guide-thumbnail.png",
                    Order = 0
                },
                new Resource
                {
                    Id= Guid.Parse("ed3c5d80-45b2-4e43-b286-79e482dfb99e"),
                    Path= "Resources\\FINAL_ImmunizNation_Infographic_Aug_29.pdf",
                    MimeType= "application/pdf",
                    FileType= "pdf",
                    FileName= "Quick-reference Infographic Summary.pdf",
                    FileSize= "9.07MB",
                    Title= "Quick-reference Infographic Summary",
                    ThumbnailPath= "images/infographic_thumbnail.png",
                    Order = 1
                }, 
                new Resource
                {
                    Id= Guid.Parse("3e93d44c-7723-461c-aa20-5a2e8e2bbdc6"),
                    Path= "Resources\\FINAL_Second_Dose_Email_Reminder_Aug 26.docx",
                    MimeType= "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    FileType= "word",
                    FileName= "HZ Second Dose Email Reminder.docx",
                    FileSize= "13KB",
                    Title= "HZ Second Dose Email Reminder",
                    ThumbnailPath= "images/word-document-thumbnail.png",
                    Order = 2
                }
            };
            return Resources;
        }

    }
}
