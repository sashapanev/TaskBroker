using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB.Utils;

namespace TaskCoordinator.SSSB
{
    public class DeferMessageResult : HandleMessageResult
    {
        private readonly IServiceBrokerHelper _serviceBrokerHelper;
        private readonly string _fromService;
        private readonly TimeSpan _lifeTime;
        private readonly DateTime _activationTime;
        private readonly int _attemptNumber;

        public class Args
        {
            public string fromService { get; set; }
            public DateTime activationTime { get; set; }
            public TimeSpan? lifeTime { get; set; }
            public int attemptNumber { get; set; }
        }

        public DeferMessageResult(IServiceBrokerHelper serviceBrokerHelper, Args args)
        {
            _serviceBrokerHelper = serviceBrokerHelper;
            _fromService = args.fromService ?? throw new ArgumentNullException(nameof(args.fromService));
            _lifeTime = args.lifeTime ?? TimeSpan.FromHours(12);
            _activationTime = args.activationTime;
            _attemptNumber = args.attemptNumber;
        }

        public override Task Execute(SqlConnection dbconnection, SSSBMessage message, CancellationToken token)
        {
            return _serviceBrokerHelper.SendPendingMessage(dbconnection, message, _fromService, _lifeTime, false, _activationTime, null, _attemptNumber);
        }
    }
}
