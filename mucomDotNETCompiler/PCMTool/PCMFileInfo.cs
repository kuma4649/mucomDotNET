using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.IO;

namespace mucomDotNET.Compiler.PCMTool
{
    public class PCMFileInfo
    {
        public int number { get; internal set; }
        public string name { get; private set; }
        public string fileName { get; private set; }
        public int volume { get; private set; }
        public int length { get; internal set; } = -1;
        public byte[] raw { get; private set; } = null;
        public byte[] encData { get; private set; } = null;
        private bool is16bit;

        public PCMFileInfo(List<string> itemList, Func<string, Stream> appendFileReaderCallback = null)
        {
            if (itemList == null) return;

            int n;
            if (itemList.Count > 0)
            {
                string item = itemList[0].ToLower().Trim();
                if (item.Length > 1 && item[0] == '$')
                {
                    n = Convert.ToInt32(item.Substring(1), 16);
                    number = n;
                }
                else if (int.TryParse(item, out n)) number = n;
                else if (item.Length > 1 && item[0] == 'o')
                {
                    if (int.TryParse(item.Substring(1, 1), out n) && item.Length > 2)
                    {
                        string[] note = new string[] { "c", "c+", "d", "d+", "e", "f", "f+", "g", "g+", "a", "a+", "b" };
                        for (int i = 0; i < note.Length; i++)
                        {
                            if (note[i] != item.Substring(2)) continue;
                            n = n * 16 + i + 1;
                            number = n;
                            break;
                        }
                    }
                }
            }
            if (itemList.Count > 1) name = itemList[1];
            if (itemList.Count > 2) fileName = itemList[2];
            if (itemList.Count > 3 && int.TryParse(itemList[3], out n)) volume = n;

            byte[] buf;
            using (Stream pd = appendFileReaderCallback?.Invoke(fileName))
            {
                buf = ReadAllBytes(pd);
            }

            if (buf == null)
            {
                if (File.Exists(fileName))
                {
                    bool isRaw;
                    int samplerate;
                    raw = GetPCMDataFromFile("", fileName, volume, out isRaw, out is16bit, out samplerate);
                    length = (ushort)raw.Length;
                }
                else
                {
                    Log.WriteLine(LogLevel.WARNING, string.Format("file[{0}] not found", fileName));
                }
            }
            else
            {
                bool isRaw;
                int samplerate;
                raw = GetPCMDataFromFile(buf, volume, out isRaw, out is16bit, out samplerate);
                length = (ushort)raw.Length;
            }
        }

