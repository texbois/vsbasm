using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BasmProgramHandler
{
    class ProgramExecutor
    {
        Dictionary<int, string> breaks;
        int currentAdr;
        Process javaProcess;

        public string GetStateAtTheBreakPoint()
        {
            breaks = AddressHandler.HandleProgram("TextFile1.txt", new List<int> { 10, 14 });
            ProcessStartInfo info = new ProcessStartInfo("java");
            info.Arguments = "-jar -Dmode=cli -Dfile.encoding=UTF8 C:\\Users\\student\\bcomp.jar";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.StandardOutputEncoding = System.Text.Encoding.UTF8;
            javaProcess = new Process();
            javaProcess.StartInfo = info;
            javaProcess.Start();
            javaProcess.StandardInput.WriteLine("asm");
            javaProcess.StandardInput.WriteLine(File.ReadAllText("TextFile1.txt"));
            javaProcess.StandardInput.WriteLine("end");
            string line = "";
            while (!line.StartsWith("Программа начинается с адреса"))
                line = javaProcess.StandardOutput.ReadLine();
            currentAdr = Int32.Parse(Regex.Match(line, "Программа начинается с адреса\\s*([0-9A-Fa-f]+)").Groups[1].Value, NumberStyles.HexNumber);
            foreach (KeyValuePair<int, string> k in this.breaks)
            {
                string hexAdr = convertToHexAddress(k.Key);
                javaProcess.StandardInput.WriteLine($"{hexAdr} a F000 w");
                javaProcess.StandardOutput.ReadLine();
                javaProcess.StandardOutput.ReadLine();
                javaProcess.StandardOutput.ReadLine();
            }
            string hex_adr = convertToHexAddress(this.currentAdr);
            javaProcess.StandardInput.WriteLine($"{hex_adr} a");
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardInput.WriteLine("run");
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardInput.WriteLine("start");
            return ReadOutput(javaProcess);

        }

        private string ReadOutput(Process javaProcess)
        {
            javaProcess.StandardOutput.ReadLine(); // Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн
            javaProcess.StandardOutput.ReadLine(); // wait for the program to halt

            javaProcess.StandardInput.WriteLine("read");

            javaProcess.StandardOutput.ReadLine(); // Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн
            string res = javaProcess.StandardOutput.ReadLine(); // final program state
            this.currentAdr = Int32.Parse(res.Split()[0], NumberStyles.HexNumber) - 1;
            return res;
        }

        private static string convertToHexAddress(int adr)
        {
            if (adr < 16)
                return "00" + adr.ToString("x");
            else if (adr < 256)
                return "0" + adr.ToString("x");
            else
                return adr.ToString("x");
        }

        public string Continue()
        {
            string hex_adr = convertToHexAddress(this.currentAdr);
            string cmd = MnemoToCode(this.breaks[this.currentAdr]);
            javaProcess.StandardInput.WriteLine($"{hex_adr} a {cmd} w");
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardInput.WriteLine($"{hex_adr} a");
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardOutput.ReadLine();
            javaProcess.StandardInput.WriteLine("start");
            return ReadOutput(javaProcess);
        }
        
        private static string MnemoToCode(string cmd)
        {
            if (Regex.Match(cmd, "ADD *").Success)
                return "4" + Regex.Match(cmd, "ADD\\s*([0-9A-Fa-f]+)").Groups[1].Value;
            if (Regex.Match(cmd, "ADC *").Success)
                return "5" + Regex.Match(cmd, "ADC\\s*([0-9A-Fa-f]+)").Groups[1].Value;
            if (Regex.Match(cmd, "SUB *").Success)
                return "6" + Regex.Match(cmd, "SUB\\s*([0-9A-Fa-f]+)").Groups[1].Value;
            if (Regex.Match(cmd, "BCS *").Success)
                return "8" + Regex.Match(cmd, "BCS\\s*([0-9A-Fa-f]+)").Groups[1].Value;
            // TODO: Implement all BASM commands
            return null;
        }
    }
}
