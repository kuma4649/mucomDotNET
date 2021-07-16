using mucomDotNET.Common;
using mucomDotNET.Driver;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.IO;

namespace Vgm
{
    class Program
    {
        private static readonly int SamplingRate = 44100;//vgm format freq
        private static readonly uint opnaMasterClock = 7987200;
        private static readonly uint opnbMasterClock = 8000000;

        private static iDriver drv = null;
        private static VgmWriter vw = null;
        private static int loop = 2;
        private static List<Tuple<string, string>> tags = null;

        static int Main(string[] args)
        {
            Log.writeLine += WriteLine;
#if DEBUG
            //Log.writeLine += WriteLineF;
            Log.level = LogLevel.INFO;//.TRACE;
#else
            Log.level = LogLevel.INFO;
#endif
            int fnIndex = AnalyzeOption(args);

            if (args == null || args.Length != fnIndex + 1)
            {
                Log.WriteLine(LogLevel.ERROR, "引数(.mubファイル)１個欲しいよぉ");
                return -1;
            }
            if (!File.Exists(args[fnIndex]))
            {
                Log.WriteLine(LogLevel.ERROR, "ファイルが見つかりません");
                return -1;
            }

            try
            {
                vw = new VgmWriter();
                vw.Open(
                    Path.Combine(
                        Path.GetDirectoryName(args[fnIndex])
                        , Path.GetFileNameWithoutExtension(args[fnIndex]) + ".vgm")
                    );

#if NETCOREAPP
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif

                List<ChipAction> lca = new List<ChipAction>();
                mucomChipAction ca;
                ca = new mucomChipAction(OPNAWriteP, null, OPNAWaitSend); lca.Add(ca);
                ca = new mucomChipAction(OPNAWriteS, null, null); lca.Add(ca);
                ca = new mucomChipAction(OPNBWriteP, OPNBWriteAdpcmP, null); lca.Add(ca);
                ca = new mucomChipAction(OPNBWriteS, OPNBWriteAdpcmS, null); lca.Add(ca);

                List<MmlDatum> bl = new List<MmlDatum>();
                byte[] srcBuf = File.ReadAllBytes(args[fnIndex]);
                foreach (byte b in srcBuf) bl.Add(new MmlDatum(b));
                vw.useChipsFromMub(srcBuf);

                drv = new Driver();
                ((Driver)drv).Init(
                    lca
                    , bl.ToArray()
                    , null
                    , new object[] {
                        false
                        , true
                        , false
                        ,args[fnIndex]
                    }
                    );

                drv.SetLoopCount(loop);

                tags = ((Driver)drv).GetTags();
                if (tags != null)
                {
                    foreach (Tuple<string, string> tag in tags)
                    {
                        if (tag.Item1 == "") continue;
                        Log.WriteLine(LogLevel.INFO, string.Format("{0,-16} : {1}", tag.Item1, tag.Item2));
                    }
                }

                //byte[] pcmdata = drv.GetPCMFromSrcBuf();
                //if (pcmdata != null && pcmdata.Length > 0) vw.WriteAdpcm(pcmdata);

                drv.StartRendering((int)SamplingRate,
                    new Tuple<string, int>[]{
                        new Tuple<string, int>("YM2608",(int)opnaMasterClock)
                        ,new Tuple<string, int>("YM2608",(int)opnaMasterClock)
                        ,new Tuple<string, int>("YM2610B",(int)opnbMasterClock)
                        ,new Tuple<string, int>("YM2610B",(int)opnbMasterClock)
                    }
                    );

                drv.MusicSTART(0);

                while (true)
                {

                    drv.Rendering();
                    vw.IncrementWaitCOunter();

                    //ステータスが0(終了)又は0未満(エラー)の場合はループを抜けて終了
                    if (drv.GetStatus() <= 0)
                    {
                        break;
                    }

                }

                drv.MusicSTOP();
                drv.StopRendering();
            }
            catch
            {
            }
            finally
            {
                if (vw != null)
                {
                    vw.Close(tags);
                }
            }

            return 0;
        }

        static void WriteLine(LogLevel level, string msg)
        {
            Console.WriteLine("[{0,-7}] {1}", level, msg);
        }

