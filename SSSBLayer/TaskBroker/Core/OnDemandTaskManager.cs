using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using TaskBroker.SSSB.Executors;
using Coordinator.Database;
using Coordinator.SSSB.EF;

namespace TaskBroker.SSSB
{
    public class OnDemandTaskManager: BaseManager
    {
        private Guid _id = Guid.Empty;
        private IExecutor _currentExecutor;
        private readonly ILogger<OnDemandTaskManager> _logger;
        private static readonly ConcurrentDictionary<int, TaskInfo> _taskInfos = new ConcurrentDictionary<int, TaskInfo>();

        public OnDemandTaskManager(IServiceProvider services, ILogger<OnDemandTaskManager> logger) :
            base(services)
        {
            this._logger = logger;
        }

        public Guid ID
        {
            get
            {
                if (_id == Guid.Empty)
                {
                    _id = Guid.NewGuid();
                }
                return this._id;
            }
        }
      
        public IExecutor CurrentExecutor
        {
            get { return _currentExecutor; }
            set { _currentExecutor = value; }
        }

        public async Task<TaskInfo> GetTaskInfo(int id)
        {
            TaskInfo res;
            if (_taskInfos.TryGetValue(id, out res))
                return res;

            using (var scope = this.Services.CreateScope())
            {
                var provider = scope.ServiceProvider;
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(30), TransactionScopeAsyncFlowOption.Enabled))
                {
                    var database = provider.GetRequiredService<SSSBDbContext>();
                    OnDemandTask task = await database.OnDemandTask.Include((d)=> d.Executor).SingleOrDefaultAsync(t => t.OnDemandTaskId == id);
                    if (task == null)
                        throw new Exception(string.Format("OnDemandTask with taskID={0} was not found", id));
                    res = TaskInfo.FromOnDemandTask(task);
                }
            }

            _taskInfos.TryAdd(id, res);
            return res;
        }

        public static void FlushTaskInfos()
        {
            _taskInfos.Clear();
        }

        public async Task<string> GetExecutorStaticSettings(int taskID)
        {
            var connectionManager = this.Services.GetRequiredService<IConnectionManager>();
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(30), TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = await connectionManager.CreateSSSBConnectionAsync(CancellationToken.None))
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "[PPS].[sp_GetOnDemandExecutorStaticSettings]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("taskID", SqlDbType.Int) { Value = taskID });
                cmd.Parameters.Add(new SqlParameter("@executorSettings", SqlDbType.Xml) { Direction = ParameterDirection.Output });

                await cmd.ExecuteNonQueryAsync();

                return cmd.Parameters["@executorSettings"].Value?.ToString();
            }
        }

        /// <summary>
        /// получить настройки экзекутора
        /// </summary>
        public async Task<string> GetStaticSettings(int settingID)
        {
            var connectionManager = this.Services.GetRequiredService<IConnectionManager>();
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(30), TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = await connectionManager.CreateSSSBConnectionAsync(CancellationToken.None))
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "[PPS].[sp_GetStaticSettings]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@settingID", SqlDbType.Int) { Value = settingID });
                cmd.Parameters.Add(new SqlParameter("@ExecutorSettings", SqlDbType.Xml) { Direction = ParameterDirection.Output });

                await cmd.ExecuteNonQueryAsync();

                return cmd.Parameters["@ExecutorSettings"].Value?.ToString();
            }
        }

        protected override void OnDispose()
        {
            try
            {
                var executor = this._currentExecutor;
                if (executor != null)
                {
                    this._currentExecutor = null;
                    (executor as IDisposable)?.Dispose();
                }
            }
            finally
            {
                base.OnDispose();
            }
        }
    }
}