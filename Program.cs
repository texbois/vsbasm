using System;
using System.Collections.Generic;

namespace BasmProgramHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            ProgramExecutor pe = new ProgramExecutor();
            string s = pe.GetStateAtTheBreakPoint();
            Console.WriteLine(s);
            s = pe.Continue();
            Console.WriteLine(s);
            Console.ReadKey();
        }
    }
}