        private static int AnalyzeOption(string[] args)
        {
            int i = 0;
            loop = 2;

            while (args != null 
                && args.Length > 0 
                && args[i].Length > 0 
                && args[i] != null 
                && args[i][0] == '-')
            {
                string op = args[i].Substring(1).ToUpper();
                if (op.Length > 2 && op.Substring(0, 2) == "L=")
                {
                    if (!int.TryParse(op.Substring(2), out loop))
                    {
                        loop = 2;
                    }
                }

                i++;
            }

            return i;
        }

        public static string GetApplicationFolder()
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(path))
            {
                path += path[path.Length - 1] == '\\' ? "" : "\\";
            }
            return path;
        }

        private static void OPNAWrite(ChipDatum dat)
        {
            if (dat != null && dat.addtionalData != null)
            {
                MmlDatum md = (MmlDatum)dat.addtionalData;
                if (md.linePos != null)
                {
                    Log.WriteLine(LogLevel.TRACE, string.Format("! r{0} c{1}"
                        , md.linePos.row
                        , md.linePos.col
                        ));
                }
            }

            vw.WriteYM2608(0, (byte)dat.port, (byte)dat.address, (byte)dat.data);
        }

        private static void OPNAWaitSend(long elapsed, int size)
        {
            return;
        }

        private static void OPNAWriteP(ChipDatum dat)
        {
            OPNAWrite(0, dat);
        }
        private static void OPNAWriteS(ChipDatum dat)
        {
            OPNAWrite(1, dat);
        }
        private static void OPNBWriteP(ChipDatum dat)
        {
            OPNBWrite(0, dat);
        }
        private static void OPNBWriteS(ChipDatum dat)
        {
            OPNBWrite(1, dat);
        }
        private static void OPNBWriteAdpcmP(byte[] pcmData, int s, int e)
        {
            if (s == 0) OPNBWrite_AdpcmA(0, pcmData);
            else OPNBWrite_AdpcmB(0, pcmData);
        }
        private static void OPNBWriteAdpcmS(byte[] pcmData, int s, int e)
        {
            if (s == 0) OPNBWrite_AdpcmA(1, pcmData);
            else OPNBWrite_AdpcmB(1, pcmData);
        }

        private static void OPNAWrite(int chipId, ChipDatum dat)
        {
            if (dat != null && dat.addtionalData != null)
            {
                MmlDatum md = (MmlDatum)dat.addtionalData;
                if (md.linePos != null)
                {
                    Log.WriteLine(LogLevel.TRACE, string.Format("! OPNA i{0} r{1} c{2}"
                        , chipId
                        , md.linePos.row
                        , md.linePos.col
                        ));
                }
            }

            Log.WriteLine(LogLevel.TRACE, string.Format("Out ChipA:{0} Port:{1} Adr:[{2:x02}] val[{3:x02}]", chipId, dat.port, (int)dat.address, (int)dat.data));

            vw.WriteYM2608((byte)chipId, (byte)dat.port, (byte)dat.address, (byte)dat.data);
        }

        private static void OPNBWrite(int chipId, ChipDatum dat)
        {
            if (dat != null && dat.addtionalData != null)
            {
                MmlDatum md = (MmlDatum)dat.addtionalData;
                if (md.linePos != null)
                {
                    Log.WriteLine(LogLevel.TRACE, string.Format("! OPNB i{0} r{1} c{2}"
                        , chipId
                        , md.linePos.row
                        , md.linePos.col
                        ));
                }
            }

            Log.WriteLine(LogLevel.TRACE, string.Format("Out ChipB:{0} Port:{1} Adr:[{2:x02}] val[{3:x02}]", chipId, dat.port, (int)dat.address, (int)dat.data));

            vw.WriteYM2610((byte)chipId, (byte)dat.port, (byte)dat.address, (byte)dat.data);
        }

        private static void OPNBWrite_AdpcmA(int chipId, byte[] pcmData)
        {
            vw.WriteYM2610_SetAdpcmA((byte)chipId, pcmData);

        }

        private static void OPNBWrite_AdpcmB(int chipId, byte[] pcmData)
        {
            vw.WriteYM2610_SetAdpcmB((byte)chipId, pcmData);

        }

    }
}
