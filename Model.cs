using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;

namespace TM
{
    /// <summary>
    ///                 THE TASK MANAGER
    ///                 Made by Piotr Cieślik
    /// </summary>
    class Model
    {

        /// <summary>
        ///  MODEL           Main method
        /// </summary>
        public static void Main(string[] args)
        {
            Console.SetWindowSize(95,40);
            string ConPath = "data.txt";    // Path for file with connection data
            FileStream FileStream = new FileStream(ConPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);        // Create a database configuration file
            StreamWriter sw;
            FileStream.Close();

            if (!File.Exists(ConPath))
            {
                sw = File.CreateText(ConPath);
                View.OutputMessageError("File with login dta not exist");
            }

            string connect = StringCon(ConPath);                        // Read file with connection string 

            SqlConnection sqlConnection = new SqlConnection(connect);   // Create a connection with local database via Integrated Security=true
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            string announce = "!!! Use ENTER KEY to accept \"green\" or \"red\" announcements !!!";
            View.OutputMessage(announce);
            ConnectDB(connect);                                         // Test connecion with database

            while (true)
            {
                string menu, menu1, menu2, menu3, menu4, menu5,task;
                menu = "====================================================================";
                task = "TASK MANAGER MENU: ";
                menu1 = "1. SHOW ALL TASKS";
                menu2 = "2. CREATE A NEW TASK";
                menu3 = "3. DELETE TASK";
                menu4 = "4. QUIT";
                menu5 = "====================================================================";

                View.OutputMessage("\n");
                View.OutputMessage(menu);
                View.OutputMessage(task);
                View.OutputMessage(menu1);
                View.OutputMessage(menu2);
                View.OutputMessage(menu3);
                View.OutputMessage(menu4);
                View.OutputMessage(menu5);// Menu 
                View.OutputMessage("\n");
                int swth = Controller.GetIntValue(); // Input
                
                switch (swth)
                {
                    case 1:
                        Console.Clear();
                        View.OutputMessage(menu1+"\n");
                        Model.ShowTask(sqlConnection, sqlCommand);           // Show all tasks
                        break;
                    case 2:
                        Console.Clear();
                        View.OutputMessage(menu2 + "\n");
                        string min,newe;
                        min = "Minimum lenght of your task is 5 but maximum 250";
                        newe = "Write your task: ";
                        View.OutputMessage(min);
                        View.OutputMessage(newe);
                        string newstr = Controller.GetStringValue();
                        Model.AddTask(newstr, sqlConnection, sqlCommand);     // Add a new task
                        break;
                    case 3:
                        Console.Clear();
                        View.OutputMessage(menu3 + "\n");
                        Model.DeleteTask(sqlConnection, sqlCommand);          // Delete task
                        break;
                    case 4:
                        View.OutputMessage(menu4 + "\n");
                        sqlConnection.Dispose();
                        Environment.Exit(0);                            // Exit from aplication
                        break;
                    default:
                        View.OutputMessageError("No optrion");
                        break;
                }
            }
        }

        #region METHODS

        /// <summary>
        /// Connecting with database method
        /// </summary>
        /// <param name="connStr">Connection string</param>
        private static void ConnectDB(string connStr)
        {
            bool error = false;

            SqlConnection sqlConnection = new SqlConnection(connStr);
            try                                                                 //Test connection
            {
                sqlConnection.Open();
                View.OutputMessage("Connecting...");
            }
            catch (SqlException e)                                              // a Sql excepion handles
            {
                error = true;
                View.OutputMessageError(string.Format("# Connection Error #\n {0}", e.Message));
            }
            finally
            {
                if (error)
                {
                    View.OutputMessageError("# An aplication can't connect with database #");
                    sqlConnection.Close();
                }
                else
                {
                    string con = "CONNECTED";
                    View.OutputMessageSuccess(con);
                    sqlConnection.Close();
                }
            }
        }

