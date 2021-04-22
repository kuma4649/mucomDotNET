using mucomDotNET.Compiler.PCMTool;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.IO;

namespace PCMTool
{
    class Program
    {
        private static string srcFile;

        static void Main(string[] args)
        {
            Log.writeLine += WriteLine;
#if DEBUG
            //Log.writeLine += WriteLineF;
            Log.level = LogLevel.TRACE;// TRACE;
#else
            Log.level = LogLevel.INFO;
#endif
            int fnIndex = AnalyzeOption(args);

            if (args == null || args.Length != fnIndex + 1)
            {
                Log.WriteLine(LogLevel.ERROR, "引数(.mucファイル)１個欲しいよぉ");
                return;
            }
            if (!File.Exists(args[fnIndex]))
            {
                Log.WriteLine(LogLevel.ERROR, "ファイルが見つかりません");
                return;
            }

            make(args[fnIndex]);
        }

        static void WriteLine(LogLevel level, string msg)
        {
            Console.WriteLine("[{0,-7}] {1}", level, msg);
        }

        private static int AnalyzeOption(string[] args)
        {
            int i = 0;
            if (args.Length == 0) return i;

            while (args[i] != null && args[i].Length > 0 && args[i][0] == '-')
            {
                string op = args[i].Substring(1).ToUpper();

                i++;
            }

            return i;
        }

        private static void make(string fn)
        {
#if NETCOREAPP
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
            try
            {
                srcFile = fn;

                //sjis crlf
                string[] src = File.ReadAllText(fn, System.Text.Encoding.GetEncoding(932)).Split("\r\n", StringSplitOptions.None);

                List<string>[] ret = divider(src);
                byte[][] pcmdata = new byte[6][];
                for (int i = 0; i < 6; i++)
                {
                    pcmdata[i] = null;
                    if (ret[i].Count > 0)
                    {
                        pcmdata[i] = GetPackedPCM(i, ret[i], appendFileReaderCallback);
                    }
                }

                string[] addName = new string[6]
                {
                    "_pcm.bin",
                    "_pcm_2nd.bin" ,
                    "_pcm_3rd_b.bin",
                    "_pcm_4th_b.bin",
                    "_pcm_3rd_a.bin",
                    "_pcm_4th_a.bin",
                };
                for (int i = 0; i < 6; i++)
                {
                    if (pcmdata[i] == null) continue;
                    string dstFn = Path.Combine(Path.GetDirectoryName(fn), Path.GetFileNameWithoutExtension(fn) + addName[i] );
                    File.WriteAllBytes(dstFn, pcmdata[i]);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.FATAL, "Fatal error.");
                Log.WriteLine(LogLevel.FATAL, " Message:");
                Log.WriteLine(LogLevel.FATAL, ex.Message);
                Log.WriteLine(LogLevel.FATAL, " StackTrace:");
                Log.WriteLine(LogLevel.FATAL, ex.StackTrace);
            }
        }

        private static List<string>[] divider(string[] src)
        {
            List<string>[] ret = new List<string>[6] { 
                new List<string>(), new List<string>(), new List<string>(),
                new List<string>(), new List<string>(), new List<string>() 
            };

            foreach(string lin in src)
            {
                if (string.IsNullOrEmpty(lin)) continue;
                if (lin.Length < 3) continue;
                if (lin[0] != '#') continue;
                if (lin[1] != '@') continue;

                string li = lin.Substring(2).ToLower();
                if (li.IndexOf("pcm") == 0) { ret[0].Add(lin.Substring(2 + 3)); }
                else if (li.IndexOf("pcm_2nd") == 0) { ret[1].Add(lin.Substring(2 + 7)); }
                else if (li.IndexOf("pcm_3rd_b") == 0) { ret[2].Add(lin.Substring(2 + 9)); }
                else if (li.IndexOf("pcm_4th_b") == 0) { ret[3].Add(lin.Substring(2 + 9)); }
                else if (li.IndexOf("pcm_3rd_a") == 0) { ret[4].Add(lin.Substring(2 + 9)); }
                else if (li.IndexOf("pcm_4th_a") == 0) { ret[5].Add(lin.Substring(2 + 9)); }
            }

            return ret;
        }

        private static byte[] GetPackedPCM(int i, List<string> list, Func<string, Stream> appendFileReaderCallback)
        {
            AdpcmMaker adpcmMaker = new AdpcmMaker(i, list, appendFileReaderCallback);
            return adpcmMaker.Make();
        }

        private static Stream appendFileReaderCallback(string arg)
        {

            string fn = Path.Combine(
                Path.GetDirectoryName(srcFile)
                , arg
                );

            if (!File.Exists(fn)) return null;

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
