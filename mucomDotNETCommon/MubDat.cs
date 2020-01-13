using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mucomDotNET.Common
{
    public class MubDat
    {
        public byte dat;
        public int? row;
        public int? col;
        public int? len;
        public int? ch;

        public MubDat(byte dat, int? row = null, int? col = null, int? len = null,int? ch=null)
        {
            this.dat = dat;
            this.row = row;
            this.col = col;
            this.len = len;
            this.ch = ch;
        }
    }
}
