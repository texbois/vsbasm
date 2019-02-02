using Microsoft.VisualStudio.Debugger.Interop;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace VSBASM.Deborgar
{
    class BasmBreakpointResolver
    {
        Dictionary<uint, uint> _lineToAddress = new Dictionary<uint, uint>();
        private readonly string _documentName;
        private AD7Engine _engine;

        public BasmBreakpointResolver(AD7Engine engine, string docName)
        {
            ParseProgram(docName);
            _documentName = docName;
            _engine = engine;
        }

        public AD7BreakpointResolution Resolve(TEXT_POSITION location)
        {
            var context = new AD7DocumentContext(_documentName, location, location);
            return new AD7BreakpointResolution(_engine, _lineToAddress[location.dwLine], context);
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
                var canonicalCommand = command.ToUpper();
                // If command is comment or blank line then skip it
                if (Regex.Match(canonicalCommand, "\\s*;").Success || command == "")
                {
                    _lineToAddress.Add(lineNumber, address);
                    lineNumber += 1;
                    continue;
                }
                // If we have ORG command, then update current offset
                if (Regex.Match(canonicalCommand, "ORG *").Success)
                {
                    address = uint.Parse(Regex.Match(canonicalCommand, "ORG\\s*([0-9A-Fa-f]+)").Groups[1].Value, NumberStyles.HexNumber);
                    _lineToAddress.Add(lineNumber, address);
                    lineNumber += 1;
                    continue;
                }
                // If command is label
                if (Regex.Match(canonicalCommand, ":$").Success)
                {
                    _lineToAddress.Add(lineNumber, address);
                    lineNumber += 1;
                    continue;
                }

                _lineToAddress.Add(lineNumber, address);

                address += 1;
                lineNumber += 1;
            }
        }
    }
}
