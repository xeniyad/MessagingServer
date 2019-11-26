using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageServer
{
    class ServiceBusManager
    {
        private const string queueName = "FilesQueue";

        private const string fileNameBase = "sbtest_";

        private int currentFileNumber = 1;

        public void CreateQueue()
        {
            var nsManager = NamespaceManager.Create();

            if (!nsManager.QueueExists(queueName))
            {
                nsManager.CreateQueue(queueName);
                Console.WriteLine($"Queue {queueName} created");
            }
            else
            {
                Console.WriteLine($"Queue {queueName} already exist");
            }
        }

        public void ReceiveAndDeleteMessage()
        {
            QueueClient queueClient = QueueClient.Create(queueName, ReceiveMode.ReceiveAndDelete);

            LargeMessageReceiver receiver = new LargeMessageReceiver(queueClient);

            BrokeredMessage largeMessage = receiver.Receive();



            Console.WriteLine("Received message");

            Console.WriteLine("Message body size: " + largeMessage.Size);



            string testFile = $"{fileNameBase}_{currentFileNumber}";

            Console.WriteLine("Saving file: " + testFile);



            // Save the message body as a file.

            Stream largeMessageStream = largeMessage.GetBody<Stream>();

            largeMessageStream.Seek(0, SeekOrigin.Begin);

            FileStream fileOut = new FileStream(testFile, FileMode.Create);

            largeMessageStream.CopyTo(fileOut);

            fileOut.Close();



            Console.WriteLine("Done!");

            queueClient.Close();
        }
    }
}
