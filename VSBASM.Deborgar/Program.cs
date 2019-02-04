using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Diagnostics;

namespace VSBASM.Deborgar
{
    // BasePC does not have the concept of threads, so the executing program and the thread are one.
    class Program : IDebugProgramNode2, IDebugProgram3, IDebugThread2
    {
        private const string _threadName = "BASM Thread";
        private const string _programName = "BASM Program";

        private readonly BasmRunner _runner;
        private readonly BasmBreakpointBackend _breakpointBackend;
        private readonly SourceFile _sourceFile;

        public Guid AttachedGuid { get; private set; }

        public Program(BasmRunner runner, BasmBreakpointBackend bpBackend, SourceFile sourceFile)
        {
            _runner = runner;
            _breakpointBackend = bpBackend;
            _sourceFile = sourceFile;
        }

        public AD_PROCESS_ID StartBasmProcess()
        {
            _runner.StartSuspended();
            return _runner.ProcessId;
        }

        public void AttachDebugger(Guid attachedGuid)
        {
            AttachedGuid = attachedGuid;
        }

        public void Launch()
        {
            _runner.LaunchProgram();
        }

        public int EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
        {
            FRAMEINFO frameinfo;
            var context = _sourceFile.GetAddressContext(_runner.ExecutionState.ProgramCounter);
            var frame = new StackFrame(context, _runner.ExecutionState);
            frame.SetFrameInfo(dwFieldSpec, out frameinfo);
            ppEnum = new AD7FrameInfoEnum(new FRAMEINFO[] { frameinfo });
            return VSConstants.S_OK;
        }

        #region IDebugProgram2/IDebugProgram3 Members

        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            ppEnum = new AD7ThreadEnum(new IDebugThread2[] { this });
            return VSConstants.S_OK;
        }

        public int GetName(out string pbstrName)
        {
            pbstrName = _programName;
            return VSConstants.S_OK;
        }

        public int GetProgramId(out Guid pguidProgramId)
        {
            Debug.Assert(AttachedGuid != Guid.Empty);
            pguidProgramId = AttachedGuid;
            return VSConstants.S_OK;
        }
        public int GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            pbstrEngine = DebugEngine.DebugEngineName;
            pguidEngine = DebugEngine.DebugEngineGuid;
            return VSConstants.S_OK;
        }

        public int Terminate()
        {
            return VSConstants.S_OK;
        }

        public int CanDetach()
        {
            return VSConstants.S_OK;
        }

        public int Detach()
        {
            throw new NotImplementedException();
        }

        public int Continue(IDebugThread2 pThread)
        {
            return ExecuteOnThread(pThread);
        }

        public int ExecuteOnThread(IDebugThread2 pThread)
        {
            _breakpointBackend.Continue();
            return VSConstants.S_OK;
        }

        public int Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT step)
        {
            if (sk == enum_STEPKIND.STEP_INTO || sk == enum_STEPKIND.STEP_OUT || sk == enum_STEPKIND.STEP_OVER)
            {
                _breakpointBackend.Step();
                return VSConstants.S_OK;
            }
            return VSConstants.E_NOTIMPL;
        }

        public int CauseBreak()
        {
            _breakpointBackend.Break();
            return VSConstants.S_OK;
        }

        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            ppProperty = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            ppMemoryBytes = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream)
        {
            ppDisassemblyStream = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumModules(out IEnumDebugModules2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetENCUpdate(out object ppUpdate)
        {
            ppUpdate = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety)
        {
            ppEnum = null;
            ppSafety = null;
            return VSConstants.E_NOTIMPL;
        }

        public int WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IDebugProgramNode2 Members

        int IDebugProgramNode2.GetEngineInfo(out string engineName, out Guid engineGuid)
        {
            engineName = DebugEngine.DebugEngineName;
            engineGuid = DebugEngine.DebugEngineGuid;
            return VSConstants.S_OK;
        }

        int IDebugProgramNode2.GetHostPid(AD_PROCESS_ID[] pHostProcessId)
        {
            pHostProcessId[0] = _runner.ProcessId;
            return VSConstants.S_OK;
        }

        int IDebugProgramNode2.GetHostName(enum_GETHOSTNAME_TYPE dwHostNameType, out string processName)
        {
            // We are using default transport and don't want to customize the process name
            processName = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.GetProgramName(out string programName)
        {
            // We are using default transport and don't want to customize the process name
            programName = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IDebugThread2 Members

        int IDebugThread2.GetName(out string pbstrName)
        {
            pbstrName = _threadName;
            return VSConstants.S_OK;
        }

        int IDebugThread2.SetThreadName(string pszName)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugThread2.GetProgram(out IDebugProgram2 ppProgram)
        {
            ppProgram = this;
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

        #endregion

        #region Deprecated interface methods

        int IDebugProgramNode2.Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
        {
            Debug.Fail("This function is not called by the debugger");
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.DetachDebugger_V7()
        {
            Debug.Fail("This function is not called by the debugger");
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.GetHostMachineName_V7(out string hostMachineName)
        {
            Debug.Fail("This function is not called by the debugger");
            hostMachineName = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Attach(IDebugEventCallback2 pCallback)
        {
            Debug.Fail("This function is not called by the debugger");
            return VSConstants.E_NOTIMPL;
        }

        public int GetProcess(out IDebugProcess2 process)
        {
            Debug.Fail("This function is not called by the debugger");
            process = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Execute()
        {
            Debug.Fail("This function is not called by the debugger.");
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}
