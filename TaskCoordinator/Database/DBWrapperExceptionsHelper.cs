using Microsoft.Extensions.Logging;
using Shared.Errors;
using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Coordinator.Database
{
    public static class DBWrapperExceptionsHelper
    {
        public const int SqlServerUniqueConstraintErrorNumber = 2627;
        public const int SqlServerUniqueIndexErrorNumber = 2601;
        public const int SqlServerReferenceConstraintErrorNumber = 547;
        public const int SqlServerDeadLockErrorNumber = 1205;
        public const int SqlServerWaitResourceRerunQueryErrorNumber = 8645;

        public static void ThrowError(SqlException ex, ILogger logger)
        {
            ThrowError(ex, string.Empty, logger); 
        }

        public static void ThrowError(SqlException ex, string UserFriendlyMessage, ILogger logger)
        {
            Debug.Assert(ex != null,"Parameter ex must not be null");

            Exception exception = ex;

            if (ex.Number == SqlServerUniqueConstraintErrorNumber || ex.Number == SqlServerUniqueIndexErrorNumber)
            {
                if (string.IsNullOrEmpty(UserFriendlyMessage))
                    exception = new UniqueConstraintDBWrapperException(ex.Message, ex);
                else
                    exception = new UniqueConstraintDBWrapperException(UserFriendlyMessage, ex);
            }

            if (ex.Number == SqlServerReferenceConstraintErrorNumber)
            {
                if (string.IsNullOrEmpty(UserFriendlyMessage))
                    exception = new ReferenceConstraintDBWrapperException(ex.Message, ex);
                else
                    exception = new ReferenceConstraintDBWrapperException(UserFriendlyMessage, ex);
            }

            if (ex.Number == SqlServerDeadLockErrorNumber)
            {
                if (string.IsNullOrEmpty(UserFriendlyMessage))
                    exception = new DeadLockDBWrapperException(ex.Message, ex);
                else
                    exception = new DeadLockDBWrapperException(UserFriendlyMessage, ex);
            }

            if (string.IsNullOrEmpty(UserFriendlyMessage))
                exception = new DBWrapperException(UserFriendlyMessage, ex);
            else
                exception = new DBWrapperException("Database Error", ex);

            if (exception is PPSException)
            {
                logger.LogError(ErrorHelper.GetFullMessage(exception));
            }

            throw exception;
        }
    }		

}
