using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Linq;

namespace LegendaryMapper
{
    public class Terminal
    {
        public List<string> StdOut { get; private set; }
        public List<string> StdErr { get; private set; }
        public int ExitCode { get; private set; }
        public bool IsActive { get; private set; }
        public delegate void TerminalCallback(Terminal ret);
        private Process proc;
        private bool killed = false;

        public Terminal()
        {
            IsActive = false;
        }

        private TerminalCallback callback;
        private TerminalCallback newLineOutCallback;
        private TerminalCallback newLineErrCallback;

        public int Exec(string fileName, string arguments, TerminalCallback callback = null, TerminalCallback newLineOutCallback = null, TerminalCallback newLineErrCallback = null)
        {
            if (IsActive)
                return 0;

            killed = false;

            IsActive = true;

            StdOut = new List<string>();
            StdErr = new List<string>();

            this.callback = callback;
            this.newLineOutCallback = newLineOutCallback;
            this.newLineErrCallback = newLineErrCallback;

            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = arguments;
            start.FileName = fileName;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            start.UseShellExecute = false;

            proc = new Process();
            proc.StartInfo = start;

            try
            {
                proc.Start();
                Thread thread = new Thread(TrackProc);
                thread.Start();
            }
            catch
            {
                IsActive = false;
                return -1;
            }

            return 1;
        }

        private void CaptureStdErrOutput()
        {
            while (true)
            {
                string line = proc.StandardError.ReadLine();
                if (line == null)
                    break;
                if (line != "")
                {
                    StdErr.Add(line);
                    newLineErrCallback?.Invoke(this);
                }
            }
        }

        private void CaptureStdOutOutput()
        {
            while (true)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line == null)
                    break;
                if (line != "")
                {
                    StdOut.Add(line);
                    newLineOutCallback?.Invoke(this);
                }

            }
        }

        private void TrackProc()
        {
            Thread stdOut = new Thread(CaptureStdOutOutput);
            Thread stdErr = new Thread(CaptureStdErrOutput);

            stdOut.Start();
            stdErr.Start();
            stdOut.Join();
            stdErr.Join();

            proc.WaitForExit();

            ExitCode = proc.ExitCode;
            IsActive = false;
            proc.Close();
            if (!killed)
                callback?.Invoke(this);
        }

        public void Kill()
        {
            if (IsActive)
            {
                killed = true;
                proc.Kill();
            }
        }

        public void PrintStdOut() =>
            StdOut.ForEach(Console.WriteLine);

        public static void PrintNewLineStdOut(Terminal t) => Console.WriteLine(t.StdOut.Last());
        public static void PrintNewLineStdErr(Terminal t) => Console.WriteLine(t.StdErr.Last());
    }
}
