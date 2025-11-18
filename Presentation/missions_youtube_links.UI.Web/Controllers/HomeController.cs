using missions_youtube_links.Business;
using missions_youtube_links.Data;
using missions_youtube_links.Entities;
using missions_youtube_links.Entities;
using missions_youtube_links.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace missions_youtube_links.UI.Web.Controllers
{
    public class HomeController : Controller
    {
        public string TAG;
        //Event declaration:
        //event for publishing messages to output
        public event EventHandler<notificationmessageEventArgs> _notificationmessageEventname;
        //list to hold messages
        public List<notificationdto> _lstnotificationdto = new List<notificationdto>();

        public HomeController()
        {

            TAG = this.GetType().Name;

            AppDomain.CurrentDomain.UnhandledException += new
UnhandledExceptionEventHandler(UnhandledException);
            System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(ThreadException);

            //Subscribing to the event: 
            //Dynamically:
            //EventName += HandlerName;
            _notificationmessageEventname += notificationmessageHandler;

            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("finished HomeController initialization", TAG));

        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            helper_utils.getInstance(_notificationmessageEventname).log_messages(ex.ToString(), TAG);
            //Log.Write_To_Log_File_temp_dir(ex);
            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
        }

        private void ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            helper_utils.getInstance(_notificationmessageEventname).log_messages(ex.ToString(), TAG);
            //Log.Write_To_Log_File_temp_dir(ex);
            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
        }

        //Event handler declaration:
        private void notificationmessageHandler(object sender, notificationmessageEventArgs args)
        {
            try
            {
                /* Handler logic */
                notificationdto _notificationdto = new notificationdto();

                DateTime currentDate = DateTime.Now;
                String dateTimenow = currentDate.ToString("dd-MM-yyyy HH:mm:ss tt");

                String _logtext = Environment.NewLine + "[ " + dateTimenow + " ]   " + args.message;

                _notificationdto._notification_message = _logtext;
                _notificationdto._created_datetime = dateTimenow;
                _notificationdto.TAG = args.TAG;

                _lstnotificationdto.Add(_notificationdto);

                Console.WriteLine(args.message);
                 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [HttpGet]
        public JsonResult get_messages()
        {
            List<log_dto> _lst_dtos = new List<log_dto>();

            DataTable dt = null;
            string query = "";
            bool showinactive = true;
            string server = "sqlite"; //selected_server.Key;

            if (showinactive)
            {
                query = DBContract.logs_entity_table.SELECT_ALL_QUERY;
            }
            else
            {
                query = DBContract.logs_entity_table.SELECT_ALL_FILTER_QUERY;
            }

            dt = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_logs(showinactive, query, DBContract.sqlite);

            if (dt != null)
            {
                _lst_dtos = utilzsingleton.getInstance(_notificationmessageEventname).Convert_DataTable_To_list<log_dto>(dt);
                _lst_dtos = _lst_dtos.OrderByDescending(i => i.id).ThenBy(t => t.timestamp).ToList();

                Console.WriteLine("Logs count: " + _lst_dtos.Count());
            }
            else
            {
                return null;
            }

            //return the records
            return Json(_lst_dtos, JsonRequestBehavior.AllowGet);
        }

        // GET: Missions
        [HttpGet]
        public ActionResult Index([Bind] missions_links_dto search_model)
        {
            missions_links_dto dto = new missions_links_dto();
            missions_view_model model = new missions_view_model();
            List<missions_links_dto> _lst_dtos = new List<missions_links_dto>();

            _lst_dtos = populate_missions(_lst_dtos);

            if (_lst_dtos != null)
            {
                Console.WriteLine("Missions count: " + _lst_dtos.Count());
                TempData["success_message"] = "Retrieved [ " + _lst_dtos.Count() + " ] records.";
                helper_utils.getInstance(_notificationmessageEventname).log_messages("Retrieved [ " + _lst_dtos.Count() + " ] records.", TAG);
            }
            else
            {
                model.dto = populate_model(dto);
                TempData["error_message"] = "Error retrieving data.";
                helper_utils.getInstance(_notificationmessageEventname).log_messages("Error retrieving data.", TAG);
                return View();
            }

            model.lst_dto = _lst_dtos;
            model.dto = populate_model(dto);

            //Display the records
            return View(model);

        }
        private List<missions_links_dto> populate_missions(List<missions_links_dto> _missions)
        {
            DataTable dt = null;
            string query = "";
            bool showinactive = true;
            string server = "sqlite"; //selected_server.Key;

            if (showinactive)
            {
                query = DBContract.missions_links_entity_table.SELECT_ALL_QUERY;
            }
            else
            {
                query = DBContract.missions_links_entity_table.SELECT_ALL_FILTER_QUERY;
            }

            dt = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_missions(showinactive, query, DBContract.sqlite);

            if (dt != null)
            {
                _missions = utilzsingleton.getInstance(_notificationmessageEventname).Convert_DataTable_To_list<missions_links_dto>(dt);
                _missions = _missions.OrderByDescending(i => i.id).ThenBy(t => t.description).ToList();

                Console.WriteLine("Missions count: " + _missions.Count());
            }
            else
            {
                return null;
            }

            foreach (missions_links_dto mission in _missions)
            {
                var year = int.Parse(mission.mission_year);
                var month = int.Parse(mission.mission_month);
                var day = int.Parse(mission.mission_day);

                DateTime date = new DateTime(year, month, day);
                string day_name = date.ToString("dddd");
                string month_name = date.ToString("MMMMM");

                mission.mission_day_str = day_name;
                mission.mission_month_str = month_name;
            }
            return _missions;
        }
        [HttpPost]
        public JsonResult search([Bind] missions_links_dto search_model)
        {

            missions_links_dto dto = new missions_links_dto();
            missions_view_model model = new missions_view_model();
            DataTable dt = null;
            List<missions_links_dto> _lst_dtos = new List<missions_links_dto>();

            dt = businesslayerapisingleton.getInstance(_notificationmessageEventname).search_missions_in_database(search_model);

            if (dt != null)
            {
                _lst_dtos = utilzsingleton.getInstance(_notificationmessageEventname).Convert_DataTable_To_list<missions_links_dto>(dt);
                _lst_dtos = _lst_dtos.OrderByDescending(i => i.id).ThenBy(t => t.description).ToList();

                Console.WriteLine("Missions count: " + _lst_dtos.Count());
                TempData["success_message"] = "Retrieved [ " + _lst_dtos.Count() + " ] records.";
                helper_utils.getInstance(_notificationmessageEventname).log_messages("Retrieved [ " + _lst_dtos.Count() + " ] records.", TAG);
            }
            else
            {
                model.lst_dto = _lst_dtos;
                model.dto = populate_model(dto);

                TempData["error_message"] = "Error retrieving data.";
                helper_utils.getInstance(_notificationmessageEventname).log_messages("Error retrieving data.", TAG);

                return Json(model);
            }

            foreach (missions_links_dto mission in _lst_dtos)
            {
                var year = int.Parse(mission.mission_year);
                var month = int.Parse(mission.mission_month);
                var day = int.Parse(mission.mission_day);

                DateTime date = new DateTime(year, month, day);
                string day_name = date.ToString("dddd");
                string month_name = date.ToString("MMMMM");

                mission.mission_day_str = day_name;
                mission.mission_month_str = month_name;
            }

            model.lst_dto = _lst_dtos;
            model.dto = populate_model(dto);

            //Display the records
            return Json(model);

        }

        private missions_links_dto populate_model(missions_links_dto model)
        {
            List<SelectListItem> mission_types = new List<SelectListItem>();
            List<SelectListItem> years = new List<SelectListItem>();
            List<SelectListItem> months = new List<SelectListItem>();
            List<SelectListItem> days = new List<SelectListItem>();

            Dictionary<string, string> mission_types_dic = new Dictionary<string, string>()
            { 
                {"Healing Service","Healing Service"},
                {"Pastors Conference","Pastors Conference"},
                {"Word Expo","Word Expo"},
                {"Miracle","Miracle"},
                {"Prophecy","Prophecy"},
                {"Prophecy Fullfilment","Prophecy Fullfilment"},
                {"Blessings","Blessings"},
                {"Grand Reception","Grand Reception"},
                {"Repentance Service","Repentance Service"},
            };

            SelectListItem mstp = new SelectListItem()
            {
                Value = null,
                Text = "--- Select Type ---"
            };

            //SelectList mission_types_lst = new SelectList(mission_types_dic.Select(x => new { Value = x.Key, Text = x.Value }), "Value", "Text");

            foreach (KeyValuePair<string, string> mission_type in mission_types_dic)
            {
                SelectListItem sli = new SelectListItem()
                {
                    Value = mission_type.Key,
                    Text = mission_type.Value
                };
                mission_types.Add(sli);
            }

            mission_types.Insert(0, mstp);

            model.mission_types = mission_types;

            List<string> _years = new List<string>();

            DateTime startyear = DateTime.Now;
            while (startyear.Year >= DateTime.Now.AddYears(-80).Year)
            {
                _years.Add(startyear.Year.ToString());
                startyear = startyear.AddYears(-1);
            }

            SelectListItem sli_yr = new SelectListItem()
            {
                Value = null,
                Text = "--- Select Year ---"
            };

            years.Add(sli_yr);

            foreach (string yr in _years)
            {
                SelectListItem sli = new SelectListItem()
                {
                    Value = yr,
                    Text = yr
                };
                years.Add(sli);
            }

            model.mission_years = years;

            months.Add(new SelectListItem { Value = null, Text = "--- Select Month ---" });
            months.Add(new SelectListItem { Value = "1", Text = "January" });
            months.Add(new SelectListItem { Value = "2", Text = "February" });
            months.Add(new SelectListItem { Value = "3", Text = "March" });
            months.Add(new SelectListItem { Value = "4", Text = "April" });
            months.Add(new SelectListItem { Value = "5", Text = "May" });
            months.Add(new SelectListItem { Value = "6", Text = "June" });
            months.Add(new SelectListItem { Value = "7", Text = "July" });
            months.Add(new SelectListItem { Value = "8", Text = "August" });
            months.Add(new SelectListItem { Value = "9", Text = "September" });
            months.Add(new SelectListItem { Value = "10", Text = "October" });
            months.Add(new SelectListItem { Value = "11", Text = "November" });
            months.Add(new SelectListItem { Value = "12", Text = "December" });

            model.mission_months = months;

            int _year = DateTime.Now.Year;
            int _month = DateTime.Now.Month;

            days.Add(new SelectListItem { Value = null, Text = "--- Select Day ---" });

            for (int i = 1; i <= DateTime.DaysInMonth(_year, _month); i++)
            {
                days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }

            model.mission_days = days;

            return model;
        }

        [HttpGet]
        public ActionResult getmonthdays(string year, string month)
        {
            List<SelectListItem> days = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year))
            {
                int _year = int.Parse(year);
                int _month = int.Parse(month);

                for (int i = 1; i <= DateTime.DaysInMonth(_year, _month); i++)
                {
                    days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
                }
                return Json(days, JsonRequestBehavior.AllowGet);
            }
            else if (string.IsNullOrEmpty(year) && !string.IsNullOrEmpty(month))
            {
                int _year = DateTime.Now.Year;
                int _month = int.Parse(month);

                for (int i = 1; i <= DateTime.DaysInMonth(_year, _month); i++)
                {
                    days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
                }
                return Json(days, JsonRequestBehavior.AllowGet);
            }
            else if (string.IsNullOrEmpty(year) && string.IsNullOrEmpty(month))
            {
                int _year = DateTime.Now.Year;
                int _month = DateTime.Now.Month;

                for (int i = 1; i <= DateTime.DaysInMonth(_year, _month); i++)
                {
                    days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
                }
                return Json(days, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Contact([Bind] contact_us_dto model)
        {
            try
            {
                // TODO: Add update logic here
                //ModelState.Remove("phone_no");
                //ModelState.Keys.Remove("phone_no");

                if (ModelState.IsValid)
                {
                    contact_us_dto dto = new contact_us_dto();
                    dto.name = model.name;
                    dto.email = model.email;
                    dto.phone_no = model.phone_no;
                    dto.message = model.message;

                    string template = string.Empty;
                    StringBuilder sb = new StringBuilder();

                    DateTime nowDate = DateTime.Now;
                    string macaddrress = Utils.GetMACAddress();
                    string ipAddresses = Utils.GetFormattedIpAddresses();
                    String now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt");

                    sb.Append("Email from [ " + dto.name + " ] ");
                    sb.Append("with email address [ " + dto.email + " ]. ");
                    sb.Append("Message [ " + dto.message + " ] ");
                    sb.Append("Sent at [ " + now + " ] ");
                    if (FQDN.GetFQDN() != null)
                    {
                        sb.Append("machine name [ " + FQDN.GetFQDN() + " ] ");
                    }
                    if (macaddrress != null)
                    {
                        sb.Append("MAC [ " + macaddrress + " ] ");
                    }
                    if (ipAddresses != null)
                    {
                        sb.Append("ip addresses [ " + ipAddresses + " ] ");
                    }

                    template = sb.ToString();

                    Console.WriteLine(template);
                    bool is_email_sent = false;

                    //send email.
                    if (Utils.IsConnectedToInternet())
                    {
                        is_email_sent = Utils.SendEmail(template);
                    }

                    if (is_email_sent)
                    {
                        Console.WriteLine(is_email_sent);
                        TempData["success_message"] = "Email sent successfully.";
                        helper_utils.getInstance(_notificationmessageEventname).log_messages("Error retrieving data.", TAG);
                    }
                    else
                    {
                        Console.WriteLine(is_email_sent);
                        TempData["error_message"] = "Email not sent.";
                        helper_utils.getInstance(_notificationmessageEventname).log_messages("Error retrieving data.", TAG);
                        return View(model);
                    }
                }
                else
                {
                    TempData["error_message"] = "Validation Error.";
                    helper_utils.getInstance(_notificationmessageEventname).log_messages("Validation Error.", TAG);
                    return View(model);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["error_message"] = ex.ToString();
                helper_utils.getInstance(_notificationmessageEventname).log_messages(ex.ToString(), TAG);
                return View(model);
            }
        }




    }
}