using System;
using System.IO;
using musicDriverInterface;

namespace PCMTool
{
    class Program
    {
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
                Log.WriteLine(LogLevel.ERROR, "引数(.pxtファイル)１個欲しいよぉ");
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
                //sjis crlf
                string[] src = File.ReadAllText(fn, System.Text.Encoding.GetEncoding(932)).Split("\r\n", StringSplitOptions.None);

                AdpcmMaker maker = new AdpcmMaker(src);
                byte[] dst = maker.Make();

                string dstFn = Path.Combine(Path.GetDirectoryName(fn), Path.GetFileNameWithoutExtension(fn) + ".bin");
                File.WriteAllBytes(dstFn, dst);
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

    }
}
