using System;
using System.Runtime.Serialization;

namespace Octopus.Exceptions
{
    [Serializable]
    public class OutOfLengthLimitationException : Exception
    {
        public OutOfLengthLimitationException() : base() { }

        public OutOfLengthLimitationException(string message) : base(message) { }

        public OutOfLengthLimitationException(string message, Exception innerException) : base(message, innerException) { }

        private OutOfLengthLimitationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
