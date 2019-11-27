using Microsoft.ServiceBus.Messaging;

namespace ServiceBusHelper
{
    public class FileMessage
    {
        public BrokeredMessage Message;
        public string FileName;
        public FileMessage(string fileName, BrokeredMessage message)
        {
            FileName = fileName;
            Message = message;
        }
    }
}
