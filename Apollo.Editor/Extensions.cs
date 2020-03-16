using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Apollo.Editor
{
    public static class Extensions
    {
        public static Color ToLegacyColor(this ConsoleColor cl)
        {
            int[] cColors = {   0x000000, //Black = 0
                        0x000080, //DarkBlue = 1
                        0x008000, //DarkGreen = 2
                        0x008080, //DarkCyan = 3
                        0x800000, //DarkRed = 4
                        0x800080, //DarkMagenta = 5
                        0x808000, //DarkYellow = 6
                        0xC0C0C0, //Gray = 7
                        0x808080, //DarkGray = 8
                        0x0000FF, //Blue = 9
                        0x00FF00, //Green = 10
                        0x00FFFF, //Cyan = 11
                        0xFF0000, //Red = 12
                        0xFF00FF, //Magenta = 13
                        0xFFFF00, //Yellow = 14
                        0xFFFFFF  //White = 15
                    };
            var c = System.Drawing.Color.FromArgb(cColors[(int)cl]);
            return Color.FromArgb(255, c.R, c.G, c.B);
        }

        public static Color ToColor(this ConsoleColor cl)
        {
            int[] cColors = {   0x0C0C0C, //Black = 0
                        0x0037DA, //DarkBlue = 1
                        0x13A10E, //DarkGreen = 2
                        0x3A96DD, //DarkCyan = 3
                        0xC50F1F, //DarkRed = 4
                        0x881798, //DarkMagenta = 5
                        0xC19C00, //DarkYellow = 6
                        0xCCCCCC, //Gray = 7
                        0x767676, //DarkGray = 8
                        0x3B78FF, //Blue = 9
                        0x16C60C, //Green = 10
                        0x61D6D6, //Cyan = 11
                        0xE74856, //Red = 12
                        0xB4009E, //Magenta = 13
                        0xF9F1A5, //Yellow = 14
                        0xF2F2F2  //White = 15
                    };
            var c = System.Drawing.Color.FromArgb(cColors[(int)cl]);
            return Color.FromArgb(255, c.R, c.G, c.B);
        }
    }
}