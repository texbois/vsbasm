using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using VSBASM.Deborgar;
using VSBASM.Language;

namespace VSBASM.Package
{
    [ProvideDebugEngine(DebugEngine.DebugEngineName, typeof(DebugEngine), DebugEngine.DebugEngineId)]
    [ProvideDebugLanguage(Constants.LanguageName, Constants.LanguageId, DebugEngine.DebugEngineId)]
    [ProvideService(typeof(BasmLanguageService), ServiceName = "BasmLanguageService")]
    [ProvideLanguageService(typeof(BasmLanguageService),
        Constants.LanguageName,
        106,
        CodeSense = true,
        RequestStockColors = true,
        EnableCommenting = true,
        EnableAsyncCompletion = true)]
    [ProvideLanguageExtension(typeof(BasmLanguageService), Constants.LanguageExtension)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(Guids.PackageId)]
    public sealed class VSPackage : Microsoft.VisualStudio.Shell.Package
    {
        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}
