using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace VSBASM.Deborgar
{
    class BreakpointResolution : IDebugBreakpointResolution2
    {
        private IDebugProgram2 _program;
        private SourceFileContext _context;

        public uint Address { get; private set; }

        public BreakpointResolution(IDebugProgram2 program, uint address, SourceFileContext context)
        {
            _program = program;
            Address = address;
            _context = context;
        }

        #region IDebugBreakpointResolution2 Members

        int IDebugBreakpointResolution2.GetBreakpointType(enum_BP_TYPE[] pBPType)
        {
            pBPType[0] = enum_BP_TYPE.BPT_CODE; // the engine only supports code breakpoints
            return VSConstants.S_OK;
        }

        int IDebugBreakpointResolution2.GetResolutionInfo(enum_BPRESI_FIELDS dwFields, BP_RESOLUTION_INFO[] pBPResolutionInfo)
        {
            if ((dwFields & enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION) != 0)
            {
                BP_RESOLUTION_LOCATION location = new BP_RESOLUTION_LOCATION();
                location.bpType = (uint) enum_BP_TYPE.BPT_CODE; // the engine only supports code breakpoints

                // The debugger will not QI the IDebugCodeContex2 interface returned here. We must pass the pointer
                // to IDebugCodeContex2 and not IUnknown.
                location.unionmember1 = Marshal.GetComInterfaceForObject(_context, typeof(IDebugCodeContext2));
                pBPResolutionInfo[0].bpResLocation = location;
                pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION;
            }

            if ((dwFields & enum_BPRESI_FIELDS.BPRESI_PROGRAM) != 0)
            {
                pBPResolutionInfo[0].pProgram = _program;
                pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_PROGRAM;
            }

            return VSConstants.S_OK;
        }

        #endregion
    }

    class AD7ErrorBreakpointResolution : IDebugErrorBreakpointResolution2
    {
        private string mMessage;
        private enum_BP_ERROR_TYPE mErrorType;

        public AD7ErrorBreakpointResolution(string msg, enum_BP_ERROR_TYPE errorType = enum_BP_ERROR_TYPE.BPET_GENERAL_WARNING)
        {
            mMessage = msg;
            mErrorType = errorType;
        }

        #region IDebugErrorBreakpointResolution2 Members

        int IDebugErrorBreakpointResolution2.GetBreakpointType(enum_BP_TYPE[] pBPType)
        {
            pBPType[0] = enum_BP_TYPE.BPT_CODE;
            return VSConstants.S_OK;
        }

        int IDebugErrorBreakpointResolution2.GetResolutionInfo(enum_BPERESI_FIELDS dwFields, BP_ERROR_RESOLUTION_INFO[] info)
        {
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_BPRESLOCATION) != 0) { }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_PROGRAM) != 0) { }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_THREAD) != 0) { }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_MESSAGE) != 0)
            {
                info[0].dwFields |= enum_BPERESI_FIELDS.BPERESI_MESSAGE;
                info[0].bstrMessage = mMessage;
            }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_TYPE) != 0)
            {
                info[0].dwFields |= enum_BPERESI_FIELDS.BPERESI_TYPE;
                info[0].dwType = mErrorType;
            }

            return VSConstants.S_OK;
        }

        #endregion
    }
}
