using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

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

        string CSharpPath, CppPath;

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
            ReadPath();

            //MessageBox.Show(RemoveBracketMeaningless("((())()()((a = b)))"));
        }

        private void init()
        {
            txtInput.Text = "";
            LastInput = "";
            LastPosition = 0;

            //AllocConsole();
            //Console.WriteLine("abc");

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

        private void ReadPath()
        {
            if (File.Exists(Application.StartupPath + "\\Path.pat"))
            {
                string[] path = File.ReadAllLines(Application.StartupPath + "\\Path.pat");
                CppPath = path[0];
                CSharpPath = path[1];
            } else
            {
                CSharpPath = getCSharpPath();
                CppPath = "C:\\MinGW\\bin";
                File.WriteAllText(Application.StartupPath + "\\Path.pat", CppPath + "\n" + CSharpPath);
            }
        }

        #region Built Code

        private void btnBuild_Click(object sender, EventArgs e)
        {
            //Luu code vao file text

            //runcode
            //RunCSharp("phanso");
            RunCPP("Input");
            //RunCPP("HelloWorld");
            //RunJava("HelloWorld");
            //RunJava("Input");

            //RunMyJavaApp("-i=\"test.txt\" -o=\"test_output.txt\"");
            //SW.Close();
        }


        void RunCPP(string filename)
        {
            using (CmdService cmdService = new CmdService("cmd.exe"))
            {
                string consoleCommand, output;

                consoleCommand = "path=%path%;" + CppPath;
                cmdService.ExecuteCommand(consoleCommand);
                consoleCommand = "g++ " + filename + ".cpp -o " + filename;
                cmdService.ExecuteCommand(consoleCommand);
                consoleCommand = filename + ".exe";
                string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                Process.Start(path + "\\" + consoleCommand);
            }
        }

        void RunCSharp(string filename)
        {
            CmdService cmdService = new CmdService("cmd.exe");

            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string consoleCommand, output;

            consoleCommand = "path=%path%;" + CSharpPath + "abc";
            output = cmdService.ExecuteCommand(consoleCommand);
            //Console.WriteLine(output);
            consoleCommand = "csc " + filename + ".cs";
            output = cmdService.ExecuteCommand(consoleCommand);
            //Console.WriteLine(output + "\n>>>");

            consoleCommand = filename + ".exe";

            Process.Start(path + "\\" + consoleCommand);
            //Process process = Process.Start(path + "\\" + consoleCommand);
            //int id = process.Id;
            //Process tempProc = Process.GetProcessById(id);
            //this.Visible = false;
            //tempProc.WaitForExit();
            //this.Visible = true;

            //CmdService cmdService2 = new CmdService("cmd.exe");
            //output = cmdService.ExecuteCommand(consoleCommand);
            //Console.WriteLine(output);

        }

        private string getCSharpPath()
        {
            if (!Directory.Exists(@"C:\Windows\Microsoft.NET\Framework"))
                return "";

            string[] folders = Directory.GetDirectories(@"C:\Windows\Microsoft.NET\Framework",
                "v*", SearchOption.AllDirectories);

            if (folders.Length > 0)
                return folders[folders.Length - 1];

            return "";
        }
        #endregion

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
            txtOutput.Text = FunctionExcute(arrSentence[2]);
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


            if (content.Contains("}."))
                return ForFunction(content);
            return IfFunction(content);
            
        }

        /// <summary>
        /// Type 1
        /// </summary>
        /// <param name="content">Condition</param>
        /// <returns></returns>
        private string IfFunction(string content)
        {
            string Func = "";
            List<string> lstCondition = new List<string>();
            content = RemoveBracketMeaningless(content);

            //cắt các điều kiện trong câu ra
            while (content.Length != 0)
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
        /// Type 2
        /// </summary>
        /// <param name="content">Condition</param>
        /// <returns></returns>
        private string ForFunction(string content)
        {
            string Func = "";
            List<string> lstCondition = new List<string>();
            List<string> lstVariable = new List<string>();

            int index = content.IndexOf("=");
            content = content.Remove(0, index + 1);
            content = RemoveBracketMeaningless(content);

            index = content.IndexOf("}.");
            while (index > 0)
            {
                lstCondition.Add(content.Substring(0, index + 1));
                content = content.Remove(0, index + 2); //2 is length of '}.'
                index = content.IndexOf("}.");
            }
            lstCondition.Add(content);

            //string s = "";
            //foreach (string item in lstCondition)
            //{
            //    s += item + "\n";
            //}
            //MessageBox.Show(s);

            //Tim ten bien do dai
            index = arrSentence[0].LastIndexOf(")");
            index = arrSentence[0].LastIndexOf(":", index);
            int startIndex = arrSentence[0].LastIndexOf(",", index) + 1;
            string length = arrSentence[0].Substring(startIndex, index - startIndex);
            //Tim bien mang
            index = arrSentence[0].LastIndexOf(":", startIndex);
            startIndex = arrSentence[0].IndexOf("(") + 1;
            string arr = arrSentence[0].Substring(startIndex, index - startIndex);

            //Luu tru diem ket thuc cua vong for cuoi
            string tail = "";

            //Tao for
            for (int i = 0; i < lstCondition.Count - 1; i++)
            {
                index = lstCondition[i].IndexOf("TH");
                //Lay ten bien (i, j,...)
                string variable = lstCondition[i].Substring(2, index - 2);
                lstVariable.Add(variable);

                //Tim phan dau va cuoi cua for
                index = lstCondition[i].IndexOf("..");
                int bracket = lstCondition[i].IndexOf("{") + 1; //Vi tri sau dau ngoac {
                string head = lstCondition[i].Substring(bracket, index - bracket); //Phan dau cua for
                bracket = lstCondition[i].IndexOf("}"); //Vi tri dau ngoac }
                tail = lstCondition[i].Substring(index + 2, bracket - (index + 2)); //Phan duoi cua for //2 is length of '..'

                

                if (head.Contains(length))
                    Func += "for (int " + variable + " = " + head + "; " + variable + " >= " + tail + "; " + variable + "--)\n";
                else Func += "for (int " + variable + " = " + head + "; " + variable + " <= " + tail + "; " + variable + "++)\n";

                //Chinh tab
                for (int j = 0; j < i; j++)
                {
                    Func += "\t";
                }
                Func += "{\n";
                for (int j = 0; j <= i; j++)
                {
                    Func += "\t";
                }
            }
            string condition = RemoveBracketMeaningless(lstCondition[lstCondition.Count - 1]);

            //Thay the 'a(i)' => 'a[i]'
            index = condition.IndexOf(arr + "(");
            while (index != -1)
            {
                condition = condition.Remove(index + arr.Length, 1);
                condition = condition.Insert(index + arr.Length, "[");

                int bracketCount = -1; //Thieu mot dau ngoac dong tu vi tri hien tai
                for (int i = index + 2; i < condition.Length; i++) //2 is length of 'a['
                {
                    if (condition[i] == '(')
                        bracketCount--;
                    else if (condition[i] == ')')
                        bracketCount++;
                    if (bracketCount == 0)
                    {
                        condition = condition.Remove(i, 1);
                        condition = condition.Insert(i, "]");
                        break;
                    }
                }
                index = condition.IndexOf(arr + "(");
            }

            condition = insertEqual(condition);

            // VM VM hoac VM
            if (lstCondition.Count == 2 && lstCondition[0].Contains("VM")//Chi co 1 for
                || lstCondition.Count == 3 && lstCondition[0].Contains("VM") && lstCondition[1].Contains("VM")) //Co 2 for  va ca 2 deu la Voi Moi
            {
                Func += "if (!" + condition + "){\n";
                for (int i = 0; i < lstCondition.Count; i++)
                {
                    Func += "\t";
                }
                Func += "return false;\n";
                for (int i = 0; i < lstCondition.Count-1; i++)
                {
                    Func += "\t";
                }
                if (lstCondition.Count == 3)
                    Func += "}\n\t}\n}\nreturn true;";
                else Func += "}\n}\nreturn true;";
            } //TT TT hoac TT
            else if (lstCondition.Count == 2 && lstCondition[0].Contains("TT")//Chi co 1 for
                || lstCondition.Count == 3 && lstCondition[0].Contains("TT") && lstCondition[1].Contains("TT")) //Co 2 for  va ca 2 deu la Ton Tai
            {
                Func += "if (" + condition + "){\n";
                for (int i = 0; i < lstCondition.Count; i++)
                {
                    Func += "\t";
                }
                Func += "return true;\n";
                for (int i = 0; i < lstCondition.Count - 1; i++)
                {
                    Func += "\t";
                }
                if (lstCondition.Count == 3)
                    Func += "}\n\t}\n}\nreturn false;";
                else Func += "}\n}\nreturn false;";
            }
            else if (lstCondition.Count == 3 && lstCondition[0].Contains("VM") && lstCondition[1].Contains("TT")) //Co 2 for va MV TT
            {
                Func += "if (" + condition + "){" +
                    "\n\t\t\treturn break;" +
                    "\n\t\tif (" + lstVariable[lstVariable.Count - 1] +" == " + tail +"){" +
                    "\n\t\t\treturn false;" +
                    "\n\t\t}\n\t" +
                    "}\n" +
                    "}\n" +
                    "return true;";
            }
            else if (lstCondition.Count == 3 && lstCondition[0].Contains("TT") && lstCondition[1].Contains("VM")) //Co 2 for va TT MV
            {
                Func += "if (!" + condition + "){" +
                    "\n\t\t\treturn break;" +
                    "\n\t\tif (" + lstVariable[lstVariable.Count - 1] + " == " + tail + "){" +
                    "\n\t\t\treturn true;" +
                    "\n\t\t}\n\t" +
                    "}\n" +
                    "}\n" +
                    "return false;";
            }

            return Func;
        }


        

        /// <summary>
        /// Tiến hành tách phần giá trị trả về điều kiện, tạo câu if hoàn chỉnh (Type 1)
        /// </summary>
        /// <param name="item">Chuỗi chứa giá trị trả về và điều kiện</param>
        /// <returns></returns>
        private string CreateConditionCode(string item)
        {
            string condition = "";
            //Tìm vị trí mà tại đó gán giá trị trả về
            int resultStart = arrSentence[0].LastIndexOf(")");
            int resultEnd = arrSentence[0].LastIndexOf(":");
            int valueIndex = item.IndexOf(arrSentence[0].Substring(resultStart + 1, resultEnd - resultStart - 1));
            int startIndex = item.IndexOf("=", valueIndex) + 1;

            //Tìm vị trí kết thúc của phần gán giá trị trả về
            int endIndex = startIndex;
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
            item = item.Remove(valueIndex, endIndex - valueIndex + 1);
            if (andIndex != -1)
            {
                if (andIndex < startIndex)
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
            item = insertEqual(item);

            if (!string.IsNullOrEmpty(item))
                condition = "if (" + item + ")\n"
                    + "{\n\treturn " + sentenceReturn + ";\n}";
            else condition = "return " + sentenceReturn + ";";
            return condition;
        }

        /// <summary>
        /// Insert '=' into condition only '='; 
        /// Example: 'a = b' => 'a == b'
        /// </summary>
        /// <param name="condition">Sentence condition</param>
        /// <returns>Condition affter insert '='</returns>
        private string insertEqual(string condition)
        {
            int index = condition.IndexOf("=");
            while (index > 0 && index < condition.Length - 1)
            {
                //if ((char.IsDigit(condition[index - 1]) || char.IsLetter(condition[index - 1]) || condition[index - 1] == '_') && condition[index + 1] != '=')
                if (condition[index - 1] != '>' && condition[index - 1] != '<' && condition[index - 1] != '!'
                    && condition[index - 1] != '=' && condition[index + 1] != '=')
                {
                    condition = condition.Insert(index, "=");
                    index++;
                }

                index = condition.IndexOf("=", index + 1);
            }

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

        private void btnEnviroment_Click(object sender, EventArgs e)
        {
            CompilerEnvironment environment = new CompilerEnvironment();
            environment.ShowDialog();
            ReadPath();
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