        /// <summary>
        /// Method adds a new task into your organizer
        /// </summary>
        /// <param name="str">New task descrioption</param>
        /// <param name="sqlConnection">SQL connection object</param>
        /// <param name="sqlCommand">SQL command object</param>
        private static void AddTask(string str, SqlConnection sqlConnection, SqlCommand sqlCommand)
        {
            bool exist = false;
            string newstr = str;
            string output = string.Empty;

            try
            {
                sqlConnection.Open();
                sqlCommand.CommandText = "USE TaskBase SELECT [Description] FROM task WITH(NOLOCK)";
                SqlDataReader dataReader = sqlCommand.ExecuteReader();                                  // Execute query above

                while (dataReader.Read())
                {
                    if (newstr == dataReader[0].ToString())                                              // Test if exist two the same tasks
                        exist = true;
                }
            }
            catch (SqlException s)
            {
                View.OutputMessageError(s.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            if (newstr.Length - 1 < 5)
            {
                View.OutputMessageError("Task must have minimum 5 characters \n");
            }
            else if (newstr.Length - 1 > 250)
            {
                View.OutputMessageError("Task must have maximum 250 characters\n");
            }
            else
            {
                if (exist)
                {
                    View.OutputMessageError("In organizer exists identical task\n");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        sqlCommand.CommandText = $"USE TaskBase BEGIN TRAN INSERT INTO Task WITH(ROWLOCK) ([Description]) VALUES ('{newstr}') COMMIT";      // insert into query
                        sqlCommand.ExecuteNonQuery();           // Execute query above
                    }
                    catch (SqlException s)
                    {
                        View.OutputMessageError(s.Message);
                    }
                    finally
                    {
                        sqlConnection.Close();
                    }

                    
                    string added = "Task was added!";
                    View.OutputMessageSuccess(String.Format("\n{0,10}\n", added));
                }
            }
        }

        /// <summary>
        /// Method show all tasks in your organizer
        /// </summary>
        /// <param name="sqlConnection">SQL connection object</param>
        /// <param name="sqlCommand">SQL command object</param>
        private static void ShowTask(SqlConnection sqlConnection, SqlCommand sqlCommand)
        {
            int num = 1;
            try
            {
                sqlConnection.Open();
                sqlCommand.CommandText = "USE TaskBase SELECT [TaskID],[Description] FROM [Task] WITH(NOLOCK)";
            }
            catch (SqlException s)
            {
                View.OutputMessageSuccess(s.Message);
            }

            using (SqlDataReader read = sqlCommand.ExecuteReader())
            {
                View.OutputMessageShowTask( "Num", "Description");
                View.OutputMessage("====================================================================");
                while (read.Read())
                {
                    View.OutputMessageShowTask(num++.ToString(), read[1].ToString());
                }
            }
            sqlConnection.Close();
        }

        /// <summary>
        /// Method deletes task in your organizer
        /// </summary>
        /// <param name="sqlConnection">SQL connection object</param>
        /// <param name="sqlCommand">SQL command object</param>
        private static void DeleteTask(SqlConnection sqlConnection, SqlCommand sqlCommand)
        {

            ShowTask(sqlConnection, sqlCommand);
            View.OutputMessage("\nWRITE NUMBER TO DELETE\n");
            int type = 0;

            type = Controller.GetIntValue();
            View.OutputMessage("ARE YOU SURE?");
            string yesno = string.Empty;
            yesno = Controller.GetStringValue().ToLower();


            if ((yesno == "yes") || (yesno == "tak"))
            {
                int num = 1, IDdel = 0;
                try
                {
                    sqlConnection.Open();
                    sqlCommand.CommandText = "SELECT TaskID FROM Task WITH(NOLOCK)";
                    using (SqlDataReader read = sqlCommand.ExecuteReader())
                    {
                        List<string> list = new List<string>();

                        while (read.Read())
                        {
                            if (num == type)
                                IDdel = int.Parse(read[0].ToString());              // Find Task Id to delete
                            num++;
                            list.Add(read[0].ToString());
                        }
                        try
                        {
                            if (type > list.Count())
                                throw new IndexOutOfRangeException();
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            View.OutputMessageError(e.Message);
                            return;
                        }
                    }
                    sqlCommand.CommandText = "BEGIN TRAN USE TaskBase DELETE FROM Task WITH(ROWLOCK) WHERE TaskID=" + IDdel + " COMMIT";
                    sqlCommand.ExecuteNonQuery();
                }
                catch (SqlException s)
                {
                    View.OutputMessageError(s.Message);
                }
                finally
                {
                    sqlConnection.Close();
                }

                string delated = "################ DELETED SUCCESSFULLY ################";
                
                View.OutputMessageSuccess(String.Format("\n{0,10}\n", delated));
                Model.ShowTask(sqlConnection, sqlCommand);
            }
            else
            {
                View.OutputMessage("Application returns to menu");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Method creates a connection string to local database
        /// </summary>
        /// <param name="path">Path to file with connection informations</param>
        /// <returns>Returns ConnectionString for SqlConnection</returns>
        private static string StringCon(string path)
        {
            StreamReader streamReader = new StreamReader(path);
            string line = String.Empty;
            string output = String.Empty;

            while ((line = streamReader.ReadLine()) != null)
            {
                output += line;
            }
            return output;
        }
        #endregion
    }
}
