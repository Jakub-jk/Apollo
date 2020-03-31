using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Editor
{
    public class Warning
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public Action OpenTarget { get; set; }
        public WarningType Type { get; set; } = WarningType.Warning;

        public Warning()
        {
        }

        public Warning(string code, string msg)
        {
            Code = code;
            Message = msg;
        }

        public Warning(string code, string msg, WarningType type) : this(code, msg)
        {
            Type = type;
        }

        public Warning(string code, string msg, WarningType type, Action target) : this(code, msg, type)
        {
            OpenTarget = target;
        }
    }

    public enum WarningType
    {
        Warning,
        Error
    }
}