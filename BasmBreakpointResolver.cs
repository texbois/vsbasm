using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Microsoft.VisualStudio.Debugger.SampleEngine
{
    class BasmBreakpointResolver
    {
        Dictionary<uint, uint> lineToAddress;
        private string documentName;
        
        public BasmBreakpointResolver(string docName)
        {
            ParseProgram(docName);
            this.documentName = docName;
        }

        public AD7BreakpointResolution Resolve(TEXT_POSITION location)
        {
            var c = new AD7DocumentContext(documentName, location, location);
            return new AD7BreakpointResolution(new AD7Engine(), lineToAddress[location.dwLine], c);
        }

        public void ParseProgram(string documentName)
        {
            string[] program = File.ReadAllLines(documentName);
            // TODO: implement token enum
            // Variables initialization
            uint address = 0;
            uint lineNumber = 0;
            foreach (string command in program)
            {
                // If command is comment or blank line then skip it
                if (Regex.Match(command, "^#").Success || command == "")
                {
                    lineToAddress.Add(lineNumber, address);
                    lineNumber += 1;
                    continue;
                }
                // If we have ORG command, then update current offset
                if (Regex.Match(command, "ORG *").Success)
                {
                    address = UInt32.Parse(Regex.Match(command, "ORG\\s*([0-9A-Fa-f]+)").Groups[1].Value, NumberStyles.HexNumber);
                    lineToAddress.Add(lineNumber, address);
                    lineNumber += 1;
                    continue;
                }
                // If command is label
                if (Regex.Match(command, ":$").Success)
                {
                    lineToAddress.Add(lineNumber, address);
                    lineNumber += 1;
                    continue;
                }
                if (Regex.Match(command, "WORD *").Success)
                {
                    lineToAddress.Add(lineNumber, address);
                    lineNumber += 1;
                    continue;
                }

                lineToAddress.Add(lineNumber, address);

                address += 1;
                lineNumber += 1;
            }
        }
    }
}
