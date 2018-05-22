using System;
using System.Security.Cryptography;
using System.Text;

namespace ChivServ
{
    public static class SHA1Util
    {
        public static string HexStringFromBytes(byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            byte[] numArray = bytes;
            for (int i = 0; i < (int)numArray.Length; i++)
            {
                string str = numArray[i].ToString("X2");
                stringBuilder.Append(str);
            }
            return stringBuilder.ToString();
        }

        public static string SHA1HashStringForUTF8String(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return SHA1Util.HexStringFromBytes(SHA1.Create().ComputeHash(bytes));
        }
    }
}