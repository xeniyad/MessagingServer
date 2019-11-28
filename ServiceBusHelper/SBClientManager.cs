using System;
using LogHelper;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using Microsoft.ServiceBus;

namespace ServiceBusHelper
{
    public class SBClientManager
    {
        // Можно было бы сделать обычной константой
        private const int SubMessageBodySize = 192 * 1024;

        private readonly QueueClient _queueFileClient;
        private readonly QueueClient _queueServerStatusClient;
        private readonly QueueClient _queueClientStatusClient;

        private readonly ILogger _logger;
        private readonly string _clientName;

        public delegate void FilePartSentHandler();
        // Лучше делать уведомление клиентов через event, ведь именно для этого они и были придуманы
        public event FilePartSentHandler FilePartSentNotify;

        public SBClientManager(ClientSettingsDto clientSettings, ILogger logger, string clientName)
        {
            _logger = logger;
            _queueFileClient = QueueClient.Create(clientSettings.FilesQueueName);
            _queueServerStatusClient = QueueClient.Create(clientSettings.ServerStatusQueueName, ReceiveMode.PeekLock);
            _queueClientStatusClient = QueueClient.Create(clientSettings.ClientsStatusQueueName);
            _clientName = clientName;

            CreateQueue(clientSettings.FilesQueueName);
            CreateQueue(clientSettings.ServerStatusQueueName);
            CreateQueue(clientSettings.ClientsStatusQueueName);
        }

        public void CreateQueue(string queueName)
        {
            var nsManager = NamespaceManager.Create();
            if (!nsManager.QueueExists(queueName))
            {
                nsManager.CreateQueue(queueName);
                _logger.LogMessage($"Queue {queueName} created");
            }
            else
            {
                _logger.LogMessage($"Queue {queueName} already exist");
            }
        }

        public void Send(FileMessage fileMessage)
        {
            var message = fileMessage.Message;
            string sessionId = Guid.NewGuid().ToString();

            _logger.LogMessage($"Message session Id: {sessionId}");

            SendFileNameMessage(fileMessage.FileName, sessionId);
            SendFilePartMessages(message, sessionId);
            _logger.LogMessage("Done!");
           
        }

        private void SendFileNameMessage(string fileName, string sessionId)
        {
            BrokeredMessage titleMessage = new BrokeredMessage(fileName)
            {
                SessionId = sessionId
            };

            _queueFileClient.Send(titleMessage);
        }

        private void SendFilePartMessages(BrokeredMessage message, string sessionId)
        {
            long messageBodySize = message.Size;
            int nrSubMessages = (int)(messageBodySize / SubMessageBodySize);

            if (messageBodySize % SubMessageBodySize != 0)
            {
                nrSubMessages++;
            }

            _logger.LogMessage($"Sending {0} sub-messages: {nrSubMessages}");

            Stream bodyStream = message.GetBody<Stream>();

            for (int streamOffset = 0; streamOffset < messageBodySize; streamOffset += SubMessageBodySize)
            {
                long arraySize = (messageBodySize - streamOffset) > SubMessageBodySize
                    ? SubMessageBodySize
                    : messageBodySize - streamOffset;

                byte[] subMessageBytes = new byte[arraySize];
                int result = bodyStream.Read(subMessageBytes, 0, (int)arraySize);
                var subMessageStream = new MemoryStream(subMessageBytes);
                var subMessage = new BrokeredMessage(subMessageStream, true)
                {
                    SessionId = sessionId
                };

                _queueFileClient.Send(subMessage);
                FilePartSentNotify?.Invoke();
            }
        }

        public void SendClientStatus(SBClientStatuses status)
        {
            _queueClientStatusClient.Send(new BrokeredMessage($"{DateTime.Now}: Client {_clientName} has status {status}"));
        }

        public string GetServerStatus()
        {
            var message = _queueServerStatusClient.Receive();
            return message.GetBody<string>();
        }

    }
}
