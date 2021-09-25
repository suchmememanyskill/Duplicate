using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LegendaryMapper
{
    public class LegendaryActionBuilder
    {
        private Legendary wrapper;
        private string fileName;
        private string arguments;
        private Terminal.TerminalCallback newLineOutCallback;
        private Terminal.TerminalCallback newLineErrCallback;
        private List<LegendaryCallback> callbacks = new List<LegendaryCallback>();
        private bool blocking = false;

        public delegate void LegendaryCallback(Legendary legendary);

        public int ExitCode { get; private set; }

        public LegendaryActionBuilder(Legendary wrapper, string fileName, string arguments, Terminal.TerminalCallback newLineOutCallback = null, Terminal.TerminalCallback newLineErrCallback = null)
        {
            this.wrapper = wrapper;
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
                callbacks.ForEach(x => x.Invoke(wrapper));
            blocking = false;
            ExitCode = t.ExitCode;
        }

        public LegendaryState Start()
        {
            Terminal t = Terminal.GetInstance();
            if (t.IsActive)
                return LegendaryState.AlreadyActive;

            if (t.Exec(fileName, arguments, Callback, newLineOutCallback, newLineErrCallback) < 0)
                return LegendaryState.StartError;

            if (blocking)
                while (t.IsActive || blocking) ;

            return LegendaryState.Started;
        }
    }
}
