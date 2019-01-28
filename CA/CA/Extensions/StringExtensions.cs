using System;
using System.Security.Cryptography;
using System.Text;

namespace CA.Extensions
{
    public static class StringExtensions
    {
        public static Boolean Contains(this String This, String Substring, StringComparison StringComparison)
        {
            return This.IndexOf(Substring, StringComparison) >= 0;
        }

        public static String GetSHA256Hash(this String This)
        {
            StringBuilder _StringBuilder = new StringBuilder();

            using (var _Hash = SHA256.Create())
            {
                Encoding _Encoding = Encoding.UTF8;

                Byte[] _Result = _Hash.ComputeHash(_Encoding.GetBytes(This));

                foreach (Byte _Byte in _Result)
                {
                    _StringBuilder.Append(_Byte.ToString("x2"));
                }
            }

            return _StringBuilder.ToString();
        }
    }
}
