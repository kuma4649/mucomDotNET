using System;
using System.Collections.Generic;
using System.IO;

namespace mucomDotNET.Compiler.PCMTool
{
    public class AdpcmMaker
    {
        private string[] src;
        private int i=-1;
        private List<string> list=null;
        private Func<string, Stream> appendFileReaderCallback=null;

        public AdpcmMaker(string[] src)
        {
            this.src = src;
        }

        public AdpcmMaker(int i, List<string> list, Func<string, Stream> appendFileReaderCallback)
        {
            this.i = i;
            this.list = list;
            this.appendFileReaderCallback = appendFileReaderCallback;
        }

        public byte[] Make()
        {
            Config config;
            PCMFileManager fileManager;
            if (i == -1)
            {
                config = GetConfig();
                fileManager = GetPCMFiles(config);
                return Make(config, fileManager);
            }
            else
            {
                config = new Config();
                if (i == 0) config.FormatType = enmFormatType.mucom88;
                else if (i == 1) config.FormatType = enmFormatType.mucomDotNET_OPNA_ADPCM;
                else if (i == 2) config.FormatType = enmFormatType.mucomDotNET_OPNB_ADPCMB;
                else if (i == 3) config.FormatType = enmFormatType.mucomDotNET_OPNB_ADPCMB;
                else if (i == 4) config.FormatType = enmFormatType.mucomDotNET_OPNB_ADPCMA;
                else if (i == 5) config.FormatType = enmFormatType.mucomDotNET_OPNB_ADPCMA;
                fileManager = new PCMFileManager(config, appendFileReaderCallback);
                foreach (string line in list)
                {
                    string lin = line.Trim();
                    if (string.IsNullOrEmpty(lin)) continue;

                    fileManager.Add(lin);
                }
                return Make(config, fileManager);
            }
        }

        private Config GetConfig()
        {
            Config config = new Config();

            foreach(string line in src)
            {
                string lin = line.Trim();
                if (string.IsNullOrEmpty(lin)) continue;
                if (lin[0] != '#') continue;

                config.Add(lin);
            }

            return config;
        }

        private PCMFileManager GetPCMFiles(Config config)
        {
            PCMFileManager filemanager = new PCMFileManager(config);

            foreach (string line in src)
            {
                string lin = line.Trim();
                if (string.IsNullOrEmpty(lin)) continue;
                if (lin[0] != '@') continue;
                if (lin.Length > 1) continue;

                filemanager.Add(lin.Substring(1));
            }

            return filemanager;
        }

        private byte[] Make(Config config, PCMFileManager fileManager)
        {
            List<byte> dst = new List<byte>();
            dst = MakeHeader(config, fileManager, dst);
            List<byte> raw = fileManager.GetRawData();
            if (raw != null) dst.AddRange(raw);

            return dst.ToArray();
        }

        private List<byte> MakeHeader(Config config, PCMFileManager fileManager, List<byte> dst)
        {
            switch (config.FormatType)
            {
                case enmFormatType.mucom88:
                    dst.AddRange(MakeHeader_mucom88(fileManager));
                    break;
                case enmFormatType.mucomDotNET_OPNA_ADPCM:
                    dst.AddRange(MakeHeader_mucomDotNET_OPNA_ADPCM(fileManager));
                    break;
                case enmFormatType.mucomDotNET_OPNB_ADPCMB:
                    dst.AddRange(MakeHeader_mucomDotNET_OPNB_ADPCMB(fileManager));
                    break;
                case enmFormatType.mucomDotNET_OPNB_ADPCMA:
                    dst.AddRange(MakeHeader_mucomDotNET_OPNB_ADPCMA(fileManager));
                    break;
            }

            return dst;
        }

        private List<byte> MakeHeader_mucom88(PCMFileManager fileManager)
        {
            List<byte> head = new List<byte>();
            int ptr = 0;
            for (int i = 0; i < 32; i++)
            {
                head.AddRange(fileManager.GetName(i, 16));//instrument name 16byte
                head.AddRange(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });//dummy 10byte(limit/sample rate/volume etc.)
                head.Add((byte)(fileManager.GetVolume(i)));
                head.Add((byte)(fileManager.GetVolume(i) >> 8));
                int length = fileManager.GetLengthAddress(i);
                if (length < 1)
                {
                    head.Add(0);
                    head.Add(0);
                    head.Add(0);
                    head.Add(0);
                }
                else
                {
                    ushort stAdr = (ushort)(ptr >> 2);
                    ptr += length - 1;
                    ushort edAdr = (ushort)(ptr >> 2);
                    ptr++;
                    head.Add((byte)(stAdr));
                    head.Add((byte)(stAdr >> 8));
                    head.Add((byte)(edAdr));
                    head.Add((byte)(edAdr >> 8));
                }
            }
            return head;
        }

