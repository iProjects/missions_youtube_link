using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using missions_youtube_links.Entities;
using System.Web;

namespace missions_youtube_links.Data
{
    public sealed class sqliteapisingleton
    {
        // Because the _instance member is made private, the only way to get the single
        // instance is via the static Instance property below. This can also be similarly
        // achieved with a GetInstance() method instead of the property.
        private static sqliteapisingleton singleInstance;

        public static sqliteapisingleton getInstance(EventHandler<notificationmessageEventArgs> notificationmessageEventname)
        {
            // The first call will create the one and only instance.
            if (singleInstance == null)
                singleInstance = new sqliteapisingleton(notificationmessageEventname);
            // Every call afterwards will return the single instance created above.
            return singleInstance;
        }

        private string CONNECTION_STRING = @"Data Source=missions_links_db.sqlite3;Pooling=true;FailIfMissing=false";
        private const string db_name = "july";//DBContract.DATABASE_NAME;
        private event EventHandler<notificationmessageEventArgs> _notificationmessageEventname;
        private event EventHandler<notificationmessageEventArgs> _databaseutilsnotificationeventname;
        private string TAG;

        // Holds our connection with the database
        SQLiteConnection m_dbConnection;

        private sqliteapisingleton(EventHandler<notificationmessageEventArgs> notificationmessageEventname)
        {
            _notificationmessageEventname = notificationmessageEventname;
            try
            {
                TAG = this.GetType().Name;
                createdatabaseonfirstload();
                createtablesonfirstload();
                createconnectionstring();
                setconnectionstring();
            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
            }
        }

        private sqliteapisingleton()
        {

        }
        public responsedto setup_database()
        {
            responsedto _responsedto = new responsedto();
            try
            {
                responsedto _db_responsedto = createdatabaseonfirstload();
                responsedto _table_responsedto = createtablesonfirstload();
                createconnectionstring();
                setconnectionstring();

                if (_db_responsedto.responsesuccessmessage != null && _db_responsedto.isresponseresultsuccessful)
                {
                    _responsedto.responsesuccessmessage += (Environment.NewLine + _db_responsedto.responsesuccessmessage);
                }
                else if (_db_responsedto.responseerrormessage != null)
                {
                    _responsedto.responseerrormessage += (Environment.NewLine + _db_responsedto.responseerrormessage);
                }

                if (_table_responsedto.responsesuccessmessage != null && _table_responsedto.isresponseresultsuccessful)
                {
                    _responsedto.responsesuccessmessage += (Environment.NewLine + _table_responsedto.responsesuccessmessage);
                }
                else if (_table_responsedto.responseerrormessage != null)
                {
                    _responsedto.responseerrormessage += (Environment.NewLine + _table_responsedto.responseerrormessage);
                }

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage += ex.ToString();
            }

            return _responsedto;
        }
        private void createconnectionstring()
        {
            try
            {
                CONNECTION_STRING = setconnectionstring();
            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
            }
        }

        private string setconnectionstring()
        {
            try
            {
                sqliteconnectionstringdto _connectionstringdto = new sqliteconnectionstringdto();

                _connectionstringdto.sqlite_database_path = System.Configuration.ConfigurationManager.AppSettings["sqlite_database_path"];
                _connectionstringdto.database = System.Configuration.ConfigurationManager.AppSettings["sqlite_database"];
                _connectionstringdto.sqlite_db_extension = System.Configuration.ConfigurationManager.AppSettings["sqlite_db_extension"];
                _connectionstringdto.sqlite_version = System.Configuration.ConfigurationManager.AppSettings["sqlite_version"];
                _connectionstringdto.sqlite_pooling = System.Configuration.ConfigurationManager.AppSettings["sqlite_pooling"];
                _connectionstringdto.sqlite_fail_if_missing = System.Configuration.ConfigurationManager.AppSettings["sqlite_fail_if_missing"];

                CONNECTION_STRING = buildconnectionstringfromobject(_connectionstringdto);

                return CONNECTION_STRING;

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                return "";
            }
        }

