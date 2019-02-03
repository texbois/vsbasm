using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.IO;

namespace VSBASM.Deborgar
{
    class SourceFileContext : IDebugDocumentContext2, IDebugCodeContext2
    {
        private readonly string _filePath;
        private TEXT_POSITION _startPosition;
        private TEXT_POSITION _endPosition;

        public string FileName { get; private set; }

        public SourceFileContext(string filePath, TEXT_POSITION begPos, TEXT_POSITION endPos)
        {
            _filePath = filePath;
            _startPosition = begPos;
            _endPosition = endPos;

            FileName = Path.GetFileName(filePath);
        }

        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            pbstrLanguage = Language.Constants.LanguageName;
            pguidLanguage = new Guid(Language.Constants.LanguageId);
            return VSConstants.S_OK;
        }

        #region IDebugDocumentContext2 Members

        int IDebugDocumentContext2.Compare(enum_DOCCONTEXT_COMPARE Compare, IDebugDocumentContext2[] rgpDocContextSet, uint dwDocContextSetLen, out uint pdwDocContext)
        {
            dwDocContextSetLen = 0;
            pdwDocContext = 0;
            return VSConstants.E_NOTIMPL;
        }

        // Retrieves a list of all code contexts associated with this document context.
        // The engine only supports one code context per document context.
        int IDebugDocumentContext2.EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts)
        {
            ppEnumCodeCxts = new AD7DebugCodeContextEnum(new IDebugCodeContext2[] { this });
            return VSConstants.S_OK;
        }

        int IDebugDocumentContext2.GetDocument(out IDebugDocument2 ppDocument)
        {
            ppDocument = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugDocumentContext2.GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName)
        {
            pbstrFileName = _filePath;
            return VSConstants.S_OK;
        }

        // The source range is typically used for mixing source statements with code in the disassembly window.
        int IDebugDocumentContext2.GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            throw new NotImplementedException();
        }

        int IDebugDocumentContext2.GetStatementRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            pBegPosition[0].dwColumn = _startPosition.dwColumn;
            pBegPosition[0].dwLine = _startPosition.dwLine;

            pEndPosition[0].dwColumn = _endPosition.dwColumn;
            pEndPosition[0].dwLine = _endPosition.dwLine;

            return VSConstants.S_OK;
        }

        // Moves the document context by a given number of statements or lines.
        // This is used primarily to support the Autos window in discovering the proximity statements around this document context. 
        int IDebugDocumentContext2.Seek(int nCount, out IDebugDocumentContext2 ppDocContext)
        {
            ppDocContext = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IDebugCodeContext2 Members

        public int GetName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        public int GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo)
        {
            pinfo[0].dwFields = 0;

            // Fields not supported by the engine
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS) != 0) { }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSOFFSET) != 0) { }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSABSOLUTE) != 0) { }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL) != 0) { }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION) != 0) { }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTIONOFFSET) != 0) { }

            return VSConstants.S_OK;
        }

        public int Add(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Compare(enum_CONTEXT_COMPARE Compare, IDebugMemoryContext2[] rgpMemoryContextSet, uint dwMemoryContextSetLen, out uint pdwMemoryContext)
        {
            pdwMemoryContext = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetDocumentContext(out IDebugDocumentContext2 ppSrcCxt)
        {
            ppSrcCxt = this;
            return VSConstants.S_OK;
        }

        #endregion
    }
}
