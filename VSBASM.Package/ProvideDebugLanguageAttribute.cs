using Microsoft.VisualStudio.Shell;

namespace VSBASM.Package
{
    class ProvideDebugLanguageAttribute : RegistrationAttribute
    {
        private readonly string _languageGuid, _languageName, _engineGuid;

        public ProvideDebugLanguageAttribute(string languageName, string languageGuid, string debugEngineGuid)
        {
            _languageName = languageName;
            _languageGuid = languageGuid;
            _engineGuid = debugEngineGuid;
        }

        public override void Register(RegistrationContext context)
        {
            var langSvcKey = context.CreateKey("Languages\\Language Services\\" + _languageName + "\\Debugger Languages\\" + _languageGuid);
            langSvcKey.SetValue("", _languageName);
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}