        private List<byte> MakeHeader_mucomDotNET_OPNA_ADPCM(PCMFileManager fileManager)
        {
            List<byte> head = new List<byte>();
            int ptr = 0;

            head.Add((byte)'m');
            head.Add((byte)'d');
            head.Add((byte)'a');
            head.Add((byte)' ');

            int num = fileManager.GetCount();
            head.Add((byte)num);
            head.Add((byte)(num >> 8));

            for (int i = 0; i <= num; i++)
            {
                head.AddRange(fileManager.GetName(i));//instrument name 16byte
                head.Add(3);
                head.Add((byte)(fileManager.GetVolume(i)));
                head.Add((byte)(fileManager.GetVolume(i) >> 8));
                int length = fileManager.GetLengthAddress(i);
                if (length < 1)
                {
                    head.Add(0);
                    head.Add(0);
                    head.Add(0);
                    head.Add(0);
                }
                else
                {
                    ushort stAdr = (ushort)(ptr >> 2);
                    ptr += length - 1;
                    ushort edAdr = (ushort)(ptr >> 2);
                    ptr++;
                    head.Add((byte)(stAdr));
                    head.Add((byte)(stAdr >> 8));
                    head.Add((byte)(edAdr));
                    head.Add((byte)(edAdr >> 8));
                }
            }
            return head;
        }

        private List<byte> MakeHeader_mucomDotNET_OPNB_ADPCMB(PCMFileManager fileManager)
        {
            List<byte> head = new List<byte>();
            int ptr = 0;

            head.Add((byte)'m');
            head.Add((byte)'d');
            head.Add((byte)'b');
            head.Add((byte)'b');

            int num = fileManager.GetCount();
            head.Add((byte)num);
            head.Add((byte)(num>>8));

            for (int i = 0; i <= num; i++)
            {
                head.AddRange(fileManager.GetName(i));//instrument name 16byte
                head.Add(3);
                head.Add((byte)(fileManager.GetVolume(i)));
                head.Add((byte)(fileManager.GetVolume(i) >> 8));
                int length = fileManager.GetLengthAddress(i);
                if (length < 1)
                {
                    head.Add(0);
                    head.Add(0);
                    head.Add(0);
                    head.Add(0);
                }
                else
                {
                    ushort stAdr = (ushort)(ptr >> 8);
                    ptr += length - 1;
                    ushort edAdr = (ushort)(ptr >> 8);
                    ptr++;
                    head.Add((byte)(stAdr));
                    head.Add((byte)(stAdr >> 8));
                    head.Add((byte)(edAdr));
                    head.Add((byte)(edAdr >> 8));
                }
            }
            return head;
        }

        private List<byte> MakeHeader_mucomDotNET_OPNB_ADPCMA(PCMFileManager fileManager)
        {
            List<byte> head = new List<byte>();
            int ptr = 0;

            head.Add((byte)'m');
            head.Add((byte)'d');
            head.Add((byte)'b');
            head.Add((byte)'a');

            int num = fileManager.GetCount();
            head.Add((byte)num);
            head.Add((byte)(num >> 8));

            for (int i = 0; i <= num; i++)
            {
                head.AddRange(fileManager.GetName(i));//instrument name 16byte
                head.Add(3);
                head.Add((byte)(fileManager.GetVolume(i)));
                head.Add((byte)(fileManager.GetVolume(i) >> 8));
                int length = fileManager.GetLengthAddress(i);
                if (length < 1)
                {
                    head.Add(0);
                    head.Add(0);
                    head.Add(0);
                    head.Add(0);
                }
                else
                {
                    ushort stAdr = (ushort)(ptr >> 8);
                    ptr += length - 1;
                    ushort edAdr = (ushort)(ptr >> 8);
                    ptr++;
                    head.Add((byte)(stAdr));
                    head.Add((byte)(stAdr >> 8));
                    head.Add((byte)(edAdr));
                    head.Add((byte)(edAdr >> 8));
                }
            }
            return head;
        }
    }
}