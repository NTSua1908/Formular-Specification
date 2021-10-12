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
        int LastPosition;

        Stack<string> undo;
        Stack<int> undoCursor;

        Stack<string> redo;
        Stack<int> redoCursor;

        enum Language { CSharp, CPlusPlus };
        Language currentLanguage = Language.CSharp;

        bool isFileOpened;
        string pathOpen; //Luu duong dan cua tep dang mo

        /// <summary>
        /// Đây sẽ là mảng có 3 phần tử;
        /// Phần tử thứ 0 lưu dòng khai báo tên hàm và các tham số;
        /// Phần tử thứ 1 lưu dòng Pre;
        /// Phần tử thứ 2 lưu dòng Post;
        /// </summary>
        string[] arrSentence;

        #region Background Worker
        BackgroundWorker worker;
        int BackupStart, BackupLength;
        List<Point> lstKeywordPos;
        bool isFormating = false;
        #endregion
        #endregion

        public Form1()
        {
            InitializeComponent();

            init();

            txtLanguage.Text = "C#";

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void init()
        {
            txtInput.Text = "";
            LastInput = "";
            LastPosition = 0;

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
            //arrSentence = txtInput.Text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string content = RemoveAllSpace(txtInput.Text);
            content = RemoveAllBreakLine(content);
            arrSentence = new string[3];

            int indexLine2 = content.IndexOf("Pre");
            if (indexLine2 < 0) 
                indexLine2 = content.IndexOf("pre"); //Tim vi tri tu Pre hoac pre
            //Den day van chua tim thay Pre hoac pre thi loi nen return
            if (indexLine2 < 0) 
                return;
            //Dong dau tien se duoc luu tai vi tri 0
            arrSentence[0] = content.Substring(0, indexLine2 - 1);
            //MessageBox.Show(arrSentence[0]);

            int indexLine3 = content.IndexOf("Post");
            if (indexLine3 < 0)
                indexLine3 = content.IndexOf("post"); //Tim vi tri tu Post hoac post
            //Den day van chua tim thay Post hoac post thi loi nen return
            if (indexLine3 < 0)
                return;
            //Dong thu 2 se duoc luu tai vi tri 1
            //indexLine2 + 3 => Lấy nội dung dòng 2 bỏ từ "Pre"
            arrSentence[1] = content.Substring(indexLine2 + 3, indexLine3 - (indexLine2 + 3));
            //MessageBox.Show(arrSentence[1]);
            //indexLine3 + 4 => Lay noi dung dong 3 bo tu "Post"
            arrSentence[2] = content.Substring(indexLine3 + 4, content.Length - (indexLine3 + 4));
            MessageBox.Show(arrSentence[2]);


        }

        private string RemoveAllBreakLine(string content)
        {
            int SpaceIndex = content.IndexOf("\n");
            while (SpaceIndex >= 0)
            {
                content = content.Remove(SpaceIndex, 1);
                SpaceIndex = content.IndexOf("\n", SpaceIndex);
            }

            return content;
        }

        private string RemoveAllSpace(string content)
        {
            int SpaceIndex = content.IndexOf(" ");
            while(SpaceIndex >= 0)
            {
                content = content.Remove(SpaceIndex, 1);
                SpaceIndex = content.IndexOf(" ", SpaceIndex);
            }

            return content;
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            
            //MessageBox.Show("Text");
            isEdited = true;
            if (isFormating)
                return;
            RichTextBox textBox = sender as RichTextBox;

            if (!isUndo && !isRedo) //Nếu không phải thao tác undo và redo thì sẽ lưu sự thay đổi lại vào stack
            {
                undo.Push(LastInput);
                undoCursor.Push(LastPosition);

                btnUndo.Enabled = true;

                if (redo.Count != 0)
                {
                    redo.Clear();
                    btnRedo.Enabled = false;
                }
                redo.Push(textBox.Text);
                redoCursor.Push(textBox.SelectionStart);
            }
            else
            {
                isUndo = false;
                isRedo = false;
            }

            //BackupStart = txtInput.SelectionStart;
            //BackupLength = txtInput.SelectionLength;

            //string content = txtInput.Text;
            //Regex breakLine = new Regex("\\n");
            //string[] lines = breakLine.Split(content);

            //Regex r = new Regex("([ \\t{}():;])");
            //string[] keywords = { "Post", "Pre", "pre", "post", "R" };
            //int index = 0;
            //foreach (string line in lines)
            //{
            //    string[] words = r.Split(line);
            //    foreach (string word in words)
            //    {
            //        txtInput.SelectionStart = index;
            //        txtInput.SelectionLength = word.Length;
            //        if (keywords.Contains(word))
            //        {
            //            //lstKeywordPos.Add(new Point(index, word.Length));
            //            txtInput.SelectionColor = Color.Blue;
            //            txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Bold);
            //        }
            //        index += word.Length;
            //    }
            //    index++;
            //}
            //txtInput.SelectionColor = Color.Black;
            //txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Regular);
            //txtInput.SelectionStart = BackupStart;
            //txtInput.SelectionLength = BackupLength;

            //MessageBox.Show("a");
            //if (worker.IsBusy)
            //    worker.CancelAsync();
            //if (!worker.IsBusy)
            //    worker.RunWorkerAsync();

            LastInput = textBox.Text;
            LastPosition = textBox.SelectionStart;

        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //MessageBox.Show("Edit");
            isFormating = true;
            txtInput.SelectionStart = 0;
            txtInput.SelectionLength = txtInput.Text.Length;
            txtInput.SelectionColor = Color.Black;
            txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Regular);
            foreach (Point item in lstKeywordPos)
            {
                txtInput.SelectionStart = item.X;
                txtInput.SelectionLength = item.Y;
                txtInput.SelectionColor = Color.Blue;
                txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Bold);
                MessageBox.Show(txtInput.SelectedText + " " + item.X + " " + item.Y);
            }
            txtInput.SelectionStart = BackupStart;
            txtInput.SelectionLength = BackupLength;
            isFormating = false;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            lstKeywordPos = new List<Point>();
            string content = txtInput.Text;
            Regex breakLine = new Regex("\\n");
            string[] lines = breakLine.Split(content);

            Regex r = new Regex("([ \\t{}():;])");
            string[] keywords = { "Post", "Pre", "pre", "post", "R" };
            int index = 0;
            foreach (string line in lines)
            {
                string[] words = r.Split(line);
                foreach (string word in words)
                {
                    if (keywords.Contains(word))
                    {
                        lstKeywordPos.Add(new Point(index, word.Length));
                    }
                    index += word.Length;
                }
                index++;
            }
        }

        void ParseLine2(string line)
        {
            // Check whether the token is a keyword.   
            String[] keywords = { "public", "void", "using", "static", "class" };
            Regex r = new Regex("([ \\t{}():;])");
            String[] tokens = r.Split(line);
            foreach (string token in tokens)
            {
                if (keywords.Contains(token))
                {
                    // Apply alternative color and font to highlight keyword.  
                    txtInput.SelectionColor = Color.Blue;
                    txtInput.SelectionFont = new Font("Courier New", 10, FontStyle.Bold);
                }

                txtInput.SelectedText = token;
            }
            txtInput.SelectedText = "\n";
        }

        void ParseLine(string line)
        {
            Regex r = new Regex("([ \\t{}():;])");
            String[] tokens = r.Split(line);
            foreach (string token in tokens)
            {
                // Set the tokens default color and font.  
                txtInput.SelectionColor = Color.Black;
                txtInput.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);
                // Check whether the token is a keyword.   
                String[] keywords = { "public", "void", "using", "static", "class" };
                for (int i = 0; i < keywords.Length; i++)
                {
                    if (keywords[i] == token)
                    {
                        // Apply alternative color and font to highlight keyword.  
                        txtInput.SelectionColor = Color.Blue;
                        txtInput.SelectionFont = new Font("Courier New", 10, FontStyle.Bold);
                        break;
                    }
                }
                txtInput.SelectedText = token;
            }
            txtInput.SelectedText = "\n";
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

        private void txtInput_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void MenuAboutTeam_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }
    }
}
