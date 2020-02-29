using System.Text;

namespace mucomDotNET.Common
{
    public class myEncoding : iEncoding
    {
        public byte[] GetSjisArrayFromString(string utfString)
        {
            return Encoding.GetEncoding("shift_jis").GetBytes(utfString);
        }

        public string GetStringFromSjisArray(byte[] sjisArray)
        {
            return Encoding.GetEncoding("shift_jis").GetString(sjisArray);
        }

        public string GetStringFromSjisArray(byte[] sjisArray, int index, int count)
        {
            return Encoding.GetEncoding("shift_jis").GetString(sjisArray, index, count);
        }

        public string GetStringFromUtfArray(byte[] utfArray)
        {
            return Encoding.UTF8.GetString(utfArray);
        }

        public byte[] GetUtfArrayFromString(string utfString)
        {
            return Encoding.UTF8.GetBytes(utfString);
        }
    }
}