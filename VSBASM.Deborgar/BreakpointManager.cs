using Microsoft.VisualStudio.Debugger.Interop;
using System.Collections.Generic;

namespace VSBASM.Deborgar
{
    class BreakpointManager
    {
        private IDebugProgram2 _program;
        private SourceFile _sourceFile;

        private BasmBreakpointBackend _backend;
        private List<AD7PendingBreakpoint> _pendingBreakpoints = new List<AD7PendingBreakpoint>();
        private EngineCallbacks _callbacks;

        public BreakpointManager(IDebugProgram2 program, BasmRunner runner, SourceFile sourceFile, EngineCallbacks callbacks)
        {
            _program = program;
            _sourceFile = sourceFile;

            _backend = new BasmBreakpointBackend(runner);
            _callbacks = callbacks;
        }

        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            var pendingBreakpoint = new AD7PendingBreakpoint(this, _backend, _callbacks, pBPRequest);
            _pendingBreakpoints.Add(pendingBreakpoint);
            ppPendingBP = pendingBreakpoint;
        }

        public AD7BreakpointResolution ResolveBreakpoint(TEXT_POSITION location)
        {
            return new AD7BreakpointResolution(_program, _sourceFile.GetLocationAddress(location), _sourceFile.GetLocationContext(location));
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
