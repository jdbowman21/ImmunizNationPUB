using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ImmunizNation.Client.ViewModels.ReflectiveQuestions
{
    public class ReflectiveQuestionViewModel
    {
        public int Id { get; set; }

        public string Description { get; set; }
        
        public string Answer { get; set; }
    }

    public class ReflectiveQuestionExamViewModel 
    {
        public string LastQuizReuslt { get; set; }
        public string HighScore { get; set; }
    }

    public class ReflectiveActivityViewModel
    {
        public IEnumerable<string> Columns { get; set; }
        public List<ReflectiveActivityResultViewModel> Results { get; set; }
    }


    public class ReflectiveActivityResultViewModel
    {
        public string Name { get; set; }

        public ICollection<string> Answers { get; set; }
    }

}
