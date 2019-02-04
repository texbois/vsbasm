using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace VSBASM.Deborgar
{
    class BasmRunner
    {
        public delegate void OnStop(uint address);

        public AD_PROCESS_ID ProcessId { get; private set; }
        public BasmExecutionState ExecutionState { get; private set; }

        private Process _process;
        private SourceFile _sourceFile;
        private OnStop _onStopHandler;
        private bool _isInRunMode = false;

        private Thread _execThread;

        public BasmRunner(SourceFile sourceFile, OnStop stopHandler)
        {
            _sourceFile = sourceFile;
            _onStopHandler = stopHandler;
        }

        ~BasmRunner()
        {
            _process.Close();
        }

        public void StartSuspended()
        {
            Debug.Assert(_process == null);

            var info = new ProcessStartInfo("java");
            info.Arguments = " -jar -Dmode=cli -Dfile.encoding=UTF-8 C:\\bcomp.jar";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.StandardOutputEncoding = Encoding.UTF8;

            _process = new Process();
            _process.StartInfo = info;
            _process.Start();

            string code = _sourceFile.GetContents();
            _process.StandardInput.WriteLine("asm");
            _process.StandardInput.WriteLine(code);
            _process.StandardInput.WriteLine("end");

            string line = "";
            while (!line.StartsWith("Программа начинается с адреса"))
                line = _process.StandardOutput.ReadLine();

            ExecutionState = new BasmExecutionState(
               programCounter: uint.Parse(Regex.Match(line, "Программа начинается с адреса\\s*([0-9A-Fa-f]+)").Groups[1].Value, NumberStyles.HexNumber)
            );

            ProcessId = new AD_PROCESS_ID()
            {
                ProcessIdType = (uint) enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM,
                dwProcessId = (uint) _process.Id
            };
        }

        public void LaunchProgram()
        {
            SetAddressToPC();
            SetRunMode(true);
            _process.StandardInput.WriteLine("start");
            _process.StandardOutput.ReadLine();
            AwaitOnThread(onFinish: () => _onStopHandler(ExecutionState.ProgramCounter));
        }

        public void Step(Action onStepComplete)
        {
            SetRunMode(false);
            SetAddressToPC();
            _process.StandardInput.WriteLine("continue");
            _process.StandardOutput.ReadLine();
            AwaitOnThread(onFinish: () =>
            {
                ExecutionState = new BasmExecutionState(ExecutionState.ProgramCounter + 1, ExecutionState);
                onStepComplete();
            });
        }

        public void BreakExecution()
        {
            Thread execThread = _execThread;
            if (execThread != null)
            {
                execThread.Abort();
                _execThread = null;

                _isInRunMode = false;
                _process.StandardInput.WriteLine("run");
                string output = _process.StandardOutput.ReadLine();
                output = _process.StandardOutput.ReadLine();
                ExecutionState = ParseExecutionState(output);
                ExecutionState = new BasmExecutionState(ExecutionState.ProgramCounter + 1, ExecutionState);
            }
        }

        public void Continue()
        {
            SetRunMode(true);
            SetAddressToPC();
            _process.StandardInput.WriteLine("continue");
            _process.StandardOutput.ReadLine();
            AwaitOnThread(onFinish: () => _onStopHandler(ExecutionState.ProgramCounter));
        }

        public string GetContents(uint address)
        {
            string hexAdr = ConvertToHexAddress(address);
            _process.StandardInput.WriteLine($"{hexAdr} a read");
            _process.StandardOutput.ReadLine();  // Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн
            _process.StandardOutput.ReadLine();  // initial state
            string res = _process.StandardOutput.ReadLine(); // final state
            return res.Split()[1];
        }

        public void SetContents(uint address, string cmd)
        {
            string hexAdr = ConvertToHexAddress(address);
            _process.StandardInput.WriteLine($"{hexAdr} a {cmd} w");
            _process.StandardOutput.ReadLine();
            _process.StandardOutput.ReadLine();
            _process.StandardOutput.ReadLine();
        }

        private void SetAddressToPC()
        {
            _process.StandardInput.WriteLine($"{ConvertToHexAddress(ExecutionState.ProgramCounter)} a");
            _process.StandardOutput.ReadLine();
            _process.StandardOutput.ReadLine();
        }

        private void SetRunMode(bool shouldRun)
        {
            if (_isInRunMode != shouldRun)
            {
                _isInRunMode = shouldRun;
                _process.StandardInput.WriteLine("run");
                string output = _process.StandardOutput.ReadLine();
                Debug.Assert(output.StartsWith("Режим работы:"), output);
            }
        }

        private void AwaitOnThread(Action onFinish)
        {
            Debug.Assert(_execThread == null, "ExecuteOnThread called when another thread is observing the program");

            _execThread = new Thread(() =>
            {
                _process.StandardOutput.ReadLine(); // wait for the program to halt

                _process.StandardInput.WriteLine("read");
                _process.StandardOutput.ReadLine(); // Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн
                string line = _process.StandardOutput.ReadLine(); // final program state

                ExecutionState = ParseExecutionState(line);
                _execThread = null;
                onFinish();
            });
            _execThread.Start();
        }

        private static BasmExecutionState ParseExecutionState(string infoLine)
        {
            string[] parts = infoLine.Split();
            // Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн
            return new BasmExecutionState(
                programCounter: uint.Parse(parts[0], NumberStyles.HexNumber) - 1,
                accumulator: uint.Parse(parts[6], NumberStyles.HexNumber)
            );
        }

        private static string ConvertToHexAddress(uint adr)
        {
            if (adr < 16)
                return "00" + adr.ToString("x");
            else if (adr < 256)
                return "0" + adr.ToString("x");
            else
                return adr.ToString("x");
        }
    }
}
