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

            //MessageBox.Show(RemoveBracketMeaningless("((())()()((a = b)))"));
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
            //MessageBox.Show(RemoveBracketMeaningless("((a = b))()"));
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
            //MessageBox.Show(arrSentence[2]);

            //MessageBox.Show(FunctionExcute(arrSentence[2]));
            FunctionExcute(arrSentence[2]);
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

        private string FunctionExcute(string content)
        {
            string Func = "";
            List<string> lstCondition = new List<string>();
            content = RemoveBracketMeaningless(content);

            while (content.Length != 0 )
            {
                //Sum = 0 => Number of bracket open and close are equal
                //Sum < 0 => Number of bracket open more than close
                //Sum > 0 => Number of bracket open less than close
                int sum = 0;
                int index = 0;
                for (index = 0; index < content.Length; index++)
                {
                    if (content[index] == '(')
                        sum += -1;
                    else if (content[index] == ')')
                        sum += 1;
                    else if (sum == 0 && content[index] == '|')
                        break;
                }

                lstCondition.Add(content.Substring(0, index));
                if (index == content.Length)
                    content = "";
                else content = content.Remove(0, index + 2); //2 is length of '||'
            }

            //string s = "";
            //foreach (string item in lstCondition)
            //{
            //    s += item + "\n";
            //}
            //MessageBox.Show(s);

            //Xử lí tách từng điều kiện và giá trị trả về
            for (int i = 0; i < lstCondition.Count; i++)
            {
                Func += CreateConditionCode(lstCondition[i]);
                if (i != lstCondition.Count - 1)
                    Func += "\n";
            }

            txtOutput.Text = Func;

            return Func;
        }

        /// <summary>
        /// Tiến hành tách phần giá trị trả về điều kiện, tạo câu if hoàn chỉnh
        /// </summary>
        /// <param name="item">Chuỗi chứa giá trị trả về và điều kiện</param>
        /// <returns></returns>
        private string CreateConditionCode(string item)
        {
            string condition = "";
            //Tìm vị trí mà tại đó gán giá trị trả về
            int ValueIndex = item.IndexOf("=");
            //Cô mặc định đầu tiên luôn là giá trị trả về :))
            //Nhưng đang rảnh làm cho thêm trường hợp
            while (ValueIndex > 0 && ValueIndex < item.Length - 1 && (
                !char.IsLetter(item[ValueIndex - 1]) && !char.IsDigit(item[ValueIndex - 1]) || //Kí tự trước đó phải là chữ hoặc số, không là dấu câu
                item[ValueIndex + 1] == '=')) //Nếu kí tự sau là dấu = thì kết hợp ra == (phép so sánh) => loại
            {
                ValueIndex = item.IndexOf("=", ValueIndex + 1);
            }
            if (ValueIndex < 1) //Error
                return "";

            //Tìm vị trí bắt đầu của phần gán giá trị trả về 
            int startIndex = ValueIndex;
            while (startIndex >= 0)
            {
                if (item[startIndex] == '&' || item[startIndex] == '(')
                {
                    startIndex++;
                    break;
                }
                else startIndex--; //Nếu chưa tìm được vị trí bắt đầu thì lùi lại tiếp
            }
            if (startIndex < 0)
                startIndex = 0;

            //Tìm vị trí kết thúc của phần gán giá trị trả về
            int endIndex = ValueIndex;
            while (endIndex < item.Length)
            {
                if (item[endIndex] == '&' || item[endIndex] == ')')
                {
                    endIndex--;
                    break;
                }
                else endIndex++; //Nếu chưa tìm được vị trí bắt đầu thì tăng tiếp
            }
            if (endIndex == item.Length)
                endIndex = item.Length - 1;

            string sentenceReturn = item.Substring(startIndex, endIndex - startIndex + 1);

            //Remove Example (kq=a)&&(a > 1) => (a > 1)
            int andIndex = item.IndexOf("&&");
            item = item.Remove(startIndex, endIndex - startIndex + 1);
            if (andIndex != -1)
            {
                if (andIndex < ValueIndex)
                {
                    andIndex = item.LastIndexOf("&&");
                    item = item.Remove(andIndex, 2); //Remove '&&'
                } else
                {
                    andIndex = item.IndexOf("&&");
                    item = item.Remove(andIndex, 2); //Remove '&&'
                }
            }
            //item = item.Remove(startIndex, endIndex - startIndex + 1);
            //item = item.Remove(andIndex, 2); //Remove '&&'
            item = RemoveBracketMeaningless(item);

            condition = "if (" + item + ")\n"
                + "{\n\t" + sentenceReturn + ";\n}";
            return condition;
        }

        /// <summary>
        /// Remove meaningless bracket like this '()' 
        /// Example: '(()&&(a = b))' => 'a = b'
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string RemoveBracketMeaningless(string content)
        {
            //Remove bracket '()'
            int index = content.IndexOf("()");
            while (index > 0)
            {
                content = content.Remove(index, 2);
                index = content.IndexOf("()");
            }

            //Remove bracket head and tail
            //'((a = b))' => 'a = b'
            bool flag = true;
            while(flag && content.Length > 1)
            {
                //Sum = 0 => Number of bracket open and close are equal
                //Sum < 0 => Number of bracket open more than close
                //Sum > 0 => Number of bracket open less than close
                int sum = 0;
                for (int i = 0; i < content.Length; i++)
                {
                    if (content[i] == '(')
                        sum += -1;
                    else if (content[i] == ')')
                        sum += 1;
                    if (sum == 0 && i != content.Length - 1) //Do not have head and tail redundant bracket
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    content = content.Substring(1, content.Length - 2);
                }
            }

            return content;
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
                //redo.Push(txtInput.Text);
                //redoCursor.Push(txtInput.SelectionStart);
            }
            else
            {
                isUndo = false;
                isRedo = false;
            }

            LastInput = txtInput.Text;
            LastPosition = txtInput.SelectionStart;
        }

        #region Syntax Highlight

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
                
 
                if (keywords.Contains(word))
                {
                    //MessageBox.Show(word);
                    txtInput.SelectionStart = index;
                    txtInput.SelectionLength = word.Length;
                    txtInput.SelectionColor = Color.Blue;
                    txtInput.SelectionFont = new Font("Courier New", 12, FontStyle.Bold);
                }
                else if (variable.Contains(word))
                {
                    txtInput.SelectionStart = index;
                    txtInput.SelectionLength = word.Length;
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

        #endregion

        #region Menu Function

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
            btnUndo.Enabled = true;

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

        #endregion
    }
}
