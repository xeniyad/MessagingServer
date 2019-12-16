using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusHelper
{
    public interface IMessageReceive
    {
        void ReceiveMessage();

        void SendServerStatus(SBServerStatuses status);

        string GetClientsStatuses();

        void CancelOperations();
    }
}
