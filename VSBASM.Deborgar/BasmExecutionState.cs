namespace VSBASM.Deborgar
{
    struct BasmExecutionState
    {
        public uint ProgramCounter { get; }
        public uint Accumulator { get; }

        public BasmExecutionState(uint programCounter, uint accumulator = 0)
        {
            ProgramCounter = programCounter;
            Accumulator = accumulator;
        }

        public BasmExecutionState(uint newProgramCounter, BasmExecutionState oldState)
        {
            ProgramCounter = newProgramCounter;
            Accumulator = oldState.Accumulator;
        }
    }
}
