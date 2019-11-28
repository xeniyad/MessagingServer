namespace ServiceBusHelper
{
    public class ClientSettingsDto
    {
        public int StatusSendPeriodMs { get; set; }
        public string FilesQueueName { get; set; }
        public string ServerStatusQueueName { get; set; }
        public string ClientsStatusQueueName { get; set; }
        public int SubMessageBodySize { get; set; }

        public ClientSettingsDto()
        {
            StatusSendPeriodMs = 60_000;
            FilesQueueName = "FilesQueueSessions";
            ServerStatusQueueName = "ServerStatusQueue";
            ClientsStatusQueueName = "ClientStatusQueue";
            SubMessageBodySize = 192 * 1024;
        }
    }
}
