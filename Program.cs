using System;
using System.Diagnostics;

namespace Hello
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = GetStateAtTheBreakPoint("begin:\nadd 010\n add 020\nhlt\n");
            Console.WriteLine(s);
        }

        static string GetStateAtTheBreakPoint(string program)
        {
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo("java");
                info.Arguments = "-jar -Dmode=cli bcomp.jar";
                info.RedirectStandardInput = true;
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
                p.StartInfo = info;
                p.Start();
                p.StandardInput.Write("asm\n");
                p.StandardInput.Write(program);
                p.StandardInput.Write("end\n");
                p.StandardInput.Write("run\n");
                p.StandardInput.Write("start\n");
                p.StandardOutput.DiscardBufferedData();
                System.Threading.Thread.Sleep(5000);
                p.StandardInput.Write("read\n");
                string line = p.StandardOutput.ReadLine();
                while (line != "Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн")
                  line = p.StandardOutput.ReadLine();
                line = p.StandardOutput.ReadLine();
                while (line != "Адр Знчн  СК  РА  РК   РД    А  C Адр Знчн")
                  line = p.StandardOutput.ReadLine();
                return p.StandardOutput.ReadLine();
            }
        }
    }
}
