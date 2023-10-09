using System;
using System.Data;
using System.ComponentModel;
using SQL.Utils;

namespace SQL.Utils
{
    public static class cUtils
    {
        private static Version _applicationVersion;
        private static string strSorgu;
        public static void ShowError(string strError, bool blnDialog = true)
        {
            View.frmErrorBox fErrorBox = new View.frmErrorBox(strError);
            if (blnDialog)
                fErrorBox.ShowDialog();
            else
                fErrorBox.Show();
        }
        public static void ShowError(clsErrorInfo cErrorInfo, bool blnDialog = true)
        {
            View.frmErrorBox fErrorBox = new View.frmErrorBox("Operasyon Adı:" + cErrorInfo.ErrorOperation + Environment.NewLine + "Hata Detayı:" + cErrorInfo.ErrorText);
            if (blnDialog)
                fErrorBox.ShowDialog();
            else
                fErrorBox.Show();
        }
        public static string GetAppVersion()
        {
            //VS icindeyken formlarda hata aliyorduk onun icin tasarim asamasinda kodun calismasi engellendi.Not: System.ComponentMode.DesignMode neden calismadi bilmiyorum.
            if (LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                _applicationVersion = new Version();
            }
            else
            {
                _applicationVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            }

            string strResult = _applicationVersion.Major.ToString("00") + _applicationVersion.Minor.ToString("00") + _applicationVersion.Build.ToString("00") + _applicationVersion.Revision.ToString("00");

            return strResult;
        }
    }
}
