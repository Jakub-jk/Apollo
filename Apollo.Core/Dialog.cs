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

        [XmlIgnore]
        bool Selected { get; set; }
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
        private ObservableCollection<DialogOption> options = new ObservableCollection<DialogOption>();

        [XmlIgnore]
        public bool Selected { get => selected; set { selected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected")); } }

        [XmlAttribute]
        public string ID { get => iD; set { iD = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ID")); } }

        [XmlAttribute]
        public string Name { get => name; set { name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name")); } }

        [XmlElement(ElementName = "Content")]
        public string Text { get => text; set { text = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text")); } }

        [XmlAttribute]
        public bool Start { get => start; set { start = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Start")); } }

        [XmlElement]
        public string Actions { get => actions; set { actions = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Actions")); } }

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

        public DialogOption()
        {
        }

        public DialogOption(string text, string target)
        {
            Text = text;
            TargetID = target;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}