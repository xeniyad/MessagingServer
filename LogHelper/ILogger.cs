using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogHelper
{
    public interface ILogger
    {
        void LogException(Exception exception);
        void LogMessage(string message);
    }
}
