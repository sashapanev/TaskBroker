
using System;
using TaskCoordinator.SSSB.EF;

namespace TaskBroker.SSSB
{
    public abstract class BaseMetaDataManager: BaseManager
    {
        private int _MetaDataID;

        public BaseMetaDataManager(IServiceProvider rootProvider, int MetaDataID)
            : base(rootProvider)
        {
            this._MetaDataID = MetaDataID;
        }

        public abstract MetaData RequestContext
        {
            get;
        }

        public abstract bool IsCanceled
        {
            get;
        }

        public abstract bool IsAllTasksCompleted(out bool isCanceled);

        public abstract void SetCompleted();
        
        public abstract void SetCompletedWithError(string message);

        public abstract void SetCompletedWithResult(string result);

        public int MetaDataID
        {
            get { return _MetaDataID; }
        }
    }
}
