using mucomDotNET.Compiler;
using mucomDotNET.Common;
using musicDriverInterface;
using System;
using System.IO;
using System.Collections.Generic;

namespace mucomDotNET.Console
{
    class Program
    {
        private static string srcFile;

        static void Main(string[] args)
        {
            Log.writeLine = WriteLine;
#if DEBUG
            Log.level = LogLevel.INFO;//.INFO;
#else
            Log.level = LogLevel.INFO;
#endif

            if (args == null || args.Length < 1)
            {
                WriteLine(LogLevel.ERROR, msg.get("E0600"));
                return;
            }

            try
            {
#if NETCOREAPP
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
                if (args.Length > 2)
                {
                    //foreach (string arg in args)
                    //{
                        //Compile(arg);
                    //}
                }
                else
                {
                    Compile(args[0], (args.Length > 1 ? args[1] : null));
                }

            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.FATAL, ex.Message);
                Log.WriteLine(LogLevel.FATAL, ex.StackTrace);
            }
        }

        static void WriteLine(LogLevel level, string msg)
        {
            System.Console.WriteLine("[{0,-7}] {1}", level, msg);
        }

        static void WriteLine(string msg)
        {
            System.Console.WriteLine(msg);
        }

        static void Compile(string srcFile)
        {
            try
            {
                Program.srcFile = srcFile;

                Compiler.Compiler compiler = new Compiler.Compiler();
                compiler.Init();

                string destFileName = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(srcFile)), string.Format("{0}.mub", Path.GetFileNameWithoutExtension(srcFile)));
                using (FileStream sourceMML = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream destCompiledBin = new FileStream(destFileName, FileMode.Create, FileAccess.Write))
                using (Stream bufferedDestStream = new BufferedStream(destCompiledBin))
                {
                    compiler.Compile(sourceMML, bufferedDestStream, appendFileReaderCallback);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.FATAL, ex.Message);
                Log.WriteLine(LogLevel.FATAL, ex.StackTrace);
            }
            finally
            {
            }

        }

        static void Compile(string srcFile, string destFile = null)
        {
            try
            {
                Program.srcFile = srcFile;

                Compiler.Compiler compiler = new Compiler.Compiler();
                compiler.Init();

                string destFileName = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(srcFile)), string.Format("{0}.mub", Path.GetFileNameWithoutExtension(srcFile)));
                if (destFile != null)
                {
                    destFileName = destFile;
                }

                using (FileStream sourceMML = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream destCompiledBin = new FileStream(destFileName, FileMode.Create, FileAccess.Write))
                using (Stream bufferedDestStream = new BufferedStream(destCompiledBin))
                {
                    compiler.Compile(sourceMML, bufferedDestStream, appendFileReaderCallback);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.FATAL, ex.Message);
                Log.WriteLine(LogLevel.FATAL, ex.StackTrace);
            }
            finally
            {
            }

        }


        private static Stream appendFileReaderCallback(string arg)
        {

            string fn = Path.Combine(
                Path.GetDirectoryName(srcFile)
                , arg
                );

            FileStream strm;
            try
            {
                strm = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (IOException)
            {
                strm = null;
            }

            return strm;
        }



    }
}