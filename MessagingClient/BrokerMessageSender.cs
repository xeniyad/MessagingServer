using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using ServiceBusHelper;

namespace MessagingClient
{
    public class BrokerMessageSender
    {
        private readonly SBClientManager _messageClient;

        public BrokerMessageSender(SBClientManager messageClient)
        {
            _messageClient = messageClient ?? throw new ArgumentNullException(nameof(messageClient));
        }

        public Task SendFile(string filename)
        {
            var file = new BrokeredMessage(new FileStream(filename, FileMode.Open));
            var fileMessage = new FileMessage(Path.GetFileName(filename), file);

            return Task.Factory.StartNew(() => _messageClient.Send(fileMessage));
        }
    }
}