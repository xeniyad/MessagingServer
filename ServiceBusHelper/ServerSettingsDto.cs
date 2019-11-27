using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusHelper
{
    public class ServerSettingsDto
    {
        public string FolderPath { get; set; }
        public int StatusSendPeriodMs { get; set; }
        public string FilesQueueName { get; set; }
        public string ServerStatusQueueName { get; set; }
        public string ClientsStatusQueueName { get; set; }

        public ServerSettingsDto()
        {
            FolderPath = @"C:\Users\xeniya_denissova\Desktop\sbtest";
            StatusSendPeriodMs = 60_000;
            FilesQueueName = "FilesQueueSessions";
            ServerStatusQueueName = "ServerStatusQueue";
            ClientsStatusQueueName = "ClientStatusQueue";
        }
    }
}
