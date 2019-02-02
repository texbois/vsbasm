using System.Collections.Generic;

namespace VSBASM.Deborgar
{
    class BasmBreakpointBackend
    {
        Dictionary<uint, string> _addressToMemoryContents = new Dictionary<uint, string>();
        BasmRunner _runner;

        public BasmBreakpointBackend(BasmRunner runner)
        {
            _runner = runner;
        }

        public void Unset(AD7BreakpointResolution resolution)
        {
            if (_addressToMemoryContents.ContainsKey(resolution.Address))
            {
                _runner.SetContents(resolution.Address, _addressToMemoryContents[resolution.Address]);
                _addressToMemoryContents.Remove(resolution.Address);
            }
        }

        public void Set(AD7BreakpointResolution resolution)
        {
            string contents = _runner.GetContents(resolution.Address);
            _addressToMemoryContents.Add(resolution.Address, contents);
            _runner.SetContents(resolution.Address, "F000");
        }

        public void Continue()
        {
            _runner.SetContents(_runner.ProgramCounter, _addressToMemoryContents[_runner.ProgramCounter]);
            _runner.Continue();
        }
    }
}
