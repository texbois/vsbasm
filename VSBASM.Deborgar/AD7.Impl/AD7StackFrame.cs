using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;

namespace VSBASM.Deborgar
{
    class AD7StackFrame : IDebugStackFrame2
    {
        readonly AD7DocumentContext _context;
        readonly BasmRunner _runner;

        public AD7StackFrame(BasmRunner runner)
        {
            _runner = runner;
            _context = new AD7DocumentContext(_runner.ProgramFile, new TEXT_POSITION(), new TEXT_POSITION());
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

        int IDebugStackFrame2.EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout, out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum)
        {
            pcelt = 1;

            DEBUG_PROPERTY_INFO[] propInfo = new DEBUG_PROPERTY_INFO[pcelt];
            new AD7Property("acc", "Accumulator", _runner.CurrentState.acc)
                .GetPropertyInfo(dwFields, nRadix, dwTimeout, null, 1, propInfo);

            ppEnum = new AD7PropertyInfoEnum(propInfo);
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

    public class AD7Property : IDebugProperty2
    {
        private readonly string _name;
        private readonly string _fullName;
        private readonly string _value;

        public AD7Property(string name, string fullName, string value)
        {
            _name = name;
            _fullName = fullName;
            _value = value;
        }

        #region Implementation of IDebugProperty2

        public int GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
        {
            pPropertyInfo[0].dwFields = enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NONE;

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME) != 0)
            {
                pPropertyInfo[0].bstrName = _name;
                pPropertyInfo[0].bstrFullName = _fullName;
                pPropertyInfo[0].dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE) != 0)
            {
                pPropertyInfo[0].bstrValue = _value;
                pPropertyInfo[0].dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE) != 0)
            {
                pPropertyInfo[0].bstrType = "HEX";
                pPropertyInfo[0].dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB) != 0)
            {
                pPropertyInfo[0].dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
                pPropertyInfo[0].dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;
            }

            pPropertyInfo[0].pProperty = this;
            pPropertyInfo[0].dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;

            return VSConstants.S_OK;
        }

        public int SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout)
        {
            return VSConstants.E_NOTIMPL;
        }

        public int SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout)
        {
            throw new NotImplementedException();
        }

        public int EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetParent(out IDebugProperty2 ppParent)
        {
            throw new NotImplementedException();
        }

        public int GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost)
        {
            throw new NotImplementedException();
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            throw new NotImplementedException();
        }

        public int GetMemoryContext(out IDebugMemoryContext2 ppMemory)
        {
            throw new NotImplementedException();
        }

        public int GetSize(out uint pdwSize)
        {
            throw new NotImplementedException();
        }

        public int GetReference(out IDebugReference2 ppReference)
        {
            throw new NotImplementedException();
        }

        public int GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}