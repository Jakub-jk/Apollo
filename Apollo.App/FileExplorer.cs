using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Apollo.App
{
    public static class FileExplorer
    {
        private static IConsoleElement selected = null;
    }

    public interface IConsoleElement
    {
        string Name { get; }
        string Path { get; }
    }

    public class ConsoleDir : IConsoleElement
    {
        public string Name => Directory.Name;
        public string Path => Directory.FullName;
        public DirectoryInfo Directory { get; set; }
        public List<ConsoleDir> Directories { get; set; } = new List<ConsoleDir>();
        public List<FileInfo> Files { get; set; } = new List<FileInfo>();

        public static implicit operator ConsoleDir(DirectoryInfo d) => new ConsoleDir() { Directory = d };

        public void List()
        {
            Directories.AddRange(Directory.GetDirectories().Cast<ConsoleDir>());
            Files.AddRange(Directory.GetFiles("*.apst"));
        }
    }
}