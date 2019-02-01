using System.Collections.Generic;

namespace Microsoft.VisualStudio.Debugger.SampleEngine
{
    class BasmBreakpointBackend
    {
        Dictionary<uint, string> mAddressToMemoryContents = new Dictionary<uint, string>();
        BasmRunner mRunner;

        public BasmBreakpointBackend(BasmRunner runner)
        {
            mRunner = runner;
        }

        public void Unset(AD7BreakpointResolution resolution)
        {
            mRunner.SetContents(resolution.Address(), mAddressToMemoryContents[resolution.Address()]);
        }

        public void Set(AD7BreakpointResolution resolution)
        {
            string contents = mRunner.GetContents(resolution.Address());
            mAddressToMemoryContents.Add(resolution.Address(), contents);
            mRunner.SetContents(resolution.Address(), "F000");
        }
    }
}
