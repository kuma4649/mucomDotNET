using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.IO;

namespace Vgm
{
    public class VgmWriter
    {
        private FileStream dest = null;
        private long waitCounter = 0;
        //header
        readonly public static byte[] hDat = new byte[] {
            //00 'Vgm '          Eof offset           Version number
            0x56,0x67,0x6d,0x20, 0x00,0x00,0x00,0x00, 0x71,0x01,0x00,0x00, 0x00,0x00,0x00,0x00,
            //10                 GD3 offset(no use)   Total # samples
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //20                 Rate(NTSC 60Hz)
            0x00,0x00,0x00,0x00, 0x3c,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //30                 VGMdataofs(0x100~)
            0x00,0x00,0x00,0x00, 0xcc,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //40                                      YM2608 clock(7987200 0x79e000)
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0xe0,0x79,0x00, 0x00,0x00,0x00,0x00,
            //50
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //60
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //70
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //80
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //90
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //A0
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //B0
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //C0
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //D0
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //E0
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
            //F0
            0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00
        };

        public long totalSample { get; private set; }

        public VgmWriter()
        {
        }

        public void WriteYM2608(int v, byte port, byte address, byte data)
        {
            if (dest == null) return;

            if (waitCounter != 0)
            {
                totalSample += waitCounter;

                //waitコマンド出力
                Log.WriteLine(LogLevel.TRACE
                    , string.Format("wait:{0}", waitCounter)
                    );

                if (waitCounter <= 882 * 3)
                {
                    while (waitCounter > 882)
                    {
                        dest.WriteByte(0x63);
                        waitCounter -= 882;
                    }
                    while (waitCounter > 735)
                    {
                        dest.WriteByte(0x62);
                        waitCounter -= 735;
                    }
                }

                while (waitCounter > 0)
                {
                    dest.WriteByte(0x61);
                    dest.WriteByte((byte)waitCounter);
                    dest.WriteByte((byte)(waitCounter >> 8));
                    waitCounter -= (waitCounter & 0xffff);
                }

                waitCounter = 0;
            }

            Log.WriteLine(LogLevel.TRACE
                , string.Format("p:{0} a:{1} d:{2}", port, address, data)
                );

            dest.WriteByte((byte)(0x56 + (port & 1)));
            dest.WriteByte(address);
            dest.WriteByte(data);

        }

        public void Close(List<Tuple<string, string>> tags)
        {
            if (dest == null) return;

            //ヘッダ、フッタの調整

            //end of data
            dest.WriteByte(0x66);

            //Total # samples
            dest.Position = 0x18;
            dest.WriteByte((byte)totalSample);
            dest.WriteByte((byte)(totalSample >> 8));
            dest.WriteByte((byte)(totalSample >> 16));
            dest.WriteByte((byte)(totalSample >> 24));

            //tag
            if (tags != null)
            {
                GD3 gd3 = new GD3();
                foreach (Tuple<string, string> tag in tags)
                {
                    switch (tag.Item1)
                    {
                        case "title":
                            gd3.TrackName = tag.Item2;
                            gd3.TrackNameJ = tag.Item2;
                            break;
                        case "composer":
                            gd3.Composer = tag.Item2;
                            gd3.ComposerJ = tag.Item2;
                            break;
                        case "author":
                            gd3.VGMBy = tag.Item2;
                            break;
                        case "comment":
                            gd3.Notes = tag.Item2;
                            break;
                        case "mucom88":
                            gd3.Version = tag.Item2;
                            gd3.Notes = tag.Item2;
                            break;
                        case "date":
                            gd3.Converted = tag.Item2;
                            break;
                    }
                }

                byte[] tagary = gd3.make();
                dest.Seek(0, SeekOrigin.End);
                long gd3ofs = dest.Length - 0x14;
                foreach (byte b in tagary) dest.WriteByte(b);

                //Tag offset
                if (tagary != null && tagary.Length > 0)
                {
                    dest.Position = 0x14;
                    dest.WriteByte((byte)gd3ofs);
                    dest.WriteByte((byte)(gd3ofs >> 8));
                    dest.WriteByte((byte)(gd3ofs >> 16));
                    dest.WriteByte((byte)(gd3ofs >> 24));
                }
            }

            //EOF offset
            dest.Position = 0x4;
            dest.WriteByte((byte)(dest.Length - 4));
            dest.WriteByte((byte)((dest.Length - 4) >> 8));
            dest.WriteByte((byte)((dest.Length - 4) >> 16));
            dest.WriteByte((byte)((dest.Length - 4) >> 24));

            dest.Close();
            dest = null;
        }

        public void Open(string fullPath)
        {
            if (dest != null) Close(null);
            dest = new FileStream(fullPath, FileMode.Create, FileAccess.Write);

            List<byte> des = new List<byte>();

            //ヘッダの出力
            dest.Write(hDat, 0, hDat.Length);

        }

        public void IncrementWaitCOunter()
        {
            waitCounter++;
        }

        public void WriteAdpcm(byte[] AdpcmData)
        {
            dest.WriteByte(0x67);
            dest.WriteByte(0x66);
            dest.WriteByte(0x81);

            int size = AdpcmData.Length - 0x400;//0x400 pcm table

            dest.WriteByte((byte)((size + 8) >> 0));
            dest.WriteByte((byte)((size + 8) >> 8));
            dest.WriteByte((byte)((size + 8) >> 16));
            dest.WriteByte((byte)((size + 8) >> 24));

            dest.WriteByte((byte)(size >> 0));
            dest.WriteByte((byte)(size >> 8));
            dest.WriteByte((byte)(size >> 16));
            dest.WriteByte((byte)(size >> 24));

            dest.WriteByte((byte)((0) >> 0));
            dest.WriteByte((byte)((0) >> 8));
            dest.WriteByte((byte)((0) >> 16));
            dest.WriteByte((byte)((0) >> 24));

            for (int i = 0x400; i < AdpcmData.Length; i++)
            {
                dest.WriteByte(AdpcmData[i]);
            }

        }
    }
}