using Microsoft.EntityFrameworkCore;
using Shared.Database;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskCoordinator.SSSB.EF;

namespace TaskBroker.SSSB
{
    public class MetaDataManager : BaseManager, IMetaDataManager
    {
        private int _metaDataID;
        private MetaData _metaData;

        public MetaDataManager(int MetaDataID, IServiceProvider services) :
            base(services)
        {
            this._metaDataID = MetaDataID;
            this._metaData = null;
        }

        public async Task<MetaData> GetMetaData(CancellationToken token = default(CancellationToken))
        {
            this._metaData = await this.SSSBDb.MetaData.AsNoTracking().Where(md => md.MetaDataId == this.MetaDataID).SingleAsync(token);
            return this._metaData;
        }

        public async Task<bool> IsCanceled(CancellationToken token = default(CancellationToken))
        {
            var entity = await GetMetaData(token);
            return entity.IsCanceled.HasValue && entity.IsCanceled.Value;
        }

        public async Task<CompletionResult> IsAllTasksCompleted(CancellationToken token = default(CancellationToken))
        {
            var entity = await GetMetaData(token);
            if (entity.Error != null)
            {
                return CompletionResult.Error;
            }
            bool isCancelled = entity.IsCanceled.HasValue && entity.IsCanceled.Value;
            if (isCancelled)
            {
                return CompletionResult.Cancelled;
            }
            return (entity.RequestCount == entity.RequestCompleted) ? CompletionResult.Completed : CompletionResult.None;
        }

        private async Task _SetCompleted(string error = null, string result = null, bool? isCancelled = null)
        {
            var idParam = new SqlParameter("@MetaDataID", System.Data.SqlDbType.Int);
            idParam.Value = _metaDataID;
            var resultParam = new SqlParameter("@Result", System.Data.SqlDbType.NVarChar, 255);
            resultParam.Value = NullableHelper.DBNullConvertFrom(result);
            var cancelledParam = new SqlParameter("@isCanceled", System.Data.SqlDbType.Bit);
            cancelledParam.Value = NullableHelper.DBNullConvertFrom(isCancelled);
            var errorParam = new SqlParameter("@ErrorMessage", System.Data.SqlDbType.NVarChar, 4000);
            errorParam.Value = NullableHelper.DBNullConvertFrom(error);

            await this.SSSBDb.Database.ExecuteSqlCommandAsync("EXEC [PPS].[sp_SetCompleted] @MetaDataID, @Result, @isCanceled,  @ErrorMessage", new object[] { idParam, resultParam, cancelledParam, errorParam });
        }

        public Task SetCancelled()
        {
            return _SetCompleted(null, null, true);
        }

        public Task SetCompleted()
        {
            return _SetCompleted();
        }
        
        public Task SetCompletedWithError(string error)
        {
            return _SetCompleted(error);
        }

        public Task SetCompletedWithResult(string result)
        {
            return _SetCompleted(null, result);
        }

        public int MetaDataID
        {
            get { return _metaDataID; }
        }

        public MetaData MetaData
        {
            get { return _metaData; }
        }
    }
}
