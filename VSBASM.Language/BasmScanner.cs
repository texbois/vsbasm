using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VSBASM.Language
{
    class BasmScanner : IScanner
    {
        private IVsTextBuffer _buffer;
        private string _source;

        public BasmScanner(IVsTextBuffer buffer)
        {
            _buffer = buffer;
        }

        bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            tokenInfo.Type = TokenType.Unknown;
            tokenInfo.Color = TokenColor.Keyword;
            return true;
        }

        void IScanner.SetSource(string source, int offset)
        {
            _source = source.Substring(offset);
        }
    }
}
