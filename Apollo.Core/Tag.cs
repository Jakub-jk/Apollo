using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Apollo
{
    public class Tag : INotifyPropertyChanged
    {
        private Color color;
        private string name;
        private string id = Guid.NewGuid().ToString();

        [XmlAttribute]
        public string ID { get => id; set { id = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ID")); } }

        [XmlIgnore]
        public Color Color { get => color; set { color = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color")); } }

        [XmlAttribute]
        public string Name { get => name; set { name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name")); } }

        [XmlAttribute]
        public string ColorHex
        {
            get => "#" + ((Color)Color).ToArgb().ToString("X8");
            set => Color = System.Drawing.Color.FromArgb(int.Parse(value.Substring(1, 8), System.Globalization.NumberStyles.HexNumber));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Tag()
        {
        }

        public Tag(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public Tag Clone()
        {
            return new Tag(Name, Color) { ID = ID };
        }
    }

    public class TagColor
    {
        public byte R { get; set; } = 0;
        public byte G { get; set; } = 0;
        public byte B { get; set; } = 0;
        public byte A { get; set; } = 255;

        public TagColor()
        {
        }

        public TagColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public TagColor(byte a, byte r, byte g, byte b) : this(r, g, b)
        {
            A = a;
        }

        public static implicit operator Color(TagColor c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static implicit operator TagColor(Color c)
        {
            try
            {
                return Color.FromArgb(c.A, c.R, c.G, c.B);
            }
            catch { return Color.FromArgb(0, 0, 0, 0); }
        }
    }
}