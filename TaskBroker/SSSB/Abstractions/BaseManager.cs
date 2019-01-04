using Microsoft.Extensions.DependencyInjection;
using System;
using TaskCoordinator.SSSB.EF;

namespace TaskBroker.SSSB
{
    public class BaseManager : IDisposable
    {
        private Lazy<SSSBDbContext> _db;
        private readonly IServiceProvider _services;

        public BaseManager(IServiceProvider services)
        {
            this._services = services;
            this._db = new Lazy<SSSBDbContext>(() => { return this._services.GetRequiredService<SSSBDbContext>(); }, true);
        }

        public SSSBDbContext SSSBDb
        {
            get
            {
                return this._db.Value;
            }
        }

        public IServiceProvider Services => _services;

        protected virtual void OnDispose()
        {
            // NOOP
        }

        #region IDisposable Members

        public void Dispose()
        {
           this.OnDispose();
        }

        #endregion
    }
}
