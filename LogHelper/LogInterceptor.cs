using System;
using System.Collections.Generic;
using System.Text;
using PostSharp.Aspects;
using System.Text.Json;
using System.IO;

namespace LogHelper
{
    [Serializable]
    public class LogPostSharp : OnMethodBoundaryAspect
    {
        string _path = @"C:\Users\xeniya_denissova\Desktop\slogs.txt";


        public override void OnEntry(MethodExecutionArgs invocation)
        {
            var args = invocation.Arguments;
            var method = invocation.Method;
            var time = DateTime.Now;
            string json = JsonSerializer.Serialize(args);
            object locker = new object();
            System.Threading.Mutex mutex = new System.Threading.Mutex();
            mutex.WaitOne();
            lock (locker)
            {
                if (File.Exists(_path))
                {
                    File.AppendAllText(_path, $"\n {time} Method name: {method.Name} \n arguments: {json}");
                }
            }
            mutex.ReleaseMutex();
            invocation.FlowBehavior = FlowBehavior.Default;
        }
    }
}
