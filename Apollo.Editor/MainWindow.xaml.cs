using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using Apollo;
using set = Apollo.Editor.Properties.Settings;
using Ookii.Dialogs.Wpf;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shell;
using System;
using System.Collections;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Data;
using GraphX.Common.Enums;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using System.Windows.Media.Animation;
using GraphX.Controls;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media;

namespace Apollo.Editor
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static RoutedCommand NewStoryCmd = new RoutedCommand("NewStoryCmd", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.N, ModifierKeys.Control) });
        public static RoutedCommand OpenCmd = new RoutedCommand("OpenCmd", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.O, ModifierKeys.Control) });
        public static RoutedCommand SaveCmd = new RoutedCommand("SaveCmd", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.S, ModifierKeys.Control) });
        public static RoutedCommand SaveAsCmd = new RoutedCommand("SaveAsCmd", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift) });

        public static RoutedCommand ShowPropertiesCmd = new RoutedCommand("ShowPropertiesCmd", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.P, ModifierKeys.Control | ModifierKeys.Shift) });
        public static RoutedCommand ShowVariablesCmd = new RoutedCommand("ShowVariablesCmd", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.V, ModifierKeys.Control | ModifierKeys.Shift) });
        public static RoutedCommand ShowTagsCmd = new RoutedCommand("ShowTagsCmd", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.T, ModifierKeys.Control) });
        public static RoutedCommand ExportCmd = new RoutedCommand("ExportCmd", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.E, ModifierKeys.Control) });

        private EditDialog editor = new EditDialog() { HorizontalAlignment = HorizontalAlignment.Stretch };
        private StoryProperties sp = new StoryProperties();
        private ExpressionsHelp eh = new ExpressionsHelp();
        private Parameters ps = new Parameters();
        private List<GraphPos> pos = new List<GraphPos>();
        private Tags ts = new Tags();
        private object placeholder = null;
        private string path = "";
        public Story Story { get; set; } = null;

        public event EventHandler<Story> StoryChanged;

        public MainWindow()
        {
            App.Current.MainWindow = this;
            InitializeComponent();
            ZoomControl.SetViewFinderVisibility(zoom, Visibility.Collapsed);
            //BindingOperations.SetBinding(editor, EditDialog.ItemProperty, new Binding("SelectedItem") { Source = tree, Mode = BindingMode.TwoWay });
            placeholder = scroll.Content;
            if (set.Default.Recent == null)
            {
                set.Default.Recent = new System.Collections.Specialized.StringCollection();
                SaveSettings();
            }
            if (JumpList.GetJumpList(App.Current) == null)
                JumpList.SetJumpList(App.Current, new JumpList());
            var args = Environment.GetCommandLineArgs();
        check:
            if (args.Length > 1 && File.Exists(args[1]) && (args[1].EndsWith(".apstp") || args[1].EndsWith(".apmap")))
            {
                if (args[1].EndsWith(".apmap"))
                {
                    args[1].Replace(".apmap", ".apstp");
                    goto check;
                }
                if (!Open(args[1]))
                {
                    args = new string[1];
                    goto check;
                }
            }
            else UpdateRecent(false);
            sp.txtTitle.TextChanged += (s, e) => Title = "Apollo Editor - " + sp.txtTitle.Text;

            List<CommandBinding> Bindings = new List<CommandBinding>()
            {
                new CommandBinding(ShowPropertiesCmd, ShowProperties),
                new CommandBinding(ShowVariablesCmd, (s, e) => ps.ShowDialog()),
                new CommandBinding(ShowTagsCmd, (s, e) => ts.ShowDialog()),
                new CommandBinding(NewStoryCmd, New),
                new CommandBinding(OpenCmd, Open),
                new CommandBinding(SaveAsCmd, SaveAs),
                new CommandBinding(SaveCmd, Save),
                new CommandBinding(ExportCmd, Export)
            };

            foreach (var v in Bindings)
            {
                CommandBindings.Add(v);
                (Resources["FileMenu"] as ContextMenu).CommandBindings.Add(v);
                (Resources["StoryMenu"] as ContextMenu).CommandBindings.Add(v);
            }

            if (path.IsNullOrEmpty())
            {
                ShowInTaskbar = false;
                Hide();
                new Splash().Show();
            }
        }

        public void Export(object sender, ExecutedRoutedEventArgs e)
        {
        }

        public void New()
        {
            path = "";
            Open(true);
        }

        public bool Open(string path)
        {
            this.path = path;
            Open(skipMessage: true);
            if (Story == null)
                path = "";
            return Story != null;
        }

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (scroll.Content.GetType() == typeof(Label))
                scroll.Content = editor;
            btnOption.IsEnabled = btnUp.IsEnabled = btnDown.IsEnabled = tree.SelectedItem != null && !Selecting;

            try
            {
                int offset = 100;
                if (tree.SelectedItem != null && !VerDown)
                {
                    var vc = graph.VertexList.FirstOrDefault(x => x.Key.Dialog.ID == ((tree.SelectedItem is Dialog) ? (tree.SelectedItem as Dialog).ID : Story.FindParent(tree.SelectedItem as DialogOption).ID)).Value;
                    if (vc != null)
                    {
                        var pos = vc.GetPosition();
                        if (tree.SelectedItem is DialogOption)
                        {
                            var vc2 = graph.VertexList.FirstOrDefault(x => x.Key.Dialog.ID == Story[(tree.SelectedItem as DialogOption).TargetID].ID).Value;
                            if (vc2 != null)
                            {
                                var pos2 = vc2.GetPosition();
                                pos = new Point((pos.X + pos2.X) / 2, (pos.Y + pos2.Y) / 2);
                            }
                        }
                        if (!(double.IsNaN(pos.X) || double.IsNaN(pos.Y)))
                            zoom.ZoomToContent(new Rect(pos.X - offset, pos.Y - offset, vc.ActualWidth + offset * 2, vc.ActualHeight + offset * 3));
                    }
                }
            }
            catch { }

            try
            {
                foreach (Dialog d in tree.Items)
                {
                    var res = tree.SelectedItem == d || (tree.SelectedItem is DialogOption && Story.FindParent(tree.SelectedItem as DialogOption) == d);
                    (tree.ItemContainerGenerator.ContainerFromItem(d) as TreeViewItem).IsExpanded = res;
                }
            }
            catch
            {
                if (tree.Items.Count == 0)
                    scroll.Content = placeholder;
            }
            if (!Selecting)
            {
                editor.Item = tree.SelectedItem as IDialog;
                editor.Update();
            }
            if (!loading && !path.IsNullOrEmpty())
                Save();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var tmp = new Dialog() { Name = "<new>" };
            Story.Dialogs.Add(tmp);
            tmp.PropertyChanged += (s, ea) => UpdateVertex(s as Dialog);
            var dv = new DialogVertex(tmp);
            var vc = new VertexControl(dv);
            gr.AddVertex(dv);
            graph.AddVertexAndData(dv, vc);
            if (graph.VertexList.Count == 1)
            {
                graph.VertexList.First().Value.SetPosition(0, 0);
                graph.UpdateLayout();
            }
            else Relayout();
            foreach (Dialog v in tree.Items)
                v.Selected = false;
            tmp.Selected = true;
            set.Default.Edited = true;
            tree.Items.Refresh();
        }

        private void BtnRem_Click(object sender, RoutedEventArgs e)
        {
            if (tree.SelectedItem == null)
                return;
            if (tree.SelectedItem is DialogOption)
            {
                var edge = graph.EdgesList.FirstOrDefault(x => x.Key.DialogOption.ID == (tree.SelectedItem as DialogOption).ID);
                if (edge.Key != null)
                {
                    graph.RemoveEdge(edge.Key);
                    gr.RemoveEdge(edge.Key);
                }
                Story.FindParent(tree.SelectedItem as DialogOption).Options.Remove(tree.SelectedItem as DialogOption);
            }
            else
            {
                var vertex = graph.VertexList.FirstOrDefault(x => x.Key.Dialog.ID == (tree.SelectedItem as Dialog).ID);
                if (vertex.Key != null)
                {
                    gr.RemoveVertex(vertex.Key);
                    graph.RemoveVertex(vertex.Key);
                }
                Story.Dialogs.Remove(tree.SelectedItem as Dialog);
            }
            Relayout();
            //graph.UpdateLayout();
            set.Default.Edited = true;
            tree.Items.Refresh();
        }

        private void BtnOption_Click(object sender, RoutedEventArgs e)
        {
            var target = tree.SelectedItem;
            var tmp = new DialogOption() { Name = "<new>", Selected = true };
            if (target is DialogOption)
                Story.FindParent(target as DialogOption).Options.Add(tmp);
            else
                (target as Dialog).Options.Add(tmp);
            set.Default.Edited = true;
            //tree.Items.Refresh();
            tmp.Selected = true;
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (tree.SelectedItem == null)
                return;
            if (tree.SelectedItem is DialogOption)
            {
                var parent = Story.FindParent(tree.SelectedItem as DialogOption);
                var index = parent.Options.IndexOf(tree.SelectedItem as DialogOption);
                if (index > 0)
                    parent.Options.Move(index, index - 1);
            }
            else
            {
                var index = Story.Dialogs.IndexOf(tree.SelectedItem as Dialog);
                if (index > 0)
                    Story.Dialogs.Move(index, index - 1);
            }
            tree.Items.Refresh();
            set.Default.Edited = true;
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            if (tree.SelectedItem == null)
                return;
            if (tree.SelectedItem is DialogOption)
            {
                var parent = Story.FindParent(tree.SelectedItem as DialogOption);
                var index = parent.Options.IndexOf(tree.SelectedItem as DialogOption);
                if (index < parent.Options.Count - 1)
                    parent.Options.Move(index, index + 1);
            }
            else
            {
                var index = Story.Dialogs.IndexOf(tree.SelectedItem as Dialog);
                if (index < Story.Dialogs.Count - 1)
                    Story.Dialogs.Move(index, index + 1);
            }
            tree.Items.Refresh();
            set.Default.Edited = true;
        }

        private void BtnFileMenu_Click(object sender, RoutedEventArgs e)
        {
            (Resources["FileMenu"] as ContextMenu).IsOpen = true;
        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(path))
                SaveAsCmd.Execute(null, this);
            else
            {
                Save();
                set.Default.Edited = false;
            }
        }

        private void SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            VistaSaveFileDialog s = new VistaSaveFileDialog();
            s.Filter = "Apollo story|*.apstp";
            s.Title = "Save Apollo story...";
            s.OverwritePrompt = true;
            s.AddExtension = true;
            s.FileOk += (sn, ev) =>
            {
                path = s.FileName + (s.FileName.EndsWith(".apstp") ? "" : ".apstp");
                UpdateRecent();
                Save();
                set.Default.Edited = false;
            };
            s.ShowDialog();
        }

        private async Task<bool> ShowSaveDialog()
        {
            if (!set.Default.Edited) return true;
            MetroDialogSettings md = new MetroDialogSettings();
            md.AffirmativeButtonText = "Yes";
            md.NegativeButtonText = "No";
            md.FirstAuxiliaryButtonText = "Cancel";
            var result = await this.ShowMessageAsync("Wait a second!", "There are some unsaved changes. Do you want to save them now?", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, md);
            switch (result)
            {
                case MessageDialogResult.Affirmative:
                    SaveCmd.Execute(null, this);
                    return true;

                case MessageDialogResult.Negative:
                    return true;

                case MessageDialogResult.FirstAuxiliary:
                default:
                    return false;
            }
        }

        private bool reqClose = false;

        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (reqClose) return;
            e.Cancel = true;
            if (await ShowSaveDialog())
            {
                reqClose = true;
                try
                {
                    App.Current.Shutdown();
                }
                catch { }
            }
        }

        private GraphExample gr = new GraphExample();

        private void GenerateGraph(bool reset = false)
        {
            gr.Clear();
            foreach (var d in Story.Dialogs)
                gr.AddVertex(new DialogVertex(d));

            foreach (var v in gr.Vertices)
            {
                foreach (var o in v.Dialog.Options)
                    if (!o.TargetID.IsNullOrEmpty())
                        gr.AddEdge(new DialogEdge(v, gr.Vertices.First(x => x.Dialog.ID == o.TargetID)) { DialogOption = o });
            }

            if (graph.LogicCore == null)
            {
                var LogicCore = new GXLogicCoreExample() { Graph = gr };
                LogicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Tree;
                LogicCore.DefaultLayoutAlgorithmParams =
                                   LogicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.Tree);
                ((SimpleTreeLayoutParameters)LogicCore.DefaultLayoutAlgorithmParams).Direction = LayoutDirection.LeftToRight;
                LogicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
                LogicCore.DefaultOverlapRemovalAlgorithmParams =
                                  LogicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
                LogicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
                LogicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;
                LogicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
                LogicCore.AsyncAlgorithmCompute = false;
                LogicCore.EdgeCurvingEnabled = true;
                LogicCore.EnableParallelEdges = true;
                LogicCore.ParallelEdgeDistance = 10;

                graph.LogicCore = LogicCore;
            }

            graph.GenerateGraph(true, true);
            if (!reset && path.Length > 0 && File.Exists(path.Replace(".apstp", ".apmap")))
            {
                System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<GraphPos>));
                try
                {
                    using (var fs = File.OpenRead(path.Replace(".apstp", ".apmap")))
                        pos = xml.Deserialize(fs) as List<GraphPos>;
                    Relayout(false);
                }
                catch { }
            }
            graph.ShowAllEdgesLabels(false);
            graph.SetVerticesDrag(true, true);
            graph.SetEdgesDashStyle(EdgeDashStyle.Dash);
            foreach (var v in graph.VertexList)
                v.Value.Background = v.Key.Dialog.Tag == null ? Brushes.LightGray : new SolidColorBrush(v.Key.Dialog.Tag.Color.ToMediaColor());
        }

        public void Relayout(bool rel = true)
        {
            if (rel)
                graph.RelayoutGraph(true);
            foreach (var p in pos)
            {
                var vx = graph.VertexList.FirstOrDefault(x => x.Key.Dialog.ID == p.DialogID);
                if (vx.Value != null)
                    vx.Value.SetPosition(p.Position);
            }
        }

        public bool Selecting = false;
        private Dialog selected = null;

        public async Task<Dialog> SelectDialog()
        {
            var prevsel = tree.SelectedItem as DialogOption;
            Selecting = true;
            btnOption.IsEnabled = btnUp.IsEnabled = btnDown.IsEnabled = btnAdd.IsEnabled = btnRem.IsEnabled = RightWindowCommands.IsEnabled = false;
            selected = null;
            (Resources["ShowSelect"] as Storyboard).Begin();
            await Task.Run(() => { while (Selecting & selected == null) { } });
            (Resources["HideSelect"] as Storyboard).Begin();

            Selecting = false;
            btnAdd.IsEnabled = btnRem.IsEnabled = RightWindowCommands.IsEnabled = true;

            foreach (Dialog d in Story.Dialogs)
            {
                foreach (IDialog id in d.Options)
                    id.Selected = false;
                d.Selected = false;
            }
            var dialog = Story.FindParent(prevsel);
            (tree.ItemContainerGenerator.ContainerFromItem(dialog) as TreeViewItem).IsExpanded = true;
            dialog.Options.First(x => x.ID == prevsel.ID).Selected = true; ;
            return selected;
        }

        public void UpdateEdge(DialogOption o)
        {
            if (graph.EdgesList.Where(x => x.Key.DialogOption.ID == o.ID).Any())
                graph.RemoveEdge(graph.EdgesList.First(x => x.Key.DialogOption.ID == o.ID).Key);
            if (!o.TargetID.IsNullOrEmpty())
            {
                var de = new DialogEdge(graph.VertexList.First(x => x.Key.Dialog.ID == Story.FindParent(o).ID).Key, graph.VertexList.First(x => x.Key.Dialog.ID == o.TargetID).Key) { DialogOption = o };
                graph.AddEdge(de, new EdgeControl(graph.VertexList.First(x => x.Key.Dialog.ID == Story.FindParent(o).ID).Value, graph.VertexList.First(x => x.Key.Dialog.ID == o.TargetID).Value, de, false));
            }
            Relayout();
            //graph.GenerateAllEdges(updateLayout: false);
            //graph.UpdateParallelEdgesData();
            //graph.RelayoutGraph(true);
        }

        private void Save()
        {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<GraphPos>));
            pos.Clear();
            foreach (var v in graph.VertexList)
                pos.Add(new GraphPos() { DialogID = v.Key.Dialog.ID, Position = v.Value.GetPosition() });
            Task.Run(() =>
            {
                Story.Save(path);
                set.Default.Edited = false;
                try
                {
                    File.Delete(path.Replace(".apstp", ".apmap"));
                    using (var fs = File.OpenWrite(path.Replace(".apstp", ".apmap")))
                        xml.Serialize(fs, pos);
                }
                catch { }
            });
        }

        private bool loading = false;

        private async void Open(bool create = false, bool skipMessage = false)
        {
            loading = true;
            if (create)
                Story = new Story();
            else
                Story = Story.Load(path);
            if (Story == null)
            {
                if (!skipMessage)
                    await this.ShowMessageAsync("Oops...", $"Error occured while loading story \"{System.IO.Path.GetFileName(path)}\"");
                loading = false;
                return;
            }
            //Story.PropertyChanged += (s, e) => Relayout();
            Story.Dialogs.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (Dialog d in e.NewItems)
                        UpdateVertex(d);
            };
            foreach (var v in Story.Dialogs)
                v.PropertyChanged += (s, e) => UpdateVertex(s as Dialog);
            BindingOperations.SetBinding(tree, TreeView.ItemsSourceProperty, new Binding("Dialogs") { Source = Story, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BindingOperations.SetBinding(sp.txtTitle, TextBox.TextProperty, new Binding("Title") { Source = Story, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BindingOperations.SetBinding(sp.txtDescription.Editor, TextBox.TextProperty, new Binding("Description") { Source = Story, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BindingOperations.SetBinding(sp.txtAuthor, TextBox.TextProperty, new Binding("Author") { Source = Story, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BindingOperations.SetBinding(sp.txtContinue, TextBox.TextProperty, new Binding("Continue") { Source = Story, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BindingOperations.SetBinding(sp.DefaultText, ComboBox.SelectedItemProperty, new Binding("DefaultText") { Source = Story, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BindingOperations.SetBinding(sp.DefaultBack, ComboBox.SelectedItemProperty, new Binding("DefaultBack") { Source = Story, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BindingOperations.SetBinding(ps.data, DataGrid.ItemsSourceProperty, new Binding("Variables") { Source = Story, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BindingOperations.SetBinding(ts.tags, DataGrid.ItemsSourceProperty, new Binding("Tags") { Source = Story, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            BindingOperations.SetBinding(editor.text.btnVariable, ItemsControl.ItemsSourceProperty, new Binding("Variables") { Source = Story, Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //BindingOperations.SetBinding(SelectTagMenu, DataGrid.ItemsSourceProperty, new Binding("Tags") { Source = Story, Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            ps.data.Items.Refresh();
            StoryChanged?.Invoke(this, Story);
            Title = "Apollo Editor - " + Story.Title;
            UpdateRecent();
            GenerateGraph();
            loading = false;
            set.Default.Edited = false;
        }

        public void UpdateVertex(Dialog d) => Dispatcher.Invoke(() =>
        {
            var ver = graph.VertexList.FirstOrDefault(x => x.Key.Dialog.ID == d.ID);
            if (ver.Value != null)
            {
                ver.Value.Vertex = new DialogVertex(d);
                ver.Value.Background = d.Tag == null ? Brushes.LightGray : new SolidColorBrush(d.Tag.Color.ToMediaColor());
            }
        });

        private async void Open(object sender, ExecutedRoutedEventArgs e)
        {
            if (!await ShowSaveDialog()) return;
            VistaOpenFileDialog o = new VistaOpenFileDialog();
            o.Filter = "Apollo story|*.apstp";
            o.Title = "Open Apollo story...";
            o.FileOk += (s, ev) =>
            {
                path = o.FileName;
                Open();
            };
            o.ShowDialog();
        }

        private async void New(object sender, ExecutedRoutedEventArgs e)
        {
            if (!await ShowSaveDialog()) return;
            path = "";
            Open(true);
        }

        private void MProperties_Click(object sender, RoutedEventArgs e)
        {
            sp.ShowDialog();
        }

        private void SaveSettings()
        {
            bool ed = set.Default.Edited;
            set.Default.Edited = false;
            set.Default.Save();
            set.Default.Edited = ed;
        }

        private void UpdateRecent(bool insert = true)
        {
            if (insert)
            {
                if (set.Default.Recent.Contains(path))
                    set.Default.Recent.Remove(path);
                set.Default.Recent.Insert(0, path);
                SaveSettings();
            }
            var v = set.Default.Recent.Cast<string>().ToList();
            foreach (string s in v)
            {
                if (!System.IO.File.Exists(s) && set.Default.Recent.Contains(s))
                    set.Default.Recent.Remove(s);
            }
            while (set.Default.Recent.Count > 10)
                set.Default.Recent.RemoveAt(set.Default.Recent.Count - 1);

            JumpList.GetJumpList(App.Current).JumpItems.Clear();
            foreach (string s in set.Default.Recent)
            {
                JumpPath p = new JumpPath() { Path = s };
                JumpList.AddToRecentCategory(p);
            }
            JumpList.GetJumpList(App.Current).Apply();
        }

        private void btnParams_Click(object sender, RoutedEventArgs e)
        {
            if (!ps.IsVisible)
                ps.Show();
            if (ps.WindowState == WindowState.Minimized)
                ps.WindowState = WindowState.Normal;
            ps.Activate();
            ps.Topmost = true;
            ps.Topmost = false;
            ps.Focus();
        }

        private void btnExpHelp_Click(object sender, RoutedEventArgs e)
        {
            if (!eh.IsVisible)
                eh.Show();
            if (eh.WindowState == WindowState.Minimized)
                eh.WindowState = WindowState.Normal;
            eh.Activate();
            eh.Topmost = true;
            eh.Topmost = false;
            eh.Focus();
        }

        private bool VerDown = false;

        private void graph_VertexSelected(object sender, GraphX.Controls.Models.VertexSelectedEventArgs args)
        {
            VerDown = true;
            if (Selecting)
            {
                selected = (args.VertexControl.Vertex as DialogVertex).Dialog;
                return;
            }
            foreach (Dialog d in Story.Dialogs)
            {
                foreach (IDialog id in d.Options)
                    id.Selected = false;
                d.Selected = (args.VertexControl.Vertex as DialogVertex).Dialog.ID == d.ID;
            }
        }

        private void graph_EdgeSelected(object sender, GraphX.Controls.Models.EdgeSelectedEventArgs args)
        {
            //VerDown = true;
            if (Selecting) return;
            var dialog = (args.EdgeControl.Source.Vertex as DialogVertex).Dialog;
            (tree.ItemContainerGenerator.ContainerFromItem(dialog) as TreeViewItem).IsExpanded = true;
            dialog.Options.First(x => x.ID == (args.EdgeControl.Edge as DialogEdge).DialogOption.ID).Selected = true; ;
        }

        private void graph_VertexMouseUp(object sender, GraphX.Controls.Models.VertexSelectedEventArgs args)
        {
            VerDown = false;
            pos.Clear();
            foreach (var v in graph.VertexList)
                pos.Add(new GraphPos() { DialogID = v.Key.Dialog.ID, Position = v.Value.GetPosition() });
        }

        private void ClearSelect(object sender, MouseButtonEventArgs e)
        {
            Selecting = false;
        }

        private void CancelSelect(object sender, MouseButtonEventArgs e)
        {
            selected = new Dialog() { ID = "Cancel" };
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            GenerateGraph(true);
            //foreach (var v in graph.VertexList)
            //    v.Value.Vertex = new DialogVertex((v.Value.Vertex as DialogVertex).Dialog);
        }

        private void mExport_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ShowProperties(object sender, ExecutedRoutedEventArgs e)
        {
            sp.ShowDialog();
        }

        private void btnStory_Click(object sender, RoutedEventArgs e)
        {
            (Resources["StoryMenu"] as ContextMenu).IsOpen = true;
        }
    }
}