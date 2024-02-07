using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImmunizNation.Client.Models
{
    public class KnowledgeTestQuestion
    {
        [Key]
        public int Id { get; set; }

        public string Question { get; set; }

        public string LessonDescription { get; set; }

        public string Answer { get; set; }

        public string ConcurrencyStamp { get; set; }

        public int Order { get; set; }
        public virtual ICollection<KnowledgeTestAnswer> Answers { get; set; }
    }

    public class KnowledgeTestAnswer
    {
        [Key]
        public int Id { get; set; }

        public string Description { get; set; }

        public string Value { get; set; }

        public int Order { get; set; }

        [ForeignKey(nameof(Question))]
        public int KnowledgeTestQuestionId { get; set; }
        public virtual KnowledgeTestQuestion Question { get; set; }
    }

    public class KnowledgeTestUserAnswer
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Question))]
        public int KnowledgeTestQuestionId { get; set; }
        public virtual KnowledgeTestQuestion Question { get; set; }

        public string Value { get; set; }
    }

    public class KnowledgeTest
{
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public double? Score { get; set; }

        public ICollection<KnowledgeTestUserAnswer> Answers { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModified { get; set; }
    }

}
