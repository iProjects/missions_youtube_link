using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace missions_youtube_links.Entities
{
    public class missions_links_dto
    {
        public Int64 id { get; set; }
        [Required(ErrorMessage = "YouTube Url cannot be null.")]
        [RegularExpression(@"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?)", ErrorMessage = "Not a valid YouTube Url")]
        [Display(Name = "YouTube Url")]
        public string youtube_url { get; set; }
        [Required(ErrorMessage = "Description cannot be null")]
        [Display(Name = "Description")]
        public string description { get; set; }
        [Required(ErrorMessage = "Location cannot be null")]
        [Display(Name = "Location")]
        public string location { get; set; }
        [Required(ErrorMessage = "Mission Type cannot be null")]
        [Display(Name = "Type")]
        public string mission_type { get; set; }
        [Required(ErrorMessage = "Mission Year cannot be null")]
        [Display(Name = "Year")]
        public string mission_year { get; set; }
        [Required(ErrorMessage = "Mission Month cannot be null")]
        [Display(Name = "Month")]
        public string mission_month { get; set; }
        [Required(ErrorMessage = "Mission Day cannot be null")]
        [Display(Name = "Day")]
        public string mission_day { get; set; }
        public string status { get; set; }
        public string created_date { get; set; }


        public string mission_month_str { get; set; }
        public string mission_day_str { get; set; }
        public IEnumerable<SelectListItem> mission_types { get; set; }
        public IEnumerable<SelectListItem> mission_years { get; set; }
        public IEnumerable<SelectListItem> mission_months { get; set; }
        public IEnumerable<SelectListItem> mission_days { get; set; }
    }
}



