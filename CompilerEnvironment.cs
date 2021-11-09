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

            if (File.Exists(Application.StartupPath + "\\JDK.path"))
            {
                txtJavaPath.Text = File.ReadAllText(Application.StartupPath + "\\JDK.path");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            File.WriteAllText(Application.StartupPath + "\\JDK.path", txtJavaPath.Text);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(dialog.SelectedPath+"\\javac.exe"))
                {
                    txtJavaPath.Text = dialog.SelectedPath;
                }
                else {
                    MessageBox.Show("Đường dẫn không hợp lệ", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }
    }
}
