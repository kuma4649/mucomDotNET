using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.Common
{
    public class mucomChipAction : ChipAction
    {
        private Action<ChipDatum> _Write;
        private Action<byte[], int, int> _WritePCMData;
        private Action<long, int> _WaitSend;

        public mucomChipAction(Action<ChipDatum> Write, Action<byte[], int, int> WritePCMData, Action<long, int> WaitSend)
        {
            _Write = Write;
            _WritePCMData = WritePCMData;
            _WaitSend = WaitSend;
        }

        public override string GetChipName()
        {
            throw new NotImplementedException();
        }

        public override void WaitSend(long t1, int t2)
        {
            //throw new NotImplementedException();
        }

        public override void WritePCMData(byte[] data, int startAddress, int endAddress)
        {
            _WritePCMData?.Invoke(data, startAddress, endAddress);
        }

        public override void WriteRegister(ChipDatum cd)
        {
            _Write?.Invoke(cd);
        }
    }
}
