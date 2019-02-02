using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;

namespace VSBASM.Deborgar
{
    class AD7Thread : IDebugThread2
    {
        private string _threadName = "BASM thread";

        private AD7DocumentContext _context;
        private IDebugProgram2 _program;
        private BasmRunner _runner;

        public AD7Thread(AD7DocumentContext context, IDebugProgram2 program, BasmRunner runner)
        {
            _context = context;
            _program = program;
            _runner = runner;
        }

        public int EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
        {
            FRAMEINFO frameinfo;
            var frame = new AD7StackFrame(_context, _runner);
            frame.SetFrameInfo(dwFieldSpec, out frameinfo);
            ppEnum = new AD7FrameInfoEnum(new FRAMEINFO[] { frameinfo });
            return VSConstants.S_OK;
        }

        int IDebugThread2.GetName(out string pbstrName)
        {
            pbstrName = _threadName;
            return VSConstants.S_OK;
        }

        int IDebugThread2.SetThreadName(string pszName)
        {
            _threadName = pszName;
            return VSConstants.S_OK;
        }

        int IDebugThread2.GetProgram(out IDebugProgram2 ppProgram)
        {
            ppProgram = _program;
            return VSConstants.S_OK;
        }

        int IDebugThread2.CanSetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            return VSConstants.S_FALSE;
        }

        int IDebugThread2.SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.GetThreadId(out uint pdwThreadId)
        {
            pdwThreadId = 0;
            return VSConstants.S_OK;
        }

        int IDebugThread2.Suspend(out uint pdwSuspendCount)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.Resume(out uint pdwSuspendCount)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] ptp)
        {
            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_ID) != 0)
            {
                ptp[0].dwThreadId = 0;
                ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_ID;
            }
            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_NAME) != 0)
            {
                ptp[0].bstrName = _threadName;
                ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_NAME;
            }
            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_STATE) != 0)
            {
                ptp[0].dwThreadState = (int) enum_THREADSTATE.THREADSTATE_STOPPED;
                ptp[0].dwFields |= enum_THREADPROPERTY_FIELDS.TPF_STATE;
            }

            return VSConstants.S_OK;
        }

        int IDebugThread2.GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread)
        {
            ppLogicalThread = null;
            return VSConstants.E_NOTIMPL;
        }
    }
}
