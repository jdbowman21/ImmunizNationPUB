using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImmunizNation.Client.Models.EvaluationSurvey
{
    /// <summary>
    /// Supported HTML input types. Datetime recieves a javascript date-time picker.
    /// </summary>
    public enum InputType 
    {
        Text,
        TextArea,
        Radio,
        Select,
        DateTime,
        Checkbox
    }

    /// <summary>
    /// Represents a Question which is appart of a question group.
    /// It is currently required that each survey question have a question group.
    /// </summary>
    public class EvaluationSurveyQuestion
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Group))]
        public int GroupId { get; set; }

        public virtual EvaluationSurveyQuestionGroup Group { get; set; }

        /// <summary>
        /// Description of the question
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specifies the HTML input type.
        /// @note - only one answer can be submitted per question.
        /// </summary>
        public InputType InputType { get; set; }

        /// <summary>
        /// Required to maintain order with EF seeding data to the database. Order was being lost by EF internal framework.
        /// </summary>
        public int Order { get; set; }

        public ICollection<EvaluationSurveyOption> Options { get; set; }

    }

    /// <summary>
    /// Answer options for an EvaluationSurveyQuestion. Options are only used with the following InputTypes: Radio, Select.
    /// </summary>
    public class EvaluationSurveyOption
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Question))]
        public int QuestionId { get; set; }
        public virtual EvaluationSurveyQuestion Question { get; set; }

        public string Description { get; set; }

        public string Value { get; set; } 

        /// <summary>
        /// Required to maintain order with EF seeding data to the database. Order was being lost by EF internal framework.
        /// </summary>
        public int Order { get; set; }
    }

    /// <summary>
    /// Represents a group of EvaluationSurveyQuestions. It is not necessary to set a title or description to leave those fields out.
    /// </summary>
    public class EvaluationSurveyQuestionGroup
    {
        [Key]
        public int Id { get; set; }

        public int Order { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public virtual ICollection<EvaluationSurveyQuestion> Questions { get; set; }
    }

    /// <summary>
    /// Represents the anonymous EvaluationSurvey submitted by a registered user. 
    /// It is not required a user complete the Evaluation Survey.
    /// </summary>
    public class EvaluationSurvey
    {
        [Key]
        public int Id { get; set; }

        public virtual ICollection<EvaluationSurveyUserAnswer> UserAnswers { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class EvaluationSurveyUserAnswer
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Survey))]
        public int SurveyId { get; set; }
        public virtual EvaluationSurvey Survey { get; set; }

        [ForeignKey(nameof(Question))]
        public int QuestionId { get; set; }
        public virtual EvaluationSurveyQuestion Question {get;set;}

        public string Value { get; set; }
    }

    

}
