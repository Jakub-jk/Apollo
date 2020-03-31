using System;
using System.Linq;

namespace Apollo.App
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var file = args.FirstOrDefault(x => x.EndsWith(".apst"));
        check:
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(file))
            {
                Console.WriteLine("Select Apollo story file");
                Console.WriteLine("\r" + new string('-', Console.WindowWidth - 1));
                var sel = FileExplorer.SelectFile("*.apst", 2);
                if (sel != null)
                    file = sel.FullName;
                Console.Clear();
                goto check;
            }
            Story s = Story.LoadCompressed(file);
            s.StoryEnded += (s, e) =>
            {
                Console.Clear();
                Console.WriteLine("Story ended. Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            };
            s.Begin();
            while (true) ;
        }
    }
}