using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using set = Apollo.Editor.Properties.Settings;

namespace Apollo.Editor
{
    /// <summary>
    /// Logika interakcji dla klasy Splash.xaml
    /// </summary>
    public partial class Splash : MetroWindow
    {
        private bool opening = false;

        public Splash()
        {
            InitializeComponent();
            BindingOperations.SetBinding(Recent, ListBox.ItemsSourceProperty, new Binding("Recent") { Source = set.Default });
        }

        private void tileOpen_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog o = new VistaOpenFileDialog();
            o.Filter = "Apollo story|*.apstp";
            o.Title = "Open Apollo story...";
            o.FileOk += (s, ev) =>
            {
                if ((App.Current.MainWindow as MainWindow).Open(o.FileName))
                    OpenMain();
                else
                    this.ShowMessageAsync("Oops...", $"Error occured while loading story \"{System.IO.Path.GetFileName(o.FileName)}\"");
            };
            o.ShowDialog();
        }

        private void tileNew_Click(object sender, RoutedEventArgs e)
        {
            (App.Current.MainWindow as MainWindow).New();
            OpenMain();
        }

        private void Recent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((App.Current.MainWindow as MainWindow).Open(Recent.SelectedItem as string))
                OpenMain();
            else
                this.ShowMessageAsync("Oops...", $"Error occured while loading story \"{System.IO.Path.GetFileName(Recent.SelectedItem as string)}\"");
        }

        private void OpenMain()
        {
            opening = true;
            (App.Current.MainWindow as MainWindow).Show();
            (App.Current.MainWindow as MainWindow).ShowInTaskbar = true;
            Close();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            if (!opening)
                Application.Current.Shutdown();
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            var path = (sender as Button).Tag as string;
            if ((App.Current.MainWindow as MainWindow).Open(path))
                OpenMain();
            else
                this.ShowMessageAsync("Oops...", $"Error occured while loading story \"{System.IO.Path.GetFileName(path)}\"");
        }

        private void RemoveClick(object sender, RoutedEventArgs e)
        {
            var path = (sender as Button).Tag as string;
            set.Default.Recent.Remove(path);
            bool ed = set.Default.Edited;
            set.Default.Edited = false;
            set.Default.Save();
            set.Default.Edited = ed;
            Recent.Items.Refresh();
        }

        private void OpenElement(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if ((App.Current.MainWindow as MainWindow).Open(Recent.SelectedItem as string))
                    OpenMain();
                else
                    this.ShowMessageAsync("Oops...", $"Error occured while loading story \"{System.IO.Path.GetFileName(Recent.SelectedItem as string)}\"");
            }
        }
    }
}