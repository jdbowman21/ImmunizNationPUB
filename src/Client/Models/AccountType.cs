using System;
using System.ComponentModel.DataAnnotations;

namespace ImmunizNation.Client.Models
{
    /// <summary>
    /// Account types for users to fill out the evaulations. Administrators have a None Account Type.
    /// </summary>
    public enum AccountTypes
    { 
        None = 0,

        [Display(Name = "General Practitioner")]
        GeneralPractitioner = 1,

        [Display(Name = "Pharmacist")]
        Pharmacist = 2,
    }
}