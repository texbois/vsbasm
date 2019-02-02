using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace VSBASM.Deborgar
{
    class BasmRunner
    {
        public class State
        {
            public string acc { get; set; }
            public State(string a)
            {
                acc = a;
            }
        }

        private Process javaProcess;

        public readonly string ProgramFile;
        public bool IsRunning { get; private set; }
        public uint ProgramCounter { get; private set; }
        public EngineCallbacks Callbacks { private get; set; }

        public State CurrentState = new State("0000");

        public BasmRunner(string programFile)
        {
            ProgramFile = programFile;
        }

        public AD_PROCESS_ID StartSuspended()
        {
            Debug.Assert(javaProcess == null);

            var info = new ProcessStartInfo("java");
            info.Arguments = " -jar -Dmode=cli -Dfile.encoding=UTF-8 C:\\bcomp.jar";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.StandardOutputEncoding = Encoding.UTF8;

            javaProcess = new Process();
            javaProcess.StartInfo = info;
            javaProcess.Start();

            string code = File.ReadAllText(ProgramFile);
            javaProcess.StandardInput.WriteLine("asm");
            javaProcess.StandardInput.WriteLine(code);
            javaProcess.StandardInput.WriteLine("end");

            string line = "";
            while (!line.StartsWith("Программа начинается с адреса"))
                line = javaProcess.StandardOutput.ReadLine();

            ProgramCounter = uint.Parse(Regex.Match(line, "Программа начинается с адреса\\s*([0-9A-Fa-f]+)").Groups[1].Value, NumberStyles.HexNumber);

            var processId = new AD_PROCESS_ID();
            processId.ProcessIdType = (uint) enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
            processId.dwProcessId = (uint) javaProcess.Id;
            return processId;
        }

        public void LaunchProgram()
        {
            Debug.Assert(Callbacks != null, "LaunchProgram() must be invoked after EngineCallbacks are set");

            string hexPC = ConvertToHexAddress(ProgramCounter);
            javaProcess.StandardInput.WriteLine($"{hexPC} a");
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardOutput.ReadLine();

            javaProcess.StandardInput.WriteLine("run");
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardInput.WriteLine("start");

            ReadState();
        }

        private void ReadState()
        {
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardOutput.ReadLine(); // wait for the program to halt

            javaProcess.StandardInput.WriteLine("read");

            javaProcess.StandardOutput.ReadLine(); // Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн
            string line = javaProcess.StandardOutput.ReadLine(); // final program state

            ProgramCounter = uint.Parse(line.Split()[0], NumberStyles.HexNumber) - 1;
            CurrentState.acc = line.Split()[6];

            Callbacks.OnProgramStop(ProgramCounter);
        }

        public void Continue()
        {
            string hexPC = ConvertToHexAddress(ProgramCounter);
            javaProcess.StandardInput.WriteLine($"{hexPC} a");
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardInput.WriteLine("start");
            ReadState();
        }

        public void Terminate()
        {
            javaProcess.Close();
        }

        public void SetContents(uint address, string cmd)
        {
            string hexAdr = ConvertToHexAddress(address);
            javaProcess.StandardInput.WriteLine($"{hexAdr} a {cmd} w");
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardOutput.ReadLine();
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

        public string GetContents(uint address)
        {
            string hexAdr = ConvertToHexAddress(address);
            javaProcess.StandardInput.WriteLine($"{hexAdr} a read");
            javaProcess.StandardOutput.ReadLine();  // Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн
            javaProcess.StandardOutput.ReadLine();  // initial state
            string res = javaProcess.StandardOutput.ReadLine(); // final state
            return res.Split()[1];
        }
    }
}
