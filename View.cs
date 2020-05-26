using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM
{
    /// <summary>
    /// VIEW 
    /// </summary>
    static class View
    {
        /// <summary>
        /// Method displays massage
        /// </summary>
        /// <param name="message">Message to display</param>
         static internal void OutputMessage(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Method displays an error massage
        /// </summary>
        /// <param name="message">Message to display</param>
        static internal void OutputMessageError(string message)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ReadKey();
        }

        /// <summary>
        /// Method displays massage about success
        /// </summary>
        /// <param name="message">Message to display</param>
        static internal void OutputMessageSuccess(string message)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ReadKey();
        }
    }
}
