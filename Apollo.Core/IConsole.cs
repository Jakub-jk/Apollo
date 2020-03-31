using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo
{
    public interface IConsole
    {
        ContentParser Parser { get; set; }
        bool CursorVisible { get; set; }
        int CursorTop { get; set; }
        int CursorLeft { get; set; }
        int WindowWidth { get; }
        ConsoleColor BackgroundColor { get; set; }
        ConsoleColor ForegroundColor { get; set; }

        void Write(string s, params object[] args);

        void Write(object o);

        void WriteLine(string s, params object[] args);

        void WriteLine(object o);

        void ResetColor();

        void InvertColor();

        void Clear();

        ConsoleKey ReadKey();

        string ReadLine();
    }

    public class DefaultConsole : IConsole
    {
        public bool CursorVisible { get => Console.CursorVisible; set => Console.CursorVisible = value; }
        public int CursorTop { get => Console.CursorTop; set => Console.CursorTop = value; }
        public int CursorLeft { get => Console.CursorLeft; set => Console.CursorLeft = value; }
        public ConsoleColor BackgroundColor { get => Console.BackgroundColor; set => Console.BackgroundColor = value; }
        public ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }
        public int WindowWidth => Console.WindowWidth;

        public ContentParser Parser { get; set; }

        public void ResetColor()
        {
            ForegroundColor = Parser.DefaultText;
            BackgroundColor = Parser.DefaultBack;
        }

        public void InvertColor()
        {
            var tmp = BackgroundColor;
            BackgroundColor = ForegroundColor;
            ForegroundColor = tmp;
        }

        public void Write(string s, params object[] args)
        {
            Console.Write(s, args);
        }

        public void Write(object o)
        {
            Console.Write(o);
        }

        public void WriteLine(string s, params object[] args)
        {
            Console.WriteLine(s, args);
        }

        public void WriteLine(object o)
        {
            Console.WriteLine(o);
        }

        public void Clear()
        {
            Console.Clear();
        }

        public ConsoleKey ReadKey()
        {
            return Console.ReadKey().Key;
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}