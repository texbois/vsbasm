using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace VSBASM.Deborgar
{
    #region Event base classes

    class AD7AsynchronousEvent : IDebugEvent2
    {
        public const uint Attributes = (uint) enum_EVENTATTRIBUTES.EVENT_ASYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    class AD7StoppingEvent : IDebugEvent2
    {
        public const uint Attributes = (uint) enum_EVENTATTRIBUTES.EVENT_ASYNC_STOP;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    class AD7SynchronousEvent : IDebugEvent2
    {
        public const uint Attributes = (uint) enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    class AD7SynchronousStoppingEvent : IDebugEvent2
    {
        public const uint Attributes = (uint) enum_EVENTATTRIBUTES.EVENT_STOPPING | (uint) enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    #endregion

    // The debug engine (DE) sends this interface to the session debug manager (SDM) when an instance of the DE is created.
    sealed class AD7EngineCreateEvent : AD7AsynchronousEvent, IDebugEngineCreateEvent2
    {
        public const string IID = "FE5B734C-759D-4E59-AB04-F103343BDD06";
        private IDebugEngine2 _engine;

        public AD7EngineCreateEvent(DebugEngine engine)
        {
            _engine = engine;
        }

        int IDebugEngineCreateEvent2.GetEngine(out IDebugEngine2 engine)
        {
            engine = _engine;
            return VSConstants.S_OK;
        }
    }

    sealed class AD7ProgramCreateEvent : AD7AsynchronousEvent, IDebugProgramCreateEvent2
    {
        public const string IID = "96CD11EE-ECD4-4E89-957E-B5D496FC4139";
    }

    sealed class AD7ProgramDestroyEvent : AD7SynchronousEvent, IDebugProgramDestroyEvent2
    {
        public const string IID = "E147E9E3-6440-4073-A7B7-A65592C714B5";

        int IDebugProgramDestroyEvent2.GetExitCode(out uint exitCode)
        {
            exitCode = 0;
            return VSConstants.S_OK;
        }
    }

    sealed class AD7LoadCompleteEvent : AD7SynchronousEvent, IDebugLoadCompleteEvent2
    {
        public const string IID = "B1844850-1349-45D4-9F12-495212F5EB0B";
    }

    sealed class AD7EntryPointEvent : AD7SynchronousEvent, IDebugEntryPointEvent2
    {
        public const string IID = "e8414a3e-1642-48ec-829e-5f4040e16da9";
    }

    sealed class AD7BreakpointBoundEvent : AD7AsynchronousEvent, IDebugBreakpointBoundEvent2
    {
        public const string IID = "1dddb704-cf99-4b8a-b746-dabb01dd13a0";

        private PendingBreakpoint _pendingBreakpoint;
        private BoundBreakpoint _boundBreakpoint;

        public AD7BreakpointBoundEvent(PendingBreakpoint pendingBreakpoint, BoundBreakpoint boundBreakpoint)
        {
            _pendingBreakpoint = pendingBreakpoint;
            _boundBreakpoint = boundBreakpoint;
        }

        int IDebugBreakpointBoundEvent2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            ppEnum = new AD7BoundBreakpointsEnum(new IDebugBoundBreakpoint2[] { _boundBreakpoint });
            return VSConstants.S_OK;
        }

        int IDebugBreakpointBoundEvent2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBP)
        {
            ppPendingBP = _pendingBreakpoint;
            return VSConstants.S_OK;
        }
    }

    sealed class AD7BreakpointEvent : AD7StoppingEvent, IDebugBreakpointEvent2
    {
        public const string IID = "501C1E21-C557-48B8-BA30-A1EAB0BC4A74";

        IEnumDebugBoundBreakpoints2 _boundBreakpoints;

        public AD7BreakpointEvent(IEnumDebugBoundBreakpoints2 boundBreakpoints)
        {
            _boundBreakpoints = boundBreakpoints;
        }

        int IDebugBreakpointEvent2.EnumBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            ppEnum = _boundBreakpoints;
            return VSConstants.S_OK;
        }
    }

    sealed class AD7StepCompleteEvent : AD7StoppingEvent, IDebugStepCompleteEvent2
    {
        public const string IID = "0F7F24C1-74D9-4EA6-A3EA-7EDB2D81441D";
    }
          
    sealed class AD7BreakCompleteEvent : AD7StoppingEvent, IDebugBreakEvent2
    {
        public const string IID = "c7405d1d-e24b-44e0-b707-d8a5a4e1641b";
    }
}
