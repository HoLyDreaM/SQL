using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Diagnostics;
using SmartFormat;

namespace SM_Lib.Utils
{
    public static class cExtensions
    {
        public static string ConvertDataTableToString(this System.Data.DataTable dtMain)
        {
            string strResult;
            StringBuilder sbMain = new StringBuilder();
            foreach (DataRow drMain in dtMain.Rows)
            {
                foreach (DataColumn dcMain in dtMain.Columns)
                {
                    sbMain.AppendFormat("{0}:{1} ", dcMain.ColumnName, drMain[dcMain]);
                }
            }
            sbMain.Append(Environment.NewLine);

            strResult = sbMain.ToString();

            return strResult;
        }

        public static void ConvertDataTableToCSV(this System.Data.DataTable dtMain, string FileName, char chSeperator = ';', bool blnShowColumnName = false, bool blnAppendFile = false)
        {
            StringBuilder sbMain = new StringBuilder();

            if (blnShowColumnName)
            {
                IEnumerable<string> columnNames = dtMain.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName);
                sbMain.AppendLine(string.Join(chSeperator.ToString(), columnNames));
            }

            foreach (DataRow row in dtMain.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field =>
                    string.Concat("", field.ToString().Replace("\"", "\"\""), ""));
                //string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                sbMain.AppendLine(string.Join(chSeperator.ToString(), fields));
            }

            if (blnAppendFile)
                System.IO.File.AppendAllText(FileName, sbMain.ToString());
            else
                System.IO.File.WriteAllText(FileName, sbMain.ToString());
        }

        public static StringBuilder ConvertDataTableToStringBuilder(this System.Data.DataTable dtMain, char chSeperator = ';', bool blnShowColumnName = false)
        {
            StringBuilder sbResult = new StringBuilder();

            if (blnShowColumnName)
            {
                IEnumerable<string> columnNames = dtMain.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName);
                sbResult.AppendLine(string.Join(chSeperator.ToString(), columnNames));
            }

            foreach (DataRow row in dtMain.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field =>
                    string.Concat("", field.ToString().Replace("\"", "\"\""), ""));
                //string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                sbResult.AppendLine(string.Join(chSeperator.ToString(), fields));
            }

            return sbResult;
        }

        [DebuggerStepThrough]
        public static double ToDouble(this object objMain)
        {
            double flResult;

            if (!Double.TryParse(objMain.ToString(), out flResult))
            {
                flResult = 0;
            }

            return flResult;
        }

        [DebuggerStepThrough]
        public static int ToInt(this object objMain)
        {
            int dResult;

            if (!Int32.TryParse(objMain == null ? "" : objMain.ToString(), out dResult))
            {
                dResult = 0;
            }

            return dResult;
        }

        [DebuggerStepThrough]
        public static DateTime ToDateTime(this object objMain)
        {
            DateTime dtResult;

            if (!DateTime.TryParse(objMain.ToString(), out dtResult))
            {
                dtResult = new DateTime();
            }

            return dtResult;
        }

        [DebuggerStepThrough]
        public static int ToInt(this string strMain)
        {
            int dResult;

            if (!int.TryParse(strMain, out dResult))
            {
                dResult = 0;
            }

            return dResult;
        }

        public static object NullToValue(this object objMain, Type typMain)
        {
            object objResult;

            if (objMain == null)
            {
                if (typMain == typeof(string))
                {
                    objResult = "";
                }
                else if (typMain == typeof(int))
                {
                    objResult = 0;
                }
                else if (typMain == typeof(double))
                {
                    objResult = 0.0f;
                }
                else if (typMain == typeof(bool))
                {
                    objResult = false;
                }
                else
                {
                    //System.Windows.Forms.MessageBox.Show("Tanımlanmamış çevrim tipi! Uygulama hata verebilir.");
                    objResult = null;
                }
            }
            else
            {
                if (typMain == typeof(bool))
                {
                    //TODO:Bu kismi cok anlamadim bence TRUE, FALSE donmeli
                    if (objMain.ToString() == "0")
                        objMain = 0;
                    else if (objMain.ToString() == "1")
                        objMain = 1;
                }
                if (typMain == typeof(int))
                {
                    if (objMain.ToString() == "")
                        objMain = 0;
                }

                objResult = objMain;
            }

            return objResult;
        }
        [DebuggerStepThrough]
        public static string Format(this string format, params object[] args)
        {
            return Smart.Format(format, args);
        }
    }
}
