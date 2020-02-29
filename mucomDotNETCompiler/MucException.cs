using mucomDotNET.Common;
using System;
using System.Runtime.Serialization;

namespace mucomDotNET.Compiler
{
    [Serializable]
    public class MucException : Exception
    {
        public MucException()
        {
        }

        public MucException(string message) : base(message)
        {
        }

        public MucException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MucException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MucException(string message, int row, int col) : base(string.Format(msg.get("E0300"), row, col, message))
        {
        }
    }
}