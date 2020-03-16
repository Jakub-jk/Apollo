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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Apollo.Editor
{
    /// <summary>
    /// Logika interakcji dla klasy TextEditor.xaml
    /// </summary>
    public partial class TextEditor : UserControl
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(TextEditor), new PropertyMetadata("Content"));
        public ContentParser cp = null;

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextEditor), new PropertyMetadata(""));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public TextEditor()
        {
            InitializeComponent();
            DataContext = this;
            sbFore.ItemsSource = sbBack.ItemsSource = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>();
            cp = new ContentParser();
            cp.Output = new PreviewConsole(Preview);
            Task.Run(() =>
            {
                while (Dispatcher.Invoke(() => App.Current.MainWindow) == null) ;
                Dispatcher.Invoke(() =>
                {
                    (App.Current.MainWindow as MainWindow).StoryChanged += (se, ea) =>
                  {
                      (App.Current.MainWindow as MainWindow).Story.PropertyChanged += (s, e) =>
                      {
                          if (e.PropertyName == "DefaultText")
                              cp.DefaultText = (s as Story).DefaultText;
                          if (e.PropertyName == "DefaultBack")
                              cp.DefaultBack = (s as Story).DefaultBack;
                          cp.Output.Clear();
                          cp.Display(Editor.Text, true);
                      };
                  };
                });
            });
        }

        private void ClearColor(object sender, RoutedEventArgs e)
        {
            int caret = Editor.CaretIndex, sel = lastSel;
            Editor.Text = Editor.Text.Insert(Editor.CaretIndex, "`c`");
            Editor.Focus();
            Editor.CaretIndex = caret + 3;
            Editor.SelectionLength = sel;
        }

        private void ClearBack(object sender, RoutedEventArgs e)
        {
            int caret = Editor.CaretIndex, sel = lastSel;
            Editor.Text = Editor.Text.Insert(Editor.CaretIndex, "`c:b`");
            Editor.Focus();
            Editor.CaretIndex = caret + 3;
            Editor.SelectionLength = sel;
        }

        private void ClearText(object sender, RoutedEventArgs e)
        {
            int caret = Editor.CaretIndex, sel = lastSel;
            Editor.Text = Editor.Text.Insert(Editor.CaretIndex, "`c:f`");
            Editor.Focus();
            Editor.CaretIndex = caret + 3;
            Editor.SelectionLength = sel;
        }

        private void btnInvert_Click(object sender, RoutedEventArgs e)
        {
            int caret = Editor.CaretIndex, sel = lastSel;
            Editor.Text = Editor.Text.Insert(Editor.CaretIndex, "`i`");
            Editor.Focus();
            Editor.CaretIndex = caret + 3;
            Editor.SelectionLength = sel;
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            cp.Output.Clear();
            cp.Display(Editor.Text, true);
            Properties.Settings.Default.Edited = true;
        }

        private void btnExpression_Click(object sender, RoutedEventArgs e)
        {
            int caret = Editor.CaretIndex, sel = lastSel;
            Editor.Text = Editor.Text.Insert(Editor.CaretIndex, "`e:`");
            Editor.Focus();
            Editor.CaretIndex = caret + 3;
            Editor.SelectionLength = sel;
        }

        private void InsertVariable(object sender, RoutedEventArgs e)
        {
            int caret = Editor.CaretIndex, sel = lastSel;
            var ins = $"`v:{(sender as MenuItem).Header.ToString()}`";
            Editor.Text = Editor.Text.Insert(Editor.CaretIndex, ins);
            Editor.Focus();
            Editor.CaretIndex = caret + ins.Length;
            Editor.SelectionLength = sel;
        }

        private void SetTextColor(object sender, RoutedEventArgs e)
        {
            var v = (ConsoleColor)(sender as MenuItem).Header;
            var grad = sbFore.BorderBrush as LinearGradientBrush;
            grad.GradientStops[0].Color = v.ToLegacyColor();
            grad.GradientStops[1].Color = v.ToColor();
            int caret = Editor.CaretIndex, sel = lastSel + caret + 6;
            Editor.Text = Editor.Text.Insert(Editor.CaretIndex, $"`f:{(int)v:00}`");
            if (sel > caret + 6 && (Editor.Text.Length == sel || (Editor.Text.Length > sel + 2 && Editor.Text.Substring(sel, 2) != "`c")))
                Editor.Text = Editor.Text.Insert(sel, "`c`");
            Editor.Focus();
            Editor.CaretIndex = caret + 6;
            Editor.SelectionLength = sel - caret - 6;
        }

        private void SetBackColor(object sender, RoutedEventArgs e)
        {
            var v = (ConsoleColor)(sender as MenuItem).Header;
            var grad = sbFore.BorderBrush as LinearGradientBrush;
            grad.GradientStops[0].Color = v.ToLegacyColor();
            grad.GradientStops[1].Color = v.ToColor();
            int caret = Editor.CaretIndex, sel = lastSel + caret + 6;
            Editor.Text = Editor.Text.Insert(Editor.CaretIndex, $"`b:{(int)v:00}`");
            if (sel > caret + 6 && (Editor.Text.Length == sel || (Editor.Text.Length > sel + 2 && Editor.Text.Substring(sel, 2) != "`c")))
                Editor.Text = Editor.Text.Insert(sel, "`c`");
            Editor.Focus();
            Editor.CaretIndex = caret + 6;
            Editor.SelectionLength = sel - caret - 6;
        }

        private int lastSel = 0;

        private void Editor_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            lastSel = Editor.SelectionLength;
        }
    }
}