using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using static System.Console;
using Apollo;

namespace Apollo.App
{
    public static class FileExplorer
    {
        public static string StartPath { get; set; } = "";

        public static FileInfo SelectFile(string filter = "*", int top = 1)
        {
            top = Math.Max(top, 1);
            int selected = 0;
            string path = StartPath;
            List<(string Text, string Path)> buffer = new List<(string Text, string Path)>();
            CursorVisible = false;
            Clear();
            while (true)
            {
                if (buffer.Count == 0)
                {
                    Clear();
                    if (path == "")
                    {
                        foreach (DriveInfo di in DriveInfo.GetDrives())
                        {
                            if (!di.IsReady) continue;
                            buffer.Add(($"[{di.Name}]{(di.VolumeLabel.IsNullOrEmpty() ? "" : " " + di.VolumeLabel)}", "?" + di.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            var dir = new DirectoryInfo(path);
                            buffer.Add(("..", dir.Parent == null ? "?" : "?" + dir.Parent.FullName));
                            foreach (var di in dir.GetDirectories())
                                buffer.Add(($"[{di.Name}]", "?" + di.FullName));
                            foreach (var fi in dir.GetFiles(filter))
                                buffer.Add(($"{fi.Name}", fi.FullName));
                        }
                        catch
                        {
                            Clear();
                            CursorTop = top;
                            WriteLine("Error occured while accessing \"" + path + "\". Trying parent directory in 3 seconds.");
                            Thread.Sleep(3000);
                            var tmp = Path.Combine(path, "..");
                            path = tmp == path ? "" : tmp;
                            continue;
                        }
                    }
                }
                CursorTop = top;
                CursorLeft = 0;
                for (int i = Math.Max(selected - WindowHeight + top + 1, 0); i < buffer.Count; i++)
                {
                    Write("\r" + new string(' ', WindowWidth - 1) + "\r");
                    if (i == selected)
                    {
                        var tmp = BackgroundColor;
                        BackgroundColor = ForegroundColor;
                        ForegroundColor = tmp;
                    }
                    Write(buffer[i].Text + (CursorTop + 1 == WindowHeight ? "" : Environment.NewLine));
                    ResetColor();
                    if (CursorTop + 1 == WindowHeight)
                    {
                        i++;
                        Write("\r" + new string(' ', WindowWidth - 1) + "\r");
                        if (i == selected)
                        {
                            var tmp = BackgroundColor;
                            BackgroundColor = ForegroundColor;
                            ForegroundColor = tmp;
                        }
                        Write(buffer[i].Text);
                        ResetColor();
                        break;
                    }
                }
                switch (ReadKey().Key)
                {
                    case ConsoleKey.Enter:
                        if (buffer[selected].Path.StartsWith("?"))
                            path = buffer[selected].Path.Substring(1);
                        else
                            return new FileInfo(Path.Combine(path, buffer[selected].Path));
                        selected = 0;
                        buffer.Clear();
                        break;

                    case ConsoleKey.UpArrow:
                        selected = Math.Max(0, selected - 1);
                        break;

                    case ConsoleKey.DownArrow:
                        selected = Math.Min(buffer.Count - 1, selected + 1);
                        break;

                    case ConsoleKey.Escape:
                        return null;
                }
            }

            void Clear()
            {
                CursorTop = top;
                for (int i = top; i < WindowHeight - 1; i++)
                    Write("\r" + new string(' ', WindowWidth - 1) + "\r\n");
                Write("\r" + new string(' ', WindowWidth - 1) + "\r");
                CursorTop = top;
                CursorLeft = 0;
            }
        }
    }
}