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
        int previousLength = 0;
        int BackupStart, BackupLength;
        string[] keywords = { "Post", "post", "pre", "Pre"};
        string[] variable = { "N", "R", "B", "char"};
        //string[] Calculation = { "+", "-", "*", "/", "%", ">", "<", "=", "!=", ">=", "<=", "!", "&&", "||"};

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

        #endregion

        public Form1()
        {
            InitializeComponent();

            init();

            txtLanguage.Text = "C#";
        }

        private void init()
        {
            txtOutput.Text = "";
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
            arrSentence[0] = content.Substring(0, indexLine2);
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
            //MessageBox.Show(arrSentence[0]);
            //MessageBox.Show(arrSentence[2]);
            ConvertInputLine(arrSentence[0]);

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

        void GenerateInput(string content, int input)
        {
            String[] arr = new string [6];
            String[] Input = new string[100];
            int count = 0;
            int TwoDot = content.IndexOf(":");         

            while (TwoDot >= 0) //Đưa biến vào mảng
            {
                //Kiểm tra có nhiều biến
                if (content.Contains(","))
                {
                    int lastindex = content.IndexOf(","); //tìm vị trí dấu ,
                    Input[count] = content.Substring(0, lastindex ); //lưu giá trị từ trước dấu ,
                    content = content.Remove(0, lastindex + 1); //xoá đến dấu ,
                }
                else //chỉ có 1 biến 
                { 
                    Input[count] = content.Substring(0);
                    content = content.Remove(TwoDot, 1);
                }

                TwoDot = content.IndexOf(":", TwoDot);
                count++;
            }

            Array.Resize(ref Input, count);

            arr[0] = arr[1] = arr[2] = arr[3]= arr[4]= arr[5]="";
            for (int i = 0; i < Input.Length; i++)  //Chuyển sang code
            {
                int twodot = Input[i].IndexOf(":");
                string value = Input[i].Substring(0, twodot);

                if (!Input[i].Contains("*"))
                {
                    if (Input[i].Contains("R") || Input[i].Contains("N") || Input[i].Contains("Z"))
                    {
                        if (arr[1] == "")
                        {
                            arr[1] += "int " + value ;
                        }
                        else arr[1] += "," + value;
                        if (input > 0)
                        {
                            arr[4] += "Console.Write(Vui long nhap vao gia tri " + value + ": );\n" 
                                + value + "=Int32.Parse(Console.Readline());\n";
                        }

                    }
                    else if (Input[i].Contains("Q"))
                    {
                        if (arr[2] == "")
                        {
                            arr[2] += "float " + value;
                        }
                        else arr[2] += "," + value;
                    }
                    else if (Input[i].Contains("B"))
                    {
                        if (arr[3] == "")
                        {
                            arr[3] += "boolean " + value;
                        }
                        else arr[3] += "," + value;
                    }
                    if (i == Input.Length - 1)
                    {
                        if (arr[1] != "") arr[1] += ";" + "\n";
                        if (arr[2] != "") arr[2] += ";" + "\n";
                        if (arr[3] != "") arr[3] += ";" + "\n";
                    }
                    input--;
                }
                else if (Input[i].Contains("*"))
                {
                    arr[5] += "Console.Write(Vui long nhap vao so phan tu cua mang " + value +": );\n"
                        +"int n = Int32.Parse.Console.Readline();\n";
                    if (Input[i].Contains("R") || Input[i].Contains("N") || Input[i].Contains("Z"))
                    {
                        arr[5] += "int[] " + value + " = new int[" + "n" + "];\n"
                        + "for (int i=0;i<n;i++)\n" + "{ \n" + "\tConsole.Write(Nhap phan tu vao mang: );\n" +
                            "\t" + value + "[i]" + "=Int32.Parse(Console.Readline());\n}\n";
                    }
                    else if (Input[i].Contains("Q"))
                    {
                        arr[5] += "float[] " + value + " = new float[" + "n" + "];\n"
                            + "for (int i=0;i<n;i++)\n" + "{ \n" + "\tConsole.Write(Nhap phan tu vao mang: );\n" +
                            "\t" + value + "[i]" + "=float.Parse(Console.Readline());\n}\n";
                    }
                    input--;
                }
            }

            arr[0] = "using System;\nusing System.Collections.Generic;\nusing System.ComponentModel;\n" +
                "using System.Data;\nusing System.Drawing;\nusing System.IO;\nusing System.Linq;\nusing System.Text;\n" +
                "using System.Text.RegularExpressions;\nusing System.Threading.Tasks;\nusing System.Windows.Forms;\n";

            txtOutput.Text = arr[0] + arr[1] + arr[2] + arr[3] + arr[4] + arr[5];
        }

        private void ConvertInputLine(string content)
        {
            RemoveAllSpace(content);
            String[] arr = new string[3];
            int indexInput = content.IndexOf("(");
            int indexOutput = content.IndexOf(")");

            arr[0] = content.Substring(indexInput+1, indexOutput - indexInput - 1); //input
            arr[1] = content.Substring(indexOutput+1); //ouput
            arr[2] = arr[0] + "," + arr[1];

            int TwoDot = arr[1].IndexOf(":");
            int Count = 0;

            while (TwoDot >= 0) //Đếm số lượng biến đầu vào
            {
                Count++;
                arr[0] = arr[0].Remove(TwoDot, 1);
                TwoDot = arr[0].IndexOf(":", TwoDot);
            }

            MessageBox.Show(arr[2]);
            GenerateInput(arr[2],Count);
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            //Xu ly highlight

            //Luu lai vi tri con tro hien tai
            BackupLength = txtInput.SelectionLength;
            BackupStart = txtInput.SelectionStart;

            if (Math.Abs(txtInput.Text.Length - previousLength) > 1 || isRedo || isUndo) 
                HighlightAllLine();
            else HighlightCurrentWord(); //type keyboard :))
            previousLength = txtInput.Text.Length;

            //Tra moi thu ve nhu cu
            txtInput.SelectionStart = BackupStart;
            txtInput.SelectionLength = BackupLength;
            txtInput.SelectionColor = Color.Black;
            txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Regular);


            isEdited = true;

            //if (previousLength == txtInput.Text.Length)
            //   return;

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
                redo.Push(txtInput.Text);
                redoCursor.Push(txtInput.SelectionStart);
            }
            else
            {
                isUndo = false;
                isRedo = false;
            }

            LastInput = txtInput.Text;
            LastPosition = txtInput.SelectionStart;
        }

        private void HighlightAllLine()
        {
            Regex r = new Regex("\\n");
            string[] lines = r.Split(txtInput.Text);

            int index = 0;
            foreach (string line in lines)
            {
                HighLight(line, index);
                index = index + line.Length + 1;
            }
        }

        private void HighLight(string line, int startPositon)
        {
            Regex r = new Regex("([ \\t{}();,:*])");
            int index = startPositon;
            //Cat dong hien tai ra thanh cac tu rieng biet de so sanh voi keyword
            string[] words = r.Split(line);
            foreach (string word in words)
            {
                txtInput.SelectionStart = index;
                txtInput.SelectionLength = word.Length;
 
                if (keywords.Contains(word))
                {
                    //MessageBox.Show(word);
                    txtInput.SelectionColor = Color.Blue;
                    txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Bold);
                }
                else if (variable.Contains(word))
                {
                    txtInput.SelectionColor = Color.Red;
                    txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Bold);
                }

                index += word.Length;
            }
        }

        private void HighlightCurrentWord()
        {
            //Tìm vị trí bắt đầu của tu đang xét
            int StartIndex = txtInput.SelectionStart - 1;
            while (StartIndex >= 0)
            {
                if (txtInput.Text[StartIndex] == '\n' || !char.IsLetter(txtInput.Text[StartIndex]))
                {
                    StartIndex++;
                    break;
                }
                StartIndex--;
            }
            if (StartIndex < 0)
                StartIndex = 0;

            //Tìm vị trí cuối dòng của tu đang xét
            int EndIndex = txtInput.SelectionStart;
            while (EndIndex < txtInput.Text.Length)
            {
                if (txtInput.Text[EndIndex] == '\n' || !char.IsLetter(txtInput.Text[EndIndex]))
                    break;
                EndIndex++;
            }


            //lay tu hien tai
            string currentWord = txtInput.Text.Substring(StartIndex, EndIndex - StartIndex);
            //MessageBox.Show(currentWord);

            txtInput.SelectionStart = StartIndex;
            txtInput.SelectionLength = currentWord.Length;
            if (keywords.Contains(currentWord))
            {
                
                txtInput.SelectionColor = Color.Blue;
                txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Bold);
            } 
            else if (variable.Contains(currentWord))
            {
                txtInput.SelectionColor = Color.Red;
                txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Bold);
            }
            else
            {
                txtInput.SelectionColor = Color.Black;
                txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Regular);
            }
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
