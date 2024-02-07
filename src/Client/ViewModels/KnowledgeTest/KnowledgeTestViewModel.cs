using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ImmunizNation.Client.ViewModels.KnowledgeTest
{
    public class KnowledgeTestViewModel
    {
        [Key]
        public int Id { get; set; }

        public virtual List<KnowledgetTestQuestionViewModel> Questions { get; set; }
    }

    public class KnowledgetTestQuestionViewModel
    {
        public int Id { get; set; }

        public string Question { get; set; }

        public string LessonDescription { get; set; }

        // @note - probably take out for cheating reasons.
        public string Answer { get; set; }

        public string UserAnswer { get; set; }

        public IList<KnowledgeTestAnswerViewModel> Answers { get; set; }
    }

    public class KnowledgeTestAnswerViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
    }
}
