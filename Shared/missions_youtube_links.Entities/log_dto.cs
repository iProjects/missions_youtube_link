using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace missions_youtube_links.Entities
{
    public class log_dto
    {
        public Int64 id { get; set; } 
        public string message { get; set; } 
        public string timestamp { get; set; } 
        public string tag { get; set; }
        public string status { get; set; } 
        public string created_date { get; set; }
    }
}
