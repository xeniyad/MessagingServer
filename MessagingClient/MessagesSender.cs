using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingClient
{
    class MessagesSender
    {
        private const string queueName = "FilesQueue";
        public void SendMessage(System.IO.FileStream message)
        {
            var client = QueueClient.Create(queueName);
            var sender = new LargeMessageSender(client);
            sender.Send(new BrokeredMessage(message));
        }
    }
}
