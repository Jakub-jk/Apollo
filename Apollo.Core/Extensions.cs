using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public static byte[] Compress(this string s)
        {
            using (var text = new MemoryStream(Encoding.UTF8.GetBytes(s)))
            using (var comp = new MemoryStream())
            {
                using (var gs = new GZipStream(comp, CompressionMode.Compress))
                {
                    text.CopyTo(gs);
                }

                return comp.ToArray();
            }
        }

        public static string Decompress(this byte[] b)
        {
            using (var comp = new MemoryStream(b))
            using (var text = new MemoryStream())
            {
                using (var gs = new GZipStream(comp, CompressionMode.Decompress))
                {
                    gs.CopyTo(text);
                }

                return Encoding.UTF8.GetString(text.ToArray());
            }
        }
    }
}