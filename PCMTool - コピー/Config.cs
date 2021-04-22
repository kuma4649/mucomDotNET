using musicDriverInterface;
using System;

namespace mucomDotNET.PCMTool
{
    public class Config
    {
        public enmFormatType FormatType { get; internal set; } = enmFormatType.mucom88;

        public void Add(string lin)
        {
            if (string.IsNullOrEmpty(lin)) return;
            if (lin.Length < 3) return;
            if (lin[0] != '#') return;

            int pos = 0;
            while (pos < lin.Length && lin[pos] != ' ' && lin[pos] != '\t') pos++;
            string contents = lin.Substring(1, pos - 1).Trim().ToUpper();
            string value = lin.Substring(pos).Trim();

            switch (contents)
            {
                case "FORMAT":
                    string val = value.ToUpper();
                    if (val == "MUCOM88") FormatType = enmFormatType.mucom88;
                    else if (val == "MUCOMDOTNET") FormatType = enmFormatType.mucomDotNET_OPNA_ADPCM;
                    else if (val == "OPNA") FormatType = enmFormatType.mucomDotNET_OPNA_ADPCM;
                    else if (val == "OPNB_B") FormatType = enmFormatType.mucomDotNET_OPNB_ADPCMB;
                    else if (val == "OPNB_A") FormatType = enmFormatType.mucomDotNET_OPNB_ADPCMA;
                    else if (val == "OPNB-B") FormatType = enmFormatType.mucomDotNET_OPNB_ADPCMB;
                    else if (val == "OPNB-A") FormatType = enmFormatType.mucomDotNET_OPNB_ADPCMA;
                    else if (val == "OPNBB") FormatType = enmFormatType.mucomDotNET_OPNB_ADPCMB;
                    else if (val == "OPNBA") FormatType = enmFormatType.mucomDotNET_OPNB_ADPCMA;
                    else Log.WriteLine(LogLevel.ERROR, string.Format("Unknown format type.[{0}]", value));
                    break;

                default:
                    Log.WriteLine(LogLevel.ERROR, string.Format("Unknown command[{0}].", contents));
                    break;
            }

        }
    }
}