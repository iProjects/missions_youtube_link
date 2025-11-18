using missions_youtube_links.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace missions_youtube_links.UI.Web.Models
{
    public class logs_view_model
    {
        public IEnumerable<log_dto> lst_dto { get; set; }
        public log_dto dto { get; set; }
    }
}