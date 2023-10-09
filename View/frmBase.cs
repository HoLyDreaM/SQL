using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace SQL.View
{
    public partial class frmBase : DevExpress.XtraEditors.XtraForm
    {
        private List<Control> lstControls;

        public void DoShown(Form frmTMP = null)
        {
            string strFileName;
            string strPreFileName = GetTMPFolder();

            GetAllControls(frmTMP == null ? this : frmTMP);

            foreach (Control ctrl in lstControls)
            {
                if (ctrl.GetType() == typeof(DevExpress.XtraGrid.GridControl))
                {
                    //Kolonlari olusturalim
                    DevExpress.XtraGrid.Views.Base.BaseView gvMain = (ctrl as DevExpress.XtraGrid.GridControl).Views[0];
                    gvMain.DataSourceChanged += new EventHandler(this.gvMain_DataSourceChanged);

                    for (int i = 0; i < (ctrl as DevExpress.XtraGrid.GridControl).Views.Count; i++)
                    {
                        strFileName = strPreFileName + "_" + this.Name + "_" + (ctrl as DevExpress.XtraGrid.GridControl).Views[i].Name + ".xml"; //Dosya adi FormAdi_BilesenAdi olacak. frmUretim_gvOrder gibi
                        if (System.IO.File.Exists(strFileName))
                        {
                            DevExpress.Utils.OptionsLayoutGrid olgTmp = new DevExpress.Utils.OptionsLayoutGrid();
                            olgTmp.Columns.AddNewColumns = true;
                            olgTmp.Columns.RemoveOldColumns = false;
                            (ctrl as DevExpress.XtraGrid.GridControl).Views[i].RestoreLayoutFromXml(strFileName, olgTmp);
                        }
                    }
                }
                else if (ctrl.GetType() == typeof(DevExpress.XtraEditors.SplitContainerControl))
                {
                    //TODO: Burada SharpConfig ile islem yapilmali
                    //frmTest_Layout.cfg 
                    //if (System.IO.File.Exists("SevkPlanLayout.cfg"))
                    //{
                    //    cfgLayout = SharpConfig.Configuration.LoadFromFile("SevkPlanLayout.cfg");

                    //    splitContainerControl1.SplitterPosition = cfgLayout["Layout"]["split1"].GetValue<int>();
                    //}

                }
                else
                {
                    //MessageBox.Show(ctrl.Name + " tipi=" + ctrl.GetType().FullName);
                }
            }
        }

        private void gvMain_DataSourceChanged(object sender, EventArgs e)
        {
            string strPropertyValue;
            bool blnPropertyValue;

            if (sender.GetType() == typeof(DevExpress.XtraGrid.Views.Grid.GridView))
            {
                DevExpress.XtraGrid.Views.Grid.GridView gvMain = (sender as DevExpress.XtraGrid.Views.Grid.GridView);
                foreach (GridColumn gcMain in gvMain.Columns)
                {
                    //Once caption ozelligine bakalim
                    strPropertyValue = GetGridViewProperty("Caption", gcMain.FieldName);
                    if (!string.IsNullOrEmpty(strPropertyValue))
                    {
                        gcMain.Caption = strPropertyValue;
                    }

                    //Gorunurluk ozelligini ayarlayalim
                    strPropertyValue = GetGridViewProperty("Visible", gcMain.FieldName);
                    if (!string.IsNullOrEmpty(strPropertyValue) && (bool.TryParse(strPropertyValue, out blnPropertyValue)))
                    {
                        gcMain.Visible = blnPropertyValue;
                    }

                    //Duzenlenebilirlik ozelligini ayarlayalim
                    strPropertyValue = GetGridViewProperty("AllowEdit", gcMain.FieldName);
                    if (!string.IsNullOrEmpty(strPropertyValue) && (bool.TryParse(strPropertyValue, out blnPropertyValue)))
                    {
                        gcMain.OptionsColumn.AllowEdit = blnPropertyValue;
                    }

                    //FieldName ozelligini normale cekelim. "LOGICALREF;Caption:Ref;Visible:false" ise "LOGICALREF" yapacagiz
                    if (gcMain.FieldName.Contains(';'))
                        gcMain.FieldName = gcMain.FieldName.Split(';')[0];
                }
            }
        }

        /// <summary>
        /// Istenen ozellik degerini dondurur. [Caption:TEST;Visible:FALSE] 
        /// </summary>
        /// <param name="strPropName"></param>
        /// <returns></returns>
        private string GetGridViewProperty(string strPropName, string strFieldName)
        {
            string strResult = "";

            //; isaretine gore bolup, aradigimiz degeri iceren ozellik var mi bakalim
            if (strFieldName.Contains(";"))
            {
                foreach (string strProperty in strFieldName.Split(';'))
                {
                    if (strProperty.Split(':')[0].Equals(strPropName, StringComparison.OrdinalIgnoreCase))
                    {
                        strResult = strProperty.Split(':')[1];
                        break;
                    }
                }
            }
            else
            {
                //; isareti hic olmadan tek bir ozellik verilmis olabilir
                if (strFieldName.Split(':')[0].Equals(strPropName, StringComparison.OrdinalIgnoreCase))
                {
                    strResult = strFieldName.Split(':')[1];
                }
            }

            return strResult;
        }

        public void DoFormClosing()
        {
            string strPreFileName;//Dosya formati WINDOWSAPPDATA//{KULLANICI_ADI}_{FORMADI}_{BILESENADI} olacak. TTUNALI_TURKER_frmUretim_gvOrder.xml gibi
            strPreFileName = GetTMPFolder();
            string strFileName;

            foreach (Control ctrl in lstControls)
            {
                if (ctrl.GetType() == typeof(DevExpress.XtraGrid.GridControl))
                {
                    for (int i = 0; i < (ctrl as DevExpress.XtraGrid.GridControl).Views.Count; i++)
                    {
                        strFileName = strPreFileName + "_" + this.Name + "_" + (ctrl as DevExpress.XtraGrid.GridControl).Views[i].Name + ".xml";
                        (ctrl as DevExpress.XtraGrid.GridControl).Views[i].SaveLayoutToXml(strFileName, DevExpress.Utils.OptionsLayoutBase.FullLayout);
                    }
                }
            }
        }

        public frmBase()
        {
            InitializeComponent();

            lstControls = new List<Control>();
        }

        public string GetTMPFolder()
        {
            string strResult = "";

            //strResult = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //strResult = Utils.clsSettings.TempFolder;
            strResult += "\\" + System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            strResult += "_" + System.IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath);

            foreach (char c in System.IO.Path.GetInvalidPathChars())
                strResult = strResult.Replace(c, '_');

            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                strResult = strResult.Replace(c, '_');

            //strResult = Utils.cSettings.TempFolder + "\\" + Utils.cSettings.AppName + "_" + strResult;

            return strResult;
        }

        public List<Control> GetAllControls(Control ctrlMain)
        {
            lstControls.Add(ctrlMain);
            if (ctrlMain.HasChildren)
            {
                for (int i = 0; i < ctrlMain.Controls.Count; i++)
                {
                    GetAllControls(ctrlMain.Controls[i]);
                }
            }

            return lstControls;
        }

        private void frmBase_Shown(object sender, EventArgs e)
        {
            DoShown();
        }

        private void frmBase_FormClosing(object sender, FormClosingEventArgs e)
        {
            DoFormClosing();
        }

        private void frmBase_KeyUp(object sender, KeyEventArgs e)
        {
            //TODO: Buraya F1 tuşuna basıldıysa yardım dosyası gösterme işini ekleyelim
        }
    }
}
