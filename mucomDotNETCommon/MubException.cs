using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace mucomDotNET.Common
{
    [Serializable]
    public class MubException : Exception
    {
        public MubException()
        {
        }

        public MubException(string message) : base(message)
        {
        }

        public MubException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MubException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MubException(string message, int row, int col) : base(string.Format(msg.get("E0300"), row, col, message))
        {
        }
    }
}
