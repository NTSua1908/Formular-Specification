using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Formular_Specification
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            //txtAbout.Text = "ST Group\n"
            //    + "Thành viên :\n"
            //    + "Nguyễn Thiện Sua 19522144"
            //    + "Mai Long Thành   19522232"
            //    + "Phần mềm này là đồ án môn học Đặc tả hình thức của giảng viên "
            //    + "Thái Thụy Hàn Uyển";
        }

        private void lbLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/NTSua1908/Formular-Specification");
        }
    }
}
