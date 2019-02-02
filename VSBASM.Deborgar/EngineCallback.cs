using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Diagnostics;

namespace VSBASM.Deborgar
{
    class EngineCallbacks
    {
        private readonly AD7Engine _engine;
        private readonly DE.BasmProgram _program;
        private readonly BreakpointManager _breakpointManager;
        private readonly IDebugEventCallback2 _ad7Callback;
        private readonly IDebugProcess2 _process;

        public EngineCallbacks(AD7Engine engine, DE.BasmProgram program, BreakpointManager bpManager, IDebugEventCallback2 ad7Callback, IDebugProcess2 process)
        {
            _engine = engine;
            _program = program;
            _breakpointManager = bpManager;
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

        public void OnProgramStop(uint address)
        {
            IDebugBoundBreakpoint2 breakpoint = _breakpointManager.MaybeGetBreakpoint(address);
            if (breakpoint != null)
            {
                var boundBreakpoints = new AD7BoundBreakpointsEnum(new IDebugBoundBreakpoint2[] { breakpoint });
                Send(new AD7BreakpointEvent(boundBreakpoints), AD7BreakpointEvent.IID);
            }
            else
            {
                Debug.WriteLine("OnProgramStop: non-synthetic HLT instruction reached, terminating.");
                Send(new AD7ProgramDestroyEvent(), AD7ProgramDestroyEvent.IID);
            }
        }

        public void OnBreakpointBound(AD7BoundBreakpoint boundBreakpoint)
        {
            IDebugPendingBreakpoint2 pendingBreakpoint;
            ((IDebugBoundBreakpoint2) boundBreakpoint).GetPendingBreakpoint(out pendingBreakpoint);

            var eventObject = new AD7BreakpointBoundEvent((AD7PendingBreakpoint) pendingBreakpoint, boundBreakpoint);
            Send(eventObject, AD7BreakpointBoundEvent.IID);
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
