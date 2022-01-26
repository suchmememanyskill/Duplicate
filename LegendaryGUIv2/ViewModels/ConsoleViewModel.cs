using LegendaryMapperV2.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using LegendaryGUIv2.Views;

namespace LegendaryGUIv2.ViewModels
{
    public class ConsoleViewModel : ViewModelBase<ConsoleView>
    {
        private MainWindowViewModel window;
        private LegendaryCommand? command;
        private ViewModelBase? returnTo;
        private List<string> log = new();

        public ConsoleViewModel(MainWindowViewModel window)
        {
            this.window = window;
        }

        public delegate void ConsoleViewCallback();
        public void ExecuteCommand(string text, LegendaryCommand command, ViewModelBase returnTo)
        {
            this.command = command;
            this.returnTo = returnTo;
            backEnabled = false;
            Text = text;

            log.Clear();

            window.SetViewModel(this);
            CliOut = $">legendary {command.Arguments}";
            command.OnNewLine(OnNewLine).OnErrLine(OnNewErrLine).OnError(OnCompletion).Then(OnCompletion).Start();
        }
        
        private void OnNewLine(LegendaryCommand x)
        {
            log.Add(x.Terminal.StdOut.Last());
            OnNewLineHandler(x);
        }
        private void OnNewErrLine(LegendaryCommand x)
        {
            log.Add(x.Terminal.StdErr.Last());
            OnNewLineHandler(x);
        }

        private void OnNewLineHandler(LegendaryCommand x)
        {

            CliOut = $">legendary {x.Arguments}\n{string.Join("\n", log)}";
            Control!.SetCaretIndex();
        }

        private void OnCompletion(LegendaryCommand x)
        {
            CliOut += $"\n>Process exited with exit code {x.ExitCode}";
            Control!.SetCaretIndex();
            BackEnabled = true;
        }

        public void Back() => window.SetViewModel(returnTo!);

        private bool backEnabled = false;
        public bool BackEnabled { get => backEnabled; set => this.RaiseAndSetIfChanged(ref backEnabled, value); }
        public string cliOut = "", text = "";
        public string CliOut { get => cliOut; set => this.RaiseAndSetIfChanged(ref cliOut, value); }
        public string Text { get => text; set => this.RaiseAndSetIfChanged(ref text, value); }
    }
}
