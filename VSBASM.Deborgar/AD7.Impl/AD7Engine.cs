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
    public class AD7Engine : IDebugEngine2, IDebugEngineLaunch2, IDebugEngineProgram2
    {
        public const string DebugEngineId = "{8355452D-6D2F-41b0-89B8-BB2AA2529E94}";
        public const string DebugEngineName = "BASM";
        public static Guid DebugEngineGuid = new Guid(DebugEngineId);

        BreakpointManager _breakpointManager;
        EngineCallbacks _callbacks;

        BasmRunner _basmRunner;
        DE.BasmProgram _basmProgram; 

        #region IDebugEngine2 Members

        int IDebugEngine2.Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 ad7Callback, enum_ATTACH_REASON dwReason)
        {
            Debug.Assert(_basmProgram != null);

            IDebugProcess2 process;
            IDebugProgram2 program = rgpPrograms[0];
            EngineUtils.RequireOk(program.GetProcess(out process));

            Guid attachedGuid;
            EngineUtils.RequireOk(program.GetProgramId(out attachedGuid));
            _basmProgram.AttachedGuid = attachedGuid;

            _breakpointManager = new BreakpointManager(_basmProgram, _basmRunner);
            _callbacks = new EngineCallbacks(this, _basmProgram, _breakpointManager, ad7Callback, process);
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
            _basmRunner.StartSuspended();

            EngineUtils.RequireOk(port.GetProcess(_basmRunner.ProcessId, out process));

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
            
            _basmProgram = new DE.BasmProgram(_basmRunner);
            EngineUtils.RequireOk(portNotify.AddProgramNode(_basmProgram));

            Debug.WriteLine("IDebugEngineLaunch2.ResumeProcess: returning S_OK");
            return VSConstants.S_OK;
        }

        int IDebugEngineLaunch2.TerminateProcess(IDebugProcess2 process)
        {
            return process.Terminate();
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

        #endregion
    }
}
