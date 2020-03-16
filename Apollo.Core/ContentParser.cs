using ExpressionEvaluation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo
{
    public class ContentParser
    {
        public IConsole Output { get => output; set { output = value; output.Parser = this; } }
        public int CharacterDelay { get; set; } = 0;
        public List<(ConsoleKey Key, Action Action)> CustomActions { get; set; } = new List<(ConsoleKey Key, Action Action)>();
        public ExpressionEval Eval { get; set; } = new ExpressionEval();
        public ConsoleColor DefaultBack { get; set; } = ConsoleColor.Black;
        public ConsoleColor DefaultText { get; set; } = ConsoleColor.White;

        private static ContentParser def = null;
        private IConsole output = new DefaultConsole();

        public static ContentParser DefaultInstance
        {
            get
            {
                if (def == null)
                    def = new ContentParser();
                return def;
            }
        }

        public ContentParser()
        {
            Eval.AdditionalFunctionEventHandler += Eval_AdditionalFunctionEventHandler;
        }

        private void Eval_AdditionalFunctionEventHandler(object sender, AdditionalFunctionEventArgs e)
        {
            object[] parameters = e.GetParameters();
            switch (e.Name)
            {
                case "set":
                    if (parameters.Length == 2 && Eval.Variables.ContainsKey(parameters[0] + ""))
                        Eval.SetVariable("" + parameters[0], parameters[1]);
                    break;

                case "back":
                    if (parameters.Length == 1)
                        Output.BackgroundColor = (ConsoleColor)int.Parse(parameters[0].ToString());
                    break;

                case "text":
                    if (parameters.Length == 1)
                        Output.ForegroundColor = (ConsoleColor)int.Parse(parameters[0].ToString());
                    break;

                case "inv":
                    Output.InvertColor();
                    break;

                case "clcr":
                    {
                        var value = parameters.Length > 0 ? parameters[0].ToString() : "";
                        if (string.IsNullOrEmpty(value) || value == "f")
                            Output.ForegroundColor = DefaultText;
                        if (string.IsNullOrEmpty(value) || value == "b")
                            Output.BackgroundColor = DefaultBack;
                    }
                    break;

                case "mul":
                    {
                        int res = 1;
                        if (parameters.Length == 2 && int.TryParse(parameters[1].ToString(), out res))
                            e.ReturnValue = parameters[0].ToString().Multiply(res);
                    }
                    break;
            }
        }

        public async Task Display(string content, bool noExpressions = false)
        {
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '`' && (i != 0 ? content[i - 1] != '\\' : true))
                {
                    i++;
                    string modifier = "", value = "";
                    modifier = content[i].ToString();
                    i++;
                    if (content[i] == ':')
                    {
                        i++;
                        while (!(content[i] == '`' && content[i - 1] != '\\'))
                        {
                            value += content[i];
                            i++;
                        }
                    }
                    switch (modifier)
                    {
                        case "f":
                            {
                                Output.ForegroundColor = (ConsoleColor)int.Parse(value);
                            }
                            break;

                        case "b":
                            {
                                Output.BackgroundColor = (ConsoleColor)int.Parse(value);
                            }
                            break;

                        case "c":
                            {
                                if (string.IsNullOrEmpty(value) || value == "f")
                                    Output.ForegroundColor = DefaultText;
                                if (string.IsNullOrEmpty(value) || value == "b")
                                    Output.BackgroundColor = DefaultBack;
                            }
                            break;

                        case "e":
                            {
                                if (noExpressions)
                                    await Write($"`v:{value}`");
                                else
                                {
                                    foreach (var v in value.Split(';'))
                                    {
                                        Eval.Expression = v;
                                        await Write(Eval.Evaluate());
                                    }
                                }
                            }
                            break;

                        case "v":
                            {
                                if (noExpressions)
                                    await Write($"`v:{value}`");
                                else
                                {
                                    Eval.Expression = $"@{{{value}}}";
                                    await Write(Eval.Evaluate());
                                }
                            }
                            break;

                        case "d":
                            {
                                if (value.IsNullOrEmpty())
                                    CharacterDelay = 0;
                                else
                                {
                                    int val = 0;
                                    if (int.TryParse(value, out val))
                                        CharacterDelay = val;
                                }
                            }
                            break;

                        case "i":
                            {
                                Output.InvertColor();
                            }
                            break;
                    }
                }
                else
                    await Write(content[i]);
            }
            if (Output.ForegroundColor != DefaultText)
                Output.ForegroundColor = DefaultText;
            if (Output.BackgroundColor != DefaultBack)
                Output.BackgroundColor = DefaultBack;
        }

        private Task Write(object o)
        {
            return Write(o.ToString());
        }

        private async Task Write(string s, params object[] args)
        {
            if (CharacterDelay == 0)
                Output.Write(s, args);
            else
            {
                string res = args == null || args.Length == 0 ? s : string.Format(s, args);
                foreach (char c in res)
                {
                    Output.Write(c);
                    await Task.Delay(1000 / CharacterDelay);
                }
            }
        }

        public async void DisplayMenu(params (string Content, Action OnSelect, bool? Enabled)[] items)
        {
            Output.CursorVisible = false;
            int selected = 0, top = Output.CursorTop;
            while (true)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    Output.Write("[");
                    if (i == selected)
                        Output.InvertColor();
                    Output.Write(items[i].Enabled == true ? (i + 1).ToString("0".Multiply(items.Length.ToString().Length)) : (items[i].Enabled == null ? "t".Multiply(items.Length.ToString().Length) : "x".Multiply(items.Length.ToString().Length)));
                    Output.ResetColor();
                    Output.Write("] ");
                    await Display(items[i].Content + Environment.NewLine);
                }

                var input = Output.ReadKey();
                switch (input)
                {
                    case ConsoleKey.Enter:
                        goto Execute;
                    case ConsoleKey.UpArrow:
                        do
                            selected = selected == 0 ? items.Length - 1 : selected - 1;
                        while (!items[selected].Enabled == true);
                        break;

                    case ConsoleKey.DownArrow:
                        do
                            selected = selected == items.Length - 1 ? 0 : selected + 1;
                        while (!items[selected].Enabled == true);
                        break;

                    default:
                        foreach (var v in CustomActions)
                        {
                            if (input == v.Key)
                            {
                                v.Action?.Invoke();
                                return;
                            }
                        }
                        break;
                }
                Output.CursorTop = top;
                Output.CursorLeft = 0;
            }
        Execute:
            items[selected].OnSelect.Invoke();
        }
    }
}