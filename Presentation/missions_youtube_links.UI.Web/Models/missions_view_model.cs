using missions_youtube_links.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace missions_youtube_links.UI.Web.Models
{
    public class missions_view_model
    {
        public IEnumerable<missions_links_dto> lst_dto { get; set; }
        public missions_links_dto dto { get; set; }
    }
}