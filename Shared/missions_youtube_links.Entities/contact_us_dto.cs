using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace missions_youtube_links.Entities
{
    public class contact_us_dto
    {
        public Int64 id { get; set; }
        [Required(ErrorMessage = "Name cannot be null.")]
        [Display(Name = "Name")]
        public string name { get; set; }
        [Required(ErrorMessage = "Email cannot be null")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email")]
        public string email { get; set; }
        [Required(ErrorMessage = "Phone no cannot be null", AllowEmptyStrings = true)]
        [Display(Name = "Phone No")]
        public string phone_no { get; set; }
        [Required(ErrorMessage = "Message cannot be null")]
        [Display(Name = "Message")]
        public string message { get; set; }
        public string status { get; set; }
        public string created_date { get; set; }
    }
}



