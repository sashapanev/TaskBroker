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
        private readonly bool _isOneWay;
        private readonly Guid? _initiatorConversationGroupID;

        public class DeferArgs
        {
            public DeferArgs()
            {
                IsOneWay = true;
            }

            public string fromService { get; set; }
            public DateTime activationTime { get; set; }
            public TimeSpan? lifeTime { get; set; }
            public bool IsOneWay { get; set; }
            public Guid? initiatorConversationGroupID { get; set; }
        }

        public DeferMessageResult(IServiceBrokerHelper serviceBrokerHelper, DeferArgs args)
        {
            _serviceBrokerHelper = serviceBrokerHelper;
            _fromService = args.fromService ?? throw new ArgumentNullException(nameof(args.fromService));
            _lifeTime = args.lifeTime ?? TimeSpan.FromDays(1);
            _activationTime = args.activationTime;
            _isOneWay = args.IsOneWay;
            _initiatorConversationGroupID = args.initiatorConversationGroupID;
        }

        public override Task Execute(SqlConnection dbconnection, SSSBMessage message, CancellationToken token)
        {
            return _serviceBrokerHelper.SendPendingMessage(dbconnection,_fromService, message, _lifeTime, false, _initiatorConversationGroupID, _activationTime, null, _isOneWay);
        }
    }
}
