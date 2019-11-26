﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new ServiceBusManager();
            manager.CreateQueue();
            manager.ReceiveAndDeleteMessage();
        }
    }
}
