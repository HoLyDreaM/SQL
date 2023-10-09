using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQL.Utils
{
    public enum enmErrorLevel { Warning, Error, Fatal };

    public class clsErrorInfo
    {
        public string ErrorText;
        public string ErrorOperation;
        public enmErrorLevel enErrorLevel = enmErrorLevel.Warning;
    }

    public static class cErrorInfoFactory
    {
        public static clsErrorInfo Create()
        {
            return new clsErrorInfo();
        }

        public static clsErrorInfo Create(string ErrorOperation, string ErrorText)
        {
            clsErrorInfo cErrorInfo = new clsErrorInfo();
            cErrorInfo.ErrorText = ErrorText;
            cErrorInfo.ErrorOperation = ErrorOperation;
            return cErrorInfo;
        }
    }
}
