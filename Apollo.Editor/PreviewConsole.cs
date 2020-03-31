using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Apollo.Editor
{
    internal class PreviewConsole : IConsole
    {
        private WebBrowser browser = null;
        private ConsoleColor backgroundColor;
        private ConsoleColor foregroundColor;

        private IHTMLDocument2 doc => browser.Document as IHTMLDocument2;
        private string target;

        public ContentParser Parser { get; set; }
        public bool CursorVisible { get; set; }
        public int CursorTop { get; set; }
        public int CursorLeft { get; set; }
        public int WindowWidth => int.MaxValue;

        public ConsoleColor BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                var span = doc.createElement("span");
                span.id = Guid.NewGuid().ToString();
                span.style.backgroundColor = value.ToColor().ToString().Replace("#FF", "#");
                Write(span.outerHTML);
                target = span.id;
            }
        }

        public ConsoleColor ForegroundColor
        {
            get => foregroundColor;
            set
            {
                foregroundColor = value;
                var span = doc.createElement("span");
                span.id = Guid.NewGuid().ToString();
                span.style.color = value.ToColor().ToString().Replace("#FF", "#");
                Write(span.outerHTML);
                target = span.id;
            }
        }

        public void Clear()
        {
            if (doc.body == null) return;
            doc.body.innerHTML = "";
            doc.body.style.color = Parser.DefaultText.ToColor().ToString().Replace("#FF", "#");
            doc.body.style.backgroundColor = Parser.DefaultBack.ToColor().ToString().Replace("#FF", "#");
            target = doc.body.id;
        }

        public void InvertColor()
        {
            var tmp = BackgroundColor;
            BackgroundColor = ForegroundColor;
            ForegroundColor = tmp;
        }

        public ConsoleKey ReadKey()
        {
            return ConsoleKey.Enter;
        }

        public void ResetColor()
        {
        }

        public void Write(string s, params object[] args)
        {
            if (doc == null) return;
            var trg = (doc as HTMLDocument).getElementById(target);
            if (trg != null && !s.IsNullOrEmpty())
                trg.innerHTML += s.Replace("\n", "</br>");
        }

        public void Write(object o)
        {
            Write(o.ToString());
        }

        public void WriteLine(string s, params object[] args)
        {
            Write(s + "</br>", args);
        }

        public void WriteLine(object o)
        {
            WriteLine(o.ToString());
        }

        public string ReadLine()
        {
            throw new NotImplementedException();
        }

        public PreviewConsole(WebBrowser browser)
        {
            this.browser = browser;
            var guid = Guid.NewGuid().ToString();
            this.browser.NavigateToString("<html><head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" >" + "<style>body { margin: 0; padding: 8px; font-family: Consolas, monospace; color: white; background-color: black;}</style>" + "</head><body id='" + guid + "'></body><html>");
            target = guid;
        }
    }
}