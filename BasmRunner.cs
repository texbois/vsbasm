using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.VisualStudio.Debugger.SampleEngine
{
    class BasmRunner
    {
        private Process javaProcess;
        private IDebugEventCallback2 deCallback;

        public bool IsRunning { get; private set; }

        public AD_PROCESS_ID Start(IDebugEventCallback2 callback)
        {
            Debug.Assert(javaProcess == null);
            deCallback = callback;

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

            var processId = new AD_PROCESS_ID();
            processId.ProcessIdType = (uint) enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
            processId.dwProcessId = (uint) javaProcess.Id;
            return processId;
        }

        public void EnterProgram(string codeFile, IDebugEngine2 engine, IDebugProcess2 process, IDebugProgram2 program)
        {
            //string code = File.ReadAllText(codeFile);
            string code = "org 010\r\nbegin:\r\ncla\r\nadd 020\r\nhlt";
            javaProcess.StandardInput.WriteLine("asm");
            javaProcess.StandardInput.WriteLine(code);
            javaProcess.StandardInput.WriteLine("end");
            javaProcess.StandardInput.WriteLine("run");
            javaProcess.StandardInput.WriteLine("start");
            string state = ReadOutput();
            //deCallback.Event(engine, process, program, null, new AD7ProgramDestroyEvent(0), new Guid(AD7ProgramDestroyEvent.IID), 0);
        }

        private string ReadOutput()
        {
            javaProcess.StandardOutput.ReadLine(); // Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн
            javaProcess.StandardOutput.ReadLine(); // wait for the program to halt

            javaProcess.StandardInput.WriteLine("read");

            javaProcess.StandardOutput.ReadLine(); // Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн
            return javaProcess.StandardOutput.ReadLine(); // final program state
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

