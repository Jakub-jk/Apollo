using GraphX.Common.Models;
using GraphX.Controls;
using GraphX.Logic.Models;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Apollo.Editor
{
    //Layout visual class
    public class GraphAreaExample : GraphArea<DialogVertex, DialogEdge, BidirectionalGraph<DialogVertex, DialogEdge>> { }

    //Graph data class
    public class GraphExample : BidirectionalGraph<DialogVertex, DialogEdge> { }

    //Logic core class
    public class GXLogicCoreExample : GXLogicCore<DialogVertex, DialogEdge, BidirectionalGraph<DialogVertex, DialogEdge>> { }

    //Vertex data object
    public class DialogVertex : VertexBase, INotifyPropertyChanged
    {
        private Dialog dialog;

        public DialogVertex() { }
        public DialogVertex(Dialog d)
        {
            Dialog = d;
        }
        /// <summary>
        /// Some string property for example purposes
        /// </summary>
        public Dialog Dialog { get => dialog; set { dialog = value; dialog.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Dialog")); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Dialog")); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            if (Dialog == null)
                return "none";
            return Dialog.Name.IsNullOrEmpty() ? Dialog.ID : Dialog.Name;
        }
    }

    //Edge data object
    public class DialogEdge : EdgeBase<DialogVertex>
    {
        public DialogEdge(DialogVertex source, DialogVertex target, double weight = 1)
            : base(source, target, weight)
        {
        }

        public DialogEdge()
            : base(null, null, 1)
        {
        }

        public DialogOption DialogOption { get; set; }

        public override string ToString()
        {
            if (DialogOption == null)
                return "none";
            return DialogOption.Name.IsNullOrEmpty() ? "Target: " + DialogOption.TargetID : DialogOption.Name;
        }
    }

    public class GraphPos
    {
        [XmlAttribute]
        public string DialogID { get; set; }
        public Point Position { get; set; }
    }
}
