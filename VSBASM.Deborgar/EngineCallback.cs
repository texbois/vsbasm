using Microsoft.VisualStudio.Debugger.Interop;
using System;

namespace VSBASM.Deborgar
{
    class EngineCallbacks
    {
        private readonly DebugEngine _engine;
        private readonly Program _program;
        private readonly IDebugEventCallback2 _ad7Callback;
        private readonly IDebugProcess2 _process;

        public EngineCallbacks(DebugEngine engine, Program program, IDebugProcess2 process, IDebugEventCallback2 ad7Callback)
        {
            _engine = engine;
            _program = program;
            _ad7Callback = ad7Callback;
            _process = process;
        }

        public void OnAttach()
        {
            Send(new AD7EngineCreateEvent(_engine), AD7EngineCreateEvent.IID);
            Send(new AD7ProgramCreateEvent(), AD7ProgramCreateEvent.IID);
            Send(new AD7LoadCompleteEvent(), AD7LoadCompleteEvent.IID);
            Send(new AD7EntryPointEvent(), AD7EntryPointEvent.IID);
        }

        public void OnBreakpointBound(BoundBreakpoint boundBreakpoint)
        {
            IDebugPendingBreakpoint2 pendingBreakpoint;
            ((IDebugBoundBreakpoint2) boundBreakpoint).GetPendingBreakpoint(out pendingBreakpoint);

            var eventObject = new AD7BreakpointBoundEvent((PendingBreakpoint) pendingBreakpoint, boundBreakpoint);
            Send(eventObject, AD7BreakpointBoundEvent.IID);
        }

        public void OnBreakpointHit(IDebugBoundBreakpoint2 breakpoint)
        {
            var boundBreakpoints = new AD7BoundBreakpointsEnum(new IDebugBoundBreakpoint2[] { breakpoint });
            Send(new AD7BreakpointEvent(boundBreakpoints), AD7BreakpointEvent.IID);
        }

        public void OnProgramTerminated()
        {
            Send(new AD7ProgramDestroyEvent(), AD7ProgramDestroyEvent.IID);
        }

        private void Send(IDebugEvent2 eventObject, string iidEvent)
        {
            uint attributes;
            Guid riidEvent = new Guid(iidEvent);
            EngineUtils.RequireOk(eventObject.GetAttributes(out attributes));
            EngineUtils.RequireOk(_ad7Callback.Event(_engine, _process, _program, _program, eventObject, ref riidEvent, attributes));
        }
    }
}
