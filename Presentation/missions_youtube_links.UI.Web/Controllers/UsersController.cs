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
    public class UsersController : Controller
    {
        public string TAG;
        //Event declaration:
        //event for publishing messages to output
        public event EventHandler<notificationmessageEventArgs> _notificationmessageEventname;
        //list to hold messages
        public List<notificationdto> _lstnotificationdto = new List<notificationdto>();

        public UsersController()
        {

            TAG = this.GetType().Name;

            AppDomain.CurrentDomain.UnhandledException += new
UnhandledExceptionEventHandler(UnhandledException);
            System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(ThreadException);

            //Subscribing to the event: 
            //Dynamically:
            //EventName += HandlerName;
            _notificationmessageEventname += notificationmessageHandler;

            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("finished UsersController initialization", TAG));

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

                //TempData["error_message"] = args.message;

                //Log.WriteToErrorLogFile_and_EventViewer(new Exception(args.message));

                var _lstmsgdto = from msgdto in _lstnotificationdto
                                 orderby msgdto._created_datetime descending
                                 select msgdto._notification_message;

                String[] _logflippedlines = _lstmsgdto.ToArray();

                if (_logflippedlines.Length > 5000)
                {
                    _lstnotificationdto.Clear();
                }

                //txtlog.Lines = _logflippedlines;
                //txtlog.ScrollToCaret();

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

        // GET: Users
        [HttpGet]
        public ActionResult Index([Bind] user_dto search_model)
        {
            user_dto dto = new user_dto();
            users_view_model model = new users_view_model();
            List<user_dto> _lst_dtos = new List<user_dto>();

            _lst_dtos = populate_dtos(_lst_dtos);

            if (_lst_dtos != null)
            {
                Console.WriteLine("Users count: " + _lst_dtos.Count());
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

        private List<user_dto> populate_dtos(List<user_dto> _users)
        {
            DataTable dt = null;
            string query = "";
            bool showinactive = true;
            string server = "sqlite"; //selected_server.Key;

            if (showinactive)
            {
                query = DBContract.users_entity_table.SELECT_ALL_QUERY;
            }
            else
            {
                query = DBContract.users_entity_table.SELECT_ALL_FILTER_QUERY;
            }

            dt = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_users(showinactive, query, DBContract.sqlite);

            if (dt != null)
            {
                _users = utilzsingleton.getInstance(_notificationmessageEventname).Convert_DataTable_To_list<user_dto>(dt);
                _users = _users.OrderByDescending(i => i.id).ThenBy(t => t.fullnames).ToList();

                foreach (user_dto dto in _users)
                {
                    int year = int.Parse(dto.year);
                    int month = int.Parse(dto.month);
                    int day = int.Parse(dto.day);
                    DateTime dob = new DateTime(year, month, day);

                    dto.dob = dob.ToString("dd-MM-yyyy");
                }

                Console.WriteLine("Users count: " + _users.Count());
            }
            else
            {
                return null;
            }

            return _users;
        }

        private user_dto populate_model(user_dto model)
        {
            List<SelectListItem> genders = new List<SelectListItem>();
            List<SelectListItem> years = new List<SelectListItem>();
            List<SelectListItem> months = new List<SelectListItem>();
            List<SelectListItem> days = new List<SelectListItem>();

            Dictionary<string, string> genders_dic = new Dictionary<string, string>()
            { 
                {"Male","Male"},
                {"FeMale","FeMale"},
                {"Prefer Not To Say","Prefer Not To Say"}, 
            };

            SelectListItem gender_item = new SelectListItem()
            {
                Value = null,
                Text = "--- Select Gender ---"
            };

            foreach (KeyValuePair<string, string> gender in genders_dic)
            {
                SelectListItem sli = new SelectListItem()
                {
                    Value = gender.Key,
                    Text = gender.Value
                };
                genders.Add(sli);
            }

            genders.Insert(0, gender_item);

            model.genders = genders;

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

            model.years = years;

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

            model.months = months;

            int _year = DateTime.Now.Year;
            int _month = DateTime.Now.Month;

            days.Add(new SelectListItem { Value = null, Text = "--- Select Day ---" });

            for (int i = 1; i <= DateTime.DaysInMonth(_year, _month); i++)
            {
                days.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }

            model.days = days;

            return model;
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

        [HttpPost]
        public JsonResult search([Bind] user_dto search_model)
        {
            user_dto dto = new user_dto();
            users_view_model model = new users_view_model();
            DataTable dt = null;
            List<user_dto> _lst_dtos = new List<user_dto>();

            dt = businesslayerapisingleton.getInstance(_notificationmessageEventname).search_users_in_database(search_model);

            if (dt != null)
            {
                _lst_dtos = utilzsingleton.getInstance(_notificationmessageEventname).Convert_DataTable_To_list<user_dto>(dt);
                _lst_dtos = _lst_dtos.OrderByDescending(i => i.id).ThenBy(t => t.fullnames).ToList();

                Console.WriteLine("Users count: " + _lst_dtos.Count());
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

            model.lst_dto = _lst_dtos;
            model.dto = populate_model(dto);

            //Display the records
            return Json(model);

        }

        [HttpGet]
        public ActionResult login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login([Bind] user_dto user_model)
        {
            users_view_model model = new users_view_model();
            user_dto dto = new user_dto();

            dto = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_user_by_email(user_model.email);

            List<user_dto> _lst_dtos = new List<user_dto>();
            _lst_dtos = populate_dtos(_lst_dtos);

            foreach (user_dto user in _lst_dtos)
            {
                if (user.email == user_model.email)
                {
                    dto = user;
                }
            }

            if (dto == null)
            {
                Console.WriteLine("User with email [ " + user_model.email + " ] does not exist.");
                TempData["error_message"] = "User with email [ " + user_model.email + " ] does not exist.";
                helper_utils.getInstance(_notificationmessageEventname).log_messages("User with email [ " + user_model.email + " ] does not exist.", TAG);

                Session.Remove("admin");

                Session.Remove("loggedinuser");

                return View("login", user_model);
            }
            else
            {
                helper_utils.getInstance(_notificationmessageEventname).log_messages("User with email [ " + user_model.email + " ] exist.", TAG);

                string salt = dto.password_salt;
                string salted_password = salt + user_model.password;
                string password_salt_hash = Utils.get_SHA512_hash(salted_password);

                string hash_from_db = dto.password_hash;

                bool is_hash_equal = hash_from_db.Equals(password_salt_hash);

                if (is_hash_equal)
                {
                    Session.Add("admin", "admin");

                    Session.Add("loggedinuser", dto.fullnames);

                    Console.WriteLine("Password match, successfully logged in.");
                    TempData["success_message"] = "Password match, successfully logged in.";
                    helper_utils.getInstance(_notificationmessageEventname).log_messages("Password match, successfully logged in.", TAG);

                    return RedirectToAction("Index", "Missions");
                }
                else
                {
                    Console.WriteLine("Incorrect password.");
                    TempData["error_message"] = "Incorrect password.";
                    helper_utils.getInstance(_notificationmessageEventname).log_messages("Incorrect password.", TAG);

                }
            }

            return View("login", user_model);
        }
        private validation_dto validate_password(string password)
        {
            validation_dto dto = new validation_dto();

            bool isvalid = false;

            const int MIN_LENGTH = 8;
            //const int MAX_LENGTH = 15;

            if (password == null) throw new ArgumentNullException();

            //bool meets_length_requirements = password.Length >= MIN_LENGTH && password.Length <= MAX_LENGTH;
            bool meets_length_requirements = password.Length >= MIN_LENGTH;
            bool has_upper_case_letter = false;
            bool has_lower_case_letter = false;
            bool has_digit = false;
            bool has_symbol = false;

            if (meets_length_requirements)
            {
                foreach (char c in password)
                {
                    if (char.IsUpper(c)) has_upper_case_letter = true;
                    else if (char.IsLower(c)) has_lower_case_letter = true;
                    else if (char.IsDigit(c)) has_digit = true;
                    else if (char.IsSymbol(c)) has_symbol = true;
                }
                has_symbol = has_special_character(password);
            }

            if (!meets_length_requirements)
            {
                dto.message += " Password does not meet the required length of 8 characters.";
            }
            if (!has_upper_case_letter)
            {
                dto.message += " Password does not contain an uppercase letter.";
            }
            if (!has_lower_case_letter)
            {
                dto.message += " Password does not contain a lowercase letter.";
            }
            if (!has_digit)
            {
                dto.message += " Password does not contain a digit.";
            }
            if (!has_symbol)
            {
                dto.message += " Password does not contain a symbol.";
            }

            isvalid = meets_length_requirements && has_upper_case_letter && has_lower_case_letter && has_digit && has_symbol;
            //isvalid = meets_length_requirements && has_upper_case_letter && has_lower_case_letter && has_digit;

            dto.isvalid = isvalid;

            return dto;
        }

        public bool has_special_character(string input)
        {
            string special_char = @"\|!#$%&/()=?><@{}[].-;'_,";
            bool has_symbol = false;
            foreach (var item in special_char)
            {
                if (input.Contains(item))
                {
                    has_symbol = true;
                }
            }
            return has_symbol;
        }
        // GET: Users/Create
        public ActionResult Create()
        {
            user_dto model = new user_dto();
            model = populate_model(model);

            return View("add_user", model);
        }

        // POST: Users/Create
        [HttpPost]
        public ActionResult Create([Bind] user_dto model)
        {
            try
            {
                // TODO: Add insert logic here

                if (ModelState.IsValid)
                {
                    string message = string.Empty;
                    validation_dto validator = validate_password(model.password);
                    if (!validator.isvalid)
                    {
                        TempData["error_message"] = "Password must be 8 and above characters and must contain an upper case letter, lower case letter, a number and a symbol.";
                        TempData["validation_message"] = validator.message;
                        helper_utils.getInstance(_notificationmessageEventname).log_messages(validator.message, TAG);

                        model = populate_model(model);
                        return View("add_user", model);
                    }

                    user_dto _dto = new user_dto();

                    string salt = Utils.create_random_salt();
                    string salted_password = salt + model.password;
                    string password_salt_hash = Utils.get_SHA512_hash(salted_password);

                    _dto.password_hash = password_salt_hash;
                    _dto.password_salt = salt;
                    _dto.password = Utils.encrypt_string(model.password);
                    _dto.email = model.email;
                    _dto.fullnames = Utils.ConvertFirstLetterToUpper(model.fullnames);
                    _dto.year = model.year;
                    _dto.month = model.month;
                    _dto.day = model.day;
                    _dto.gender = model.gender;

                    //check if email exists.
                    user_dto model_exists = new user_dto();
                    model_exists = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_user_by_email(model.email);

                    if (model_exists != null)
                    {
                        TempData["error_message"] = "Email [ " + model.email + " ] Exists.";
                        helper_utils.getInstance(_notificationmessageEventname).log_messages("Email [ " + model.email + " ] Exists.", TAG);
                        model = populate_model(model);
                        return View("add_user", model);
                    }

                    //save data in database.
                    responsedto _responsedto = new responsedto();
                    _responsedto = businesslayerapisingleton.getInstance(_notificationmessageEventname).create_user_in_database(_dto);

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
                        return View("add_user", model);
                    }
                }
                else
                {
                    TempData["error_message"] = "Validation Error.";
                    helper_utils.getInstance(_notificationmessageEventname).log_messages("Validation Error.", TAG);
                    model = populate_model(model);
                    return View("add_user", model);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["error_message"] = ex.ToString();
                helper_utils.getInstance(_notificationmessageEventname).log_messages(ex.ToString(), TAG);
                model = populate_model(model);
                return View("add_user", model);
            }
        }
        // GET: Users/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Users/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Users/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Logout()
        {
            Session.Remove("admin");

            Session.Remove("loggedinuser");

            return RedirectToAction("login");
        }


    }
}
