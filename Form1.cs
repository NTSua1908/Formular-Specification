using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Formular_Specification
{
    public partial class Form1 : Form
    {
        #region Properties
        bool isEdited;
        bool isUndo;
        bool isRedo;

        string LastInput;
        int LastPosion;

        Stack<string> undo;
        Stack<int> undoCursor;

        Stack<string> redo;
        Stack<int> redoCursor;

        enum Language { CSharp, CPlusPlus };
        Language currentLanguage = Language.CSharp;

        bool isFileOpened;
        string pathOpen; //Luu duong dan cua tep dang mo

        string[] arrSentence;

        #endregion

        public Form1()
        {
            InitializeComponent();

            init();

            txtLanguage.Text = "C#";
        }

        private void init()
        {
            txtInput.Text = "";
            LastInput = "";
            LastPosion = 0;

            undo = new Stack<string>();
            undoCursor = new Stack<int>();

            redo = new Stack<string>();
            redoCursor = new Stack<int>();

            btnUndo.Enabled = false;
            btnRedo.Enabled = false;

            isEdited = false;
            isUndo = false;
            isRedo = false;
            isFileOpened = false;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            arrSentence = txtInput.Text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //for (int i = 0; i < arrSentence.Length; i++)
            //{
            //    MessageBox.Show("|" + arrSentence[i] + "|");
            //}
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            isEdited = true;
            TextBox textBox = sender as TextBox;

            if (!isUndo && !isRedo) //Nếu không phải thao tác undo và redo thì sẽ lưu sự thay đổi lại vào stack
            {   
                undo.Push(LastInput);
                undoCursor.Push(LastPosion);

                btnUndo.Enabled = true;

                if (redo.Count != 0)
                {
                    redo.Clear();
                    btnRedo.Enabled = false;
                }
                redo.Push(textBox.Text);
                redoCursor.Push(textBox.SelectionStart);
            } else
            {
                isUndo = false;
                isRedo = false;
            }

            LastInput = textBox.Text;
            LastPosion = textBox.SelectionStart;
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            if (undo.Count == 0)
                return;

            string content = undo.Pop();
            int position = undoCursor.Pop();

            isUndo = true;
            btnRedo.Enabled = true;

            redo.Push(txtInput.Text);
            redoCursor.Push(txtInput.SelectionStart);

            txtInput.Text = content;
            txtInput.SelectionStart = position;

            if (undo.Count == 0)
            {
                btnUndo.Enabled = false;
            }
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            if (redo.Count == 0)
                return;

            string content = redo.Pop();
            int position = redoCursor.Pop();
            isRedo = true;

            undo.Push(txtInput.Text);
            undoCursor.Push(txtInput.SelectionStart);

            txtInput.Text = content;
            txtInput.SelectionStart = position;

            if (redo.Count == 0)
            {
                btnRedo.Enabled = false;
            }
        }

        private void btnCSharp_Click(object sender, EventArgs e)
        {
            txtLanguage.Text = "C#";
            currentLanguage = Language.CSharp;
        }

        private void btnCPlusPlus_Click(object sender, EventArgs e)
        {
            txtLanguage.Text = "C++";
            currentLanguage = Language.CPlusPlus;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            if (isEdited)
            {
                DialogResult result = MessageBox.Show("Bạn có muốn lưu lại chỉnh sửa", "Thông báo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes) //Neu chon yes thi luu
                    SaveFile();
                if (result != DialogResult.Cancel) //Ngoài chọn cancel ra thì chọn các lựa chọn còn lại đều phải tiến hành reset
                    init();
            }
            else init();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Formular Language(.stg)|*.stg";
            open.RestoreDirectory = true;

            if (open.ShowDialog() == DialogResult.OK)
            {
                pathOpen = open.FileName;
                isFileOpened = true;
                txtInput.Text =  File.ReadAllText(pathOpen);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void MenuSaveAs_Click(object sender, EventArgs e)
        {
            SaveAsFile();
        }

        private void SaveFile()
        {
            if (isFileOpened)
            {
                File.WriteAllText(pathOpen, txtInput.Text);
                isEdited = false;    
            }
            else SaveAsFile();
        }

        private void SaveAsFile()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Formular Language(.stg)|*.stg";
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pathOpen = dialog.FileName;
                File.WriteAllText(pathOpen, txtInput.Text);
                isFileOpened = true;
                isEdited = false;
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            txtInput.Copy();
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            txtInput.Paste();
        }

        private void btnCut_Click(object sender, EventArgs e)
        {
            txtInput.Cut();
        }

        private void MenuAboutTeam_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }
    }
}
