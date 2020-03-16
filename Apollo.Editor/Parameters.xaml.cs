using MahApps.Metro.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
    /// Logika interakcji dla klasy Parameters.xaml
    /// </summary>
    public partial class Parameters : MetroWindow
    {
        public ObservableCollection<Variable> Params { get; set; } = new ObservableCollection<Variable>() { new Variable() { Name = "dummy", Value = true } };

        public Parameters()
        {
            InitializeComponent();
            //Params.CollectionChanged += (s, e) => System.Diagnostics.Debug.WriteLine(e.NewItems);
            //data.ItemsSource = Params;
            //data.Items.Refresh();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string newname = "<new>";
            for (int i = 1; Params.Where(x => x.Name == newname).Any(); i++)
                newname = $"<new> ({i})";
            (App.Current.MainWindow as MainWindow).Story.Variables.Add(new Variable() { Name = newname });
        }

        private void BtnRem_Click(object sender, RoutedEventArgs e)
        {
            if (data.SelectedItem != null && data.SelectedItem is Variable)
                (App.Current.MainWindow as MainWindow).Story.Variables.Remove((Variable)data.SelectedItem);
        }

        private void data_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                var txt = e.EditingElement as TextBox;
                string newname = txt.Text.IsNullOrEmpty() ? "<unnamed>" : txt.Text;
                for (int i = 1; Params.Where(x => x.Name == newname).Any(); i++)
                    newname = txt.Text + $" ({i})";
                txt.Text = newname;
            }
            catch { }
        }
    }
}