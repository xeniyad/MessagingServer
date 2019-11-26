using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageServer

{
    public class LargeMessageReceiver

    {

        private QueueClient m_QueueClient;



        public LargeMessageReceiver(QueueClient queueClient)

        {

            m_QueueClient = queueClient;

        }



        public BrokeredMessage Receive()

        {

            // Create a memory stream to store the large message body.

            MemoryStream largeMessageStream = new MemoryStream();



            // Accept a message session from the queue.

            MessageSession session = m_QueueClient.AcceptMessageSession();

            Console.WriteLine("Message session Id: " + session.SessionId);

            Console.Write("Receiving sub messages");



            while (true)

            {

                // Receive a sub message

                BrokeredMessage subMessage = session.Receive(TimeSpan.FromSeconds(5));



                if (subMessage != null)

                {

                    // Copy the sub message body to the large message stream.

                    Stream subMessageStream = subMessage.GetBody<Stream>();

                    subMessageStream.CopyTo(largeMessageStream);



                    // Mark the message as complete.

                    subMessage.Complete();

                    Console.Write(".");

                }

                else

                {

                    // The last message in the sequence is our completeness criteria.

                    Console.WriteLine("Done!");

                    break;

                }

            }



            // Create an aggregated message from the large message stream.

            BrokeredMessage largeMessage = new BrokeredMessage(largeMessageStream, true);

            return largeMessage;

        }

    }


}
