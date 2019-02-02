using Microsoft.VisualStudio.Debugger.Interop;
using System.Collections.Generic;
using System.Diagnostics;

namespace VSBASM.Deborgar
{
    class BreakpointManager
    {
        private BasmBreakpointBackend _backend;
        private BasmBreakpointResolver _resolver;
        private List<AD7PendingBreakpoint> _pendingBreakpoints = new List<AD7PendingBreakpoint>();

        public EngineCallbacks Callbacks { get; set; }

        public BreakpointManager(IDebugProgram2 program, BasmRunner runner)
        {
            _backend = new BasmBreakpointBackend(runner);
            _resolver = new BasmBreakpointResolver(program, runner.ProgramFile);
        }

        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            Debug.Assert(Callbacks != null);
            AD7PendingBreakpoint pendingBreakpoint = new AD7PendingBreakpoint(_resolver, _backend, Callbacks, pBPRequest);
            _pendingBreakpoints.Add(pendingBreakpoint);
            ppPendingBP = pendingBreakpoint;
        }

        public IDebugBoundBreakpoint2 MaybeGetBreakpoint(uint address)
        {
            foreach (AD7PendingBreakpoint pendingBreakpoint in _pendingBreakpoints)
            {
                IDebugBoundBreakpoint2 bp = pendingBreakpoint.GetIfBoundAtAddress(address);
                if (bp != null) return bp;
            }
            return null;
        }

        public void ClearBoundBreakpoints()
        {
            foreach (AD7PendingBreakpoint pendingBreakpoint in _pendingBreakpoints)
            {
                pendingBreakpoint.ClearBoundBreakpoints();
            }
        }
    }
}
