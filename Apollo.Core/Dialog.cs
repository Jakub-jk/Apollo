using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Apollo
{
    public interface IDialog : INotifyPropertyChanged
    {
        string Name { get; set; }
        string Text { get; set; }
        string ID { get; set; }
        string Note { get; set; }

        string PostActions { get; set; }

        [XmlIgnore]
        bool Selected { get; set; }

        void ProcessPostActions();
    }

    [XmlRoot("Dialog")]
    public class Dialog : IDialog
    {
        private bool selected;
        private string iD = Guid.NewGuid().ToString();
        private string name;
        private string text;
        private bool start;
        private string actions;
        private string note;
        private string tagid;
        private string postActions;
        private Tag tag;
        private ObservableCollection<DialogOption> options = new ObservableCollection<DialogOption>();

        [XmlIgnore]
        public bool Selected { get => selected; set { selected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected")); } }

        [XmlAttribute]
        public string ID { get => iD; set { iD = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ID")); } }

        [XmlAttribute]
        public string Name { get => name; set { name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name")); } }

        [XmlIgnore]
        public Tag Tag
        {
            get => tag;
            set
            {
                tag = value;
                if (tag != null)
                    tag.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tag"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tag"));
            }
        }

        [XmlAttribute("Tag")]
        public string TagID { get => tagid; set { tagid = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TagID")); } }

        [XmlElement(ElementName = "Content")]
        public string Text { get => text; set { text = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text")); } }

        [XmlAttribute]
        public bool Start { get => start; set { start = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Start")); } }

        [XmlElement]
        public string Actions { get => actions; set { actions = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Actions")); } }

        [XmlElement]
        public string PostActions { get => postActions; set { postActions = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PostActions")); } }

        [XmlElement]
        public string Note { get => note; set { note = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Note")); } }

        [XmlArray]
        public ObservableCollection<DialogOption> Options
        {
            get => options;
            set
            {
                options = value;
                options.CollectionChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Options"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Options"));
            }
        }

        public Dialog()
        {
            Options = new ObservableCollection<DialogOption>();
        }

        public Dialog(string name, string text, params DialogOption[] options)
        {
            Name = name;
            Text = text;
            Options = new ObservableCollection<DialogOption>(options);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ProcessActions()
        {
            if (!Actions.IsNullOrEmpty())
            {
                foreach (var v in Actions.Split('\n'))
                {
                    ContentParser.DefaultInstance.Eval.Expression = v;
                    ContentParser.DefaultInstance.Eval.Evaluate();
                }
            }
        }

        public void ProcessPostActions()
        {
            if (!PostActions.IsNullOrEmpty())
            {
                foreach (var v in PostActions.Split('\n'))
                {
                    ContentParser.DefaultInstance.Eval.Expression = v;
                    ContentParser.DefaultInstance.Eval.Evaluate();
                }
            }
        }

        public Dialog Clone()
        {
            Dialog d = new Dialog();
            d.Selected = Selected;
            d.ID = ID;
            d.Name = Name;
            d.Text = Text;
            d.Start = Start;
            d.Actions = Actions;
            d.Note = Note;
            foreach (DialogOption o in Options)
                d.Options.Add(o.Clone());
            return d;
        }

        public void ClearUnnecessary()
        {
            Note = "";
            foreach (DialogOption o in Options)
                o.Note = "";
        }
    }

    [XmlRoot("Option")]
    public class DialogOption : IDialog
    {
        private string iD = Guid.NewGuid().ToString();
        private string name;
        private string text;
        private string targetID;
        private string requirement;
        private bool selected;
        private string postActions;
        private string note;

        [XmlAttribute]
        public string ID { get => iD; set { iD = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ID")); } }

        [XmlAttribute]
        public string Name { get => name; set { name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name")); } }

        [XmlText]
        public string Text { get => text; set { text = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text")); } }

        [XmlAttribute]
        public string TargetID { get => targetID; set { targetID = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetID")); } }

        [XmlAttribute]
        public string Requirement { get => requirement; set { requirement = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Requirement")); } }

        [XmlIgnore]
        public bool Selected { get => selected; set { selected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected")); } }

        [XmlElement]
        public string PostActions { get => postActions; set { postActions = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PostActions")); } }

        [XmlElement]
        public string Note { get => note; set { note = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Note")); } }

        public DialogOption()
        {
        }

        public DialogOption(string text, string target)
        {
            Text = text;
            TargetID = target;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ProcessPostActions()
        {
            if (!PostActions.IsNullOrEmpty())
            {
                foreach (var v in PostActions.Split('\n'))
                {
                    ContentParser.DefaultInstance.Eval.Expression = v;
                    ContentParser.DefaultInstance.Eval.Evaluate();
                }
            }
        }

        public DialogOption Clone()
        {
            DialogOption o = new DialogOption();
            o.ID = ID;
            o.Name = Name;
            o.Text = Text;
            o.TargetID = TargetID;
            o.Requirement = Requirement;
            o.Selected = Selected;
            o.Note = Note;
            return o;
        }
    }
}