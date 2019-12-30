using mucomDotNET.Compiler;
using System;
using System.IO;

namespace mucomDotNET.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.writeLine = writeLine;
#if DEBUG
            Log.level = LogLevel.TRACE;
#else
            Log.level = LogLevel.INFO;
#endif

            if (args == null || args.Length < 1)
            {
                System.Console.WriteLine("引数(.mucファイル)欲しいよぉ");
                return;
            }

            try
            {
                foreach (string arg in args)
                {
                    compiler compiler = new compiler();
                    compiler.Init();
                    byte[] dat = compiler.Start(arg);
                    if (dat != null)
                    {
                        File.WriteAllBytes(compiler.outFileName, dat);
                    }
                }

            }
            catch(Exception ex)
            {
                Log.WriteLine(LogLevel.FATAL, ex.Message);
                Log.WriteLine(LogLevel.FATAL, ex.StackTrace);
            }
        }

        static void writeLine(LogLevel level, string msg)
        {
            System.Console.WriteLine("[{0,-7}] {1}", level, msg);
        }
    }
}
