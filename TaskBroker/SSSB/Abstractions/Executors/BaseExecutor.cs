using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using TaskCoordinator.Database;
using TaskCoordinator.SSSB;
using TaskCoordinator.SSSB.EF;

namespace TaskBroker.SSSB.Executors
{
    public abstract class BaseExecutor: IExecutor, IDisposable
    {
        protected static readonly ConcurrentDictionary<int, Task<ExecutorSettings>> _staticSettings = new ConcurrentDictionary<int, Task<ExecutorSettings>>();

        public BaseExecutor(ExecutorArgs args)
        {
            Type loggerType = typeof(ILogger<>);
            this.Logger = (ILogger)args.TasksManager.Services.GetRequiredService(loggerType.MakeGenericType(this.GetType()));
            this.Message = args.Message;
            this.TasksManager = args.TasksManager;
            this.TaskInfo = args.TaskInfo;
            this.EventDate = args.Atributes.EventDate;
            this.Parameters = args.Atributes.Parameters;
        }

        protected ILogger Logger { get; }

        protected OnDemandTaskManager TasksManager { get; }
       
        public SSSBMessage Message { get; }

        public Guid ConversationHandle
        {
            get
            {
                return this.Message.ConversationHandle;
            }
        }

        public Guid ConversationGroupID
        {
            get
            {
                return this.Message.ConversationGroupID;
            }
        }

        public virtual string Name
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        /// <summary>
        /// Determines if the message is processed after
        /// the message is read from the queue and transaction commited
        /// The Other Messages from the Queue on the same Dialog can be read
        /// and processed out of sync from the current message processing!!!
        /// Useful only if the processing of the message taskes long time.
        /// </summary>
        public virtual bool IsAsyncProcessing
        {
            get
            {
                return false;
            }
        }

        public IServiceProvider Services
        {
            get
            {
                return this.TasksManager.Services;
            }
        }

        public SSSBDbContext DB
        {
            get
            {
                return this.TasksManager.SSSBDb;
            }
        }

        protected virtual Task BeforeExecuteTask(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected virtual Task<HandleMessageResult> DoExecuteTask(CancellationToken token)
        {
            this.Debug(string.Format("Executing SSSB Task: {0}", this.TaskInfo.OnDemandTaskID.ToString()));
            return Task.FromResult(this.EndDialog());
        }

        protected virtual Task AfterExecuteTask(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task<HandleMessageResult> ExecuteTaskAsync(CancellationToken token)
        {
                await this.BeforeExecuteTask(token);
                try
                {
                    return await this.DoExecuteTask(token);
                }
                finally
                {
                    await this.AfterExecuteTask(token);
                }
        }

        public TaskInfo TaskInfo { get; }

        /// <summary>
        /// When the task was scheduledto the queue
        /// </summary>
        public DateTime EventDate { get; }
        
        /// <summary>
        /// parameters that was passed to the task
        /// </summary>
        public NameValueCollection Parameters { get; }

        public bool HasStaticSettings
        {
            get
            {
                return this.TaskInfo.SettingID.HasValue;
            }
        }

        protected ExecutorSettings StaticSettings
        {
            get
            {
                if (!this.HasStaticSettings)
                    return null;
                return GetStaticSettingsByID(this.TaskInfo.SettingID.Value).GetAwaiter().GetResult();
            }
        }

        protected Task<T> GetStaticSettings<T>()
        where T : class
        {
            if (!this.HasStaticSettings)
                return null;
            return GetStaticSettings<T>(this.TaskInfo.SettingID.Value);
        }

        public Task<ExecutorSettings> GetStaticSettingsByID(int settingID)
        {
            return _staticSettings.GetOrAdd(settingID, (key) => {
                Task<string> settings = TasksManager.GetStaticSettings(key);
                return settings.ContinueWith((antecedent) => new ExecutorSettings(antecedent.Result));
            });
        }

        public async Task<T> GetStaticSettings<T>(int settingID)
        where T : class
        {
            ExecutorSettings settings = default(ExecutorSettings);
            try
            {
                settings = await GetStaticSettingsByID(settingID);
            }
            catch
            {
                _staticSettings.TryRemove(settingID, out var _);
                throw;
            }

            return settings.GetDeserialized<T>();
        }

        public static void FlushStaticSettings()
        {
            _staticSettings.Clear();
        }

        public static void FlushStaticSettings(int settingID)
        {
            _staticSettings.TryRemove(settingID, out var _);
        }

        protected virtual string GetAlertEmails()
        {
            return string.Empty;
        }

        public async Task SendEmail(string subject, string message, bool isHtml)
        {
            string address = this.GetAlertEmails();
            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            var connectionManager = this.Services.GetRequiredService<IConnectionManager>();
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromSeconds(30), TransactionScopeAsyncFlowOption.Enabled))
            using (var connection = await connectionManager.CreateSSSBConnectionAsync(CancellationToken.None))
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "[PPS].[sp_SendEmailNotification]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@address", SqlDbType.NVarChar, 2000) { Value = address });
                cmd.Parameters.Add(new SqlParameter("@subject", SqlDbType.NVarChar, 255) { Value = subject });
                cmd.Parameters.Add(new SqlParameter("@message", SqlDbType.NVarChar) { Value = message });
                cmd.Parameters.Add(new SqlParameter("@type", SqlDbType.SmallInt) { Value = isHtml ? (short)1 : (short)0 });

                await cmd.ExecuteNonQueryAsync();

                transactionScope.Complete();
            }
        }

        [Conditional("DEBUG")]
        public void Debug(string msg)
        {
            Logger.LogInformation(msg);
        }

        protected virtual void Dispose()
        {
            // Debug($"Executor {this.GetType().Name} is Disposed");
        }

        void IDisposable.Dispose()
        {
            this.Dispose();
        }
    }
}
