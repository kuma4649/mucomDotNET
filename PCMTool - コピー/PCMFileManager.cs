using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.PCMTool
{
    public class PCMFileManager
    {
        private Dictionary<int, PCMFileInfo> dicFile = new Dictionary<int, PCMFileInfo>();
        private Config config;

        public PCMFileManager(Config config)
        {
            this.config = config;
        }

        public void Add(string lin)
        {
            if (string.IsNullOrEmpty(lin)) return;
            if (lin.Length < 3) return;
            if (lin[0] != '@') return;

            List<string> itemList = AnalyzeLine(lin);
            PCMFileInfo fi = new PCMFileInfo(itemList);
            if (dicFile.ContainsKey(fi.number)) dicFile.Remove(fi.number);
            dicFile.Add(fi.number, fi);
            fi.Encode(config.FormatType);
        }

        public List<byte> GetRawData()
        {
            List<byte> ret = new List<byte>();
            foreach (PCMFileInfo o in dicFile.Values)
            {
                if (o.encData != null)
                    foreach (byte d in o.encData) ret.Add(d);
                else
                    foreach (byte d in o.raw) ret.Add(d);
            }
            return ret;
        }

        public List<byte> GetName(int i, int v)
        {
            List<byte> ret = new List<byte>();

            if (!dicFile.ContainsKey(i) || dicFile[i] == null || string.IsNullOrEmpty(dicFile[i].name))
            {
                for (int n = 0; n < v; n++) ret.Add(0);
                return ret;
            }

            byte[] data = System.Text.Encoding.GetEncoding(932).GetBytes(dicFile[i].name);
            for(int n = 0; n < v; n++)
            {
                if (n < data.Length)
                    ret.Add(data[n]);
                else
                    ret.Add(0x20);
            }

            return ret;
        }

        public ushort GetVolume(int i)
        {
            if (!dicFile.ContainsKey(i) || dicFile[i] == null)
            {
                return 0;
            }
            return (ushort)dicFile[i].volume;
        }

        public ushort GetLengthAddress(int i)
        {
            if (!dicFile.ContainsKey(i) || dicFile[i] == null)
            {
                return 0;
            }
            return (ushort)dicFile[i].length;
        }

        public List<byte> GetName(int i)
        {
            List<byte> ret = new List<byte>();

            if (!dicFile.ContainsKey(i) || dicFile[i] == null || string.IsNullOrEmpty(dicFile[i].name))
            {
                ret.Add(0);
                return ret;
            }

            byte[] data = System.Text.Encoding.GetEncoding(932).GetBytes(dicFile[i].name);
            for (int n = 0; n < data.Length; n++)
            {
                ret.Add(data[n]);
            }
            ret.Add(0);

            return ret;
        }

        public int GetCount()
        {
            int i = 0;
            foreach(PCMFileInfo o in dicFile.Values)
            {
                i = Math.Max(i, o.number);
            }

            return i;
        }


        private List<string> AnalyzeLine(string lin)
        {
            List<string> itemList = new List<string>();
            int pos = 1;
            string item = "";
            bool str = false;
            while (pos < lin.Length)
            {
                if (lin[pos] == '"')
                {
                    if (pos + 1 < lin.Length && lin[pos + 1] == '"' && str)
                    {
                        pos++;
                    }
                    else
                    {
                        str = !str;
                        pos++;
                        continue;
                    }
                }

                if (lin[pos] == ',' && !str)
                {
                    itemList.Add(item.Trim());
                    pos++;
                    item = "";
                    continue;
                }

                item += lin[pos++];
            }

            if (!string.IsNullOrEmpty(item))
            {
                itemList.Add(item.Trim());
            }

            return itemList;
        }


    }
}