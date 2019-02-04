using Microsoft.VisualStudio.Debugger.Interop;
using System.Collections.Generic;

namespace VSBASM.Deborgar
{
    class BreakpointManager
    {
        private readonly IDebugProgram2 _program;
        private readonly BasmBreakpointBackend _backend;
        private readonly SourceFile _sourceFile;
        private readonly EngineCallbacks _callbacks;

        private readonly List<PendingBreakpoint> _pendingBreakpoints = new List<PendingBreakpoint>();

        public BreakpointManager(IDebugProgram2 program, BasmBreakpointBackend backend, SourceFile sourceFile, EngineCallbacks callbacks)
        {
            _program = program;
            _sourceFile = sourceFile;
            _backend = backend;
            _callbacks = callbacks;
        }

        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            var pendingBreakpoint = new PendingBreakpoint(this, _backend, _callbacks, pBPRequest);
            _pendingBreakpoints.Add(pendingBreakpoint);
            ppPendingBP = pendingBreakpoint;
        }

        public BreakpointResolution ResolveBreakpoint(TEXT_POSITION location)
        {
            return new BreakpointResolution(_program, _sourceFile.GetLocationAddress(location), _sourceFile.GetLocationContext(location));
        }

        public IDebugBoundBreakpoint2 MaybeGetBreakpoint(uint address)
        {
            foreach (PendingBreakpoint pendingBreakpoint in _pendingBreakpoints)
            {
                IDebugBoundBreakpoint2 bp = pendingBreakpoint.GetIfBoundAtAddress(address);
                if (bp != null) return bp;
            }
            return null;
        }

        public void ClearBoundBreakpoints()
        {
            foreach (PendingBreakpoint pendingBreakpoint in _pendingBreakpoints)
            {
                pendingBreakpoint.ClearBoundBreakpoints();
            }
        }
    }
}
