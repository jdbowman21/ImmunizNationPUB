using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImmunizNation.Client.Models
{
    public class ResourceDownload
    {
        [Key]
        public int Id { get; set; }

        public string UserId;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        public Guid ResourceId { get; set; }

        [ForeignKey(nameof(ResourceId))]
        public virtual Resource Resource { get; set; }

        public int Count { get; set; }
    }
}
