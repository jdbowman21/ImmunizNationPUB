using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ImmunizNation.Client.Models
{
    public enum Provinces
    {
        [Display(Name = "Alberta")]
        Alberta,
        [Display(Name = "British Columbia")]
        British_Columbia,
        [Display(Name = "Manitoba")]
        Manitoba,
        [Display(Name = "New Brunswick")]
        New_Brunswick,
        [Display(Name = "Newfoundland and Labrador")]
        Newfoundland_and_Labrador,
        [Display(Name = "Northwest Territories")]
        Northwest_Territories,
        [Display(Name = "Nova Scotia")]
        Nova_Scotia,
        [Display(Name = "Nunavut")]
        Nunavut,
        [Display(Name = "Ontario")]
        Ontario,
        [Display(Name = "Prince Edward Island")]
        Prince_Edward_Island,
        [Display(Name = "Quebec")]
        Quebec,
        [Display(Name = "Saskatchewan")]
        Saskatchewan,
        [Display(Name = "Yukon")]
        Yukon
    }
}
