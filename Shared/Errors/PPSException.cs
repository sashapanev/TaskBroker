using System;
using System.Runtime.Serialization;

namespace Shared.Errors
{
    [Serializable]
    public class PPSException : Exception
    {
        public PPSException()
            : base("UnExpected Application Error")
        {
        }

        public PPSException(string message)
            : base(message)
        {
        }

        public PPSException(Exception innerException)
          : base(innerException.Message, innerException)
        {
        }

        public PPSException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PPSException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
