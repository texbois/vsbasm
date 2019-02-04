using System;
using System.Collections.Generic;

namespace VSBASM.Deborgar
{
    class BasmBreakpointBackend
    {
        private const string HaltCommand = "F000";

        private readonly Dictionary<uint, string> _addressToMemoryContents = new Dictionary<uint, string>();
        private readonly BasmRunner _runner;
        private readonly Action _onStepComplete;
        private readonly Action _onBreakComplete;

        public BasmBreakpointBackend(BasmRunner runner, Action onStepComplete, Action onBreakComplete)
        {
            _runner = runner;
            _onStepComplete = onStepComplete;
            _onBreakComplete = onBreakComplete;
        }

        public void RemoveBreakpoint(uint address)
        {
            if (_addressToMemoryContents.ContainsKey(address))
            {
                _runner.SetContents(address, _addressToMemoryContents[address]);
                _addressToMemoryContents.Remove(address);
            }
        }

        public void SetBreakpoint(uint address)
        {
            string contents = _runner.GetContents(address);
            _addressToMemoryContents[address] = contents;
            _runner.SetContents(address, HaltCommand);
        }

        public void Break()
        {
            _runner.BreakExecution();
            _onBreakComplete();
        }

        public void Step(Action customOnComplete = null)
        {
            var lastExecutedPC = _runner.ExecutionState.ProgramCounter;
            if (_addressToMemoryContents.ContainsKey(lastExecutedPC))
            {
                _runner.SetContents(lastExecutedPC, _addressToMemoryContents[lastExecutedPC]);
                _runner.Step(() =>
                {
                    // Restore the breakpoint, re-reading the contents of the target address;
                    // the command we've just executed could have overwritten it!

                    SetBreakpoint(lastExecutedPC);
                    if (customOnComplete != null)
                        customOnComplete();
                    else
                        _onStepComplete();
                });
            }
            else _runner.Step(() =>
            {
                if (customOnComplete != null)
                    customOnComplete();
                else
                    _onStepComplete();
            });
        }

        public void Continue()
        {
            // If the command we've stopped at has a breakpoint bound to it, we need to remove it first,
            // execute the command, and put the synthentic HLT back.
            // Essentially, we step by one instruction, then continue execution.
            Step(() => _runner.Continue());
        }
    }
}
