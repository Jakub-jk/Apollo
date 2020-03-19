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
    /// Logika interakcji dla klasy Tags.xaml
    /// </summary>
    public partial class Tags : MetroWindow
    {
        public Tags()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            (App.Current.MainWindow as MainWindow).Story.Tags.Add(new Tag("<new>", System.Drawing.Color.White));
        }

        private void UpdateColor(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (updating) return;
            ((sliderR.Parent as Grid).Background as LinearGradientBrush).GradientStops[0].Color = Color.FromRgb(0, (byte)sliderG.Value, (byte)sliderB.Value);
            ((sliderR.Parent as Grid).Background as LinearGradientBrush).GradientStops[1].Color = Color.FromRgb(255, (byte)sliderG.Value, (byte)sliderB.Value);
            ((sliderG.Parent as Grid).Background as LinearGradientBrush).GradientStops[0].Color = Color.FromRgb((byte)sliderR.Value, 0, (byte)sliderB.Value);
            ((sliderG.Parent as Grid).Background as LinearGradientBrush).GradientStops[1].Color = Color.FromRgb((byte)sliderR.Value, 255, (byte)sliderB.Value);
            ((sliderB.Parent as Grid).Background as LinearGradientBrush).GradientStops[0].Color = Color.FromRgb((byte)sliderR.Value, (byte)sliderG.Value, 0);
            ((sliderB.Parent as Grid).Background as LinearGradientBrush).GradientStops[1].Color = Color.FromRgb((byte)sliderR.Value, (byte)sliderG.Value, 255);
            ((sliderA.Parent as Grid).Background as LinearGradientBrush).GradientStops[0].Color = Color.FromRgb((byte)sliderR.Value, (byte)sliderG.Value, (byte)sliderB.Value);
            var col = Color.FromArgb((byte)sliderA.Value, (byte)sliderR.Value, (byte)sliderG.Value, (byte)sliderB.Value);
            tagColor.Fill = new SolidColorBrush(col);
            if (tags.SelectedItem != null)
                (tags.SelectedItem as Tag).Color = col.ToDrawingColor();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private bool updating = false;

        private void tags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updating = true;
            var tag = tags.SelectedItem as Tag;
            sliderR.Value = tag.Color.R;
            sliderG.Value = tag.Color.G;
            sliderB.Value = tag.Color.B;
            sliderA.Value = tag.Color.A;
            updating = false;
            UpdateColor(null, null);
        }
    }
}