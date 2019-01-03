using System;

namespace TaskBroker.SSSB
{
    public enum LockMode
    {
        Exclusive,
        Shared,
        Update
    }

    public abstract class BaseLockManager : BaseManager
    {
        public BaseLockManager(IServiceProvider rootProvider)
            : base(rootProvider)
        {
        }

        public abstract void LockResource(Guid id);

        public abstract void LockResource(Guid id, LockMode lockMode, int timeOutMsec);

        public abstract bool TryLockResource(Guid id, LockMode lockMode);

        public abstract void UnLockResource(Guid id);
    }
}
