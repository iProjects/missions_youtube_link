using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using missions_youtube_links.Entities;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using missions_youtube_links.Business;
using missions_youtube_links.Data;
using System.Data;

namespace missions_youtube_links.Test
{
    [TestClass]
    public class UnitTest
    {
        public string TAG;
        //Event declaration:
        //event for publishing messages to output
        public event EventHandler<notificationmessageEventArgs> _notificationmessageEventname;
        //list to hold messages
        public List<notificationdto> _lstnotificationdto = new List<notificationdto>();

        public UnitTest()
        {

            TAG = this.GetType().Name;

            //            AppDomain.CurrentDomain.UnhandledException += new
            //UnhandledExceptionEventHandler(UnhandledException);
            //            System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(ThreadException);

            //Subscribing to the event: 
            //Dynamically:
            //EventName += HandlerName;
            _notificationmessageEventname += notificationmessageHandler;

            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("finished UnitTest initialization", TAG));

        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Log.Write_To_Log_File_temp_dir(ex);
            _notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
        }

        private void ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            Log.Write_To_Log_File_temp_dir(ex);
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

        [TestMethod]
        public void TestMethod_get_missions()
        {
            try
            {
                DataTable dt = null;
                string query = "";
                List<missions_links_dto> _lst_dtos = new List<missions_links_dto>();
                bool showinactive = true;
                //KeyValuePair<string, string> selected_server = (KeyValuePair<string, string>)cboserver.SelectedItem;
                string server = "sqlite"; //selected_server.Key;

                if (showinactive)
                {
                    query = DBContract.MISSIONS_SELECT_ALL_QUERY;
                }
                else
                {
                    query = DBContract.MISSIONS_SELECT_ALL_FILTER_QUERY;
                }

                dt = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_missions(showinactive, query, DBContract.sqlite);

                if (dt != null)
                {
                    _lst_dtos = utilzsingleton.getInstance(_notificationmessageEventname).Convert_DataTable_To_list<missions_links_dto>(dt);
                    _lst_dtos = _lst_dtos.OrderByDescending(i => i.id).ThenBy(t => t.description).ToList();

                    Console.WriteLine("Missions count: " + _lst_dtos.Count());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void TestMethod_create_missions()
        {
            try
            {
                missions_links_dto dto = new missions_links_dto();
                dto.youtube_url = "http://www.youtube.com/tyryrtr65757ty";
                dto.description = "Menengai 7 Healing Service";
                dto.mission_type = "Healing Service";
                dto.mission_year = "2024";
                dto.mission_month = "12";
                dto.mission_day = "30";
                dto.status = "active";
                dto.created_date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt");

                responsedto _responsedto = new responsedto();
                _responsedto = businesslayerapisingleton.getInstance(_notificationmessageEventname).create_mission_in_database(dto);

                if (_responsedto.isresponseresultsuccessful)
                {
                    Console.WriteLine(_responsedto.responsesuccessmessage);
                }
                else
                {
                    Console.WriteLine(_responsedto.responseerrormessage);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void TestMethod_get_users()
        {
            try
            {
                DataTable dt = null;
                string query = "";
                List<user_dto> _lst_dtos = new List<user_dto>();
                bool showinactive = true;
                //KeyValuePair<string, string> selected_server = (KeyValuePair<string, string>)cboserver.SelectedItem;
                string server = "sqlite"; //selected_server.Key;

                if (showinactive)
                {
                    query = DBContract.USERS_SELECT_ALL_QUERY;
                }
                else
                {
                    query = DBContract.USERS_SELECT_ALL_FILTER_QUERY;
                }

                dt = businesslayerapisingleton.getInstance(_notificationmessageEventname).get_users(showinactive, query, DBContract.sqlite);

                if (dt != null)
                {
                    _lst_dtos = utilzsingleton.getInstance(_notificationmessageEventname).Convert_DataTable_To_list<user_dto>(dt);
                    _lst_dtos = _lst_dtos.OrderByDescending(i => i.id).ThenBy(t => t.fullnames).ToList();

                    Console.WriteLine("Users count: " + _lst_dtos.Count());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void TestMethod_create_user()
        {
            try
            {
                user_dto dto = new user_dto();
                dto.email = "kevinmk30@gmail.com";
                dto.password = "ObraVoke@27809543";
                dto.fullnames = "Kevin Mutugi Kithinji";
                dto.year = "1989";
                dto.month = "11";
                dto.day = "08";
                dto.gender = "Male"; 
                dto.status = "active";
                dto.created_date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt");


                string salt = Utils.create_random_salt();
                string salted_password = salt + dto.password;
                string password_salt_hash = Utils.get_SHA512_hash(salted_password);

                dto.password_hash = password_salt_hash;
                dto.password_salt = salt;

                dto.password = Utils.encrypt_string(dto.password);


                responsedto _responsedto = new responsedto();
                _responsedto = businesslayerapisingleton.getInstance(_notificationmessageEventname).create_user_in_database(dto);

                if (_responsedto.isresponseresultsuccessful)
                {
                    Console.WriteLine(_responsedto.responsesuccessmessage);
                }
                else
                {
                    Console.WriteLine(_responsedto.responseerrormessage);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



    }
}
