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
        private Terminal.TerminalCallback newLineOutCallback;
        private Terminal.TerminalCallback newLineErrCallback;
        private List<LegendaryCallback> callbacks = new List<LegendaryCallback>();
        private bool blocking = false;
        private bool done = false;
        

        public delegate void LegendaryCallback(LegendaryActionBuilder action);

        public int ExitCode { get; private set; }

        public LegendaryActionBuilder(Legendary legendary, string fileName, string arguments, Terminal.TerminalCallback newLineOutCallback = null, Terminal.TerminalCallback newLineErrCallback = null)
        {
            Legendary = legendary;
            this.fileName = fileName;
            this.arguments = arguments;
            this.newLineErrCallback = newLineErrCallback;
            this.newLineOutCallback = newLineOutCallback;
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

        private void Callback(Terminal t)
        {
            if (t.ExitCode == 0)
                callbacks.ForEach(x => x.Invoke(this));
            ExitCode = t.ExitCode;
            done = true;
        }

        public void WaitUntilCompletion()
        {
            while (!done) ;
        }

        public LegendaryState Start()
        {
            Terminal = new Terminal();
            if (Terminal.IsActive)
                return LegendaryState.AlreadyActive;

            if (Terminal.Exec(fileName, arguments, Callback, newLineOutCallback, newLineErrCallback) < 0)
                return LegendaryState.StartError;

            if (blocking)
                while (!done) ;

            return LegendaryState.Started;
        }
    }
}
