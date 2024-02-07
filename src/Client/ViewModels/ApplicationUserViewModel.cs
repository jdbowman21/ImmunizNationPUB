using ImmunizNation.Client.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ImmunizNation.Client.ViewModels
{
    public class ApplicationUserViewModel
    {
        public string Id { get; set; }
        [Display(Name ="Email")]

        public string Email { get; set; }
        [Display(Name = "First Name")]

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Display(Name = "Account Type")]
        public AccountTypes AccountType { get; set; }

        public string City { get; set; }
        
        public Provinces Province { get; set; }

        public string CertSessionId { get; set; }

        public DateTime? DateOfSession { get; set; }

        public string LocationOfSession { get; set; }

        public bool SubscriptionEmail { get; set; }

        public DateTime CreatedDate { get; set; }

        [Display(Name = "Assessment Test")]
        public double? Score { get; set; }

        public bool RACompleted { get; set; }

        /// <summary>
        /// Returns the name of the user with a space seperator.
        /// </summary>
        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }

        /// <summary>
        /// Returns the users score as a percent or a dash if the Assessment Test has not been completed.
        /// </summary>
        public string ScoreAsPercent
        {
            get 
            {
                if (!Score.HasValue) return "-";
                return String.Format("{0:P0}", Score.Value);
            }
        }

        /// <summary>
        /// Had the user completed all the examinations based on their account type.
        /// </summary>
        /// <returns>Returns true if all the examinations are complete and user can be issued a certificate of completion.</returns>
        public bool IsPassingUser()
        {
            if (!Score.HasValue) 
                return false;

            if (AccountType == AccountTypes.GeneralPractitioner)
            {
                return Score.Value >= 0.7 && RACompleted;
            }
            else
            {
                return Score.Value >= 0.7;
            }
        }
    }
}
