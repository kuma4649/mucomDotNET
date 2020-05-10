using mucomDotNET.Compiler;
using mucomDotNET.Common;
using musicDriverInterface;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace mucomDotNET.Console
{
    class Program
    {
        private static string srcFile;
        private static bool isXml = false;

        static void Main(string[] args)
        {
            Log.writeLine = WriteLine;
#if DEBUG
            Log.level = LogLevel.INFO;//.INFO;
            Log.off = 0;
#else
            Log.level = LogLevel.INFO;
#endif
            int fnIndex = AnalyzeOption(args);

            if (args == null || args.Length < 1 + fnIndex)
            {
                WriteLine(LogLevel.INFO, msg.get("I0600"));
                return;
            }

            try
            {
#if NETCOREAPP
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif

                Compile(args[fnIndex], (args.Length > fnIndex + 1 ? args[fnIndex + 1] : null));

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

        //static void Compile(string srcFile)
        //{
        //    try
        //    {
        //        Program.srcFile = srcFile;

        //        Compiler.Compiler compiler = new Compiler.Compiler();
        //        compiler.Init();

        //        if (!isXml)
        //        {
        //            string destFileName = Path.Combine(
        //                Path.GetDirectoryName(Path.GetFullPath(srcFile))
        //                , string.Format("{0}.mub", Path.GetFileNameWithoutExtension(srcFile)));

        //            using (FileStream sourceMML = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
        //            using (FileStream destCompiledBin = new FileStream(destFileName, FileMode.Create, FileAccess.Write))
        //            using (Stream bufferedDestStream = new BufferedStream(destCompiledBin))
        //            {
        //                compiler.Compile(sourceMML, bufferedDestStream, appendFileReaderCallback);
        //            }
        //        }
        //        else
        //        {
        //            string destFileName = Path.Combine(
        //                Path.GetDirectoryName(Path.GetFullPath(srcFile))
        //                , string.Format("{0}.xml", Path.GetFileNameWithoutExtension(srcFile)));
        //            MmlDatum[] dest = null;

        //            using (FileStream sourceMML = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
        //            {
        //                dest = compiler.Compile(sourceMML, appendFileReaderCallback);
        //            }

        //            XmlSerializer serializer = new XmlSerializer(typeof(MmlDatum[]), typeof(MmlDatum[]).GetNestedTypes());
        //            using (StreamWriter sw = new StreamWriter(destFileName, false, Encoding.UTF8))
        //            {
        //                serializer.Serialize(sw, dest);
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteLine(LogLevel.FATAL, ex.Message);
        //        Log.WriteLine(LogLevel.FATAL, ex.StackTrace);
        //    }
        //    finally
        //    {
        //    }

        //}

        static void Compile(string srcFile, string destFile = null)
        {
            try
            {
                if (Path.GetExtension(srcFile) == "")
                    srcFile = Path.Combine(
                    Path.GetDirectoryName(Path.GetFullPath(srcFile))
                    , string.Format("{0}.muc", Path.GetFileNameWithoutExtension(srcFile))
                    );

                srcFile = Path.Combine(
                Path.GetDirectoryName(Path.GetFullPath(srcFile))
                , srcFile
                );

                Program.srcFile = srcFile;

                Compiler.Compiler compiler = new Compiler.Compiler();
                compiler.Init();

                //compiler.SetCompileSwitch("IDE");
                //compiler.SetCompileSwitch("SkipPoint=R19:C30");

                if (!isXml)
                {
                    string destFileName = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(srcFile)), string.Format("{0}.mub", Path.GetFileNameWithoutExtension(srcFile)));
                    if (destFile != null)
                    {
                        destFileName = destFile;
                    }

                    if (!File.Exists(srcFile))
                    {
                        Log.WriteLine(LogLevel.ERROR, string.Format(msg.get("E0601"), srcFile));
                        return;
                    }

                    bool isSuccess = false;
                    using (FileStream sourceMML = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (MemoryStream destCompiledBin = new MemoryStream())
                    using (Stream bufferedDestStream = new BufferedStream(destCompiledBin))
                    {
                        isSuccess= compiler.Compile(sourceMML, bufferedDestStream, appendFileReaderCallback);

                        if (isSuccess)
                        {
                            bufferedDestStream.Flush();
                            byte[] destbuf = destCompiledBin.ToArray();
                            File.WriteAllBytes(destFileName, destbuf);
                        }
                    }
                }
                else
                {
                    string destFileName = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(srcFile)), string.Format("{0}.xml", Path.GetFileNameWithoutExtension(srcFile)));
                    if (destFile != null)
                    {
                        destFileName = destFile;
                    }
                    MmlDatum[] dest = null;

                    using (FileStream sourceMML = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        dest = compiler.Compile(sourceMML, appendFileReaderCallback);
                    }

                    XmlSerializer serializer = new XmlSerializer(typeof(MmlDatum[]), typeof(MmlDatum[]).GetNestedTypes());
                    using (StreamWriter sw = new StreamWriter(destFileName, false, Encoding.UTF8))
                    {
                        serializer.Serialize(sw, dest);
                    }

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


        private static int AnalyzeOption(string[] args)
        {
            if (args.Length < 1) return 0;

            int i = 0;
            while (i<args.Length &&args[i] != null && args[i].Length > 0 && args[i][0] == '-')
            {
                string op = args[i].Substring(1).ToUpper();
                if (op == "LOGLEVEL=FATAL")
                {
                    Log.level = LogLevel.FATAL;
                }
                else if (op == "LOGLEVEL=ERROR")
                {
                    Log.level = LogLevel.ERROR;
                }
                else if (op == "LOGLEVEL=WARNING")
                {
                    Log.level = LogLevel.WARNING;
                }
                else if (op == "LOGLEVEL=INFO")
                {
                    Log.level = LogLevel.INFO;
                }
                else if (op == "LOGLEVEL=DEBUG")
                {
                    Log.level = LogLevel.DEBUG;
                }
                else if (op == "LOGLEVEL=TRACE")
                {
                    Log.level = LogLevel.TRACE;
                }

                if (op == "OFFLOG=WARNING")
                {
                    Log.off = (int)LogLevel.WARNING;
                }

                if(op=="XML")
                {
                    isXml = true;
                }

                i++;
            }

            return i;
        }

    }
}