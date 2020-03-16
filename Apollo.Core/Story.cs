using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Apollo
{
    [XmlRoot("Story")]
    public class Story : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString(), _title, _author, _description, _continue = "Press any key to continue...";
        private ConsoleColor _back = ConsoleColor.Black, _text = ConsoleColor.White;
        private ObservableCollection<Dialog> _dialogs = new ObservableCollection<Dialog>();
        private ObservableCollection<Variable> _vars = new ObservableCollection<Variable>();

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

        [XmlIgnore]
        public Dialog StartDialog { get => Dialogs.First((d) => d.Start); }

        [XmlIgnore]
        public string CurrentID { get; private set; }

        public event EventHandler<Dialog> StoryEnded;

        public event EventHandler<Dialog> DialogRequested;

        public event PropertyChangedEventHandler PropertyChanged;

        public Story()
        {
            Dialogs.CollectionChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Dialogs"));
            Variables.CollectionChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Variables"));
        }

        public Dialog FindParent(DialogOption o)
        {
            return Dialogs.FirstOrDefault(y => y.Options.Where(z => z.ID == o.ID).Any());
        }

        public static Story Load(string filename)
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(Story));
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    return xml.Deserialize(fs) as Story;
            }
            catch { return null; }
        }

        public void Save(string filename)
        {
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

    public class Variable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name = "";
        private object _value;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public object Value
        {
            get => _value;
            set
            {
                this._value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
            }
        }

        public Variable()
        {
        }

        public Variable(string name, object value = null)
        {
            _name = name;
            _value = value;
        }
    }
}