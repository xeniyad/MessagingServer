﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using ServiceBusHelper;

namespace MessagingClient
{
    public class BrokerMessageSender
    {
        private readonly IMessageSend _messageClient;

        public BrokerMessageSender(IMessageSend messageClient)
        {
            _messageClient = messageClient ?? throw new ArgumentNullException(nameof(messageClient));
        }

        public Task SendFile(string filename)
        {
            var file = new BrokeredMessage(OpenFile(filename));
            var fileMessage = new FileMessage(Path.GetFileName(filename), file);

            return Task.Factory.StartNew(() => _messageClient.Send(fileMessage));
        }

        private FileStream OpenFile(string filename)
        {
            try
            {
                return new FileStream(filename, FileMode.Open);
            }
            catch (Exception exception)
            {
                // Создавая подобные кастомные эксепшены с человеко-читаемым текстом, мы упрощаем работу с логами.
                // ПРи этом не забываем класть пойманное исключение из API в наше создаваемое как innerException.
                throw new InvalidOperationException($"Cannot open file {filename}", exception);
            }
        }
    }
}