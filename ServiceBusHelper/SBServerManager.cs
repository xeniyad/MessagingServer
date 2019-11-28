using LogHelper;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.IO;
using System.Threading;

namespace ServiceBusHelper
{
    public class SBServerManager
    {
        private readonly ServerSettingsDto _serverSettings;
        private readonly QueueClient _queueClient;
        private readonly QueueClient _queueServerStatusClient;
        private readonly QueueClient _queueClientStatusClient;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancelTokenSource;

        public SBServerManager(ServerSettingsDto serverSettings, ILogger logger)
        {
            _serverSettings = serverSettings;
            _queueClient = QueueClient.Create(serverSettings.FilesQueueName, ReceiveMode.PeekLock);
            _queueServerStatusClient = QueueClient.Create(serverSettings.ServerStatusQueueName);
            _queueClientStatusClient = QueueClient.Create(serverSettings.ClientsStatusQueueName, ReceiveMode.PeekLock);
            _logger = logger;
            _cancelTokenSource = new CancellationTokenSource();

            CreateQueue(_serverSettings.FilesQueueName);
            CreateQueue(_serverSettings.ServerStatusQueueName);
            CreateQueue(_serverSettings.ClientsStatusQueueName);
        }

        // Если метод не дергается снаружи, то и выставлять его наружу "по-умолчанию" не стоит.
        private void CreateQueue(string queueName)
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

        public void ReceiveMessage()
        {
            FileMessage largeMessage = ReceiveLargeMessage(_cancelTokenSource);

            _logger.LogMessage("Received message");
            _logger.LogMessage("Message body size: " + largeMessage.Message.Size);
            _logger.LogMessage("Saving file: " + largeMessage.FileName);

            SaveMessageToFile(largeMessage);

            _logger.LogMessage("Done!");
            
        }

        private void SaveMessageToFile(FileMessage largeMessage)
        {
            string folderPath = _serverSettings.FolderPath;
            Stream largeMessageStream = largeMessage.Message.GetBody<Stream>();
            largeMessageStream.Seek(0, SeekOrigin.Begin);
            FileStream fileOut = new FileStream(Path.Combine(folderPath, largeMessage.FileName), FileMode.Create);
            largeMessageStream.CopyTo(fileOut);
            fileOut.Close();
        }


        public void CancelOperations()
        {
            SendServerStatus(SBServerStatuses.Stopped);
            _cancelTokenSource.Cancel();
            _queueClient.Close();
            _queueServerStatusClient.Close();
            _queueClientStatusClient.Close();
        }

        public FileMessage ReceiveLargeMessage(CancellationTokenSource cancel)
        {
            var largeMessageStream = new MemoryStream();
            MessageSession session = _queueClient.AcceptMessageSession();

            _logger.LogMessage($"Message session Id: {session.SessionId}");
            _logger.LogMessage("Receiving sub messages");

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
                        _logger.LogException(ex);
                    }
                }
                else
                {
                    _logger.LogMessage("Done!");
                    break;
                }
            }
            BrokeredMessage largeMessage = new BrokeredMessage(largeMessageStream, true);
            
            return new FileMessage(fileName, largeMessage);
        }

        public void SendServerStatus(SBServerStatuses status)
        {
            _queueServerStatusClient.Send(new BrokeredMessage($"{DateTime.Now}: Server has status {status}"));
        }

        public string GetClientsStatuses()
        {
            var message = _queueClientStatusClient.Receive();
            return message.GetBody<string>();
        }
    }
}
