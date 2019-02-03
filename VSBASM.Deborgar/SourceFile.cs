using Microsoft.VisualStudio.Debugger.Interop;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace VSBASM.Deborgar
{
    class SourceFile
    {
        private Dictionary<uint, uint> _lineToAddress = new Dictionary<uint, uint>();
        /* Unlike _lineToAddress, _addressToLine does not contain entries for labels, comments, and ORG directives. */
        private Dictionary<uint, uint> _addressToLine = new Dictionary<uint, uint>();

        public string FilePath { get; private set; }

        public SourceFile(string filePath)
        {
            FilePath = filePath;
            ParseProgram();
        }

        public string GetContents()
        {
            return File.ReadAllText(FilePath);
        }

        public uint GetLocationAddress(TEXT_POSITION location)
        {
            return _lineToAddress[location.dwLine];
        }

        public AD7DocumentContext GetLocationContext(TEXT_POSITION location)
        {
            return new AD7DocumentContext(FilePath, location, location);
        }

        public AD7DocumentContext GetAddressContext(uint address)
        {
            var location = new TEXT_POSITION() { dwLine = _addressToLine[address], dwColumn = 0 };
            return new AD7DocumentContext(FilePath, location, location);
        }

        private void ParseProgram()
        {
            string[] program = File.ReadAllLines(FilePath);
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
                _addressToLine.Add(address, lineNumber);

                address += 1;
                lineNumber += 1;
            }
        }
    }
}
