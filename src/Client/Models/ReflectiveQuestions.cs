using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ImmunizNation.Client.Models
{
    public class ReflectiveQuestionExam
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public bool IsDraft { get; set; }

        public ICollection<ReflectiveQuestionUserAnswer> Answers { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ReflectiveQuestion
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
    }

    public class ReflectiveQuestionUserAnswer
    {
        [Key]
        public int Id { get; set; }

        public string Answer { get; set; }

        public virtual ReflectiveQuestionExam Exam { get; set; }

        public int ReflectiveQuestionId { get; set; }
        public virtual ReflectiveQuestion ReflectiveQuestion { get; set; }
    }
}
