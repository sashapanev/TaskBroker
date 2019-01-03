using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace TaskCoordinator.Database
{
    [Serializable]
    public class DBWrapperException : Exception
    {
        public DBWrapperException()
            : base()
        {
        }

        public DBWrapperException(string message)
            : base(message)
        {
        }

        public DBWrapperException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DBWrapperException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class DeadLockDBWrapperException : DBWrapperException
    {
        public DeadLockDBWrapperException()
            : base()
        {
        }

        public DeadLockDBWrapperException(string message)
            : base(message)
        {
        }

        public DeadLockDBWrapperException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DeadLockDBWrapperException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }

    [Serializable]
    public class UniqueConstraintDBWrapperException : DBWrapperException
    {
        public UniqueConstraintDBWrapperException()
            : base()
        {
        }

        public UniqueConstraintDBWrapperException(string message)
            : base(message)
        {
        }

        public UniqueConstraintDBWrapperException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        protected UniqueConstraintDBWrapperException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class ReferenceConstraintDBWrapperException : DBWrapperException
    {
        public ReferenceConstraintDBWrapperException()
            : base()
        {
        }

        public ReferenceConstraintDBWrapperException(string message)
            : base(message)
        {
        }

        public ReferenceConstraintDBWrapperException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ReferenceConstraintDBWrapperException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    public class DBWrapperExceptionsHelper
    {
        public const int SqlServerUniqueConstraintErrorNumber = 2627;
        public const int SqlServerUniqueIndexErrorNumber = 2601;
        public const int SqlServerReferenceConstraintErrorNumber = 547;
        public const int SqlServerDeadLockErrorNumber = 1205;
        public const int SqlServerWaitResourceRerunQueryErrorNumber = 8645;

        public static void ThrowError(SqlException ex)
        {
            ThrowError(ex, string.Empty); 
        }

        public static void ThrowError(SqlException ex, string UserFriendlyMessage)
        {
            Debug.Assert(ex != null,"Parameter ex must not be null");

            if (ex.Number == SqlServerUniqueConstraintErrorNumber || ex.Number == SqlServerUniqueIndexErrorNumber)
            {
                if (string.IsNullOrEmpty(UserFriendlyMessage))
                    throw new UniqueConstraintDBWrapperException(ex.Message, ex);
                else
                    throw new UniqueConstraintDBWrapperException(UserFriendlyMessage, ex);
            }

            if (ex.Number == SqlServerReferenceConstraintErrorNumber)
            {
                if (string.IsNullOrEmpty(UserFriendlyMessage))
                    throw new ReferenceConstraintDBWrapperException(ex.Message, ex);
                else
                    throw new ReferenceConstraintDBWrapperException(UserFriendlyMessage, ex);
            }

            if (ex.Number == SqlServerDeadLockErrorNumber)
            {
                if (string.IsNullOrEmpty(UserFriendlyMessage))
                    throw new DeadLockDBWrapperException(ex.Message, ex);
                else
                    throw new DeadLockDBWrapperException(UserFriendlyMessage, ex);
            }

            if (string.IsNullOrEmpty(UserFriendlyMessage))
                throw new DBWrapperException(UserFriendlyMessage, ex);
            else
                throw new DBWrapperException("Database error", ex);
        }
    }		

}
