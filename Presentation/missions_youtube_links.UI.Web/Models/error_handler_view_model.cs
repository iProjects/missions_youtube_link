using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace missions_youtube_links.UI.Web.Models
{
    public class error_handler_view_model
    {
        public Exception ex { get; set; }
        public string message { get; set; }
        public string stack_trace { get; set; }
    }
}