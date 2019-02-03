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
    }
}
