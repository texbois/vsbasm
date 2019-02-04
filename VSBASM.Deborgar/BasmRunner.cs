using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace VSBASM.Deborgar
{
    class BasmRunner
    {
        public delegate void OnStop(uint address);
        public delegate void OnStepComplete();

        public AD_PROCESS_ID ProcessId { get; private set; }
        public BasmExecutionState ExecutionState { get; private set; }

        private Process _process;
        private SourceFile _sourceFile;
        private OnStop _onStopHandler;
        private OnStepComplete _onStepHandler;
        private bool _isInRunMode = false;

        public BasmRunner(SourceFile sourceFile, OnStop stopHandler, OnStepComplete stepHandler)
        {
            _sourceFile = sourceFile;
            _onStopHandler = stopHandler;
            _onStepHandler = stepHandler;
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
            RunUpdatingState(resetAccAndIO: true);
            _onStopHandler(ExecutionState.ProgramCounter);
        }

        public void Step(bool executeStepHandler)
        {
            SetRunMode(false);
            SetAddressToPC();
            RunUpdatingState(resetAccAndIO: false);
            ExecutionState = new BasmExecutionState(ExecutionState.ProgramCounter + 1, ExecutionState);
            if (executeStepHandler) _onStepHandler();
        }

        public void Continue()
        {
            SetRunMode(true);
            SetAddressToPC();
            RunUpdatingState(resetAccAndIO: false);
            _onStopHandler(ExecutionState.ProgramCounter);
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
                _process.StandardOutput.ReadLine();
            }
        }

        private void RunUpdatingState(bool resetAccAndIO)
        {
            _process.StandardInput.WriteLine(resetAccAndIO ? "start" : "continue");
            _process.StandardOutput.ReadLine();
            _process.StandardOutput.ReadLine(); // wait for the program to halt

            _process.StandardInput.WriteLine("read");
            _process.StandardOutput.ReadLine(); // Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн
            string line = _process.StandardOutput.ReadLine(); // final program state

            ExecutionState = new BasmExecutionState(
                programCounter: uint.Parse(line.Split()[0], NumberStyles.HexNumber) - 1,
                accumulator: uint.Parse(line.Split()[6], NumberStyles.HexNumber)
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
