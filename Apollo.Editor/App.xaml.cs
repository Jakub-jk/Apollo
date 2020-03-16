using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Apollo.Editor
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unexpected error occured" + Environment.NewLine + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace, "Apollo Editor error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}