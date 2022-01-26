using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LegendaryMapperV2.Service
{
    public class LegendaryCommand
    {
        public Terminal Terminal { get; private set; }
        private string fileName = "legendary";
        private string arguments;
        private List<CommandCallback> callbacks = new();
        private List<CommandCallback> errorCallbacks = new();
        private List<CommandCallback> stdOutCallbacks = new();
        private List<CommandCallback> stdErrCallbacks = new();
        private bool blocking = false;
        private bool done = false;
        
        public delegate void CommandCallback(LegendaryCommand action);
        public string Arguments { get => arguments; }

        public int ExitCode { get; private set; }

        public LegendaryCommand(string arguments)
        {
            this.arguments = arguments;
            Terminal = new Terminal();
        }

        public LegendaryCommand Block()
        {
            blocking = true;
            return this;
        }

        public LegendaryCommand Then(CommandCallback callback)
        {
            callbacks.Add(callback);
            return this;
        }

        public LegendaryCommand OnError(CommandCallback callback)
        {
            errorCallbacks.Add(callback);
            return this;
        }

        public LegendaryCommand OnNewLine(CommandCallback callback)
        {
            stdOutCallbacks.Add(callback);
            return this;
        }

        public LegendaryCommand OnErrLine(CommandCallback callback)
        {
            stdErrCallbacks.Add(callback);
            return this;
        }

        private void Callback(Terminal t)
        {
            if (t.ExitCode == 0)
                callbacks.ForEach(x => x.Invoke(this));
            else
                errorCallbacks.ForEach(x => x.Invoke(this));
            ExitCode = t.ExitCode;
            done = true;
        }

        private void StdOutCallback(Terminal t) => stdOutCallbacks.ForEach(x => x.Invoke(this));
        private void StdErrCallback(Terminal t) => stdErrCallbacks.ForEach(x => x.Invoke(this));

        public void WaitUntilCompletion()
        {
            while (!done) ;
        }

        public void Start()
        {
            if (Terminal.IsActive)
                throw new Exception("Terminal already active");

            if (Terminal.Exec(fileName, arguments, Callback, StdOutCallback, StdErrCallback) < 0)
                throw new Exception("Terminal couldn't start");

            if (blocking)
                while (!done && !(!Terminal.IsActive && Terminal.ExitCode != 0)) ;
        }

        public void Stop() => Terminal.Kill();

        public static void PrintNewLineStdOut(LegendaryCommand t) => Console.WriteLine(t.Terminal.StdOut.Last());
        public static void PrintNewLineStdErr(LegendaryCommand t) => Console.WriteLine(t.Terminal.StdErr.Last());
    }
}
