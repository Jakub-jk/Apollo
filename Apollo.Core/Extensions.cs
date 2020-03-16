using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo
{
    public static class Extensions
    {
        public static string Multiply(this string s, int times)
        {
            string ret = "";
            for (int i = 0; i < times; i++)
                ret += s;
            return ret;
        }

        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
    }
}
