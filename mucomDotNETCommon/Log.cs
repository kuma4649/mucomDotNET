using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.Common
{
    public static class Log
    {
        public static Action<LogLevel, string> writeLine = null;
        public static LogLevel level = LogLevel.INFO;
        public static Action<string> writeMethod;

        public static void WriteLine(LogLevel level, string msg)
        {

            if (level <= Log.level)
            {
                if (writeMethod != null)
                    writeMethod(String.Format("[{0,-7}] {1}", level, msg));
                else
                    writeLine?.Invoke(level, msg);
            }
        }
    }
}
