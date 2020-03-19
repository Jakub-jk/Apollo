using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Apollo
{
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

        public Variable Clone()
        {
            return new Variable(Name, Value);
        }
    }
}