        string buildconnectionstringfromobject(sqliteconnectionstringdto _connectionstringdto)
        {
            //string base_dir = AppDomain.CurrentDomain.BaseDirectory;
            string base_dir = System.Web.HttpRuntime.AppDomainAppPath;
            string database_dir = Path.Combine(base_dir, _connectionstringdto.sqlite_database_path);

            string plain_dbname = _connectionstringdto.database;
            string database_version = _connectionstringdto.sqlite_version;
            string db_extension = _connectionstringdto.sqlite_db_extension;
            string dbname = plain_dbname + "." + db_extension + database_version;

            if (!Directory.Exists(database_dir))
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("sqlite datastore path with name [ " + database_dir + " ] does not exist.", TAG));
            }
            else
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("sqlite datastore path with name [ " + _connectionstringdto.sqlite_database_path + " ] exist.", TAG));
            }

            string full_database_name_with_path = Path.Combine(database_dir, dbname);
            string _secure_path_name_response = dbname;

            if (!File.Exists(full_database_name_with_path))
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("sqlite database with name [ " + _secure_path_name_response + " ] does not exist.", TAG));
            }
            else
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("sqlite database with name [ " + _secure_path_name_response + " ] exist.", TAG));
            }

            string CONNECTION_STRING = @"Data Source=" + full_database_name_with_path + ";" +
            "Version=" + _connectionstringdto.sqlite_version + ";" +
            "Pooling=" + _connectionstringdto.sqlite_pooling + ";" +
            "FailIfMissing=" + _connectionstringdto.sqlite_fail_if_missing;

            return CONNECTION_STRING;
        }

        // Creates a connection with our database file.
        public void connectToDatabase()
        {
            try
            {
                CONNECTION_STRING = setconnectionstring();
                m_dbConnection = new SQLiteConnection(CONNECTION_STRING);
                m_dbConnection.Open();
            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
            }
        }

        private responsedto createdatabaseonfirstload()
        {
            sqliteconnectionstringdto _connectionstringdto = new sqliteconnectionstringdto();

            _connectionstringdto.sqlite_database_path = System.Configuration.ConfigurationManager.AppSettings["sqlite_database_path"];
            _connectionstringdto.database = System.Configuration.ConfigurationManager.AppSettings["sqlite_database"];
            _connectionstringdto.new_database_name = System.Configuration.ConfigurationManager.AppSettings["sqlite_database"];
            _connectionstringdto.sqlite_db_extension = System.Configuration.ConfigurationManager.AppSettings["sqlite_db_extension"];
            _connectionstringdto.sqlite_version = System.Configuration.ConfigurationManager.AppSettings["sqlite_version"];
            _connectionstringdto.sqlite_pooling = System.Configuration.ConfigurationManager.AppSettings["sqlite_pooling"];
            _connectionstringdto.sqlite_fail_if_missing = System.Configuration.ConfigurationManager.AppSettings["sqlite_fail_if_missing"];

            responsedto _responsedto = createdatabasegivenname(_connectionstringdto);
            return _responsedto;
        }
        private responsedto createtablesonfirstload()
        {
            sqliteconnectionstringdto _connectionstringdto = new sqliteconnectionstringdto();

            _connectionstringdto.sqlite_database_path = System.Configuration.ConfigurationManager.AppSettings["sqlite_database_path"];
            _connectionstringdto.database = System.Configuration.ConfigurationManager.AppSettings["sqlite_database"];
            _connectionstringdto.new_database_name = System.Configuration.ConfigurationManager.AppSettings["sqlite_database"];
            _connectionstringdto.sqlite_db_extension = System.Configuration.ConfigurationManager.AppSettings["sqlite_db_extension"];
            _connectionstringdto.sqlite_version = System.Configuration.ConfigurationManager.AppSettings["sqlite_version"];
            _connectionstringdto.sqlite_pooling = System.Configuration.ConfigurationManager.AppSettings["sqlite_pooling"];
            _connectionstringdto.sqlite_fail_if_missing = System.Configuration.ConfigurationManager.AppSettings["sqlite_fail_if_missing"];

            responsedto _responsedto = createtables(_connectionstringdto);
            return _responsedto;
        }
        public responsedto createdatabasegivenname(sqliteconnectionstringdto _connectionstringdto)
        {
            responsedto _responsedto = new responsedto();
            try
            {
                //string base_dir = AppDomain.CurrentDomain.BaseDirectory;
                string base_dir = System.Web.HttpRuntime.AppDomainAppPath;
                string database_dir = Path.Combine(base_dir, _connectionstringdto.sqlite_database_path);

                string new_database_name = _connectionstringdto.new_database_name;
                string database_version = _connectionstringdto.sqlite_version;
                string db_extension = _connectionstringdto.sqlite_db_extension;
                string dbname = new_database_name + "." + db_extension + database_version;

                if (!Directory.Exists(database_dir))
                {
                    _responsedto.responsesuccessmessage += "\nsqlite datastore path with name [ " + database_dir + " ] does not exist.";
                    _responsedto.responsesuccessmessage += "\n creating path...";

                    Directory.CreateDirectory(database_dir);

                    _responsedto.responsesuccessmessage += "\ncreated sqlite datastore path with name [ " + database_dir + " ].";
                }
                else
                {
                    _responsedto.responsesuccessmessage += "\nsqlite datastore path with name [ " + _connectionstringdto.sqlite_database_path + " ] exist.";
                }

                string full_database_name_with_path = Path.Combine(database_dir, dbname);
                string _secure_path_name_response = dbname;

                if (!File.Exists(full_database_name_with_path))
                {
                    _responsedto.responsesuccessmessage += "\nsqlite database with name [ " + _secure_path_name_response + " ] does not exist.";
                    _responsedto.responsesuccessmessage += "\n creating database...";

                    SQLiteConnection.CreateFile(full_database_name_with_path);

                    _responsedto.responsesuccessmessage += "\nsuccessfully created database [ " + _secure_path_name_response + " ] in sqlite.";
                }
                else
                {
                    _responsedto.responsesuccessmessage += "\nsqlite database with name [ " + _secure_path_name_response + " ] exist.";
                }

                _responsedto.isresponseresultsuccessful = true;
                return _responsedto;

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }
        public responsedto createdatabase()
        {
            responsedto _responsedto = new responsedto();
            try
            {
                string _default_db_path = utilzsingleton.getInstance(_notificationmessageEventname).getappsettinggivenkey("sqlite_database_path", @"\databases\");
                string dbname = utilzsingleton.getInstance(_notificationmessageEventname).getappsettinggivenkey("sqlite_database", "missions_links_db");
                string database_version = utilzsingleton.getInstance(_notificationmessageEventname).getappsettinggivenkey("sqlite_version", "3");
                string db_extension = utilzsingleton.getInstance(_notificationmessageEventname).getappsettinggivenkey("sqlite_db_extension", "sqlite");
                dbname = dbname + "." + db_extension + database_version;

                //string base_dir = AppDomain.CurrentDomain.BaseDirectory;
                string base_dir = System.Web.HttpRuntime.AppDomainAppPath;

                string database_dir = Path.Combine(base_dir, _default_db_path);


                if (!Directory.Exists(database_dir))
                {
                    Directory.CreateDirectory(database_dir);
                }

                string full_database_name_with_path = Path.Combine(database_dir, dbname);
                string _secure_path_name_response = dbname;

                if (!File.Exists(full_database_name_with_path))
                {
                    SQLiteConnection.CreateFile(full_database_name_with_path);
                    _responsedto.isresponseresultsuccessful = true;
                    _responsedto.responsesuccessmessage = "successfully created database [ " + _secure_path_name_response + " ] in sqlite.";
                    return _responsedto;
                }
                else
                {
                    _responsedto.isresponseresultsuccessful = false;
                    _responsedto.responseerrormessage = "sqlite datastore with name [ " + _secure_path_name_response + " ] exists.";
                    return _responsedto;
                }
            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }
        public responsedto createtables(sqliteconnectionstringdto _connectionstringdto)
        {
            responsedto _responsedto = new responsedto();
            responsedto _innerresponsedto = new responsedto();
            try
            {

                _connectionstringdto.database = _connectionstringdto.new_database_name;
                string CONNECTION_STRING = buildconnectionstringfromobject(_connectionstringdto);

                bool does_table_exist_in_db = checkiftableexists(CONNECTION_STRING, DBContract.missions_links_entity_table.TABLE_NAME);

                //Create missions table 
                string SQL_CREATE_MISSIONS_TABLE = " CREATE TABLE IF NOT EXISTS " + DBContract.missions_links_entity_table.TABLE_NAME + " (" +
                      DBContract.missions_links_entity_table.ID + " INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                      DBContract.missions_links_entity_table.YOUTUBE_URL + " TEXT, " +
                      DBContract.missions_links_entity_table.DESCRIPTION + " TEXT, " +
                      DBContract.missions_links_entity_table.LOCATION + " TEXT, " +
                      DBContract.missions_links_entity_table.MISSION_TYPE + " TEXT, " +
                      DBContract.missions_links_entity_table.MISSION_YEAR + " TEXT, " +
                      DBContract.missions_links_entity_table.MISSION_MONTH + " TEXT, " +
                      DBContract.missions_links_entity_table.MISSION_DAY + " TEXT, " +
                      DBContract.missions_links_entity_table.STATUS + " TEXT, " +
                      DBContract.missions_links_entity_table.CREATED_DATE + " TEXT " +
                       " ); ";

                _innerresponsedto = createtable(SQL_CREATE_MISSIONS_TABLE, CONNECTION_STRING);
                if (_innerresponsedto.isresponseresultsuccessful)
                    _responsedto.responsesuccessmessage += _innerresponsedto.responsesuccessmessage;
                else
                    _responsedto.responseerrormessage += _innerresponsedto.responseerrormessage;

                //Create users table 
                string SQL_CREATE_USERS_TABLE = " CREATE TABLE IF NOT EXISTS " + DBContract.users_entity_table.TABLE_NAME + " (" +
                      DBContract.users_entity_table.ID + " INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                      DBContract.users_entity_table.EMAIL + " TEXT, " +
                      DBContract.users_entity_table.PASSWORD + " TEXT, " +
                      DBContract.users_entity_table.PASSWORD_SALT + " TEXT, " +
                      DBContract.users_entity_table.PASSWORD_HASH + " TEXT, " +
                      DBContract.users_entity_table.FULLNAMES + " TEXT, " +
                      DBContract.users_entity_table.YEAR + " TEXT, " +
                      DBContract.users_entity_table.MONTH + " TEXT, " +
                      DBContract.users_entity_table.DAY + " TEXT, " +
                      DBContract.users_entity_table.GENDER + " TEXT, " +
                      DBContract.users_entity_table.STATUS + " TEXT, " +
                      DBContract.users_entity_table.CREATED_DATE + " TEXT " +
                       " ); ";

                _innerresponsedto = createtable(SQL_CREATE_USERS_TABLE, CONNECTION_STRING);
                if (_innerresponsedto.isresponseresultsuccessful)
                    _responsedto.responsesuccessmessage += _innerresponsedto.responsesuccessmessage;
                else
                    _responsedto.responseerrormessage += _innerresponsedto.responseerrormessage;

                //Create logs table 
                string SQL_CREATE_LOGS_TABLE = " CREATE TABLE IF NOT EXISTS " + DBContract.logs_entity_table.TABLE_NAME + " (" +
                      DBContract.logs_entity_table.ID + " INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                      DBContract.logs_entity_table.MESSAGE + " TEXT, " +
                      DBContract.logs_entity_table.TIMESTAMP + " TEXT, " +
                      DBContract.logs_entity_table.TAG + " TEXT, " +
                      DBContract.logs_entity_table.STATUS + " TEXT, " +
                      DBContract.logs_entity_table.CREATED_DATE + " TEXT " +
                       " ); ";

                _innerresponsedto = createtable(SQL_CREATE_LOGS_TABLE, CONNECTION_STRING);
                if (_innerresponsedto.isresponseresultsuccessful)
                    _responsedto.responsesuccessmessage += _innerresponsedto.responsesuccessmessage;
                else
                    _responsedto.responseerrormessage += _innerresponsedto.responseerrormessage;

                string successmsg = "successfully created tables in database [ " + _connectionstringdto.database + " ] - server [ " + DBContract.sqlite + " ].";
                int msg_length = successmsg.Length;
                msg_length = msg_length + 1;
                int stars_printed = 0;
                string str_stars = "";
                string str_newline = Environment.NewLine;

                while (stars_printed != msg_length)
                {
                    str_stars += "*";
                    ++stars_printed;
                }

                _responsedto.responsesuccessmessage += str_newline;
                _responsedto.responsesuccessmessage += str_stars;
                _responsedto.responsesuccessmessage += str_newline;
                _responsedto.responsesuccessmessage += successmsg;
                _responsedto.responsesuccessmessage += str_newline;
                _responsedto.responsesuccessmessage += str_newline;
                _responsedto.responsesuccessmessage += str_stars;

                _responsedto.isresponseresultsuccessful = true;
                return _responsedto;

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage += Environment.NewLine + ex.Message;
                return _responsedto;
            }
        }

        bool checkiftableexists(string CONNECTION_STRING, string table_name)
        {
            try
            {
                string query = "SELECT EXISTS (SELECT name FROM sqlite_master WHERE type='table' AND name = '" + table_name + "')";

                using (var con = new SQLiteConnection(CONNECTION_STRING))
                {
                    con.Open();
                    //open a new command
                    using (var cmd = new SQLiteCommand(query, con))
                    {
                        //execute the query  
                        int _rows_affected = cmd.ExecuteNonQuery();
                        Console.WriteLine("_rows_affected [ " + _rows_affected + " ]");

                        var da = new SQLiteDataAdapter(cmd);
                        var dt = new DataTable();
                        da.Fill(dt);
                        da.Dispose();

                        int _rows_count = dt.Rows.Count;
                        if (_rows_count > 0) return true;
                        else return false;
                    }
                }

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                return false;
            }
        }

        public responsedto createtable(string query, string CONNECTION_STRING)
        {
            responsedto _responsedto = new responsedto();
            try
            {
                using (var con = new SQLiteConnection(CONNECTION_STRING))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(query, con))
                    {
                        cmd.ExecuteNonQuery();
                        _responsedto.isresponseresultsuccessful = true;
                        string[] _conn_arr = CONNECTION_STRING.Split(new char[] { ';' });
                        _conn_arr.SetValue("", _conn_arr.Length - 1);
                        _conn_arr.SetValue("", _conn_arr.Length - 2);
                        string _sanitized_conn_arr = _conn_arr[0] + ";" + _conn_arr[1];
                        _responsedto.responsesuccessmessage += "successfully executed query [ " + query + " ] against connection [ " + _sanitized_conn_arr + " ]." + Environment.NewLine;
                        return _responsedto;
                    }
                }
            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage += Environment.NewLine + "Error executing query [ " + query + " ].";
                _responsedto.responseerrormessage += Environment.NewLine + Environment.NewLine + ex.Message + Environment.NewLine;
                return _responsedto;
            }
        }

        public responsedto checksqliteconnectionstate()
        {
            responsedto _responsedto = new responsedto();
            try
            {
                sqliteconnectionstringdto _connectionstringdto = getsqliteconnectionstringdto();

                _responsedto = checkconnectionasadmin(_connectionstringdto, _databaseutilsnotificationeventname);

                return _responsedto;
            }
            catch (Exception ex)
            {
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }

        public sqliteconnectionstringdto getsqliteconnectionstringdto()
        {
            try
            {
                sqliteconnectionstringdto _connectionstringdto = new sqliteconnectionstringdto();

                _connectionstringdto.sqlite_database_path = System.Configuration.ConfigurationManager.AppSettings["sqlite_database_path"];
                _connectionstringdto.database = System.Configuration.ConfigurationManager.AppSettings["sqlite_database"];
                _connectionstringdto.sqlite_db_extension = System.Configuration.ConfigurationManager.AppSettings["sqlite_db_extension"];
                _connectionstringdto.sqlite_version = System.Configuration.ConfigurationManager.AppSettings["sqlite_version"];
                _connectionstringdto.sqlite_pooling = System.Configuration.ConfigurationManager.AppSettings["sqlite_pooling"];
                _connectionstringdto.sqlite_fail_if_missing = System.Configuration.ConfigurationManager.AppSettings["sqlite_fail_if_missing"];

                return _connectionstringdto;

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                return null;
            }
        }

        public responsedto checkconnectionasadmin(sqliteconnectionstringdto _connectionstringdto, EventHandler<notificationmessageEventArgs> databaseutilsnotificationeventname)
        {
            _databaseutilsnotificationeventname = databaseutilsnotificationeventname;
            responsedto _responsedto = new responsedto();
            try
            {
                string base_dir = Environment.CurrentDirectory;
                string database_dir = base_dir + _connectionstringdto.sqlite_database_path;
                string dbname = _connectionstringdto.database;
                string database_version = _connectionstringdto.sqlite_version;
                string db_extension = _connectionstringdto.sqlite_db_extension;
                dbname = dbname + "." + db_extension + database_version;

                if (!Directory.Exists(database_dir))
                {
                    _responsedto.isresponseresultsuccessful = false;
                    _responsedto.responseerrormessage = "sqlite datastore path with name [ " + database_dir + " ] does not exist.";
                    return _responsedto;
                }
                else
                {
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("sqlite datastore path with name [ " + _connectionstringdto.sqlite_database_path + " ] exist.", TAG));

                    //_databaseutilsnotificationeventname.Invoke(this, new notificationmessageEventArgs("sqlite datastore path with name [ " + _connectionstringdto.sqlite_database_path + " ] exist.", TAG));
                }

                string full_database_name_with_path = database_dir + dbname;
                string _secure_path_name_response = _connectionstringdto.sqlite_database_path + dbname;

                if (!File.Exists(full_database_name_with_path))
                {
                    _responsedto.isresponseresultsuccessful = false;
                    _responsedto.responseerrormessage = "sqlite database with name [ " + _secure_path_name_response + " ] does not exist.";
                    return _responsedto;
                }
                else
                {
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs("sqlite database with name [ " + _secure_path_name_response + " ] exist.", TAG));

                    //_databaseutilsnotificationeventname.Invoke(this, new notificationmessageEventArgs("sqlite database with name [ " + _secure_path_name_response + " ] exist.", TAG));
                }

                string CONNECTION_STRING = @"Data Source=" + full_database_name_with_path + ";" +
                "Version=" + _connectionstringdto.sqlite_version + ";" +
                "Pooling=" + _connectionstringdto.sqlite_pooling + ";" +
                "FailIfMissing=" + _connectionstringdto.sqlite_fail_if_missing;

                string query = DBContract.missions_links_entity_table.SELECT_ALL_QUERY;

                int count = getrecordscountgiventable(DBContract.missions_links_entity_table.TABLE_NAME, CONNECTION_STRING);

                if (count != -1)
                {
                    _responsedto.isresponseresultsuccessful = true;
                    _responsedto.responsesuccessmessage = "connection to sqlite successfull. Records count [ " + count + " ].";
                    return _responsedto;
                }
                else
                {
                    _responsedto.isresponseresultsuccessful = false;
                    _responsedto.responseerrormessage = "connection to sqlite failed.";
                    return _responsedto;
                }
            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.ToString(), TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }

        public bool isdbconnectionalive(string CONNECTION_STRING)
        {
            var con = new SQLiteConnection(CONNECTION_STRING);
            con.Open();
            return true;
        }

        public bool isdbconnectionalive()
        {
            try
            {
                //setup the connection to the database
                var con = new SQLiteConnection(CONNECTION_STRING);
                con.Open();
                return true;
            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                return false;
            }
        }

        public int getrecordscountgiventable(string tablename, string CONNECTION_STRING)
        {
            string query = "SELECT * FROM " + tablename;
            DataTable dt = getallrecordsglobal(query, CONNECTION_STRING);
            if (dt != null)
                return dt.Rows.Count;
            else
                return -1;
        }

        public DataTable getallrecordsglobal(string query)
        {
            if (!isdbconnectionalive()) return null;

            if (string.IsNullOrEmpty(query.Trim()))
                return null;
            using (var con = new SQLiteConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con))
                {
                    var da = new SQLiteDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    da.Dispose();
                    return dt;
                }
            }
        }

        public DataTable getallrecordsglobal(string query, string CONNECTION_STRING)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;
            using (var con = new SQLiteConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con))
                {
                    var da = new SQLiteDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    da.Dispose();
                    return dt;
                }
            }
        }
        public DataTable getallrecordsglobal()
        {
            DataTable dt = getallrecordsglobal(DBContract.missions_links_entity_table.SELECT_ALL_QUERY);
            return dt;
        }

        public List<missions_links_dto> get_all_missions()
        {
            List<missions_links_dto> lst_dtos = new List<missions_links_dto>();
            DataTable dt = getallrecordsglobal();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                missions_links_dto record_dto = utilzsingleton.getInstance(_notificationmessageEventname).build_missions_dto_given_datatable(dt, i);
                lst_dtos.Add(record_dto);
            }
            return lst_dtos;
        }

        public int insertgeneric(string query, Dictionary<string, object> args)
        {
            int numberOfRowsAffected;
            //setup the connection to the database
            using (var con = new SQLiteConnection(CONNECTION_STRING))
            {
                con.Open();
                //open a new command
                using (var cmd = new SQLiteCommand(query, con))
                {
                    //set the arguments given in the query
                    foreach (var pair in args)
                    {
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }
                    //execute the query and get the number of row affected
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }
                return numberOfRowsAffected;
            }
        }

        public int insertgeneric(string query, Dictionary<string, object> args, string CONNECTION_STRING)
        {
            int numberOfRowsAffected;
            if (CONNECTION_STRING == null)
            {
                numberOfRowsAffected = insertgeneric(query, args);
                return numberOfRowsAffected;
            }
            else if (String.IsNullOrEmpty(CONNECTION_STRING))
            {
                numberOfRowsAffected = insertgeneric(query, args);
                return numberOfRowsAffected;
            }
            else
            {
                //setup the connection to the database
                using (var con = new SQLiteConnection(CONNECTION_STRING))
                {
                    con.Open();
                    //open a new command
                    using (var cmd = new SQLiteCommand(query, con))
                    {
                        //set the arguments given in the query
                        foreach (var pair in args)
                        {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                        //execute the query and get the number of row affected
                        numberOfRowsAffected = cmd.ExecuteNonQuery();
                    }
                    return numberOfRowsAffected;
                }
            }
        }

        private DataTable ExecuteRead(string query, Dictionary<string, object> args)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;
            using (var con = new SQLiteConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con))
                {
                    foreach (KeyValuePair<string, object> entry in args)
                    {
                        cmd.Parameters.AddWithValue(entry.Key, "%" + entry.Value + "%");
                    }
                    var da = new SQLiteDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    da.Dispose();
                    return dt;
                }
            }
        }

        private DataTable ExecuteRead(string query, Dictionary<string, object> args, string CONNECTION_STRING)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;
            using (var con = new SQLiteConnection(CONNECTION_STRING))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con))
                {
                    foreach (KeyValuePair<string, object> entry in args)
                    {
                        cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                    }
                    var da = new SQLiteDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    da.Dispose();
                    return dt;
                }
            }
        }

        public int deletegeneric(string query, Dictionary<string, object> args)
        {
            int numberOfRowsAffected;
            //setup the connection to the database
            using (var con = new SQLiteConnection(CONNECTION_STRING))
            {
                con.Open();
                //open a new command
                using (var cmd = new SQLiteCommand(query, con))
                {
                    //set the arguments given in the query
                    foreach (var pair in args)
                    {
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }
                    //execute the query and get the number of row affected
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }
                return numberOfRowsAffected;
            }
        }

        public int updategeneric(string query, Dictionary<string, object> args)
        {
            int numberOfRowsAffected;
            //setup the connection to the database
            using (var con = new SQLiteConnection(CONNECTION_STRING))
            {
                con.Open();
                //open a new command
                using (var cmd = new SQLiteCommand(query, con))
                {
                    //set the arguments given in the query
                    foreach (var pair in args)
                    {
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }
                    //execute the query and get the number of row affected
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }
                return numberOfRowsAffected;
            }
        }

        #region "missions"
        public responsedto create_mission_in_database(missions_links_dto _dto)
        {
            responsedto _responsedto = new responsedto();
            try
            {
                string query = "INSERT INTO " +
                DBContract.missions_links_entity_table.TABLE_NAME +
                " ( " +
                DBContract.missions_links_entity_table.YOUTUBE_URL + ", " +
                DBContract.missions_links_entity_table.DESCRIPTION + ", " +
                DBContract.missions_links_entity_table.LOCATION + ", " +
                DBContract.missions_links_entity_table.MISSION_TYPE + ", " +
                DBContract.missions_links_entity_table.MISSION_YEAR + ", " +
                DBContract.missions_links_entity_table.MISSION_MONTH + ", " +
                DBContract.missions_links_entity_table.MISSION_DAY + ", " +
                DBContract.missions_links_entity_table.STATUS + ", " +
                DBContract.missions_links_entity_table.CREATED_DATE +
                " ) VALUES(@youtube_url, @description, @location, @mission_type, @mission_year, @mission_month, @mission_day, @status, @created_date)";

                //here we are setting the parameter values that will be actually replaced in the query in Execute method
                var args = new Dictionary<string, object>
			    {
				    {"@youtube_url", _dto.youtube_url},
				    {"@description", _dto.description},
                    {"@location", _dto.location},
                    {"@mission_type", _dto.mission_type},
                    {"@mission_year", _dto.mission_year},
                    {"@mission_month", _dto.mission_month},
                    {"@mission_day", _dto.mission_day},
                    {"@status", "active"},
				    {"@created_date", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt")}
			    };

                int numberOfRowsAffected = insertgeneric(query, args, CONNECTION_STRING);
                if (numberOfRowsAffected != 1)
                {
                    _responsedto.isresponseresultsuccessful = false;
                    //_responsedto.responseerrormessage = "Record creation failed in " + DBContract.sqlite + ".";
                    _responsedto.responseerrormessage = "Record creation failed.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responseerrormessage, TAG));
                    return _responsedto;
                }
                else
                {
                    _responsedto.isresponseresultsuccessful = true;
                    //_responsedto.responsesuccessmessage = "Record created successfully in " + DBContract.sqlite + ".";
                    _responsedto.responsesuccessmessage = "Record created successfully.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responsesuccessmessage, TAG));
                    return _responsedto;
                }

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }
        public missions_links_dto get_mission_by_id(int id)
        {
            try
            {
                var query = "SELECT * FROM " +
                    DBContract.missions_links_entity_table.TABLE_NAME +
                    " WHERE " +
                    DBContract.missions_links_entity_table.ID +
                    " = " +
                    "@id";

                var args = new Dictionary<string, object>
				{
					{"@id", id}
				};

                DataTable dt = ExecuteRead(query, args);
                if (dt == null || dt.Rows.Count == 0)
                {
                    return null;
                }

                missions_links_dto _dto = new missions_links_dto();
                _dto.id = Convert.ToInt64(dt.Rows[0]["id"]);
                _dto.youtube_url = Convert.ToString(dt.Rows[0]["youtube_url"]);
                _dto.description = Convert.ToString(dt.Rows[0]["description"]);
                _dto.location = Convert.ToString(dt.Rows[0]["location"]);
                _dto.mission_type = Convert.ToString(dt.Rows[0]["mission_type"]);
                _dto.mission_year = Convert.ToString(dt.Rows[0]["mission_year"]);
                _dto.mission_month = Convert.ToString(dt.Rows[0]["mission_month"]);
                _dto.mission_day = Convert.ToString(dt.Rows[0]["mission_day"]);
                _dto.status = Convert.ToString(dt.Rows[0]["status"]);
                _dto.created_date = Convert.ToString(dt.Rows[0]["created_date"]);

                return _dto;

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                return null;
            }
        }
        public missions_links_dto get_mission_by_url(string youtube_url)
        {
            try
            {

                var query = "SELECT * FROM " +
                    DBContract.missions_links_entity_table.TABLE_NAME +
                    " WHERE " +
                    DBContract.missions_links_entity_table.YOUTUBE_URL +
                    " = " +
                    "@youtube_url";

                var args = new Dictionary<string, object>
				{
					{"@youtube_url", youtube_url}
				};

                DataTable dt = ExecuteRead(query, args);
                if (dt == null || dt.Rows.Count == 0)
                {
                    return null;
                }

                missions_links_dto _dto = new missions_links_dto();
                _dto.id = Convert.ToInt64(dt.Rows[0]["id"]);
                _dto.youtube_url = Convert.ToString(dt.Rows[0]["youtube_url"]);
                _dto.description = Convert.ToString(dt.Rows[0]["description"]);
                _dto.location = Convert.ToString(dt.Rows[0]["location"]);
                _dto.mission_type = Convert.ToString(dt.Rows[0]["mission_type"]);
                _dto.mission_year = Convert.ToString(dt.Rows[0]["mission_year"]);
                _dto.mission_month = Convert.ToString(dt.Rows[0]["mission_month"]);
                _dto.mission_day = Convert.ToString(dt.Rows[0]["mission_day"]);
                _dto.status = Convert.ToString(dt.Rows[0]["status"]);
                _dto.created_date = Convert.ToString(dt.Rows[0]["created_date"]);

                return _dto;

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                return null;
            }
        }
        public responsedto update_mission_in_database(missions_links_dto _dto)
        {
            responsedto _responsedto = new responsedto();
            try
            {

                string query = "UPDATE " +
                DBContract.missions_links_entity_table.TABLE_NAME +
                " SET " +
                "youtube_url = @youtube_url, " +
                "description = @description, " +
                "location = @location, " +
                "mission_type = @mission_type, " +
                "mission_year = @mission_year, " +
                "mission_month = @mission_month, " +
                "mission_day = @mission_day " +
                "WHERE id = @id";

                //here we are setting the parameter values that will be actually replaced in the query in Execute method
                var args = new Dictionary<string, object>
			    {
				    {"@id", _dto.id},
				    {"@youtube_url", _dto.youtube_url},					
                    {"@description", _dto.description},					
                    {"@location", _dto.location},					
                    {"@mission_type", _dto.mission_type},					
                    {"@mission_year", _dto.mission_year},					
                    {"@mission_month", _dto.mission_month},					
                    {"@mission_day", _dto.mission_day}
			    };

                int numberOfRowsAffected = updategeneric(query, args);
                if (numberOfRowsAffected != 1)
                {
                    _responsedto.isresponseresultsuccessful = false;
                    //_responsedto.responseerrormessage = "Record update failed in " + DBContract.sqlite + ".";
                    _responsedto.responseerrormessage = "Record update failed.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responseerrormessage, TAG));
                    return _responsedto;
                }
                else
                {
                    _responsedto.isresponseresultsuccessful = true;
                    _responsedto.responsesuccessmessage = "Record updated successfully.";
                    //_responsedto.responsesuccessmessage = "Record updated successfully in " + DBContract.sqlite + ".";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responsesuccessmessage, TAG));
                    return _responsedto;
                }

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }
        public responsedto delete_mission_in_database(missions_links_dto _dto)
        {
            responsedto _responsedto = new responsedto();
            try
            {
                string query = "DELETE FROM " +
                        DBContract.missions_links_entity_table.TABLE_NAME +
                        " WHERE " +
                        DBContract.missions_links_entity_table.ID +
                        " = " +
                        "@id";

                //here we are setting the parameter values that will be actually replaced in the query in Execute method
                var args = new Dictionary<string, object>
				{
					{"@id", _dto.id}  
				};

                int numberOfRowsAffected = deletegeneric(query, args);
                if (numberOfRowsAffected != 1)
                {
                    _responsedto.isresponseresultsuccessful = false;
                    //_responsedto.responseerrormessage = "Record deletion failed in " + DBContract.sqlite + ".";
                    _responsedto.responseerrormessage = "Record deletion failed.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responseerrormessage, TAG));
                    return _responsedto;
                }
                else
                {
                    _responsedto.isresponseresultsuccessful = true;
                    //_responsedto.responsesuccessmessage = "Record deleted successfully in " + DBContract.sqlite + ".";
                    _responsedto.responsesuccessmessage = "Record deleted successfully.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responsesuccessmessage, TAG));
                    return _responsedto;
                }

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }
        public DataTable search_missions_in_database(missions_links_dto _dto)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("SELECT * FROM ");
                sb.Append(DBContract.missions_links_entity_table.TABLE_NAME);

                //no field specified
                if (string.IsNullOrEmpty(_dto.youtube_url) && string.IsNullOrEmpty(_dto.description) && string.IsNullOrEmpty(_dto.location) && string.IsNullOrEmpty(_dto.mission_type) && string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {

                }
                //youtube url only
                if (!string.IsNullOrEmpty(_dto.youtube_url) && string.IsNullOrEmpty(_dto.description) && string.IsNullOrEmpty(_dto.location) && string.IsNullOrEmpty(_dto.mission_type) && string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.YOUTUBE_URL);
                    sb.Append(" LIKE ");
                    sb.Append(" @youtube_url ");
                }
                //description only
                if (string.IsNullOrEmpty(_dto.youtube_url) && !string.IsNullOrEmpty(_dto.description) && string.IsNullOrEmpty(_dto.location) && string.IsNullOrEmpty(_dto.mission_type) && string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.DESCRIPTION);
                    sb.Append(" LIKE ");
                    sb.Append(" @description ");
                }
                //location only
                if (string.IsNullOrEmpty(_dto.youtube_url) && string.IsNullOrEmpty(_dto.description) && !string.IsNullOrEmpty(_dto.location) && string.IsNullOrEmpty(_dto.mission_type) && string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.LOCATION);
                    sb.Append(" LIKE ");
                    sb.Append(" @location ");
                }
                //mission type only
                if (string.IsNullOrEmpty(_dto.youtube_url) && string.IsNullOrEmpty(_dto.description) && string.IsNullOrEmpty(_dto.location) && !string.IsNullOrEmpty(_dto.mission_type) && string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_TYPE);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_type ");
                }
                //mission year only
                if (string.IsNullOrEmpty(_dto.youtube_url) && string.IsNullOrEmpty(_dto.description) && string.IsNullOrEmpty(_dto.location) && string.IsNullOrEmpty(_dto.mission_type) && !string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_YEAR);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_year ");
                }
                //mission month only
                if (string.IsNullOrEmpty(_dto.youtube_url) && string.IsNullOrEmpty(_dto.description) && string.IsNullOrEmpty(_dto.location) && string.IsNullOrEmpty(_dto.mission_type) && string.IsNullOrEmpty(_dto.mission_year) && !string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_MONTH);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_month ");
                }
                //mission day only
                if (string.IsNullOrEmpty(_dto.youtube_url) && string.IsNullOrEmpty(_dto.description) && string.IsNullOrEmpty(_dto.location) && string.IsNullOrEmpty(_dto.mission_type) && string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && !string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_DAY);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_day ");
                }

                //youtube url and description
                if (!string.IsNullOrEmpty(_dto.youtube_url) && !string.IsNullOrEmpty(_dto.description) && string.IsNullOrEmpty(_dto.location) && string.IsNullOrEmpty(_dto.mission_type) && string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.YOUTUBE_URL);
                    sb.Append(" LIKE ");
                    sb.Append(" @youtube_url ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.DESCRIPTION);
                    sb.Append(" LIKE ");
                    sb.Append(" @description ");
                }
                //youtube url and description and location
                if (!string.IsNullOrEmpty(_dto.youtube_url) && !string.IsNullOrEmpty(_dto.description) && !string.IsNullOrEmpty(_dto.location) && string.IsNullOrEmpty(_dto.mission_type) && string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.YOUTUBE_URL);
                    sb.Append(" LIKE ");
                    sb.Append(" @youtube_url ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.DESCRIPTION);
                    sb.Append(" LIKE ");
                    sb.Append(" @description ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.LOCATION);
                    sb.Append(" LIKE ");
                    sb.Append(" @location ");
                }
                //youtube url and description and location and mission type
                if (!string.IsNullOrEmpty(_dto.youtube_url) && !string.IsNullOrEmpty(_dto.description) && !string.IsNullOrEmpty(_dto.location) && !string.IsNullOrEmpty(_dto.mission_type) && string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.YOUTUBE_URL);
                    sb.Append(" LIKE ");
                    sb.Append(" @youtube_url ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.DESCRIPTION);
                    sb.Append(" LIKE ");
                    sb.Append(" @description ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.LOCATION);
                    sb.Append(" LIKE ");
                    sb.Append(" @location ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_TYPE);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_type ");
                }
                //youtube url and description and location and mission type and mission year
                if (!string.IsNullOrEmpty(_dto.youtube_url) && !string.IsNullOrEmpty(_dto.description) && !string.IsNullOrEmpty(_dto.location) && !string.IsNullOrEmpty(_dto.mission_type) && !string.IsNullOrEmpty(_dto.mission_year) && string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.YOUTUBE_URL);
                    sb.Append(" LIKE ");
                    sb.Append(" @youtube_url ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.DESCRIPTION);
                    sb.Append(" LIKE ");
                    sb.Append(" @description ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.LOCATION);
                    sb.Append(" LIKE ");
                    sb.Append(" @location ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_TYPE);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_type ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_YEAR);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_year ");
                }
                //youtube url and description and location and mission type and mission year and mission month
                if (!string.IsNullOrEmpty(_dto.youtube_url) && !string.IsNullOrEmpty(_dto.description) && !string.IsNullOrEmpty(_dto.location) && !string.IsNullOrEmpty(_dto.mission_type) && !string.IsNullOrEmpty(_dto.mission_year) && !string.IsNullOrEmpty(_dto.mission_month) && string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.YOUTUBE_URL);
                    sb.Append(" LIKE ");
                    sb.Append(" @youtube_url ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.DESCRIPTION);
                    sb.Append(" LIKE ");
                    sb.Append(" @description ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.LOCATION);
                    sb.Append(" LIKE ");
                    sb.Append(" @location ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_TYPE);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_type ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_YEAR);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_year ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_MONTH);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_month ");
                }
                //youtube url and description and location and mission type and mission year and mission month and mission day
                if (!string.IsNullOrEmpty(_dto.youtube_url) && !string.IsNullOrEmpty(_dto.description) && !string.IsNullOrEmpty(_dto.location) && !string.IsNullOrEmpty(_dto.mission_type) && !string.IsNullOrEmpty(_dto.mission_year) && !string.IsNullOrEmpty(_dto.mission_month) && !string.IsNullOrEmpty(_dto.mission_day))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.missions_links_entity_table.YOUTUBE_URL);
                    sb.Append(" LIKE ");
                    sb.Append(" @youtube_url ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.DESCRIPTION);
                    sb.Append(" LIKE ");
                    sb.Append(" @description ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.LOCATION);
                    sb.Append(" LIKE ");
                    sb.Append(" @location ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_TYPE);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_type ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_YEAR);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_year ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_MONTH);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_month ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.missions_links_entity_table.MISSION_DAY);
                    sb.Append(" LIKE ");
                    sb.Append(" @mission_day ");
                }

                var query = sb.ToString();

                var args = new Dictionary<string, object>
				{
					{"@youtube_url", _dto.youtube_url},					
                    {"@description", _dto.description},					
                    {"@location", _dto.location},					
                    {"@mission_type", _dto.mission_type},					
                    {"@mission_year", _dto.mission_year},					
                    {"@mission_month", _dto.mission_month},					
                    {"@mission_day", _dto.mission_day}
				};

                DataTable dt = ExecuteRead(query, args);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return null;
                }

                return dt;
            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                return null;
            }
        }
        #endregion "missions"

        #region "users"
        public responsedto create_user_in_database(user_dto _dto)
        {
            responsedto _responsedto = new responsedto();
            try
            {
                string query = "INSERT INTO " +
                DBContract.users_entity_table.TABLE_NAME +
                " ( " +
                DBContract.users_entity_table.EMAIL + ", " +
                DBContract.users_entity_table.PASSWORD + ", " +
                DBContract.users_entity_table.PASSWORD_SALT + ", " +
                DBContract.users_entity_table.PASSWORD_HASH + ", " +
                DBContract.users_entity_table.FULLNAMES + ", " +
                DBContract.users_entity_table.YEAR + ", " +
                DBContract.users_entity_table.MONTH + ", " +
                DBContract.users_entity_table.DAY + ", " +
                DBContract.users_entity_table.GENDER + ", " +
                DBContract.users_entity_table.STATUS + ", " +
                DBContract.users_entity_table.CREATED_DATE +
                " ) VALUES(@email, @password, @password_salt, @password_hash, @fullnames, @year, @month, @day, @gender, @status, @created_date)";

                //here we are setting the parameter values that will be actually replaced in the query in Execute method
                var args = new Dictionary<string, object>
			    {
				    {"@email", _dto.email},
				    {"@password", _dto.password},
                    {"@password_salt", _dto.password_salt},
                    {"@password_hash", _dto.password_hash},
                    {"@fullnames", _dto.fullnames},
                    {"@year", _dto.year},
                    {"@month", _dto.month},
                    {"@day", _dto.day},
                    {"@gender", _dto.gender}, 
                    {"@status", "active"},
				    {"@created_date", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt")}
			    };

                int numberOfRowsAffected = insertgeneric(query, args, CONNECTION_STRING);
                if (numberOfRowsAffected != 1)
                {
                    _responsedto.isresponseresultsuccessful = false;
                    //_responsedto.responseerrormessage = "Record creation failed in " + DBContract.sqlite + ".";
                    _responsedto.responseerrormessage = "Record creation failed.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responseerrormessage, TAG));
                    return _responsedto;
                }
                else
                {
                    _responsedto.isresponseresultsuccessful = true;
                    //_responsedto.responsesuccessmessage = "Record created successfully in " + DBContract.sqlite + ".";
                    _responsedto.responsesuccessmessage = "Record created successfully.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responsesuccessmessage, TAG));
                    return _responsedto;
                }

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }
        public user_dto get_user_by_id(int id)
        {
            try
            {
                var query = "SELECT * FROM " +
                    DBContract.users_entity_table.TABLE_NAME +
                    " WHERE " +
                    DBContract.users_entity_table.ID +
                    " = " +
                    "@id";

                var args = new Dictionary<string, object>
				{
					{"@id", id}
				};

                DataTable dt = ExecuteRead(query, args);
                if (dt == null || dt.Rows.Count == 0)
                {
                    return null;
                }

                user_dto _dto = new user_dto();
                _dto.id = Convert.ToInt64(dt.Rows[0]["id"]);
                _dto.email = Convert.ToString(dt.Rows[0]["email"]);
                _dto.password = Convert.ToString(dt.Rows[0]["password"]);
                _dto.fullnames = Convert.ToString(dt.Rows[0]["fullnames"]);
                _dto.year = Convert.ToString(dt.Rows[0]["year"]);
                _dto.month = Convert.ToString(dt.Rows[0]["month"]);
                _dto.day = Convert.ToString(dt.Rows[0]["day"]);
                _dto.gender = Convert.ToString(dt.Rows[0]["gender"]);
                _dto.status = Convert.ToString(dt.Rows[0]["status"]);
                _dto.created_date = Convert.ToString(dt.Rows[0]["created_date"]);

                return _dto;

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                return null;
            }
        }
        public user_dto get_user_by_email(string email)
        {
            try
            {

                var query = "SELECT * FROM " +
                    DBContract.users_entity_table.TABLE_NAME +
                    " WHERE " +
                    DBContract.users_entity_table.EMAIL +
                    " = " +
                    "@email";

                var args = new Dictionary<string, object>
				{
					{"@email", email}
				};

                DataTable dt = ExecuteRead(query, args);
                if (dt == null || dt.Rows.Count == 0)
                {
                    return null;
                }

                user_dto _dto = new user_dto();
                _dto.id = Convert.ToInt64(dt.Rows[0]["id"]);
                _dto.email = Convert.ToString(dt.Rows[0]["email"]);
                _dto.password = Convert.ToString(dt.Rows[0]["password"]);
                _dto.fullnames = Convert.ToString(dt.Rows[0]["fullnames"]);
                _dto.year = Convert.ToString(dt.Rows[0]["year"]);
                _dto.month = Convert.ToString(dt.Rows[0]["month"]);
                _dto.day = Convert.ToString(dt.Rows[0]["day"]);
                _dto.gender = Convert.ToString(dt.Rows[0]["gender"]);
                _dto.status = Convert.ToString(dt.Rows[0]["status"]);
                _dto.created_date = Convert.ToString(dt.Rows[0]["created_date"]);

                return _dto;

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                return null;
            }
        }
        public responsedto update_user_in_database(user_dto _dto)
        {
            responsedto _responsedto = new responsedto();
            try
            {

                string query = "UPDATE " +
                DBContract.users_entity_table.TABLE_NAME +
                " SET " +
                "fullnames = @fullnames, " +
                "year = @year, " +
                "month = @month, " +
                "day = @day, " +
                "gender = @gender " +
                "WHERE id = @id";

                //here we are setting the parameter values that will be actually replaced in the query in Execute method
                var args = new Dictionary<string, object>
			    { 
                    {"@id", _dto.id},
                    {"@fullnames", _dto.fullnames},
                    {"@year", _dto.year},
                    {"@month", _dto.month},
                    {"@day", _dto.day},
                    {"@gender", _dto.gender} 
			    };

                int numberOfRowsAffected = updategeneric(query, args);
                if (numberOfRowsAffected != 1)
                {
                    _responsedto.isresponseresultsuccessful = false;
                    //_responsedto.responseerrormessage = "Record update failed in " + DBContract.sqlite + ".";
                    _responsedto.responseerrormessage = "Record update failed.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responseerrormessage, TAG));
                    return _responsedto;
                }
                else
                {
                    _responsedto.isresponseresultsuccessful = true;
                    _responsedto.responsesuccessmessage = "Record updated successfully.";
                    //_responsedto.responsesuccessmessage = "Record updated successfully in " + DBContract.sqlite + ".";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responsesuccessmessage, TAG));
                    return _responsedto;
                }

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }
        public responsedto delete_user_in_database(user_dto _dto)
        {
            responsedto _responsedto = new responsedto();
            try
            {
                string query = "DELETE FROM " +
                        DBContract.users_entity_table.TABLE_NAME +
                        " WHERE " +
                        DBContract.users_entity_table.ID +
                        " = " +
                        "@id";

                //here we are setting the parameter values that will be actually replaced in the query in Execute method
                var args = new Dictionary<string, object>
				{
					{"@id", _dto.id}  
				};

                int numberOfRowsAffected = deletegeneric(query, args);
                if (numberOfRowsAffected != 1)
                {
                    _responsedto.isresponseresultsuccessful = false;
                    //_responsedto.responseerrormessage = "Record deletion failed in " + DBContract.sqlite + ".";
                    _responsedto.responseerrormessage = "Record deletion failed.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responseerrormessage, TAG));
                    return _responsedto;
                }
                else
                {
                    _responsedto.isresponseresultsuccessful = true;
                    //_responsedto.responsesuccessmessage = "Record deleted successfully in " + DBContract.sqlite + ".";
                    _responsedto.responsesuccessmessage = "Record deleted successfully.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responsesuccessmessage, TAG));
                    return _responsedto;
                }

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }
        public DataTable search_users_in_database(user_dto _dto)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("SELECT * FROM ");
                sb.Append(DBContract.users_entity_table.TABLE_NAME);

                //no field specified
                if (string.IsNullOrEmpty(_dto.email) && string.IsNullOrEmpty(_dto.fullnames))
                {

                }
                //email only
                if (!string.IsNullOrEmpty(_dto.email) && string.IsNullOrEmpty(_dto.fullnames))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.users_entity_table.EMAIL);
                    sb.Append(" LIKE ");
                    sb.Append(" @email ");
                }
                //fullnames only
                if (string.IsNullOrEmpty(_dto.email) && !string.IsNullOrEmpty(_dto.fullnames))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.users_entity_table.FULLNAMES);
                    sb.Append(" LIKE ");
                    sb.Append(" @fullnames ");
                }
                //email and fullnames
                if (!string.IsNullOrEmpty(_dto.email) && !string.IsNullOrEmpty(_dto.fullnames))
                {
                    sb.Append(" WHERE ");
                    sb.Append(DBContract.users_entity_table.EMAIL);
                    sb.Append(" LIKE ");
                    sb.Append(" @email ");
                    sb.Append(" AND ");
                    sb.Append(DBContract.users_entity_table.FULLNAMES);
                    sb.Append(" LIKE ");
                    sb.Append(" @fullnames ");
                }

                var query = sb.ToString();

                var args = new Dictionary<string, object>
				{
                    {"@email", _dto.email},
					{"@fullnames", _dto.fullnames} 
				};

                DataTable dt = ExecuteRead(query, args);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return null;
                }

                return dt;
            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                return null;
            }
        }
        public responsedto login(user_dto dto)
        {
            responsedto _responsedto = new responsedto();
            try
            {

                var query = "SELECT * FROM " +
                    DBContract.users_entity_table.TABLE_NAME +
                    " WHERE " +
                    DBContract.users_entity_table.EMAIL +
                    " = " +
                    "@email" +
                    " AND " +
                    DBContract.users_entity_table.PASSWORD +
                    " = " +
                    "@password";

                var args = new Dictionary<string, object>
				{
					{"@email", dto.email},
                    {"@password", dto.password}
				};

                DataTable dt = ExecuteRead(query, args);

                if (dt == null || dt.Rows.Count == 0)
                {
                    _responsedto.isresponseresultsuccessful = false;
                    //_responsedto.responseerrormessage = "Record deletion failed in " + DBContract.sqlite + ".";
                    _responsedto.responseerrormessage = "Log in failed.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responseerrormessage, TAG));
                    return _responsedto;
                }
                else
                {
                    user_dto _dto = new user_dto();
                    _dto.id = Convert.ToInt64(dt.Rows[0]["id"]);
                    _dto.email = Convert.ToString(dt.Rows[0]["email"]);
                    _dto.password = Convert.ToString(dt.Rows[0]["password"]);
                    _dto.fullnames = Convert.ToString(dt.Rows[0]["fullnames"]);
                    _dto.year = Convert.ToString(dt.Rows[0]["year"]);
                    _dto.month = Convert.ToString(dt.Rows[0]["month"]);
                    _dto.day = Convert.ToString(dt.Rows[0]["day"]);
                    _dto.gender = Convert.ToString(dt.Rows[0]["gender"]);
                    _dto.status = Convert.ToString(dt.Rows[0]["status"]);
                    _dto.created_date = Convert.ToString(dt.Rows[0]["created_date"]);

                    _responsedto.responseresultobject = _dto;

                    _responsedto.isresponseresultsuccessful = true;
                    //_responsedto.responsesuccessmessage = "Record deleted successfully in " + DBContract.sqlite + ".";
                    _responsedto.responsesuccessmessage = "Logged in  successfully.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responsesuccessmessage, TAG));
                    return _responsedto;
                }

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }
        #endregion "users"

        #region "logs"
        public responsedto create_log_in_database(log_dto _dto)
        {
            responsedto _responsedto = new responsedto();
            try
            {
                string query = "INSERT INTO " +
                DBContract.logs_entity_table.TABLE_NAME +
                " ( " +
                DBContract.logs_entity_table.MESSAGE + ", " +
                DBContract.logs_entity_table.TIMESTAMP + ", " +
                DBContract.logs_entity_table.TAG + ", " +
                DBContract.logs_entity_table.STATUS + ", " +
                DBContract.logs_entity_table.CREATED_DATE +
                " ) VALUES(@message, @timestamp, @tag, @status, @created_date)";

                //here we are setting the parameter values that will be actually replaced in the query in Execute method
                var args = new Dictionary<string, object>
			    {
				    {"@message", _dto.message},
				    {"@timestamp", _dto.timestamp},
                    {"@tag", _dto.tag},  
                    {"@status", "active"},
				    {"@created_date", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss tt")}
			    };

                int numberOfRowsAffected = insertgeneric(query, args, CONNECTION_STRING);
                if (numberOfRowsAffected != 1)
                {
                    _responsedto.isresponseresultsuccessful = false;
                    //_responsedto.responseerrormessage = "Record creation failed in " + DBContract.sqlite + ".";
                    _responsedto.responseerrormessage = "Record creation failed.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responseerrormessage, TAG));
                    return _responsedto;
                }
                else
                {
                    _responsedto.isresponseresultsuccessful = true;
                    //_responsedto.responsesuccessmessage = "Record created successfully in " + DBContract.sqlite + ".";
                    _responsedto.responsesuccessmessage = "Record created successfully.";
                    this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(_responsedto.responsesuccessmessage, TAG));
                    return _responsedto;
                }

            }
            catch (Exception ex)
            {
                this._notificationmessageEventname.Invoke(this, new notificationmessageEventArgs(ex.Message, TAG));
                _responsedto.isresponseresultsuccessful = false;
                _responsedto.responseerrormessage = ex.Message;
                return _responsedto;
            }
        }
        #endregion "logs"







    }
}
