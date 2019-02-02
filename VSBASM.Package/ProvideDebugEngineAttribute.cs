using Microsoft.VisualStudio.Shell;
using System;
using System.IO;

namespace VSBASM.Package
{
    // https://github.com/Microsoft/PTVS/blob/1d04f01b7b902a9e1051b4080770b4a27e6e97e7/Common/Product/SharedProject/ProvideDebugEngineAttribute.cs
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    class ProvideDebugEngineAttribute : RegistrationAttribute
    {
        private readonly string _id, _name;
        private readonly Type _debugEngine;

        public ProvideDebugEngineAttribute(string name, Type debugEngine, string id)
        {
            _name = name;
            _debugEngine = debugEngine;
            _id = id;
        }

        public override void Register(RegistrationContext context)
        {
            var engineKey = context.CreateKey("AD7Metrics\\Engine\\" + _id);
            engineKey.SetValue("Name", _name);

            engineKey.SetValue("CLSID", _debugEngine.GUID.ToString("B"));
            engineKey.SetValue("PortSupplier", "{708C1ECA-FF48-11D2-904F-00C04FA302A1}");

            engineKey.SetValue("Attach", 1);
            engineKey.SetValue("AddressBP", 0);
            engineKey.SetValue("AutoSelectPriority", 6);
            engineKey.SetValue("CallstackBP", 0);
            engineKey.SetValue("ConditionalBP", 0);
            engineKey.SetValue("DataBP", 0);
            engineKey.SetValue("Exceptions", 0);
            engineKey.SetValue("SetNextStatement", 0);
            engineKey.SetValue("RemoteDebugging", 0);
            engineKey.SetValue("HitCountBP", 0);
            engineKey.SetValue("JustMyCodeStepping", 0);

            engineKey.SetValue("EngineClass", _debugEngine.FullName);
            engineKey.SetValue("EngineAssembly", _debugEngine.Assembly.FullName);

            engineKey.SetValue("LoadProgramProviderUnderWOW64", 1);
            engineKey.SetValue("AlwaysLoadProgramProviderLocal", 1);
            engineKey.SetValue("LoadUnderWOW64", 1);

            var clsidKey = context.CreateKey("CLSID");
            var clsidGuidKey = clsidKey.CreateSubkey(_debugEngine.GUID.ToString("B"));
            clsidGuidKey.SetValue("Assembly", _debugEngine.Assembly.FullName);
            clsidGuidKey.SetValue("Class", _debugEngine.FullName);
            clsidGuidKey.SetValue("InprocServer32", context.InprocServerPath);
            clsidGuidKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, _debugEngine.Module.Name));
            clsidGuidKey.SetValue("ThreadingModel", "Free");
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}
