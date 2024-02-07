using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ImmunizNation.Client.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Users first name
        public string FirstName { get; set; }

        // users last name
        public string LastName { get; set; }

        // license number for Pharmasist account type.
        public string LicenseNumber { get; set; }

        public string City { get; set; }

        public Provinces Province { get; set; }

        public AccountTypes AccountType { get; set; }

        // whether or not the user wants to recieve notifications about discussion forum posts.
        public bool SubscriptionEmail { get; set; }

        public string CertSessionId { get; set; }

        public DateTime DateOfSession { get; set; }

        public string LocationOfSession { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
