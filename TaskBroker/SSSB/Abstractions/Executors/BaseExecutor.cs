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
            this.ConversationHandle = args.Message.ConversationHandle;
            this.TasksManager = args.TasksManager;
            this.TaskInfo = args.TaskInfo;
            this.EventDate = args.EventDate;
            this.Parameters = args.Parameters;
        }

        protected ILogger Logger { get; }

        protected OnDemandTaskManager TasksManager { get; }
       
        public SSSBMessage Message { get; }

        public Guid ConversationHandle { get; }

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
            return Task.FromResult(EndDialog());
        }

        protected virtual Task AfterExecuteTask(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        #region HandleMessage Results Helper Methods
        public HandleMessageResult Noop()
        {
            var res = (HandleMessageResult)this.Services.GetRequiredService<NoopMessageResult>();
            return res;
        }

        public HandleMessageResult EndDialogWithError(string error, int? errocode, Guid? conversationHandle = null)
        {
            EndDialogMessageResult.Args args = new EndDialogMessageResult.Args()
            {
                error= error,
                errorCode= errocode,
                conversationHandle = conversationHandle
            };
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<EndDialogMessageResult>(this.Services, new object[] { args });
            return res;
        }

        public HandleMessageResult EndDialog(Guid? conversationHandle = null)
        {
            EndDialogMessageResult.Args args = new EndDialogMessageResult.Args()
            {
               conversationHandle= conversationHandle
            };
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<EndDialogMessageResult>(this.Services, new object[] { args });
            return res;
        }

        public HandleMessageResult Defer(string fromService, DateTime activationTime, Guid? initiatorConversationGroupID = null, TimeSpan? lifeTime = null)
        {
            if (string.IsNullOrEmpty(fromService))
                throw new ArgumentNullException(nameof(fromService));
            DeferMessageResult.Args args = new DeferMessageResult.Args() {
                IsOneWay = true,
                fromService = fromService,
                activationTime = activationTime,
                initiatorConversationGroupID = initiatorConversationGroupID,
                lifeTime = lifeTime
            };
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<DeferMessageResult>(this.Services, new object[] { args });
            return res;
        }

        public HandleMessageResult StepCompleted(Guid? conversationHandle = null)
        {
            StepCompleteMessageResult.Args args = new StepCompleteMessageResult.Args()
            {
                conversationHandle = conversationHandle
            };
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<StepCompleteMessageResult>(this.Services, new object[] { args });
            return res;
        }

        public HandleMessageResult EmptyMessage(Guid? conversationHandle = null)
        {
            EmptyMessageResult.Args args = new EmptyMessageResult.Args()
            {
                conversationHandle = conversationHandle
            };
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<EmptyMessageResult>(this.Services, new object[] { args });
            return res;
        }

        public HandleMessageResult CombinedResult(params HandleMessageResult[] resultHandlers)
        {
            var res = (HandleMessageResult)ActivatorUtilities.CreateInstance<CombinedMessageResult>(this.Services, new object[] { resultHandlers });
            return res;
        }
        #endregion

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
