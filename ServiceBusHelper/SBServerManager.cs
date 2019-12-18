using LogHelper;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.IO;
using System.Threading;

namespace ServiceBusHelper
{
    public class SBServerManager : IMessageReceive
    {
        private readonly ServerSettingsDto _serverSettings;
        private readonly QueueClient _queueClient;
        private readonly QueueClient _queueServerStatusClient;
        private readonly QueueClient _queueClientStatusClient;
        private readonly CancellationTokenSource _cancelTokenSource;

        public SBServerManager(ServerSettingsDto serverSettings)
        {
            _serverSettings = serverSettings;
            _queueClient = QueueClient.Create(serverSettings.FilesQueueName, ReceiveMode.PeekLock);
            _queueServerStatusClient = QueueClient.Create(serverSettings.ServerStatusQueueName);
            _queueClientStatusClient = QueueClient.Create(serverSettings.ClientsStatusQueueName, ReceiveMode.PeekLock);
            _cancelTokenSource = new CancellationTokenSource();

            CreateQueue(_serverSettings.FilesQueueName);
            CreateQueue(_serverSettings.ServerStatusQueueName);
            CreateQueue(_serverSettings.ClientsStatusQueueName);
        }

        [LogPostSharp]
        private void CreateQueue(string queueName)
        {
            var nsManager = NamespaceManager.Create();
            if (!nsManager.QueueExists(queueName))
            {
                nsManager.CreateQueue(queueName);
            }
        }

        public void ReceiveMessage()
        {
            FileMessage largeMessage = ReceiveLargeMessage(_cancelTokenSource);


            SaveMessageToFile(largeMessage);

            
        }
        [LogPostSharp]
        private void SaveMessageToFile(FileMessage largeMessage)
        {
            string folderPath = _serverSettings.FolderPath;
            Stream largeMessageStream = largeMessage.Message.GetBody<Stream>();
            largeMessageStream.Seek(0, SeekOrigin.Begin);
            FileStream fileOut = new FileStream(Path.Combine(folderPath, largeMessage.FileName), FileMode.Create);
            largeMessageStream.CopyTo(fileOut);
            fileOut.Close();
        }

        [LogPostSharp]
        public void CancelOperations()
        {
            SendServerStatus(SBServerStatuses.Stopped);
            _cancelTokenSource.Cancel();
            _queueClient.Close();
            _queueServerStatusClient.Close();
            _queueClientStatusClient.Close();
        }
        [LogPostSharp]
        public FileMessage ReceiveLargeMessage(CancellationTokenSource cancel)
        {
            var largeMessageStream = new MemoryStream();
            MessageSession session = _queueClient.AcceptMessageSession();


            bool isFirst = true;
            string fileName = "";

            while (true)
            {
                BrokeredMessage subMessage = session.Receive(TimeSpan.FromSeconds(5));

                if (subMessage != null)
                {
                    if (cancel.IsCancellationRequested)
                    {
                        break;
                    }
                    try
                    {
                        if (isFirst)
                        {
                            //receive filename
                            fileName = subMessage.GetBody<string>();
                            isFirst = false;
                        }
                        else
                        {
                            //receive filestream
                            Stream subMessageStream = subMessage.GetBody<Stream>();
                            subMessageStream.CopyTo(largeMessageStream);
                        }
                        subMessage.Complete();
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                else
                {
                    
                    break;
                }
            }
            BrokeredMessage largeMessage = new BrokeredMessage(largeMessageStream, true);
            
            return new FileMessage(fileName, largeMessage);
        }
        [LogPostSharp]
        public void SendServerStatus(SBServerStatuses status)
        {
            _queueServerStatusClient.Send(new BrokeredMessage($"{DateTime.Now}: Server has status {status}"));
        }
        [LogPostSharp]
        public string GetClientsStatuses()
        {
            var message = _queueClientStatusClient.Receive();
            return message.GetBody<string>();
        }
    }
}
