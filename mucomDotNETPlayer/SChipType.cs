namespace mucomDotNET.Player
{
    internal class SChipType
    {
        private bool _UseEmu = true;
        public bool UseEmu
        {
            get
            {
                return _UseEmu;
            }

            set
            {
                _UseEmu = value;
            }
        }

        private bool _UseEmu2 = false;
        public bool UseEmu2
        {
            get
            {
                return _UseEmu2;
            }

            set
            {
                _UseEmu2 = value;
            }
        }

        private bool _UseEmu3 = false;
        public bool UseEmu3
        {
            get
            {
                return _UseEmu3;
            }

            set
            {
                _UseEmu3 = value;
            }
        }


        private bool _UseScci = false;
        public bool UseScci
        {
            get
            {
                return _UseScci;
            }

            set
            {
                _UseScci = value;
            }
        }

        private string _InterfaceName = "";
        public string InterfaceName
        {
            get
            {
                return _InterfaceName;
            }

            set
            {
                _InterfaceName = value;
            }
        }

        private int _SoundLocation = -1;
        public int SoundLocation
        {
            get
            {
                return _SoundLocation;
            }

            set
            {
                _SoundLocation = value;
            }
        }

        private int _BusID = -1;
        public int BusID
        {
            get
            {
                return _BusID;
            }

            set
            {
                _BusID = value;
            }
        }

        private int _SoundChip = -1;
        public int SoundChip
        {
            get
            {
                return _SoundChip;
            }

            set
            {
                _SoundChip = value;
            }
        }

        private string _ChipName = "";
        public string ChipName
        {
            get
            {
                return _ChipName;
            }

            set
            {
                _ChipName = value;
            }
        }


        private bool _UseScci2 = false;
        public bool UseScci2
        {
            get
            {
                return _UseScci2;
            }

            set
            {
                _UseScci2 = value;
            }
        }

        private string _InterfaceName2A = "";
        public string InterfaceName2A
        {
            get
            {
                return _InterfaceName2A;
            }

            set
            {
                _InterfaceName2A = value;
            }
        }

        private int _SoundLocation2A = -1;
        public int SoundLocation2A
        {
            get
            {
                return _SoundLocation2A;
            }

            set
            {
                _SoundLocation2A = value;
            }
        }

        private int _BusID2A = -1;
        public int BusID2A
        {
            get
            {
                return _BusID2A;
            }

            set
            {
                _BusID2A = value;
            }
        }

        private int _SoundChip2A = -1;
        public int SoundChip2A
        {
            get
            {
                return _SoundChip2A;
            }

            set
            {
                _SoundChip2A = value;
            }
        }

        private string _ChipName2A = "";
        public string ChipName2A
        {
            get
            {
                return _ChipName2A;
            }

            set
            {
                _ChipName2A = value;
            }
        }

        private string _InterfaceName2B = "";
        public string InterfaceName2B
        {
            get
            {
                return _InterfaceName2B;
            }

            set
            {
                _InterfaceName2B = value;
            }
        }

        private int _SoundLocation2B = -1;
        public int SoundLocation2B
        {
            get
            {
                return _SoundLocation2B;
            }

            set
            {
                _SoundLocation2B = value;
            }
        }

        private int _BusID2B = -1;
        public int BusID2B
        {
            get
            {
                return _BusID2B;
            }

            set
            {
                _BusID2B = value;
            }
        }

        private int _SoundChip2B = -1;
        public int SoundChip2B
        {
            get
            {
                return _SoundChip2B;
            }

            set
            {
                _SoundChip2B = value;
            }
        }

        private string _ChipName2B = "";
        public string ChipName2B
        {
            get
            {
                return _ChipName2B;
            }

            set
            {
                _ChipName2B = value;
            }
        }


        private bool _UseWait = true;
        public bool UseWait
        {
            get
            {
                return _UseWait;
            }

            set
            {
                _UseWait = value;
            }
        }

        private bool _UseWaitBoost = false;
        public bool UseWaitBoost
        {
            get
            {
                return _UseWaitBoost;
            }

            set
            {
                _UseWaitBoost = value;
            }
        }

        private bool _OnlyPCMEmulation = false;
        public bool OnlyPCMEmulation
        {
            get
            {
                return _OnlyPCMEmulation;
            }

            set
            {
                _OnlyPCMEmulation = value;
            }
        }

        private int _LatencyForEmulation = 0;
        public int LatencyForEmulation
        {
            get
            {
                return _LatencyForEmulation;
            }

            set
            {
                _LatencyForEmulation = value;
            }
        }

        private int _LatencyForScci = 0;
        public int LatencyForScci
        {
            get
            {
                return _LatencyForScci;
            }

            set
            {
                _LatencyForScci = value;
            }
        }


        public SChipType Copy()
        {
            SChipType ct = new SChipType();
            ct.UseEmu = this.UseEmu;
            ct.UseEmu2 = this.UseEmu2;
            ct.UseEmu3 = this.UseEmu3;
            ct.UseScci = this.UseScci;
            ct.SoundLocation = this.SoundLocation;

            ct.BusID = this.BusID;
            ct.InterfaceName = this.InterfaceName;
            ct.SoundChip = this.SoundChip;
            ct.ChipName = this.ChipName;
            ct.UseScci2 = this.UseScci2;
            ct.SoundLocation2A = this.SoundLocation2A;

            ct.InterfaceName2A = this.InterfaceName2A;
            ct.BusID2A = this.BusID2A;
            ct.SoundChip2A = this.SoundChip2A;
            ct.ChipName2A = this.ChipName2A;
            ct.SoundLocation2B = this.SoundLocation2B;

            ct.InterfaceName2B = this.InterfaceName2B;
            ct.BusID2B = this.BusID2B;
            ct.SoundChip2B = this.SoundChip2B;
            ct.ChipName2B = this.ChipName2B;

            ct.UseWait = this.UseWait;
            ct.UseWaitBoost = this.UseWaitBoost;
            ct.OnlyPCMEmulation = this.OnlyPCMEmulation;
            ct.LatencyForEmulation = this.LatencyForEmulation;
            ct.LatencyForScci = this.LatencyForScci;

            return ct;
        }
    }
}
