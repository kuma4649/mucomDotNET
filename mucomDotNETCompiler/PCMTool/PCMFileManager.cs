using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace mucomDotNET.Compiler.PCMTool
{
    public class PCMFileManager
    {
        private Dictionary<int, PCMFileInfo> dicFile = new Dictionary<int, PCMFileInfo>();
        private Config config;
        private Func<string, Stream> appendFileReaderCallback = null;

        public PCMFileManager(Config config, Func<string, Stream> appendFileReaderCallback = null)
        {
            this.config = config;
            this.appendFileReaderCallback = appendFileReaderCallback;
        }

        public void Add(string lin)
        {
            if (string.IsNullOrEmpty(lin)) return;
            if (lin.Length < 3) return;

            List<string> itemList = AnalyzeLine(lin);
            PCMFileInfo fi = new PCMFileInfo(itemList, appendFileReaderCallback);
            if (dicFile.ContainsKey(fi.number - 1)) dicFile.Remove(fi.number - 1);
            dicFile.Add(fi.number - 1, fi);
            if (fi.length > -1) fi.Encode(config.FormatType);
        }

        public List<byte> GetRawData()
        {
            List<byte> ret = new List<byte>();
            foreach (PCMFileInfo o in dicFile.Values)
            {
                if (o.encData != null)
                    foreach (byte d in o.encData) ret.Add(d);
                else if(o.raw!=null)
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

        public int GetLengthAddress(int i)
        {
            if (!dicFile.ContainsKey(i) || dicFile[i] == null)
            {
                return 0;
            }
            return dicFile[i].length;
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
            int pos = 0;
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