using System;
using System.Runtime.Serialization;

namespace Shared.Errors
{
    [Serializable]
    public class DBWrapperException : PPSException
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
}
