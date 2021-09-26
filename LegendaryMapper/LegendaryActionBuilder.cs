using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LegendaryMapper
{
    public class LegendaryActionBuilder
    {
        public Legendary Legendary { get; private set; }
        public Terminal Terminal { get; private set; }
        private string fileName;
        private string arguments;
        private List<LegendaryCallback> callbacks = new List<LegendaryCallback>();
        private List<LegendaryCallback> stdOutCallbacks = new List<LegendaryCallback>();
        private List<LegendaryCallback> stdErrCallbacks = new List<LegendaryCallback>();
        private bool blocking = false;
        private bool done = false;
        

        public delegate void LegendaryCallback(LegendaryActionBuilder action);

        public int ExitCode { get; private set; }

        public LegendaryActionBuilder(Legendary legendary, string fileName, string arguments)
        {
            Legendary = legendary;
            this.fileName = fileName;
            this.arguments = arguments;
        }

        public LegendaryActionBuilder Block()
        {
            blocking = true;
            return this;
        }

        public LegendaryActionBuilder Then(LegendaryCallback callback)
        {
            callbacks.Add(callback);
            return this;
        }

        public LegendaryActionBuilder OnNewLine(LegendaryCallback callback)
        {
            stdOutCallbacks.Add(callback);
            return this;
        }

        public LegendaryActionBuilder OnErrLine(LegendaryCallback callback)
        {
            stdErrCallbacks.Add(callback);
            return this;
        }

        private void Callback(Terminal t)
        {
            if (t.ExitCode == 0)
                callbacks.ForEach(x => x.Invoke(this));
            ExitCode = t.ExitCode;
            done = true;
        }

        private void StdOutCallback(Terminal t) => stdOutCallbacks.ForEach(x => x.Invoke(this));
        private void StdErrCallback(Terminal t) => stdErrCallbacks.ForEach(x => x.Invoke(this));

        public void WaitUntilCompletion()
        {
            while (!done) ;
        }

        public LegendaryState Start()
        {
            Terminal = new Terminal();
            if (Terminal.IsActive)
                return LegendaryState.AlreadyActive;

            if (Terminal.Exec(fileName, arguments, Callback, StdOutCallback, StdErrCallback) < 0)
                return LegendaryState.StartError;

            if (blocking)
                while (!done) ;

            return LegendaryState.Started;
        }

        public static void PrintNewLineStdOut(LegendaryActionBuilder t) => Console.WriteLine(t.Terminal.StdOut.Last());
        public static void PrintNewLineStdErr(LegendaryActionBuilder t) => Console.WriteLine(t.Terminal.StdErr.Last());
    }
}
