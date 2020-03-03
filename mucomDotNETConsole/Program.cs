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
        private static List<Stream> appendStream = null;
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
                foreach (string arg in args)
                {
                    Compile(arg);
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

                MmlDatum[] ret = null;
                Program.srcFile = srcFile;

                iCompiler compiler = new Compiler.Compiler();
                compiler.Init();
                using (FileStream sourceMML = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    ret = compiler.Compile(sourceMML, appendFileReaderCallback);
                }

                if (ret != null) WriteMUBFile(srcFile, ret);

            }
            catch (Exception ex)
            {
                WriteLine(string.Format(
                    "Message\r\n{0}\r\nStackTrace\r\n{1}\r\n"
                    , ex.Message
                    , ex.StackTrace
                    ));
            }
            finally
            {
                if (appendStream != null)
                {
                    foreach (Stream strm in appendStream)
                    {
                        if (strm != null) strm.Close();
                    }
                }

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

            if (appendStream == null) appendStream = new List<Stream>();
            appendStream.Add(strm);

            return strm;
        }

        private static void WriteMUBFile(string srcFile, MmlDatum[] aryMd)
        {
            List<byte> dat = new List<byte>();
            foreach (MmlDatum md in aryMd)
            {
                dat.Add((byte)md.dat);
            }

            string fn = Path.Combine(
                Path.GetDirectoryName(srcFile)
                , Path.GetFileNameWithoutExtension(srcFile) + ".mub"
                );

            File.WriteAllBytes(fn, dat.ToArray());
        }


    }
}