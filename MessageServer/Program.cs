
using ServiceBusHelper;
using Topshelf;

namespace MessageServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(
               x => {
                   x.Service(() => new FilesQueueService(new ServerSettingsDto()));
                   x.EnableServiceRecovery(
                            r => r.RestartService(0).RestartService(1));
               }
            );
        }
    }
}
