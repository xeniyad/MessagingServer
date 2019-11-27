using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using ServiceBusHelper;
using LogHelper;

namespace MessageServer
{
    public class FilesQueueService : ServiceControl
    {
        private readonly Timer mainTimer;
        private readonly Timer statusTimer;
        private SBServerManager _sbManager;
        private const int queueListenRepeat = 20_000;
        private ServerSettingsDto _serverSettings;
        private ILogger _logger;

        public FilesQueueService(ServerSettingsDto serverSettings)
        {
            mainTimer = new Timer(WorkProcedure);
            statusTimer = new Timer(StatusProcedure);
            _logger = new ConsoleLogger();
            _sbManager = new SBServerManager(serverSettings, _logger);
            _serverSettings = serverSettings;
        }

        private void WorkProcedure(object target)
        {
            Task.Factory.StartNew(() => _sbManager.ReceiveMessage());
        }

        private void StatusProcedure(object target)
        {
            Task.Factory.StartNew(() => _sbManager.SendServerStatus(SBServerStatuses.Working));
            Task.Factory.StartNew(() => _sbManager.GetClientsStatuses())
                        .ContinueWith((t) => _logger.LogMessage(t.GetAwaiter().GetResult()));
        }

        public bool Start(HostControl hostControl)
        {
            mainTimer.Change(0, queueListenRepeat);
            statusTimer.Change(0, _serverSettings.StatusSendPeriodMs);
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            mainTimer.Change(Timeout.Infinite, 0);
            statusTimer.Change(Timeout.Infinite, 0);
            _sbManager.CancelOperations();
            return true;
        }
    }
}
