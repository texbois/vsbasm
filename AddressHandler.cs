using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;

namespace BasmProgramHandler
{
    public class AddressHandler
    {
        public static Dictionary<int, string> HandleProgram(string filename, List<int> breakpoints)
        {
            // TODO: implement token enum
            // Variables initialization
            int offset = 0;
            string[] program;
            var mappedProgram = new Dictionary<int, string>();
            var breaks = new Dictionary<int, string>();
            var labels = new Dictionary<string, int>();

            // Convert input basm program
            // to array where every string
            // represent one command
            program = File.ReadAllLines(filename);

            foreach (string command in program)
            {
                if (breakpoints.Contains(offset))
                    breaks.Add(offset, command);
                // If command is comment or blank line then skip it
                if (Regex.Match(command, "^#").Success || command == "")
                    continue;
                // If we have ORG command, then update current offset
                if (Regex.Match(command, "ORG *").Success)
                {
                    offset = Int32.Parse(Regex.Match(command, "ORG\\s*([0-9A-Fa-f]+)").Groups[1].Value, NumberStyles.HexNumber);
                    continue;
                }
                // If command is label
                if (Regex.Match(command, ":$").Success)
                {
                    labels.Add(command, offset);
                    continue;
                }

                mappedProgram.Add(offset, command);

                offset += 1;
            }
            return breaks;
        }
    }
}
