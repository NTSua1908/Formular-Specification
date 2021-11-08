using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Formular_Specification
{
    public partial class CompilerEnvironment : Form
    {
        public CompilerEnvironment()
        {
            InitializeComponent();

            if (File.Exists(Application.StartupPath + "\\Path.pat"))
            {
                string[] path = File.ReadAllLines(Application.StartupPath + "\\Path.pat");
                txtCppPath.Text = path[0];
                txtCsPath.Text = path[1];
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            File.WriteAllText(Application.StartupPath +"\\Path.pat",txtCppPath.Text + "\n" + txtCsPath.Text);
        }

        private void btnCPP_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtCppPath.Text = dialog.SelectedPath;
            }
        }

        private void btnCS_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtCsPath.Text = dialog.SelectedPath;
            }
        }
    }
}
