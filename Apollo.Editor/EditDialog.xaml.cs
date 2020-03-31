using System;
using System.Collections.Generic;
using System.Globalization;
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
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using set = Apollo.Editor.Properties.Settings;

namespace Apollo.Editor
{
    /// <summary>
    /// Logika interakcji dla klasy EditDialog.xaml
    /// </summary>
    public partial class EditDialog : UserControl
    {
        public static RoutedCommand SelectTarget = new RoutedCommand("SelectTarget", typeof(EditDialog), new InputGestureCollection() { new KeyGesture(Key.T, ModifierKeys.Control) });

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(IDialog), typeof(TextEditor), new PropertyMetadata(null));

        public IDialog Item
        {
            get => (IDialog)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        private bool updating = false;

        public EditDialog()
        {
            InitializeComponent();
            text.Editor.TextChanged += SetEdited;
        }

        public void Update()
        {
            updating = true;
            try
            {
                BindingOperations.SetBinding(txtName, TextBox.TextProperty, new Binding("Name") { Source = Item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                BindingOperations.SetBinding(txtNote, TextBox.TextProperty, new Binding("Note") { Source = Item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                BindingOperations.SetBinding(text.Editor, TextBox.TextProperty, new Binding("Text") { Source = Item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                BindingOperations.SetBinding(txtPostActions, TextBox.TextProperty, new Binding("PostActions") { Source = Item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                if (Item.GetType() == typeof(Dialog))
                {
                    text.Editor.AcceptsReturn = true;
                    spDialog.Visibility = Visibility.Visible;
                    spOption.Visibility = Visibility.Collapsed;
                    BindingOperations.SetBinding(cbStart, CheckBox.IsCheckedProperty, new Binding("Start") { Source = Item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    BindingOperations.SetBinding(txtActions, TextBox.TextProperty, new Binding("Actions") { Source = Item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    BindingOperations.SetBinding(cbTag, ComboBox.ItemsSourceProperty, new Binding("Tags") { Source = (App.Current.MainWindow as MainWindow).Story, Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    BindingOperations.SetBinding(cbTag, ComboBox.SelectedItemProperty, new Binding("Tag") { Source = Item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    txtPostActions.ToolTip = "Actions to execute after displaying dialog, but before displaying options";
                    txtID.Text = "ID: " + Item.ID.ToUpper();
                }
                else if (Item.GetType() == typeof(DialogOption))
                {
                    text.Editor.AcceptsReturn = false;
                    spDialog.Visibility = Visibility.Collapsed;
                    spOption.Visibility = Visibility.Visible;
                    BindingOperations.SetBinding(txtRequirement, TextBox.TextProperty, new Binding("Requirement") { Source = Item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    BindingOperations.SetBinding(txtTarget, TextBox.TextProperty, new Binding("TargetID") { Source = Item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    txtPostActions.ToolTip = "Actions to execute after selecting option";
                }
            }
            catch { }
            finally { updating = false; }
        }

        private void SetEdited(object sender, TextChangedEventArgs e)
        {
            if (updating) return;
            set.Default.Edited = true;
        }

        private void CbStart_Checked(object sender, RoutedEventArgs e)
        {
            if (updating) return;
            foreach (Dialog d in (App.Current.MainWindow as MainWindow).Story)
                if (d.ID != Item.ID)
                    d.Start = false;
            set.Default.Edited = true;
        }

        private async void SelectTarget_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(Item is DialogOption))
                return;
            IsEnabled = false;
            var target = await (App.Current.MainWindow as MainWindow).SelectDialog();
            IsEnabled = true;
            if (target != null && target.ID == "Cancel")
                return;
            (Item as DialogOption).TargetID = target == null ? "" : target.ID;
            (App.Current.MainWindow as MainWindow).UpdateEdge(Item as DialogOption);
            set.Default.Edited = true;
        }

        private void txtTarget_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void SelTarget_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Item is DialogOption;
        }

        private void txtTarget_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectTarget.Execute(null, this);
        }
    }
}