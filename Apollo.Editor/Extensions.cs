using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public static Color ToMediaColor(this System.Drawing.Color c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static Color ToMediaColor(this System.Drawing.Color c, byte alpha)
        {
            return Color.FromArgb(alpha, c.R, c.G, c.B);
        }

        public static Color ToMediaColor(this TagColor c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static Color ToMediaColor(this TagColor c, byte alpha)
        {
            return Color.FromArgb(alpha, c.R, c.G, c.B);
        }

        public static System.Drawing.Color ToDrawingColor(this Color c)
        {
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static System.Drawing.Color ToDrawingColor(this Color c, byte alpha)
        {
            return System.Drawing.Color.FromArgb(alpha, c.R, c.G, c.B);
        }

        public static Tag GetTag(this Dialog d)
        {
            if (d == null || App.Current.MainWindow == null || (App.Current.MainWindow as MainWindow).Story == null) return null;
            var res = (App.Current.MainWindow as MainWindow).Story.Tags.FirstOrDefault(x => x.ID == d.TagID);
            return res;
        }

        public static System.Drawing.Color ColorFromHLS(double h, double l, double s)
        {
            int r, g, b;
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double double_r, double_g, double_b;
            if (s == 0)
            {
                double_r = l;
                double_g = l;
                double_b = l;
            }
            else
            {
                double_r = QqhToRgb(p1, p2, h + 120);
                double_g = QqhToRgb(p1, p2, h);
                double_b = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 255 range.
            r = (int)(double_r * 255.0);
            g = (int)(double_g * 255.0);
            b = (int)(double_b * 255.0);
            return System.Drawing.Color.FromArgb(r, g, b);
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }

        public static BitmapImage ToMediaBitmap(this System.Drawing.Bitmap b)
        {
            using (var ms = new MemoryStream())
            {
                b.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Position = 0;
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                return bmp;
            }
        }
    }
}