using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;

namespace VSBASM.Deborgar
{
    class StackFrame : IDebugStackFrame2
    {
        private readonly SourceFileContext _context;
        private readonly BasmExecutionState _execState;

        public StackFrame(SourceFileContext context, BasmExecutionState execState)
        {
            _context = context;
            _execState = execState;
        }

        public void SetFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, out FRAMEINFO frameInfo)
        {
            frameInfo = new FRAMEINFO();
            frameInfo.m_bstrFuncName = _context.FileName;
            frameInfo.m_bstrLanguage = Language.Constants.LanguageName;
            frameInfo.m_pFrame = this;
            frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FRAME
                | enum_FRAMEINFO_FLAGS.FIF_LANGUAGE
                | enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
        }

        int IDebugStackFrame2.EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, uint dwTimeout, out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum)
        {
            ppEnum = new AD7PropertyInfoEnum(new DEBUG_PROPERTY_INFO[] {
                new StackFrameRegisterProperty("Accumulator", _execState.Accumulator).GetInfo(dwFields, dwRadix)
            });
            ppEnum.GetCount(out pcelt);
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetCodeContext(out IDebugCodeContext2 ppCodeCxt)
        {
            ppCodeCxt = _context;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetDocumentContext(out IDebugDocumentContext2 ppCxt)
        {
            ppCxt = _context;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetName(out string pbstrName)
        {
            pbstrName = _context.FileName;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
        {
            SetFrameInfo(dwFieldSpec, out pFrameInfo[0]);
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetPhysicalStackRange(out ulong paddrMin, out ulong paddrMax)
        {
            paddrMin = 0;
            paddrMax = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugStackFrame2.GetExpressionContext(out IDebugExpressionContext2 ppExprCxt)
        {
            ppExprCxt = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugStackFrame2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            pbstrLanguage = Language.Constants.LanguageName;
            pguidLanguage = new Guid(Language.Constants.LanguageId);
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            ppProperty = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugStackFrame2.GetThread(out IDebugThread2 ppThread)
        {
            ppThread = null;
            return VSConstants.E_NOTIMPL;
        }
    }
}