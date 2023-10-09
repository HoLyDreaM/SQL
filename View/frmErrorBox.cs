using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SQL.View
{
    public partial class frmErrorBox : Form
    {
        public frmErrorBox()
        {
            InitializeComponent();
        }

        public frmErrorBox(string strErrorMsg)
        {
            InitializeComponent();

            this.teError.Text = strErrorMsg;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
