using ImmunizNation.Client.Models.EvaluationSurvey;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImmunizNation.Client.ViewModels.EvaluationSurvey
{
    public class EvaluationSurveyQuestionsViewModel
    {
        public int Id { get; set; }

        public int GroupId { get; set; }

        public string Description { get; set; }

        public InputType InputType { get; set; }

        public string UserAnswer { get; set; }

        public IList<EvaluationSurveyOption> Options { get; set; }

        public IEnumerable<SelectListItem> AsSelectList()
        {
            return Options
                .Select(m => new SelectListItem(
                    m.Description,
                    String.IsNullOrEmpty(m.Value) ? m.Description : m.Value)
                ).ToList();
        }
    }

    public class EvaluationSurveyQuestionOptionsViewModel
    {
        public int Id { get; set; }
    }

    public class EvaluationSurveyGroupsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IList<EvaluationSurveyQuestionsViewModel> Questions { get; set; }

    }

    public class EvaluationSurveyReportViewModel
    {
        public ICollection<EvaluationSurveyReportColumnViewModel> Columns { get; set; }
    }

    public class EvaluationSurveyReportColumnViewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// Column header shall be a EvaluationSurveyQuestion.
        /// </summary>
        public string Header { get; set; }

        public InputType InputType { get; set; }  

        /// <summary>
        /// Collection of rows for this specific column.
        /// </summary>
        public ICollection<string> Rows { get; set; }

        public EvaluationSurveyReportColumnViewModel()
        {
            Rows = new List<string>();
        }
    }

    //public class EvaluationSurveyReportRowViewModel
    //{
    //    public string Answer { get; set; }
    //}


    //public class EvaluationSurveyViewModel
    //{
    //    public IList<EvaluationSurveyQuestionViewModel> Questions;
    //}

    //public class EvaluationSurveyQuestionViewModel
    //{
    //    public int Id { get; set; }
    //    public InputType InputType { get; set; }

    //    public string Description { get; set; }

    //    public IList<EvaluationSurveyQuestionAnswerViewModel> QuestionAnswers;

    //    public string UserAnswer { get; set; }
    //}

    //public class EvaluationSurveyQuestionAnswerViewModel
    //{
    //    public int Id { get; set; }

    //    public string Description { get; set; }
    //}
}
