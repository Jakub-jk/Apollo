using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO.Compression;

namespace Apollo
{
    [XmlRoot("Story")]
    public class Story : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString(), _title, _author, _description, _continue = "Press any key to continue...";
        private ConsoleColor _back = ConsoleColor.Black, _text = ConsoleColor.White;
        private ObservableCollection<Dialog> _dialogs = new ObservableCollection<Dialog>();
        private ObservableCollection<Variable> _vars = new ObservableCollection<Variable>();
        private ObservableCollection<Tag> _tags = new ObservableCollection<Tag>();

        [XmlAttribute]
        public string ID { get => _id; set { _id = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ID")); } }

        [XmlAttribute]
        public string Title { get => _title; set { _title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title")); } }

        [XmlAttribute]
        public string Author { get => _author; set { _author = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Author")); } }

        [XmlAttribute]
        public string Continue { get => _continue; set { _continue = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Continue")); } }

        [XmlAttribute]
        public ConsoleColor DefaultBack { get => _back; set { _back = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DefaultBack")); } }

        [XmlAttribute]
        public ConsoleColor DefaultText { get => _text; set { _text = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DefaultText")); } }

        [XmlElement]
        public string Description { get => _description; set { _description = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Description")); } }

        [XmlArray("Dialogs")]
        public ObservableCollection<Dialog> Dialogs { get => _dialogs; set { _dialogs = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Dialogs")); } }

        [XmlArray("Variables")]
        public ObservableCollection<Variable> Variables { get => _vars; set { _vars = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Variables")); } }

        [XmlArray("Tags")]
        public ObservableCollection<Tag> Tags { get => _tags; set { _tags = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tags")); } }

        [XmlIgnore]
        public Dialog StartDialog => Dialogs.FirstOrDefault((d) => d.Start);

        [XmlIgnore]
        public string CurrentID { get; private set; }

        public event EventHandler<Dialog> StoryEnded;

        public event EventHandler<Dialog> DialogRequested;

        public event PropertyChangedEventHandler PropertyChanged;

        public Story()
        {
            Dialogs.CollectionChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Dialogs"));
            Variables.CollectionChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Variables"));
            Tags.CollectionChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tags"));
        }

        public Dialog FindParent(DialogOption o)
        {
            return Dialogs.FirstOrDefault(y => y.Options.Where(z => z.ID == o.ID).Any());
        }

        public static Story Load(string filename)
        {
            try
            {
                Story ret = null;
                XmlSerializer xml = new XmlSerializer(typeof(Story));
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    ret = xml.Deserialize(fs) as Story;
                foreach (Dialog d in ret)
                    d.Tag = ret.Tags.FirstOrDefault(x => x.ID == d.TagID);
                return ret;
            }
            catch (Exception e) { return null; }
        }

        public static Story LoadCompressed(string filename)
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(Story));
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                    return xml.Deserialize(gz) as Story;
            }
            catch { return null; }
        }

        public void Save(string filename)
        {
            foreach (Dialog d in this)
                d.TagID = d.Tag == null ? "" : d.Tag.ID;
            XmlSerializer xml = new XmlSerializer(typeof(Story));
            bool done = false;
            while (!done)
            {
                try
                {
                    using (var fs = new FileStream(filename + ".tmp", FileMode.OpenOrCreate, FileAccess.Write))
                        xml.Serialize(fs, this);
                    File.Delete(filename);
                    File.Move(filename + ".tmp", filename);
                    done = true;
                }
                catch { }
            }
        }

        public void SaveCompressed(string filename)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Story));
            bool done = false;
            while (!done)
            {
                try
                {
                    var tmp = Clone();
                    foreach (var v in tmp)
                        v.ClearUnnecessary();
                    tmp.Tags = null;
                    using (var fs = new FileStream(filename + ".tmp", FileMode.OpenOrCreate, FileAccess.Write))
                    using (var gz = new GZipStream(fs, CompressionMode.Compress))
                        xml.Serialize(gz, this);
                    File.Delete(filename);
                    File.Move(filename + ".tmp", filename);
                    done = true;
                }
                catch { }
            }
        }

        public async void DisplayDialog(Dialog d)
        {
            DialogRequested?.Invoke(this, d);
            foreach (var v in d.Actions.Split('\n'))
            {
                ContentParser.DefaultInstance.Eval.Expression = v;
                ContentParser.DefaultInstance.Eval.Evaluate();
            }
            CurrentID = d.ID;
            ContentParser.DefaultInstance.Output.Clear();
            await ContentParser.DefaultInstance.Display(d.Text + Environment.NewLine + Environment.NewLine);
            if (d.Options.Count == 0)
            {
                await ContentParser.DefaultInstance.Display(Environment.NewLine + Continue);
                ContentParser.DefaultInstance.Output.ReadKey();
                StoryEnded?.Invoke(this, d);
            }
            List<(string Content, Action OnSelect, bool? Enabled)> items = new List<(string Content, Action OnSelect, bool? Enabled)>();
            var cont = d.Options.Where(x =>
            {
                var av = true;
                if (!x.Requirement.IsNullOrEmpty())
                {
                    ContentParser.DefaultInstance.Eval.Expression = x.Requirement;
                    av = ContentParser.DefaultInstance.Eval.EvaluateBool();
                }
                return x.Name == ":cont" && av;
            });
            if (d.Options.Where(x => x.Name == ":cont").Any())
            {
                var dop = cont.First();
                await ContentParser.DefaultInstance.Display(Environment.NewLine + Continue);
                ContentParser.DefaultInstance.Output.ReadKey();
                if (dop.TargetID.IsNullOrEmpty())
                    StoryEnded?.Invoke(this, d);
                else
                    DisplayDialog(this[dop.TargetID]);
            }
            else
            {
                foreach (DialogOption dop in d.Options)
                {
                    bool? av = true;
                    if (!dop.Requirement.IsNullOrEmpty())
                    {
                        ContentParser.DefaultInstance.Eval.Expression = dop.Requirement;
                        av = ContentParser.DefaultInstance.Eval.EvaluateBool();
                    }
                    if (av == false)
                        continue;
                    items.Add((dop.Text, new Action(() => DisplayDialog(this[dop.TargetID])), dop.TargetID.IsNullOrEmpty() ? null : (bool?)true));
                }
                ContentParser.DefaultInstance.DisplayMenu(items.ToArray());
            }
        }

        public void Begin()
        {
            ContentParser.DefaultInstance.Eval.ClearVariable();
            foreach (var v in Variables)
                ContentParser.DefaultInstance.Eval.SetVariable(v.Name, v.Value);
            ContentParser.DefaultInstance.DefaultBack = DefaultBack;
            ContentParser.DefaultInstance.DefaultText = DefaultText;
            DisplayDialog(StartDialog);
        }

        public void Add(Dialog d) => Dialogs.Add(d);

        public void Clear() => Dialogs.Clear();

        public IEnumerator<Dialog> GetEnumerator()
        {
            return Dialogs.GetEnumerator();
        }

        public Story Clone()
        {
            Story s = new Story();
            s.ID = ID;
            s.Title = Title;
            s.Author = Author;
            s.Continue = Continue;
            s.DefaultBack = DefaultBack;
            s.DefaultText = DefaultText;
            s.Description = Description;
            foreach (Dialog d in Dialogs)
                s.Dialogs.Add(d.Clone());
            foreach (Variable v in Variables)
                s.Variables.Add(v.Clone());
            foreach (Tag t in Tags)
                s.Tags.Add(t.Clone());
            return s;
        }

        public Dialog this[int index]
        {
            get => Dialogs[index];
            set => Dialogs[index] = value;
        }

        public Dialog this[string ID]
        {
            get => Dialogs.Where((d) => ID == d.ID).First();
            set => Dialogs[Dialogs.IndexOf(Dialogs.Where((d) => ID == d.ID).First())] = value;
        }
    }
}