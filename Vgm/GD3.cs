using System.Collections.Generic;
using System.Text;

namespace Vgm
{
    public class GD3
    {
        public GD3()
        { }

        public string TrackName = "";
        public string TrackNameJ = "";
        public string GameName = "";
        public string GameNameJ = "";
        public string SystemName = "";
        public string SystemNameJ = "";
        public string Composer = "";
        public string ComposerJ = "";
        public string Converted = "";
        public string Notes = "";
        public string VGMBy = "";
        public string Version = "";
        public string UsedChips = "";

        public byte[] make()
        {
            List<byte> dat = new List<byte>();

            //'Gd3 '
            dat.Add(0x47);
            dat.Add(0x64);
            dat.Add(0x33);
            dat.Add(0x20);

            //GD3 Version
            dat.Add(0x00);
            dat.Add(0x01);
            dat.Add(0x00);
            dat.Add(0x00);

            //GD3 Length(dummy)
            dat.Add(0x00);
            dat.Add(0x00);
            dat.Add(0x00);
            dat.Add(0x00);

            //TrackName
            if (!string.IsNullOrEmpty(TrackName))
                foreach (byte b in Encoding.Unicode.GetBytes(TrackName)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            if (!string.IsNullOrEmpty(TrackNameJ))
                foreach (byte b in Encoding.Unicode.GetBytes(TrackNameJ)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //GameName
            if (!string.IsNullOrEmpty(GameName))
                foreach (byte b in Encoding.Unicode.GetBytes(GameName)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            if (!string.IsNullOrEmpty(GameNameJ))
                foreach (byte b in Encoding.Unicode.GetBytes(GameNameJ)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //SystemName
            if (!string.IsNullOrEmpty(SystemName))
                foreach (byte b in Encoding.Unicode.GetBytes(SystemName)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            if (!string.IsNullOrEmpty(SystemNameJ))
                foreach (byte b in Encoding.Unicode.GetBytes(SystemNameJ)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //Composer
            if (!string.IsNullOrEmpty(Composer))
                foreach (byte b in Encoding.Unicode.GetBytes(Composer)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            if (!string.IsNullOrEmpty(ComposerJ))
                foreach (byte b in Encoding.Unicode.GetBytes(ComposerJ)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //Converted
            if (!string.IsNullOrEmpty(Converted))
                foreach (byte b in Encoding.Unicode.GetBytes(Converted)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //ReleaseDate
            foreach (byte b in Encoding.Unicode.GetBytes(VGMBy)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //Notes
            if (!string.IsNullOrEmpty(Notes))
                foreach (byte b in Encoding.Unicode.GetBytes(Notes)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            dat[8] = (byte)dat.Count;
            dat[9] = (byte)(dat.Count >> 8);
            dat[10] = (byte)(dat.Count >> 16);
            dat[11] = (byte)(dat.Count >> 24);

            return dat.ToArray();
        }

    }
}
