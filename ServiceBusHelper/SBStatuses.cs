using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusHelper
{
    public enum SBServerStatuses
    {
        Working = 1,
        Stopped = 0
    }

    public enum SBClientStatuses
    {
        UploadingFile = 1,
        WaitingForFile = 0
    }
}
