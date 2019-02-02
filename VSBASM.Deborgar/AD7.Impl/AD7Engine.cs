using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace VSBASM.Deborgar
{
    [ComVisible(true)]
    [Guid("8355452D-6D2F-41b0-89B8-BB2AA2529E94")]
    public class AD7Engine : IDebugEngine2, IDebugEngineLaunch2, IDebugProgram3, IDebugEngineProgram2
    {
        public const string DebugEngineId = "{8355452D-6D2F-41b0-89B8-BB2AA2529E94}";
        public const string DebugEngineName = "BASM";
        public static Guid DebugEngineGuid = new Guid(DebugEngineId);

        BreakpointManager _breakpointManager;
        EngineCallbacks _callbacks;

        BasmRunner _basmRunner;
        Guid _attachedGuid;

        #region IDebugEngine2 Members

        int IDebugEngine2.Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 ad7Callback, enum_ATTACH_REASON dwReason)
        {
            IDebugProcess2 process;
            IDebugProgram2 program = rgpPrograms[0];
            IDebugProgramNode2 node = rgpProgramNodes[0];
            EngineUtils.RequireOk(program.GetProcess(out process));
            EngineUtils.RequireOk(program.GetProgramId(out _attachedGuid));

            var context = new AD7DocumentContext(_basmRunner.ProgramFile,
                new TEXT_POSITION { dwColumn = 0, dwLine = 0 },
                new TEXT_POSITION { dwColumn = 0, dwLine = 0 });
            var thread = new AD7Thread(context, this, _basmRunner);

            _breakpointManager = new BreakpointManager(this, _basmRunner);
            _callbacks = new EngineCallbacks(this, thread, _breakpointManager, ad7Callback, process);
            _breakpointManager.Callbacks = _callbacks;
            _basmRunner.Callbacks = _callbacks;

            Debug.WriteLine("IDebugEngine2.Attach: invoking load callbacks");
            _callbacks.OnAttach();

            return VSConstants.S_OK;
        }

        // Requests that all programs being debugged by this DE stop execution the next time one of their threads attempts to run.
        // This is normally called in response to the user clicking on the pause button in the debugger.
        // When the break is complete, an AsyncBreakComplete event will be sent back to the debugger.
        int IDebugEngine2.CauseBreak()
        {
            return ((IDebugProgram2) this).CauseBreak();
        }

        int IDebugEngine2.ContinueFromSynchronousEvent(IDebugEvent2 eventObject)
        {
            if (eventObject is AD7ProgramDestroyEvent)
            {
                _basmRunner = null;
            }
            if (eventObject is AD7LoadCompleteEvent)
            {
                _basmRunner.LaunchProgram();
            }

            return VSConstants.S_OK;
        }

        int IDebugEngine2.CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            Debug.Assert(_breakpointManager != null);
            _breakpointManager.CreatePendingBreakpoint(pBPRequest, out ppPendingBP);
            return VSConstants.S_OK;
        }

        int IDebugEngine2.DestroyProgram(IDebugProgram2 program)
        {
            return program.Terminate();
        }

        int IDebugEngine2.GetEngineId(out Guid guidEngine)
        {
            guidEngine = DebugEngineGuid;
            return VSConstants.S_OK;
        }

        #region Exceptions (unsupported)

        int IDebugEngine2.RemoveAllSetExceptions(ref Guid guidType)
        {
            return VSConstants.S_OK;
        }

        int IDebugEngine2.RemoveSetException(EXCEPTION_INFO[] pException)
        {
            return VSConstants.S_OK;
        }

        int IDebugEngine2.SetException(EXCEPTION_INFO[] pException)
        {
            return VSConstants.S_OK;
        }

        #endregion

        int IDebugEngine2.SetLocale(ushort wLangID)
        {
            return VSConstants.S_OK;
        }

        int IDebugEngine2.SetMetric(string pszMetric, object varValue)
        {
            return VSConstants.S_OK;
        }

        int IDebugEngine2.SetRegistryRoot(string pszRegistryRoot)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugEngineLaunch2 Members

        int IDebugEngineLaunch2.CanTerminateProcess(IDebugProcess2 process)
        {
            return _basmRunner.IsRunning ? VSConstants.S_OK : VSConstants.S_FALSE;
        }

        int IDebugEngineLaunch2.LaunchSuspended(string pszServer, IDebugPort2 port, string exe, string args, string dir, string env, string options, enum_LAUNCH_FLAGS launchFlags, uint hStdInput, uint hStdOutput, uint hStdError, IDebugEventCallback2 ad7Callback, out IDebugProcess2 process)
        {
            Debug.Assert(_basmRunner == null);
            _basmRunner = new BasmRunner(Path.Combine(dir, exe));

            var processId = _basmRunner.StartSuspended();
            EngineUtils.RequireOk(port.GetProcess(processId, out process));

            Debug.WriteLine("IDebugEngineLaunch2.LaunchSuspended: returning S_OK");
            return VSConstants.S_OK;
        }

        // Resume a process launched by IDebugEngineLaunch2.LaunchSuspended
        int IDebugEngineLaunch2.ResumeProcess(IDebugProcess2 process)
        {
            IDebugPort2 port;
            EngineUtils.RequireOk(process.GetPort(out port));
            IDebugDefaultPort2 defaultPort = (IDebugDefaultPort2) port;

            IDebugPortNotify2 portNotify;
            EngineUtils.RequireOk(defaultPort.GetPortNotify(out portNotify));
            EngineUtils.RequireOk(portNotify.AddProgramNode(new AD7ProgramNode(EngineUtils.GetProcessId(process))));

            Debug.WriteLine("IDebugEngineLaunch2.ResumeProcess: returning S_OK");
            return VSConstants.S_OK;
        }

        int IDebugEngineLaunch2.TerminateProcess(IDebugProcess2 process)
        {
            return process.Terminate();
        }

        #endregion

        #region IDebugProgram2 Members

        public int CanDetach()
        {
            return VSConstants.S_OK;
        }

        // The debugger calls CauseBreak when the user clicks on the pause button in VS. The debugger should respond by entering
        // breakmode. 
        public int CauseBreak()
        {
            throw new NotImplementedException();
        }

        // Continue is called from the SDM when it wants execution to continue in the debugee
        // but have stepping state remain. An example is when a tracepoint is executed, 
        // and the debugger does not want to actually enter break mode.
        public int Continue(IDebugThread2 pThread)
        {
            throw new NotImplementedException();
        }

        // Detach is called when debugging is stopped and the process was attached to (as opposed to launched)
        // or when one of the Detach commands are executed in the UI.
        public int Detach()
        {
            _breakpointManager.ClearBoundBreakpoints();

            return VSConstants.E_NOTIMPL;
        }

        public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumCodePaths(string hint, IDebugCodeContext2 start, IDebugStackFrame2 frame, int fSource, out IEnumCodePaths2 pathEnum, out IDebugCodeContext2 safetyContext)
        {
            pathEnum = null;
            safetyContext = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumModules(out IEnumDebugModules2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            ppProperty = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 codeContext, out IDebugDisassemblyStream2 disassemblyStream)
        {
            disassemblyStream = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetENCUpdate(out object update)
        {
            update = null;
            return VSConstants.S_OK;
        }

        public int GetEngineInfo(out string engineName, out Guid engineGuid)
        {
            engineName = DebugEngineName;
            engineGuid = DebugEngineGuid;
            return VSConstants.S_OK;
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            ppMemoryBytes = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetName(out string programName)
        {
            programName = null;
            return VSConstants.S_OK;
        }

        // Must return the program identifier originally passed to the IDebugEngine2::Attach method.
        public int GetProgramId(out Guid guidProgramId)
        {
            Debug.Assert(_attachedGuid != Guid.Empty);
            guidProgramId = _attachedGuid;
            return VSConstants.S_OK;
        }

        public int Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step)
        {
            return VSConstants.S_OK;
        }

        public int Terminate()
        {
            return VSConstants.S_OK;
        }

        public int WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IDebugProgram3 Members

        public int ExecuteOnThread(IDebugThread2 pThread)
        {
            _basmRunner.Continue();
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugEngineProgram2 Members

        // Called when this program is being debugged in a multi-program environment. This engine only has one program per-process.
        public int Stop()
        {
            throw new NotImplementedException();
        }

        // WatchForExpressionEvaluationOnThread is used to cooperate between two different engines debugging 
        // the same process. This engine doesn't cooperate with other engines.
        public int WatchForExpressionEvaluationOnThread(IDebugProgram2 pOriginatingProgram, uint dwTid, uint dwEvalFlags, IDebugEventCallback2 pExprCallback, int fWatch)
        {
            return VSConstants.S_OK;
        }

        // WatchForThreadStep is used to cooperate between two different engines debugging the same process.
        // This engine doesn't cooperate with other engines.
        public int WatchForThreadStep(IDebugProgram2 pOriginatingProgram, uint dwTid, int fWatch, uint dwFrame)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region Deprecated interface methods

        int IDebugEngine2.EnumPrograms(out IEnumDebugPrograms2 programs)
        {
            Debug.Fail("This function is not called by the debugger");
            programs = null;
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
