using MahApps.Metro.Controls;
using Markdig;
using mshtml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Apollo.Editor
{
    /// <summary>
    /// Logika interakcji dla klasy ExpressionsHelp.xaml
    /// </summary>
    public partial class ExpressionsHelp : MetroWindow
    {
        private Dictionary<string, string> HelpFiles = new Dictionary<string, string>();
        private string path = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Help");

        public ExpressionsHelp()
        {
            InitializeComponent();
            if (Directory.Exists(path))
            {
                string style = "";
                if (File.Exists(Path.Combine(path, "style.css")))
                    style = $"<style>{File.ReadAllText(Path.Combine(path, "style.css"))}</style>";
                var pipeline = new MarkdownPipelineBuilder().UseSoftlineBreakAsHardlineBreak().UsePipeTables().Build();
                foreach (var v in Directory.GetFiles(path, "*.md"))
                    HelpFiles.Add(Path.GetFileNameWithoutExtension(v).Replace("_", " "), $@"<!DOCTYPE html>
            <html>
            <head>
            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" >
            { style}
            </head>
            <body>
            { Markdown.ToHtml(File.ReadAllText(v), pipeline).Replace("<thead>", "").Replace("</thead>", "").Replace("<tbody>", "").Replace("</tbody>", "")}
            </body>
            </html> ");
                Tree.ItemsSource = HelpFiles;
                Tree.Items.Refresh();
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Tree_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            Browser.NavigateToString(((KeyValuePair<string, string>)Tree.SelectedItem).Value);
        }

        private void MetroWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Tree.ItemContainerGenerator.ContainerFromItem(Tree.ItemContainerGenerator.Items.Cast<KeyValuePair<string, string>>().First(x => x.Key == "Introduction")).SetValue(TreeViewItem.IsSelectedProperty, true);
        }

        private void Browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (((KeyValuePair<string, string>)Tree.SelectedItem).Key == "Colors")
            {
                var doc = Browser.Document as HTMLDocument;
                foreach (ConsoleColor v in Enum.GetValues(typeof(ConsoleColor)))
                {
                    doc.getElementById(((int)v).ToString("00")).parentElement.style.backgroundColor = v.ToLegacyColor().ToString().Replace("#FF", "#");
                    doc.getElementById(((int)v).ToString("00") + "Win").parentElement.style.backgroundColor = v.ToColor().ToString().Replace("#FF", "#");
                    doc.getElementById(((int)v).ToString("00") + "Text").parentElement.innerHTML = v.ToString();
                }
            }
        }

        private void Browser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri != null)
            {
                if (HelpFiles.ContainsKey(e.Uri.AbsolutePath))
                {
                    e.Cancel = true;
                    Tree.ItemContainerGenerator.ContainerFromItem(Tree.ItemContainerGenerator.Items.Cast<KeyValuePair<string, string>>().First(x => x.Key == e.Uri.AbsolutePath)).SetValue(TreeViewItem.IsSelectedProperty, true);
                }
                else if (e.Uri.AbsolutePath.StartsWith("Licenses/"))
                {
                    e.Cancel = true;
                    var file = Path.Combine(path, e.Uri.AbsolutePath);
                    if (File.Exists(file))
                        Process.Start(file);
                }
                else
                {
                    e.Cancel = true;
                    Process.Start(e.Uri.ToString());
                }
            }
        }
    }
}