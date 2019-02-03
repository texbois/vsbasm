using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VSBASM.Deborgar
{
    //  When a user creates a new breakpoint, the pending breakpoint is created and is later bound. The bound breakpoints
    // become children of the pending breakpoint.
    class PendingBreakpoint : IDebugPendingBreakpoint2
    {
        private readonly BreakpointManager _manager;

        private BasmBreakpointBackend _backend;
        private EngineCallbacks _callbacks;
        private IDebugBreakpointRequest2 _bpRequest;
        private BP_REQUEST_INFO _requestInfo;

        private BoundBreakpoint _boundBreakpoint;

        private bool _enabled = false;
        private bool _deleted = false;

        public PendingBreakpoint(BreakpointManager manager, BasmBreakpointBackend backend, EngineCallbacks callbacks, IDebugBreakpointRequest2 pBPRequest)
        {
            _manager = manager;
            _backend = backend;
            _callbacks = callbacks;
            _bpRequest = pBPRequest;

            BP_REQUEST_INFO[] requestInfo = new BP_REQUEST_INFO[1];
            EngineUtils.RequireOk(_bpRequest.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_BPLOCATION, requestInfo));
            _requestInfo = requestInfo[0];
        }

        public IDebugBoundBreakpoint2 GetIfBoundAtAddress(uint address)
        {
            if (_boundBreakpoint != null)
            {
                IDebugBreakpointResolution2 resolution;
                ((IDebugBoundBreakpoint2) _boundBreakpoint).GetBreakpointResolution(out resolution);
                if (((BreakpointResolution) resolution).Address == address)
                {
                    return _boundBreakpoint;
                }
            }
            return null;
        }

        private bool CanBind()
        {
            // The engine only supports breakpoints on a file and line number.
            return !_deleted && _requestInfo.bpLocation.bpLocationType == (uint) enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE;
        }

        public void ClearBoundBreakpoints()
        {
            _boundBreakpoint = null;
        }

        public void OnBoundBreakpointDeleted(BoundBreakpoint boundBreakpoint)
        {
            Debug.Assert(boundBreakpoint == _boundBreakpoint);
            _boundBreakpoint = null;
        }

        #region IDebugPendingBreakpoint2 Members

        int IDebugPendingBreakpoint2.Bind()
        {
            if (!CanBind())
            {
                // The breakpoint could not be bound. This may occur for many reasons such as an invalid location, an invalid expression, etc...
                // We may want to send an instance of IDebugBreakpointErrorEvent2 to the UI and return a valid instance of
                // IDebugErrorBreakpoint2 from IDebugPendingBreakpoint2::EnumErrorBreakpoints. The debugger will then
                // display information about why the breakpoint did not bind to the user.
                return VSConstants.S_FALSE;
            }
            if (_boundBreakpoint != null)
                throw new NotImplementedException(); // multiple bound breakpoints are not supported

            IDebugDocumentPosition2 docPosition = (IDebugDocumentPosition2)
                Marshal.GetObjectForIUnknown(_requestInfo.bpLocation.unionmember2);

            string documentName;
            TEXT_POSITION[] startPosition = new TEXT_POSITION[1];
            TEXT_POSITION[] endPosition = new TEXT_POSITION[1];

            EngineUtils.RequireOk(docPosition.GetFileName(out documentName));
            EngineUtils.RequireOk(docPosition.GetRange(startPosition, endPosition));

            var resolution = _manager.ResolveBreakpoint(startPosition[0]);
            _boundBreakpoint = new BoundBreakpoint(_backend, this, resolution);
            _boundBreakpoint.CompleteBind();

            _callbacks.OnBreakpointBound(_boundBreakpoint);

            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum)
        {
            ppErrorEnum = null;

            if (!CanBind())
            {
                // Called to determine if a pending breakpoint can be bound. 
                // The breakpoint may not be bound for many reasons such as an invalid location, an invalid expression, etc...
                // We may want to return a valid enumeration of IDebugErrorBreakpoint2.
                // The debugger will then display information about why the breakpoint did not bind to the user.
                ppErrorEnum = null;
                return VSConstants.S_FALSE;
            }

            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.Delete()
        {
            if (_boundBreakpoint != null)
            {
                ((IDebugBoundBreakpoint2) _boundBreakpoint).Delete();
            }
            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.Enable(int fEnable)
        {
            _enabled = fEnable == 0 ? false : true;
            if (_boundBreakpoint != null)
            {
                ((IDebugBoundBreakpoint2) _boundBreakpoint).Enable(fEnable);
            }
            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            ppEnum = new AD7BoundBreakpointsEnum(new IDebugBoundBreakpoint2[] { _boundBreakpoint });
            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugPendingBreakpoint2.GetBreakpointRequest(out IDebugBreakpointRequest2 ppBPRequest)
        {
            ppBPRequest = _bpRequest;
            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.GetState(PENDING_BP_STATE_INFO[] pState)
        {
            pState[0].state = _deleted ? (enum_PENDING_BP_STATE) enum_BP_STATE.BPS_DELETED
                            : _enabled ? (enum_PENDING_BP_STATE) enum_BP_STATE.BPS_ENABLED
                            : (enum_PENDING_BP_STATE) enum_BP_STATE.BPS_DISABLED;

            return VSConstants.S_OK;
        }

        // The sample engine does not support conditions on breakpoints.
        int IDebugPendingBreakpoint2.SetCondition(BP_CONDITION bpCondition)
        {
            throw new NotImplementedException();
        }

        // The sample engine does not support pass counts on breakpoints.
        int IDebugPendingBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            throw new NotImplementedException();
        }

        // Toggles the virtualized state of this pending breakpoint. When a pending breakpoint is virtualized, 
        // the debug engine will attempt to bind it every time new code loads into the program. (not supported)
        int IDebugPendingBreakpoint2.Virtualize(int fVirtualize)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
