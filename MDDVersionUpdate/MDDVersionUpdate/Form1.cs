using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDDVersionUpdate
{

    public delegate void DelAddLine(string line);
    public delegate void DelUpdateBar(int value);

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!CheckValues())
            {
                txtOutput.Text += "\nParametreler Bos Gecilemez";
            }
            else
            {
                txtOutput.Clear();
                progBar.Value = 0;
                DelAddLine del = AddLineToOutput;
                DelUpdateBar updDel = UpdateProgressBar;

                Main main = new Main(txtPath.Text, txtRootPath.Text, txtLogPath.Text, txtIntegrationPath.Text, del, updDel);
                main.Upgrade();
            }
        }

        private bool CheckValues()
        {         
            return true;
        }

        #region GUI_FUNCTIONS

        private void AddLineToOutput(string line)
        {
            this.txtOutput.Text += "\n" + line;
        }

        private void UpdateProgressBar(int val)
        {
            this.progBar.Value = val;
        }

        #endregion

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            this.txtOutput.SelectionStart = this.txtOutput.Text.Length;
            this.txtOutput.ScrollToCaret();
        }
    }
}
