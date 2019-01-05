﻿using Microsoft.Extensions.Logging;
using Shared.Errors;
using System;
using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB;

namespace TaskBroker.SSSB.Executors
{
    public class MultyStepExecutor : BaseExecutor
    {
        private int _metaDataID = -1;
        private string _error = null;
        private Lazy<IMetaDataManager> _metaDataManager;

        public MultyStepExecutor(ExecutorArgs args):
            base(args)
        {
            this._metaDataManager = new Lazy<IMetaDataManager>(() => new MetaDataManager(this._metaDataID, this.Services), true);
        }

        protected IMetaDataManager MetaDataManager
        {
            get
            {
                return _metaDataManager.Value;
            }
        }

        protected override void BeforeExecuteTask()
        {
            try
            {
                this._metaDataID = Int32.Parse(this.Parameters["MetaDataID"]);
            }
            catch (Exception ex)
            {
                Logger.LogError(ErrorHelper.GetFullMessage(ex));
                _error = ex.Message;
            }
        }

        protected override async Task<HandleMessageResult> DoExecuteTask(CancellationToken token)
        {
            if (!string.IsNullOrEmpty(_error))
            {
                throw new OperationCanceledException();
            }

            var metaData = await MetaDataManager.GetMetaData(token);
            if (metaData.IsCanceled == true)
            {
                throw new OperationCanceledException();
            }

            this.Debug(string.Format("Executing SSSB Task: {0}", this.TaskInfo.OnDemandTaskID.ToString()));
            await Task.Delay(3000);

            CompletionResult completion = await MetaDataManager.SetCompleted();
           
            return EndDialog();
        }
    }
}