        /// <summary>
        /// ストリームから一括でバイナリを読み込む
        /// </summary>
        private byte[] ReadAllBytes(Stream stream)
        {
            if (stream == null) return null;

            var buf = new byte[8192];
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    var r = stream.Read(buf, 0, buf.Length);
                    if (r < 1)
                    {
                        break;
                    }
                    ms.Write(buf, 0, r);
                }
                return ms.ToArray();
            }
        }

        public void Encode(enmFormatType formatType)
        {
            EncAdpcmA enc = new EncAdpcmA();

            switch(formatType)
            {
                case enmFormatType.mucom88:
                case enmFormatType.mucomDotNET_OPNA_ADPCM:
                    encData = enc.YM_ADPCM_B_Encode(raw, is16bit, false);
                    break;
                case enmFormatType.mucomDotNET_OPNB_ADPCMA:
                    encData = enc.YM_ADPCM_A_Encode(raw, is16bit);
                    break;
                case enmFormatType.mucomDotNET_OPNB_ADPCMB:
                    encData = enc.YM_ADPCM_B_Encode(raw, is16bit, true);
                    break;
            }
            length = encData.Length;
        }

        public static byte[] GetPCMDataFromFile(string path, string fileName, int vol, out bool isRaw, out bool is16bit, out int samplerate)
        {
            string fnPcm = Path.Combine(path, fileName).Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

            isRaw = false;
            is16bit = false;
            samplerate = 8000;

            if (!File.Exists(fnPcm))
            {
                Log.WriteLine( LogLevel.ERROR, "File not found.");
                return null;
            }

            // ファイルの読み込み
            byte[] buf = File.ReadAllBytes(fnPcm);

            if (Path.GetExtension(fileName).ToUpper().Trim() != ".WAV")
            {
                isRaw = true;
                return buf;
            }

            return GetPCMDataFromFile(buf, vol, out isRaw, out is16bit, out samplerate);
        }

        public static byte[] GetPCMDataFromFile(byte[] buf, int vol, out bool isRaw, out bool is16bit, out int samplerate)
        {
            isRaw = false;
            is16bit = false;
            samplerate = 8000;

            if (buf.Length < 4)
            {
                Log.WriteLine(LogLevel.ERROR, "This file is not wave.");
                return null;
            }
            if (buf[0] != 'R' || buf[1] != 'I' || buf[2] != 'F' || buf[3] != 'F')
            {
                Log.WriteLine(LogLevel.ERROR, "This file is not wave.");
                return null;
            }

            // サイズ取得
            int fSize = buf[0x4] + buf[0x5] * 0x100 + buf[0x6] * 0x10000 + buf[0x7] * 0x1000000;

            if (buf[0x8] != 'W' || buf[0x9] != 'A' || buf[0xa] != 'V' || buf[0xb] != 'E')
            {
                Log.WriteLine(LogLevel.ERROR, "This file is not wave.");
                return null;
            }

            try
            {
                int p = 12;
                byte[] des = null;

                while (p < fSize + 8)
                {
                    if (buf[p + 0] == 'f' && buf[p + 1] == 'm' && buf[p + 2] == 't' && buf[p + 3] == ' ')
                    {
                        p += 4;
                        int size = buf[p + 0] + buf[p + 1] * 0x100 + buf[p + 2] * 0x10000 + buf[p + 3] * 0x1000000;
                        p += 4;
                        int format = buf[p + 0] + buf[p + 1] * 0x100;
                        if (format != 1)
                        {
                            Log.WriteLine(LogLevel.ERROR, "isn't Mono.");
                            return null;
                        }

                        int channels = buf[p + 2] + buf[p + 3] * 0x100;
                        if (channels != 1)
                        {
                            Log.WriteLine(LogLevel.ERROR, "isn't Mono.");
                            return null;
                        }

                        samplerate = buf[p + 4] + buf[p + 5] * 0x100 + buf[p + 6] * 0x10000 + buf[p + 7] * 0x1000000;
                        if (samplerate != 8000 && samplerate != 16000 && samplerate != 18500 && samplerate != 14000)
                        {
                            //Log.WriteLine(LogLevel.WARNING, "Unknown samplerate.");
                            //return null;
                        }

                        int bytepersec = buf[p + 8] + buf[p + 9] * 0x100 + buf[p + 10] * 0x10000 + buf[p + 11] * 0x1000000;
                        if (bytepersec != 8000)
                        {
                            //    msgBox.setWrnMsg(string.Format("PCMファイル：仕様とは異なる平均データ割合です。({0})", bytepersec));
                            //    return null;
                        }

                        int bitswidth = buf[p + 14] + buf[p + 15] * 0x100;
                        if (bitswidth != 8 && bitswidth != 16)
                        {
                            Log.WriteLine(LogLevel.ERROR, "Unknown bitswidth.");
                            return null;
                        }

                        is16bit = bitswidth == 16;

                        int blockalign = buf[p + 12] + buf[p + 13] * 0x100;
                        if (blockalign != (is16bit ? 2 : 1))
                        {
                            Log.WriteLine(LogLevel.ERROR, "Unknown blockalign.");
                            return null;
                        }


                        p += size;
                    }
                    else if (buf[p + 0] == 'd' && buf[p + 1] == 'a' && buf[p + 2] == 't' && buf[p + 3] == 'a')
                    {
                        p += 4;
                        int size = buf[p + 0] + buf[p + 1] * 0x100 + buf[p + 2] * 0x10000 + buf[p + 3] * 0x1000000;
                        p += 4;

                        des = new byte[size];
                        Array.Copy(buf, p, des, 0x00, size);
                        p += size;
                    }
                    else
                    {
                        p += 4;

                        if (p > buf.Length - 4)
                        {
                            p = fSize + 8;
                            break;
                        }

                        int size = buf[p + 0] + buf[p + 1] * 0x100 + buf[p + 2] * 0x10000 + buf[p + 3] * 0x1000000;
                        p += 4;

                        p += size;
                    }
                }

                // volumeの加工
                if (is16bit)
                {
                    for (int i = 0; i < des.Length; i += 2)
                    {
                        //16bitのwavファイルはsignedのデータのためそのままボリューム変更可能
                        int b = (int)((short)(des[i] | (des[i + 1] << 8)) * vol * 0.01);
                        b = (b > 0x7fff) ? 0x7fff : b;
                        b = (b < -0x8000) ? -0x8000 : b;
                        des[i] = (byte)(b & 0xff);
                        des[i + 1] = (byte)((b & 0xff00) >> 8);
                    }
                }
                else
                {
                    for (int i = 0; i < des.Length; i++)
                    {
                        //8bitのwavファイルはunsignedのデータのためsignedのデータに変更してからボリューム変更する
                        int d = des[i];
                        //signed化
                        d -= 0x80;
                        d = (int)(d * vol * 0.01);
                        //clip
                        d = (d > 127) ? 127 : d;
                        d = (d < -128) ? -128 : d;
                        //unsigned化
                        d += 0x80;

                        des[i] = (byte)d;
                    }
                }

                return des;
            }
            catch
            {
                Log.WriteLine(LogLevel.ERROR, "Unknown error.");
                return null;
            }
        }
    }
}