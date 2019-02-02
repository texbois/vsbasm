using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace VSBASM.Language
{
    public class BasmLanguageService : LanguageService
    {
        private LanguagePreferences _langPreferences;
        private BasmScanner _scanner;

        public override string Name => Constants.LanguageName;

        public override string GetFormatFilterList()
        {
            throw new NotImplementedException();
        }

        public override LanguagePreferences GetLanguagePreferences()
        {
            if (_langPreferences == null)
            {
                _langPreferences = new LanguagePreferences(Site, new Guid(Constants.LanguageId), Name);
                _langPreferences.Init();
            }
            return _langPreferences;
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            if (_scanner == null)
            {
                _scanner = new BasmScanner(buffer);
            }
            return _scanner;
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
