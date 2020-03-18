using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace mucomDotNET.Common
{
    public static class msg
    {

        private static Dictionary<string, string> dicMsg = null;
        private static string otherLangFilename = Path.Combine("lang", "mucomDotNETmessage.{0}.txt");
        private static string englishFilename = Path.Combine("lang", "mucomDotNETmessage.txt");

        public static void MakeMessageDic(string[] lines)
        {
            MakeMessageDic(lines != null ? ParseMesseageDicDatas(lines) : null);
        }

        public static void MakeMessageDic(IEnumerable<KeyValuePair<string, string>> datas)
        {
            dicMsg = dicMsg ?? new Dictionary<string, string>();
            if (datas == null) return;
            dicMsg.Clear();

            foreach (var data in datas)
            {
                if (dicMsg.ContainsKey(data.Key)) continue;
                dicMsg.Add(data.Key, data.Value);
            }
        }

        public static string get(string code)
        {
            if (dicMsg == null) LoadDefaultMessage();

            if (dicMsg.ContainsKey(code))
            {
                return dicMsg[code].Replace("\\r", "\r").Replace("\\n", "\n");
            }

            return string.Format("<no message>({0})", code);
        }

        /// <summary>
        /// デフォルトで読み込むメッセージファイル名を変更する
        /// </summary>
        /// <param name="engFilename">ex)lang\mucomDotNETmessage.txt</param>
        /// <param name="otherFilename">ex)lang\mucomDotNETmessage.{0}.txt</param>
        public static void changeFilename(string engFilename, string otherFilename)
        {
            otherLangFilename = otherFilename;
            englishFilename = engFilename;
        }


        private static IEnumerable<KeyValuePair<string, string>> ParseMesseageDicDatas(string[] lines)
        {
            foreach (string line in lines)
            {
                string code;
                string msg;
                try
                {
                    if (line == null) continue;
                    if (line == "") continue;
                    string str = line.Trim();
                    if (str == "") continue;
                    if (str[0] == ';') continue;
                    code = str.Substring(0, str.IndexOf("=")).Trim();
                    msg = str.Substring(str.IndexOf("=") + 1, str.Length - str.IndexOf("=") - 1);

                }
                catch
                {
                    ;//握りつぶす
                    continue;
                }
                yield return new KeyValuePair<string, string>(code, msg);
            }
        }

        private static void LoadDefaultMessage()
        {
            string[] lines = null;
            try
            {
                Assembly myAssembly = Assembly.GetEntryAssembly();
                string path = Path.GetDirectoryName(myAssembly.Location);
                string lang = System.Globalization.CultureInfo.CurrentCulture.Name;
                string file = Path.Combine(path, string.Format(otherLangFilename, lang));
                file = file.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
                if (!File.Exists(file))
                {
                    file = Path.Combine(path, englishFilename);
                    file = file.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
                }
                lines = File.ReadAllLines(file);
            }
            catch
            {
                ;//握りつぶす
            }

            MakeMessageDic(lines);
        }

    }
}
