using Microsoft.Extensions.DependencyInjection;
using System;
using TaskCoordinator.SSSB.EF;

namespace TaskBroker.SSSB
{
    public class BaseManager : IDisposable
    {
        private SSSBDbContext _db;
        private readonly IServiceProvider _services;

        public BaseManager(IServiceProvider services)
        {
            this._services = services;
        }

        public SSSBDbContext SSSBDb
        {
            get
            {
                if (this._db== null)
                {

                    this._db = this._services.GetRequiredService<SSSBDbContext>();
                }

                return this._db;
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
