using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM
{
    /// <summary>
    /// CONTROLLER
    /// </summary>
    static class Controller
    {
        /// <summary>
        /// Method gets an integer value  from user. Checks if the user has entered a number
        /// </summary>
        /// <returns>Input value</returns>
        internal static int GetIntValue()
        {
            int value = 0;
            try
            {
                value = int.Parse(Console.ReadLine());
            }
            catch (Exception e)
            {
                View.OutputMessageError(e.Message);
            }
            return value;
        }

        /// <summary>
        /// Method gets a string value from user
        /// </summary>
        /// <returns>Input string</returns>
        internal static string GetStringValue()
        {
            string str = Console.ReadLine();
            return str;
        } 
    }
}
