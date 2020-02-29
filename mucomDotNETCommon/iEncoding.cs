using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.Common
{
    public interface iEncoding
    {
        string GetStringFromSjisArray(byte[] sjisArray);

        string GetStringFromSjisArray(byte[] sjisArray,int index,int count);

        byte[] GetSjisArrayFromString(string utfString);

        string GetStringFromUtfArray(byte[] utfArray);

        byte[] GetUtfArrayFromString(string utfString);

    }
}
