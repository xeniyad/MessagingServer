using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusHelper
{
    public delegate void FilePartSentHandler();

    public interface IMessageSend
    {
        void Send(FileMessage fileMessage);

        void SendClientStatus(SBClientStatuses status);

        string GetServerStatus();



        event FilePartSentHandler FilePartSentNotify;
    }
}
