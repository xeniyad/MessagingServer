using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogHelper
{
    public class ConsoleLogger : ILogger
    {
        public void LogException(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }

        public void LogMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
