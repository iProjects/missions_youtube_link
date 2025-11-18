using missions_youtube_links.Business;
using missions_youtube_links.Data;
using missions_youtube_links.Entities;
using missions_youtube_links.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace missions_youtube_links.UI.Web.Controllers
{
    public class MissionsController : Controller
    {
        public string TAG;
        //Event declaration:
        //event for publishing messages to output
        public event EventHandler<notificationmessageEventArgs> _notificationmessageEventname;
        //list to hold messages
        public List<notificationdto> _lstnotificationdto = new List<notificationdto>();

        public MissionsController()
        {

            TAG = this.GetType().Name;

            AppDomain.CurrentDomain.UnhandledException += new
UnhandledExceptionEventHandler(UnhandledException);
            System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(ThreadException);

            //Subscribing to the event: 
            //Dynamically:
            //EventName += HandlerName;
            _notificationmessageEventname += notificationmessageHandler;

            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("finished MissionsController initialization", TAG));

        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

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
            //Log.Write_To_Log_File_temp_dir(ex);
            helper_utils.getInstance(_notificationmessageEventname).log_messages(ex.ToString(), TAG);
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

                Console.WriteLine(args.message);

                _lstnotificationdto = (from msg in _lstnotificationdto
                                       orderby msg._created_datetime descending
                                       select msg).ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // GET: Missions
        [HttpGet]
        public ActionResult Index([Bind] missions_links_dto search_model)
        {
            try
            {
                missions_links_dto dto = new missions_links_dto();
                missions_view_model model = new missions_view_model();
                List<missions_links_dto> _lst_dtos = new List<missions_links_dto>();

                _lst_dtos = populate_dtos(_lst_dtos);

                if (_lst_dtos != null)
                {
                    Console.WriteLine("Missions count: " + _lst_dtos.Count());
                    TempData["success_message"] = "Retrieved [ " + _lst_dtos.Count() + " ] records.";
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("Retrieved [ " + _lst_dtos.Count() + " ] records.", TAG));
                    helper_utils.getInstance(_notificationmessageEventname).log_messages("Retrieved [ " + _lst_dtos.Count() + " ] records.", TAG);
                }
                else
                {
                    model.dto = populate_model(dto);
                    TempData["error_message"] = "Error retrieving data.";
                    _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("Error retrieving data.", TAG));
                    helper_utils.getInstance(_notificationmessageEventname).log_messages("Error retrieving data.", TAG);
                    return View();
                }

                model.lst_dto = _lst_dtos;
                model.dto = populate_model(dto);

                //Display the records
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["error_message"] = ex.ToString();
                helper_utils.getInstance(_notificationmessageEventname).log_messages(ex.ToString(), TAG);
                error_handler_view_model error_model = new error_handler_view_model();
                error_model.ex = ex;
                return View("Error_View", error_model);
            }
        }
        private List<missions_links_dto> populate_dtos(List<missions_links_dto> _lst_dtos)
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
                _lst_dtos = utilzsingleton.getInstance(_notificationmessageEventname).Convert_DataTable_To_list<missions_links_dto>(dt);
                _lst_dtos = _lst_dtos.OrderByDescending(i => i.id).ThenBy(t => t.description).ToList();

                Console.WriteLine("Missions count: " + _lst_dtos.Count());
            }
            else
            {
                return null;
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
            return _lst_dtos;
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
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("Retrieved [ " + _lst_dtos.Count() + " ] records.", TAG));
                helper_utils.getInstance(_notificationmessageEventname).log_messages("Retrieved [ " + _lst_dtos.Count() + " ] records.", TAG);
            }
            else
            {
                model.lst_dto = _lst_dtos;
                model.dto = populate_model(dto);

                TempData["error_message"] = "Error retrieving data.";
                _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("Error retrieving data.", TAG));
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

        [HttpGet]
        public JsonResult getmonthdays(string year, string month)
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

        // GET: Missions/Create
        public ActionResult Create()
        {
            missions_links_dto model = new missions_links_dto();
            model = populate_model(model);

            return View("add_mission", model);
        }

        // POST: Missions/Create
        [HttpPost]
        public ActionResult Create([Bind] missions_links_dto model)
        {
            try
            {
                // TODO: Add insert logic here

                if (ModelState.IsValid)
                {
                    missions_links_dto dto = new missions_links_dto();
                    dto.youtube_url = model.youtube_url;
                    dto.description = model.description;
                    dto.location = model.location;
                    dto.mission_type = model.mission_type;
                    dto.mission_year = model.mission_year;
                    dto.mission_month = model.mission_month;
                    dto.mission_day = model.mission_day;
                    dto.status = "active";
                    dto.created_date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt");

                    //check if link exists.
                    missions_links_dto model_exists = new missions_links_dto();
                    model_exists = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_mission_by_url(model.youtube_url);

                    if (model_exists != null)
                    {
                        TempData["error_message"] = "YouTube link [ " + model.youtube_url + " ] Exists.";
                        helper_utils.getInstance(_notificationmessageEventname).log_messages("YouTube link [ " + model.youtube_url + " ] Exists.", TAG);
                        model = populate_model(model);
                        return View("add_mission", model);
                    }

                    //save data in database.
                    responsedto _responsedto = new responsedto();
                    _responsedto = businesslayerapisingleton.getInstance(_notificationmessageEventname).create_mission_in_database(dto);

                    if (_responsedto.isresponseresultsuccessful)
                    {
                        Console.WriteLine(_responsedto.responsesuccessmessage);
                        TempData["create_message"] = _responsedto.responsesuccessmessage;
                        helper_utils.getInstance(_notificationmessageEventname).log_messages(_responsedto.responsesuccessmessage, TAG);
                    }
                    else
                    {
                        Console.WriteLine(_responsedto.responseerrormessage);
                        TempData["error_message"] = _responsedto.responseerrormessage;
                        helper_utils.getInstance(_notificationmessageEventname).log_messages(_responsedto.responseerrormessage, TAG);
                        model = populate_model(model);
                        return View("add_mission", model);
                    }
                }
                else
                {
                    TempData["error_message"] = "Validation Error.";
                    helper_utils.getInstance(_notificationmessageEventname).log_messages("Validation Error.", TAG);
                    model = populate_model(model);
                    return View("add_mission", model);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["error_message"] = ex.ToString();
                helper_utils.getInstance(_notificationmessageEventname).log_messages(ex.ToString(), TAG);
                model = populate_model(model);
                return View("add_mission", model);
            }
        }

        // GET: Missions/Edit/5
        public ActionResult Edit(int id)
        {
            missions_links_dto model = new missions_links_dto();

            model = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_mission_by_id(id);

            if (model == null)
            {
                missions_view_model _model = new missions_view_model();
                missions_links_dto dto = new missions_links_dto();
                List<missions_links_dto> _lst_dtos = new List<missions_links_dto>();
                _lst_dtos = populate_dtos(_lst_dtos);
                _model.lst_dto = _lst_dtos;
                _model.dto = populate_model(dto);
                TempData["error_message"] = "Error retrieving Data.";
                helper_utils.getInstance(_notificationmessageEventname).log_messages("Error retrieving Data.", TAG);
                return View("Index", _model);
            }

            model = populate_model(model);
            TempData["success_message"] = "Successfully retrieved Data.";
            helper_utils.getInstance(_notificationmessageEventname).log_messages("Successfully retrieved Data.", TAG);
            return View("edit_mission", model);
        }

        // POST: Missions/Edit/5
        [HttpPost]
        public ActionResult Edit([Bind] missions_links_dto model)
        {
            try
            {
                // TODO: Add update logic here

                if (ModelState.IsValid)
                {
                    missions_links_dto dto = new missions_links_dto();
                    dto.id = model.id;
                    dto.youtube_url = model.youtube_url;
                    dto.description = model.description;
                    dto.location = model.location;
                    dto.mission_type = model.mission_type;
                    dto.mission_year = model.mission_year;
                    dto.mission_month = model.mission_month;
                    dto.mission_day = model.mission_day;
                    //dto.status = "active";
                    //dto.created_date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt");

                    //check if link exists.
                    missions_links_dto model_exists = new missions_links_dto();
                    model_exists = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_mission_by_url(model.youtube_url);

                    if (model.id != model_exists.id)
                    {
                        TempData["error_message"] = "YouTube link [ " + model.youtube_url + " ] Exists.";
                        model = populate_model(model);
                        return View("edit_mission", model);
                    }

                    //save in database.
                    responsedto _responsedto = new responsedto();
                    _responsedto = businesslayerapisingleton.getInstance(_notificationmessageEventname).update_mission_in_database(dto);

                    if (_responsedto.isresponseresultsuccessful)
                    {
                        Console.WriteLine(_responsedto.responsesuccessmessage);
                        TempData["update_message"] = _responsedto.responsesuccessmessage;
                    }
                    else
                    {
                        Console.WriteLine(_responsedto.responseerrormessage);
                        TempData["error_message"] = _responsedto.responseerrormessage;
                        model = populate_model(model);
                        return View("edit_mission", model);
                    }
                }
                else
                {
                    TempData["error_message"] = "Validation Error.";
                    model = populate_model(model);
                    return View("edit_mission", model);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["error_message"] = ex.ToString();
                model = populate_model(model);
                return View("edit_mission", model);
            }
        }

        // GET: Missions/Details/5
        public ActionResult Details(int id)
        {
            missions_links_dto model = new missions_links_dto();

            model = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_mission_by_id(id);

            if (model == null)
            {
                missions_youtube_links.UI.Web.Models.missions_view_model _model = new missions_youtube_links.UI.Web.Models.missions_view_model();
                missions_links_dto dto = new missions_links_dto();
                List<missions_links_dto> _lst_dtos = new List<missions_links_dto>();
                _lst_dtos = populate_dtos(_lst_dtos);
                _model.lst_dto = _lst_dtos;
                _model.dto = populate_model(dto);
                TempData["error_message"] = "Error retrieving Data.";
                return View("Index", _model);
            }

            model = populate_model(model);
            TempData["success_message"] = "Successfully retrieved Data.";
            return View("details", model);
        }

        // GET: Missions/Delete/5
        public ActionResult Delete(int id)
        {
            missions_links_dto model = new missions_links_dto();

            model = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_mission_by_id(id);

            if (model == null)
            {
                missions_youtube_links.UI.Web.Models.missions_view_model _model = new missions_youtube_links.UI.Web.Models.missions_view_model();
                missions_links_dto dto = new missions_links_dto();
                List<missions_links_dto> _lst_dtos = new List<missions_links_dto>();
                _lst_dtos = populate_dtos(_lst_dtos);
                _model.lst_dto = _lst_dtos;
                _model.dto = populate_model(dto);
                TempData["error_message"] = "Error retrieving Data.";
                return View("Index", _model);
            }

            model = populate_model(model);
            TempData["success_message"] = "Successfully retrieved Data.";
            TempData["delete_id"] = id;

            return View("delete_mission", model);
        }

        // POST: Missions/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                if (id <= 0)
                {
                    TempData["error_message"] = "Error retrieving id.";
                    missions_links_dto delete_model = new missions_links_dto();
                    delete_model.id = (long)TempData["delete_id"];

                    return View("delete_mission", delete_model);
                }

                missions_links_dto model = new missions_links_dto();
                model = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_mission_by_id(id);

                if (model == null)
                {
                    missions_youtube_links.UI.Web.Models.missions_view_model _model = new missions_youtube_links.UI.Web.Models.missions_view_model();
                    missions_links_dto dto = new missions_links_dto();
                    List<missions_links_dto> _lst_dtos = new List<missions_links_dto>();
                    _lst_dtos = populate_dtos(_lst_dtos);
                    _model.lst_dto = _lst_dtos;
                    _model.dto = populate_model(dto);
                    TempData["error_message"] = "Error retrieving Data.";
                    return View("Index", _model);
                }

                responsedto _responsedto = new responsedto();
                long delete_id = (long)id;
                _responsedto = businesslayerapisingleton.getInstance(_notificationmessageEventname).delete_mission_in_database(model);

                if (_responsedto.isresponseresultsuccessful)
                {
                    Console.WriteLine(_responsedto.responsesuccessmessage);
                    TempData["delete_message"] = _responsedto.responsesuccessmessage;
                }
                else
                {
                    Console.WriteLine(_responsedto.responseerrormessage);
                    TempData["error_message"] = _responsedto.responseerrormessage;
                    model = populate_model(model);
                    return View("delete_mission", model);
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }






    }
}
