using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Diagnostics;

namespace VSBASM.Deborgar
{
    class AD7ProgramNode : IDebugProgramNode2
    {
        private readonly int _processId;

        public AD7ProgramNode(int processId)
        {
            _processId = processId;
        }

        #region IDebugProgramNode2 Members

        int IDebugProgramNode2.GetEngineInfo(out string engineName, out Guid engineGuid)
        {
            engineName = AD7Engine.DebugEngineName;
            engineGuid = AD7Engine.DebugEngineGuid;
            return VSConstants.S_OK;
        }

        int IDebugProgramNode2.GetHostPid(AD_PROCESS_ID[] pHostProcessId)
        {
            pHostProcessId[0].ProcessIdType = (uint) enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
            pHostProcessId[0].dwProcessId = (uint) _processId;
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

        #endregion
    }
}