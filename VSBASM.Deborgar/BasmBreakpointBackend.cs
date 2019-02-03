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

        public void Unset(BreakpointResolution resolution)
        {
            if (_addressToMemoryContents.ContainsKey(resolution.Address))
            {
                _runner.SetContents(resolution.Address, _addressToMemoryContents[resolution.Address]);
                _addressToMemoryContents.Remove(resolution.Address);
            }
        }

        public void Set(BreakpointResolution resolution)
        {
            string contents = _runner.GetContents(resolution.Address);
            _addressToMemoryContents.Add(resolution.Address, contents);
            _runner.SetContents(resolution.Address, "F000");
        }

        public void Continue()
        {
            _runner.SetContents(
                _runner.ExecutionState.ProgramCounter,
                _addressToMemoryContents[_runner.ExecutionState.ProgramCounter]);
            _runner.Continue();
        }
    }
}
