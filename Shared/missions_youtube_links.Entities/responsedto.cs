/*
 * Created by: "kevin mutugi, kevinmk30@gmail.com, +254717769329"
 * Date: 04/02/2020
 * Time: 02:31
 */
using System;

namespace missions_youtube_links.Entities
{
	public class responsedto
	{ 
		public string responsesuccessmessage { get; set; }
		public string responseerrormessage { get; set; }
		public string responsemethod { get; set; }
		public string responseclass { get; set; }
		public bool isresponseresultsuccessful { get; set; }
		public object responseresultobject { get; set; } 
	}
}
