using System;
using System.Collections.Generic;
using System.Text;
using Castle.DynamicProxy;
using System.Text.Json;
using System.IO;

namespace LogHelper
{
    public class LogInterceptor : IInterceptor
    {
        public LogInterceptor(string path)
        {
            _path = path;
        }

        string _path;
        public static readonly object locker = new object();
        private System.Threading.Mutex mutex = new System.Threading.Mutex();

        public void Intercept(IInvocation invocation)
        {
            var args = invocation.Arguments;
            var method = invocation.Method;
            var time = DateTime.Now;
            string json = JsonSerializer.Serialize(args);
            mutex.WaitOne();
            lock (locker) {
                if (File.Exists(_path))
                {
                    File.AppendAllText(_path, $"\n {time} Method name: {method.Name} \n arguments: {json}");
                }
            }
            mutex.ReleaseMutex();
            invocation.Proceed();
        }
    }
}
