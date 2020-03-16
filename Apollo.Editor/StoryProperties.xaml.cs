using MahApps.Metro.Controls;
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

namespace Apollo.Editor
{
    /// <summary>
    /// Logika interakcji dla klasy StoryProperties.xaml
    /// </summary>
    public partial class StoryProperties : MetroWindow
    {
        public StoryProperties()
        {
            InitializeComponent();
            DefaultBack.ItemsSource = DefaultText.ItemsSource = Enum.GetValues(typeof(ConsoleColor));
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void SetEdited(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.Edited = true;
        }

        private void DefaultText_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.Edited = true;
        }

        private void DefaultBack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.Edited = true;
        }
    }
}