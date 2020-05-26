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
            string announce = "!!! Use ENTER KEY to accept \"green\" or \"red\" announces !!!";
            View.OutputMessage(announce);
            ConnectDB(connect);                                         // Test connecion with database

            while (true)
            {

                View.OutputMessage("TASK MANAGER MENU:\n");
                View.OutputMessage("1. SHOW ALL TASKS\n2. CREATE A NEW TASK\n3. DELETE TASK\n4. QUIT");     // Menu 
                int swth = Controller.GetIntValue(); // Input

                switch (swth)
                {
                    case 1:
                        Console.Clear();
                        Model.ShowTask(sqlConnection, sqlCommand);           // Show all tasks
                        break;
                    case 2:
                        Console.Clear();
                        View.OutputMessage("\nMinimum lenght Your task is 5 but maximum 250\n\nWrite your task: ");
                        string newstr = Controller.GetStringValue();
                        Model.AddTask(newstr, sqlConnection, sqlCommand);     // Add a new task
                        break;
                    case 3:
                        Console.Clear();
                        Model.DeleteTask(sqlConnection, sqlCommand);          // Delete task
                        break;
                    case 4:
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
                    View.OutputMessageSuccess(" CONNECTED ");
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

                    Console.Clear();
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
                View.OutputMessage(String.Format("{0,-10} {1,10} ", "Num", "Description"));
                View.OutputMessage("=============================================");
                while (read.Read())
                {
                    View.OutputMessage(String.Format("{0,-10} {1,-10}", num++, read[1]));
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
                Console.Clear();
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
