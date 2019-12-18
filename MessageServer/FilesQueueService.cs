using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using ServiceBusHelper;
using LogHelper;
using Castle.DynamicProxy;

namespace MessageServer
{
    // Неплохая идея не использовать сервера для очередей, а написать свою.
    // Действительно, поднимать специальный сервер для этого было бы избыточно.
    public class FilesQueueService : ServiceControl
    {
        private readonly Timer _mainTimer;
        private readonly Timer _statusTimer;

        // Непонятно, чего именно 20 тысяч. Секунд, миллисекунд?
        private const int QueueListenRepeat = 20_000;
        private readonly ServerSettingsDto _serverSettings;
        private readonly ILogger _logger;
        private IMessageReceive _messageReceive;

        public FilesQueueService(ServerSettingsDto serverSettings)
        {
            _mainTimer = new Timer(WorkProcedure);
            _statusTimer = new Timer(StatusProcedure);
            _logger = new ConsoleLogger();
            var generator = new ProxyGenerator();
           
            _serverSettings = serverSettings;
            _messageReceive = new SBServerManager(serverSettings);
        }

        private void WorkProcedure(object target)
        {
            Task.Factory.StartNew(() => _messageReceive.ReceiveMessage());
        }

        private void StatusProcedure(object target)
        {
            Task.Factory.StartNew(() => _messageReceive.SendServerStatus(SBServerStatuses.Working));
            Task.Factory.StartNew(() => _messageReceive.GetClientsStatuses())
                        .ContinueWith((t) => _logger.LogMessage(t.GetAwaiter().GetResult()));
        }

        public bool Start(HostControl hostControl)
        {
            _mainTimer.Change(0, QueueListenRepeat);
            _statusTimer.Change(0, _serverSettings.StatusSendPeriodMs);
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _mainTimer.Change(Timeout.Infinite, 0);
            _statusTimer.Change(Timeout.Infinite, 0);
            _messageReceive.CancelOperations();
            return true;
        }
    }
}
