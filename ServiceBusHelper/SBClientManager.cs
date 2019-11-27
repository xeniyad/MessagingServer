using System;
using LogHelper;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using Microsoft.ServiceBus;

namespace ServiceBusHelper
{
    public class SBClientManager
    {
        private static int SubMessageBodySize = 192 * 1024;
        private QueueClient _queueFileClient;
        private QueueClient _queueServerStatusClient;
        private QueueClient _queueClientStatusClient;
        private ILogger _logger;
        private string _clientName;
        private Action _updateProgress;

        public SBClientManager(ClientSettingsDto clientSettings, ILogger logger, Action updateProgress, string clientName)
        {
            _logger = logger;
            _queueFileClient = QueueClient.Create(clientSettings.FilesQueueName);
            _queueServerStatusClient = QueueClient.Create(clientSettings.ServerStatusQueueName, ReceiveMode.PeekLock);
            _queueClientStatusClient = QueueClient.Create(clientSettings.ClientsStatusQueueName);
            _updateProgress = updateProgress;
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
            BrokeredMessage titleMessage = new BrokeredMessage(fileName);
            titleMessage.SessionId = sessionId;
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

            for (int streamOffest = 0; streamOffest < messageBodySize; streamOffest += SubMessageBodySize)
            {
                long arraySize = (messageBodySize - streamOffest) > SubMessageBodySize ? SubMessageBodySize : messageBodySize - streamOffest;
                byte[] subMessageBytes = new byte[arraySize];
                int result = bodyStream.Read(subMessageBytes, 0, (int)arraySize);
                MemoryStream subMessageStream = new MemoryStream(subMessageBytes);
                BrokeredMessage subMessage = new BrokeredMessage(subMessageStream, true);
                subMessage.SessionId = sessionId;
                _queueFileClient.Send(subMessage);
                _updateProgress.Invoke();
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
