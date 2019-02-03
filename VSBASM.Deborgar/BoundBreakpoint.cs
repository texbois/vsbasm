using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;

namespace VSBASM.Deborgar
{
    class BoundBreakpoint : IDebugBoundBreakpoint2
    {
        private PendingBreakpoint _pendingBreakpoint;
        private BreakpointResolution _resolution;
        private BasmBreakpointBackend _backend;

        private bool _enabled = true;
        private bool _deleted = false;

        public BoundBreakpoint(BasmBreakpointBackend backend, PendingBreakpoint pendingBreakpoint, BreakpointResolution breakpointResolution)
        {
            _backend = backend;
            _pendingBreakpoint = pendingBreakpoint;
            _resolution = breakpointResolution;
        }

        public void CompleteBind()
        {
            _backend.Set(_resolution);
        }

        #region IDebugBoundBreakpoint2 Members

        int IDebugBoundBreakpoint2.Delete()
        {
            if (!_deleted)
            {
                _deleted = true;
                _backend.Unset(_resolution);
                _pendingBreakpoint.OnBoundBreakpointDeleted(this);
            }

            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.Enable(int fEnable)
        {
            bool enabled = fEnable == 0 ? false : true;
            if (_enabled != enabled)
            {
                _enabled = enabled;
                if (enabled)
                    _backend.Set(_resolution);
                else
                    _backend.Unset(_resolution);
            }
            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.GetBreakpointResolution(out IDebugBreakpointResolution2 ppBPResolution)
        {
            ppBPResolution = _resolution;
            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint)
        {
            ppPendingBreakpoint = _pendingBreakpoint;
            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.GetState(enum_BP_STATE[] pState)
        {
            pState[0] = _deleted ? enum_BP_STATE.BPS_DELETED
                      : _enabled ? enum_BP_STATE.BPS_ENABLED
                      : enum_BP_STATE.BPS_DISABLED;

            return VSConstants.S_OK;
        }

        // The engine does not support hit counts on breakpoints.
        int IDebugBoundBreakpoint2.GetHitCount(out uint pdwHitCount)
        {
            pdwHitCount = 0;
            return VSConstants.E_NOTIMPL;
        }

        // The engine does not support conditions on breakpoints.
        int IDebugBoundBreakpoint2.SetCondition(BP_CONDITION bpCondition)
        {
            throw new NotImplementedException();
        }

        // The engine does not support hit counts on breakpoints.
        int IDebugBoundBreakpoint2.SetHitCount(uint dwHitCount)
        {
            throw new NotImplementedException();
        }

        // The engine does not support pass counts on breakpoints (used to specify the breakpoint hit count condition).
        int IDebugBoundBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
