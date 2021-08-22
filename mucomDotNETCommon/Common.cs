using musicDriverInterface;
using System;

namespace mucomDotNET.Common
{
    public static class Cmn
    {
        public static UInt32 getBE16(byte[] buf, UInt32 adr)
        {
            if (buf == null || buf.Length - 1 < adr + 1)
            {
                throw new IndexOutOfRangeException();
            }

            UInt32 dat;
            dat = (UInt32)buf[adr] * 0x100 + (UInt32)buf[adr + 1];

            return dat;
        }

        public static UInt32 getLE16(byte[] buf, UInt32 adr)
        {
            if (buf == null || buf.Length - 1 < adr + 1)
            {
                throw new IndexOutOfRangeException();
            }

            UInt32 dat;
            dat = (UInt32)buf[adr] + (UInt32)buf[adr + 1] * 0x100;

            return dat;
        }

        public static UInt32 getLE16(MmlDatum[] buf, UInt32 adr)
        {
            if (buf == null || buf.Length - 1 < adr + 1)
            {
                throw new IndexOutOfRangeException();
            }

            UInt32 dat;
            dat = (UInt32)buf[adr].dat + (UInt32)buf[adr + 1].dat * 0x100;

            return dat;
        }

        public static UInt32 getLE24(byte[] buf, UInt32 adr)
        {
            if (buf == null || buf.Length - 1 < adr + 2)
            {
                throw new IndexOutOfRangeException();
            }

            UInt32 dat;
            dat = (UInt32)buf[adr] + (UInt32)buf[adr + 1] * 0x100 + (UInt32)buf[adr + 2] * 0x10000;

            return dat;
        }

        public static UInt32 getLE32(byte[] buf, UInt32 adr)
        {
            if (buf == null || buf.Length - 1 < adr + 3)
            {
                throw new IndexOutOfRangeException();
            }

            UInt32 dat;
            dat = (UInt32)buf[adr] + (UInt32)buf[adr + 1] * 0x100 + (UInt32)buf[adr + 2] * 0x10000 + (UInt32)buf[adr + 3] * 0x100_0000;

            return dat;
        }

        public static UInt32 getLE32(MmlDatum[] buf, UInt32 adr)
        {
            if (buf == null || buf.Length - 1 < adr + 3)
            {
                throw new IndexOutOfRangeException();
            }

            UInt32 dat;
            dat = (UInt32)buf[adr].dat + (UInt32)buf[adr + 1].dat * 0x100 + (UInt32)buf[adr + 2].dat * 0x10000 + (UInt32)buf[adr + 3].dat * 0x100_0000;

            return dat;
        }

        public static string GetChipName(int ChipIndex)
        {
            switch (ChipIndex)
            {
                case 0:
                case 1:
                    return "YM2608";
                case 2:
                case 3:
                    return "YM2610B";
                case 4:
                    return "YM2151";
                default:
                    return "Unknown";
            }
        }
        public static int GetChipNumber(int ChipIndex)
        {
            switch (ChipIndex)
            {
                case 0:
                case 1:
                    return 0;
                case 2:
                case 3:
                    return 1;
                case 4:
                    return 0;
                default:
                    return -1;
            }
        }

    }
}
