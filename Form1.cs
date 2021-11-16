using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace Formular_Specification
{
    public partial class Form1 : Form
    {
        #region Properties
        bool isEdited;
        bool isUndo;
        bool isRedo;

        string FunctionName;
        string LastInput;
        int LastPosition;
        int previousLength = 0;
        int BackupStart, BackupLength;
        string[] keywords = { "Post", "post", "pre", "Pre"};
        string[] variable = { "N", "R", "B", "char"};
        //string[] Calculation = { "+", "-", "*", "/", "%", ">", "<", "=", "!=", ">=", "<=", "!", "&&", "||"};

        string[] codeKeywords = { "using", "namespace", "class", "static", "if", "for", 
            "return", "void", "public", "private", "protected", "float", "int", "Int32","bool", 
            "string", "Console" };
        string[] function = { "Write", "WriteLine", "Parse", "Readline"};

        Stack<string> undo;
        Stack<int> undoCursor;

        Stack<string> redo;
        Stack<int> redoCursor;

        string JavaPath;

        enum Language { CSharp, Java };
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
            txtOutput.BackColor = txtOutput.BackColor;
            //MessageBox.Show(RemoveBracketMeaningless("((())()()((a = b)))"));
        }

        private void init()
        {
            txtOutput.Text = "";
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
            if (File.Exists(Application.StartupPath + "\\JDK.path"))
            {
                JavaPath = File.ReadAllText(Application.StartupPath + "\\JDK.path");
            } else
            {
                JavaPath = getJavaPath();
                if (!string.IsNullOrEmpty(JavaPath))
                    File.WriteAllText(Application.StartupPath + "\\JDK.path", JavaPath);
                else MessageBox.Show("Can't file Java JDK\nGo to More -> Enviroment to set JDK path",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Built Code

        private void btnBuild_Click(object sender, EventArgs e)
        {
            //Luu code vao file text

            //runcode
            if (currentLanguage == Language.Java)
                RunJava("Input"); 
            else RunCSharp("phanso");
        }

        

        void RunJava(string filename)
        {
            string text = "path =%path%;" + getJavaPath() + "@javac Input.java@java Input@pause";
            text = text.Replace("@", System.Environment.NewLine);
            File.WriteAllText(Application.StartupPath + "\\Run.bat", text);

            Process proc = null;
            try
            {
                proc = new Process();
                proc.StartInfo.FileName = Application.StartupPath + "\\Run.bat";
                proc.StartInfo.CreateNoWindow = false;
                proc.Start();
                proc.WaitForExit();
            }
            catch { }
        }

        void RunCSharp(string filename)
        {
            
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            ICodeCompiler icc = codeProvider.CreateCompiler();
            //string Output = txtExeName + ".exe";
            string Output = "Output.exe";

            string output = "";
            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
            //Make sure we generate an EXE, not a DLL
            parameters.GenerateExecutable = true;
            parameters.OutputAssembly = Output;
            CompilerResults results = icc.CompileAssemblyFromSource(parameters, File.ReadAllText(@"D:\University\Junior\HK1\Formular Specification\Formular-Specification\bin\Debug\phanso.cs"));

            if (results.Errors.Count > 0)
            {
                foreach (CompilerError CompErr in results.Errors)
                {
                    output = output +
                                "Line number " + CompErr.Line +
                                ", Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine + Environment.NewLine;
                }
                MessageBox.Show(output);
            }
            else
            {
                //Successful Compile
                Process.Start(Output);
            }
        }

        string getJavaPath()
        {
            if (!Directory.Exists(@"C:\Program Files\Java"))
                return "";

            string[] folders = Directory.GetDirectories(@"C:\Program Files\Java\",
                "*", SearchOption.AllDirectories);

            foreach (string item in folders)
            {
                if (item.Contains("jdk"))
                    return item + "\\bin";
            }

            return "";
        }

        #endregion

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(RemoveBracketMeaningless("((a = b))()"));
            //arrSentence = txtInput.Text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string content = RemoveAllSpace(txtInput.Text);
            string param = "";
            string MainInputCode = "";
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
<<<<<<< HEAD

            //Xử lý input
            int indexInput = arrSentence[0].IndexOf("(");  
            int indexOutput = arrSentence[0].IndexOf(")");
            FunctionName = content.Substring(0, indexInput);   //lấy tên hàm
            String InputVariable = arrSentence[0].Substring(indexInput + 1, indexOutput - indexInput - 1);  //lấy input
            String OutputVariable = arrSentence[0].Substring(indexOutput + 1);   //lấy output

            //Gọi hàm
            string InputFunctioncall = "";  //hàm nhập
            string FunctionCall = "";  //tham số truyền vào

            //hiển thị lên màn hình kết quả
            txtOutput.Text = "using System;\n" + "namespace FomularSpecification\n"+"{\n" +"\tpublic class "+ FunctionName+"\n\t{\n";
            txtOutput.Text += GenerateInput(InputVariable, OutputVariable, FunctionName, ref param, ref MainInputCode, ref InputFunctioncall, ref FunctionCall);

            //Đặt tên hàm
            string OutputFunctionCall = "Xuat_"+FunctionName;  //hàm xuất
            string PreFunctionCall = "Kiemtra_"+FunctionName+"("+FunctionCall;    //Hàm điều kiện
            string FunctionalCall = FunctionName+"("+FunctionCall;    //Hàm xử lý

            txtOutput.Text+= GenerateOutput(OutputVariable,FunctionName)+ GeneratePre(arrSentence[1], FunctionName, param) 
                + GenerateFunction(arrSentence[2],OutputVariable, FunctionName, param)  
                + GenerateMain(OutputVariable,MainInputCode, FunctionName,InputFunctioncall, PreFunctionCall,FunctionalCall, OutputFunctionCall);
            //GeneratePre(arrSentence[1], arrSentence[2]);
            //txtOutput.Text += FunctionExcute(arrSentence[2]);
=======
            
            ConvertInputLine(arrSentence[0]);
            txtOutput.Text += FunctionExcute(arrSentence[2]);
            HighlightAllCode();
>>>>>>> 80648667ced217d9fd0d2f3ac635b3a33437fd8a
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

        private string RemoveAllBracket(string content)
        {
            int OBracket = content.IndexOf("(");
            while (OBracket >= 0)
            {
                content = content.Remove(OBracket, 1);
                content = content.Insert(OBracket, " ");
                OBracket = content.IndexOf("(", OBracket);
            }
            int CBracket = content.IndexOf(")");
            while (CBracket >= 0)
            {
                content = content.Remove(CBracket, 1);
                content = content.Insert(CBracket, " ");
                CBracket = content.IndexOf(")", CBracket);
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

        public string GenerateInput(string InputVariable, string OutputVariable, string FunctionName, ref string param, ref string MainInputCode, ref string InputFunctionCall, ref string FunctionCall)
        {
            String[] arr = new string [4];
            String[] Input = new string[100];
            
            string Code = "";
            int type = 1;

            string temp = InputVariable;
            int iTwoDot = temp.IndexOf(":");
            int input = 0;

            while (iTwoDot >= 0) //Đếm số lượng biến đầu vào
            {
                temp = temp.Remove(iTwoDot, 1);
                iTwoDot = temp.IndexOf(":", iTwoDot);
                input++;
            }

            string content = InputVariable +","+ OutputVariable;
            int count = 0;
            int TwoDot = content.IndexOf(":");

            while (TwoDot >= 0) //Đưa biến vào mảng
            {
                //Kiểm tra có nhiều biến
                if (content.Contains(","))
                {
                    int lastindex = content.IndexOf(","); //tìm vị trí dấu 
                    Input[count] = content.Substring(0, lastindex); //lưu giá trị từ trước dấu ,
                    content = content.Remove(0, lastindex + 1); //xoá đến dấu ,
                }
                else //chỉ có 1 biến 
                { 
                    Input[count] = content.Substring(0);
                    content = content.Remove(TwoDot, 1);
                }
                TwoDot = content.IndexOf(":"); //cập nhật lại vị trí dấu :
                count++;
            }

            Array.Resize(ref Input, count);

            arr[0] = arr[1] = arr[2] = arr[3]= "";

            string HamNhap = "\t\tpublic void Nhap_"+ FunctionName+"(";
            InputFunctionCall = "Nhap_" + FunctionName + "(";
            for (int i = 0; i < input; i++)
            {
                int itwodot = Input[i].IndexOf(":");
                string itype = Input[i].Substring(itwodot);  //lấy kiểu dữ liệu sau :

                if (itype.Contains("*"))
                {
                    type = 2;
                    break;
                }
            }

            if (type == 1)
            {
                for (int i = 0; i < Input.Length; i++)
                {
                    int itwodot = Input[i].IndexOf(":");
                    string ivalue = Input[i].Substring(0, itwodot);  //lấy biến trước :
                    string itype = Input[i].Substring(itwodot);  //lấy kiểu dữ liệu sau :

                    //Biến kết quả
                    int fntwodot = Input[Input.Length - 1].IndexOf(":");
                    string fnvalue = Input[Input.Length - 1].Substring(0, fntwodot);  //lấy biến trước :
                    string fntype = Input[Input.Length - 1].Substring(fntwodot);  //lấy kiểu dữ liệu sau :

                    if (!itype.Contains("*"))
                    {
                        if (itype.Contains("N") || itype.Contains("Z"))
                        {
                            if (arr[0] == "")
                            {
                                arr[0] += "\t\t\tint " + ivalue + " = 0";
                            }
                            else
                            {
                                arr[0] += "," + ivalue + " = 0";
                            }
                            if (input > 0)
                            {
                                InputFunctionCall += "ref " + ivalue + ",";
                                FunctionCall += ivalue + ",";
                                HamNhap += "ref int " + ivalue + ",";
                                param += "int " + ivalue + ",";
                                Code += "\t\t\tConsole.WriteLine(\"Nhap vao gia tri " + ivalue + ": \");\n\t\t\t"
                                    + ivalue + "=Int32.Parse(Console.ReadLine());\n";
                            }
                        }
                        else if (itype.Contains("Q") || itype.Contains("R"))
                        {
                            if (arr[1] == "")
                            {
                                arr[1] += "\t\t\tfloat " + ivalue + " = 0";
                            }
                            else
                            {
                                arr[1] += "," + ivalue + " = 0";
                            }
                            if (input > 0)
                            {
                                InputFunctionCall += "ref " + ivalue + ",";
                                FunctionCall += ivalue + ",";
                                HamNhap += "ref float " + ivalue + ",";
                                param += "float " + ivalue + ",";
                                Code += "\t\t\tConsole.WriteLine(\"Nhap vao gia tri " + ivalue + ": \");\n\t\t\t"
                                    + ivalue + "=float.Parse(Console.ReadLine());\n";
                            }
                        }
                        else if (itype.Contains("B"))
                        {
                            if (arr[2] == "")
                            {
                                arr[2] += "\t\t\tboolean " + ivalue + " = true";
                            }
                            else
                            {
                                arr[2] += "," + ivalue + " = true";
                            }
                            if (input > 0)
                            {
                                InputFunctionCall += "ref " + ivalue + ",";
                                FunctionCall += ivalue + ",";
                                HamNhap += "ref bool " + ivalue + ",";
                                param += "bool " + ivalue + ",";
                            }
                        }
                    }
                    else if (itype.Contains("*"))
                    {
                        if (itype.Contains("N") || itype.Contains("Z"))
                        {
                            arr[3] += "\t\t\tint[] " + ivalue + " = new int [100];\n";
                        }
                        else if (itype.Contains("Q") || itype.Contains("R"))
                        {
                            arr[3] += "\t\t\tfloat[] " + ivalue + " = new float [100];\n";
                        }
                        else if (itype.Contains("char"))
                        {
                            arr[3] += "\t\t\tchar[] " + ivalue + " = new char [100];\n";
                        }
                    }


                    if (i == Input.Length - 1)
                    {
                        if (arr[0] != "") arr[0] += ";" + "\n";
                        if (arr[1] != "") arr[1] += ";" + "\n";
                        if (arr[2] != "") arr[2] += ";" + "\n";
                    }

                    input--; //input = 0
                    if (input == 0)
                    {
                        //MessageBox.Show(HamNhap);
                        HamNhap = HamNhap.Remove(HamNhap.Length - 1);
                        HamNhap += ")";

                        InputFunctionCall = InputFunctionCall.Remove(InputFunctionCall.Length - 1);
                        InputFunctionCall += ")";

                        FunctionCall = FunctionCall.Remove(FunctionCall.Length - 1);
                        FunctionCall += ")";

                        param = param.Remove(param.Length - 1);
                    }

                }
            }
            if (type == 2)
            {
                for (int i = 0; i < Input.Length-2; i++)
                {
                    //Biến hiện tại i
                    int itwodot = Input[i].IndexOf(":");
                    string ivalue = Input[i].Substring(0, itwodot);  //lấy biến trước :
                    string itype = Input[i].Substring(itwodot);  //lấy kiểu dữ liệu sau :

                    //Biến sau đó i + 1
                    int jtwodot = Input[i + 1].IndexOf(":");
                    string jvalue = Input[i + 1].Substring(0, jtwodot);  //lấy biến trước :
                    string jtype = Input[i + 1].Substring(jtwodot);  //lấy kiểu dữ liệu sau :

                    //Biến sau đó i + 2
                    int ztwodot = Input[i + 2].IndexOf(":");
                    string ztype = Input[i + 2].Substring(ztwodot);  //lấy kiểu dữ liệu sau :

                    //Biến kết quả
                    int fntwodot = Input[Input.Length - 1].IndexOf(":");
                    string fnvalue = Input[Input.Length - 1].Substring(0, fntwodot);  //lấy biến trước :
                    string fntype = Input[Input.Length - 1].Substring(fntwodot);  //lấy kiểu dữ liệu sau :

                    if (!itype.Contains("*"))
                    {
                        if (jtype.Contains("*"))
                        {
                            if (arr[0] == "")
                            {
                                arr[0] += "\t\t\tint " + ivalue + " = 0";
                            }
                            else
                            {
                                arr[0] += "," + ivalue + " = 0";
                            }

                            Code += "\t\t\tConsole.WriteLine(\"Nhap so phan tu cua mang " + jvalue + ": \");\n"
                                + "\t\t\tint " + ivalue + " = Int32.Parse.Console.ReadLine();\n";
                            if (jtype.Contains("N") || jtype.Contains("Z"))
                            {
                                arr[3] += "\t\t\tint[] "+jvalue+" = new int [100];\n";
                                Code += "\t\t\tint[] " + jvalue + " = new int[" + ivalue + "];\n"
                                + "\t\t\tfor (int i=0;i<" + ivalue + ";i++)\n" + "\t\t\t{ \n" +
                                "\t\t\t\tConsole.WriteLine(\"Nhap phan tu vao mang: \");\n" +
                                    "\t\t\t\t" + jvalue + "[i]" + "=Int32.Parse(Console.ReadLine());\n\t\t\t}\n";
                                if (input > 0)
                                {
                                    FunctionCall += ivalue + ",";
                                    InputFunctionCall += "ref " + ivalue + "," + jvalue + ",";
                                    HamNhap += "ref int " + ivalue + "," + "[] int " + jvalue + ",";
                                    param += "int " + ivalue + "," + "[] int " + jvalue + ",";
                                }
                            }
                            else if (jtype.Contains("Q") || jtype.Contains("R"))
                            {
                                arr[3] += "\t\t\tfloat[] " + jvalue + " = new float [100];\n";
                                Code += "\t\t\tfloat[] " + jvalue + " = new float[" + ivalue + "];\n"
                                    + "\t\t\tfor (int i=0;i<" + ivalue + ";i++)\n" + "\t\t\t{ \n" +
                                    "\t\t\t\tConsole.WriteLine(\"Nhap phan tu vao mang: \");\n" +
                                    "\t\t\t\t" + jvalue + "[i]" + "=float.Parse(Console.ReadLine());\n\t\t\t}\n";
                                if (input > 0)
                                {
                                    FunctionCall += ivalue + ",";
                                    InputFunctionCall += "ref " + ivalue + "," + jvalue + ",";
                                    HamNhap += "ref int " + ivalue + "," + "[] float " + jvalue + ",";
                                    param += "int " + ivalue + "," + "[] float " + jvalue + ",";
                                }
                            }

                            i++;

                            if (i == Input.Length - 2)
                            {
                                MessageBox.Show(fnvalue);
                                if (!fntype.Contains("*"))
                                {
                                    if (fntype.Contains("N") || fntype.Contains("Z"))
                                    {
                                        if (arr[0] == "")
                                        {
                                            arr[0] += "\t\tint " + fnvalue + " = 0";
                                        }
                                        else
                                        {
                                            arr[0] += "," + fnvalue + " = 0";
                                        }
                                    }
                                    else if (fntype.Contains("Q") || fntype.Contains("R"))
                                    {
                                        if (arr[1] == "")
                                        {
                                            arr[1] += "\t\t\tfloat " + fnvalue + " = 0";
                                        }
                                        else
                                        {
                                            arr[1] += "," + fnvalue + " = 0";
                                        }
                                    }
                                    else if (fntype.Contains("B"))
                                    {
                                        if (arr[2] == "")
                                        {
                                            arr[2] += "\t\t\tboolean " + fnvalue + " = true";
                                        }
                                        else
                                        {
                                            arr[2] += "," + fnvalue + " = true";
                                        }
                                    }
                                }

                                if (fntype.Contains("*"))
                                {
                                    if (fntype.Contains("N") || fntype.Contains("Z"))
                                    {
                                        arr[3] += "\t\t\tint[] " + fnvalue + " = new int [100];\n";
                                    }
                                    else if (fntype.Contains("Q") || fntype.Contains("R"))
                                    {
                                        arr[3] += "\t\t\tfloat[] " + fnvalue + " = new float [100];\n";
                                    }
                                    else if(fntype.Contains("char"))
                                    {
                                        arr[3] += "\t\t\tchar[] " + fnvalue + " = new char [100];\n";
                                    }
                                }

                                if (arr[0] != "") arr[0] += ";" + "\n";
                                if (arr[1] != "") arr[1] += ";" + "\n";
                                if (arr[2] != "") arr[2] += ";" + "\n";
                            }

                            input-=2;

                            if (input == 0)
                            {
                                HamNhap = HamNhap.Remove(HamNhap.Length - 1);
                                HamNhap += ")";

                                InputFunctionCall = InputFunctionCall.Remove(InputFunctionCall.Length - 1);
                                InputFunctionCall += ")";

                                FunctionCall = FunctionCall.Remove(FunctionCall.Length - 1);
                                FunctionCall += ")";

                                param = param.Remove(param.Length - 1);
                            }
                        }
                        if (!jtype.Contains("*"))
                        {
                            if (ztype.Contains("*"))
                            {
                                string ztemp;
                                ztemp = Input[i + 2];
                                Input[i + 2] = Input[i + 1];
                                Input[i + 1] = ztemp;
                                i--;
                            } else MessageBox.Show("Luồng nhập sai quy tắc");
                        }
                    }
                    if (itype.Contains("*"))
                    {
                        if (!jtype.Contains("*"))
                        {
                            if (arr[0] == "")
                            {
                                arr[0] += "\t\t\tint " + jvalue + " = 0";
                            }
                            else
                            {
                                arr[0] += "," + jvalue + " = 0";
                            }

                            Code += "\t\t\tConsole.WriteLine(\"Nhap so phan tu cua mang " + ivalue + ": \");\n"
                                + "\t\t\tint " + jvalue + " = Int32.Parse.Console.ReadLine();\n";
                            if (itype.Contains("N") || itype.Contains("Z"))
                            {
                                arr[3] += "\t\t\tint[] " + ivalue + " = new int [100];\n";
                                Code += "\t\t\tint[] " + ivalue + " = new int[" + jvalue + "];\n"
                                + "\t\t\tfor (int i=0;i<" + ivalue + ";i++)\n" + "\t\t\t{ \n" +
                                "\t\t\t\tConsole.WriteLine(\"Nhap phan tu vao mang: \");\n" +
                                    "\t\t\t\t" + ivalue + "[i]" + "=Int32.Parse(Console.ReadLine());\n\t\t\t}\n";
                                if (input > 0)
                                {
                                    FunctionCall += jvalue + "," + ivalue + ",";
                                    InputFunctionCall += "ref " + jvalue + "," + ivalue + ",";
                                    HamNhap += "ref int " + jvalue + "," + "[] int " + ivalue + ",";
                                    param += "int " + jvalue + "," + "[] int " + ivalue + ",";
                                }
                            }
                            else if (itype.Contains("Q") || itype.Contains("R"))
                            {
                                arr[3] += "\t\t\tfloat[] " + ivalue + " = new float [100];\n";
                                Code += "\t\t\tfloat[] " + ivalue + " = new float[" + jvalue + "];\n"
                                    + "\t\t\tfor (int i=0;i<" + jvalue + ";i++)\n" + "\t\t\t{ \n" +
                                    "\t\t\t\tConsole.WriteLine(\"Nhap phan tu vao mang: \");\n" +
                                    "\t\t\t\t" + ivalue + "[i]" + "=float.Parse(Console.ReadLine());\n\t\t\t}\n";
                                if (input > 0)
                                {
                                    FunctionCall += jvalue + "," + ivalue + ",";
                                    InputFunctionCall += "ref " + jvalue + "," + ivalue + ",";
                                    HamNhap += "ref int " + jvalue + "," + "[] float " + ivalue + ",";
                                    param += "int " + jvalue + "," + "[] float " + ivalue + ",";
                                }
                            }

                            i++;

                            if (i == Input.Length - 2)
                            {
                                if (fntype.Contains("N") || fntype.Contains("Z"))
                                {
                                    if (arr[0] == "")
                                    {
                                        arr[0] += "\t\tint " + fnvalue + " = 0";
                                    }
                                    else
                                    {
                                        arr[0] += "," + fnvalue + " = 0";
                                    }
                                }
                                else if (fntype.Contains("Q") || fntype.Contains("R"))
                                {
                                    if (arr[1] == "")
                                    {
                                        arr[1] += "\t\t\tfloat " + fnvalue + " = 0";
                                    }
                                    else
                                    {
                                        arr[1] += "," + fnvalue + " = 0";
                                    }
                                }
                                else if (fntype.Contains("B"))
                                {
                                    if (arr[2] == "")
                                    {
                                        arr[2] += "\t\t\tboolean " + fnvalue + " = true";
                                    }
                                    else
                                    {
                                        arr[2] += "," + fnvalue + " = true";
                                    }
                                }

                                if (arr[0] != "") arr[0] += ";" + "\n";
                                if (arr[1] != "") arr[1] += ";" + "\n";
                                if (arr[2] != "") arr[2] += ";" + "\n";
                            }

                            input -= 2;

                            if (input == 0)
                            {
                                HamNhap = HamNhap.Remove(HamNhap.Length - 1);
                                HamNhap += ")";

                                InputFunctionCall = InputFunctionCall.Remove(InputFunctionCall.Length - 1);
                                InputFunctionCall += ")";

                                FunctionCall = FunctionCall.Remove(FunctionCall.Length - 1);
                                FunctionCall += ")";

                                param = param.Remove(param.Length - 1);
                            }
                        }
                        if (jtype.Contains("*"))
                        {
                            if (!ztype.Contains("*"))
                            {
                                string ztemp;
                                ztemp = Input[i + 2];
                                Input[i + 2] = Input[i + 1];
                                Input[i + 1] = ztemp;
                                i--;
                            }
                            else MessageBox.Show("Luồng nhập sai quy tắc");
                        }
                    }
                }
            }

            //Code += arr[4];

            MainInputCode = arr[0] + arr[1] + arr[2] + arr[3]; 
            return HamNhap + "\n\t\t{\n" + Code + "\t\t}\n\n";
        }

        private string GenerateOutput(string OutputVariable, string FunctionName)
        {
            string code = "\t\tpublic void Xuat_"+ FunctionName;
            int twodot = OutputVariable.IndexOf(":");
            string value = OutputVariable.Substring(0, twodot);
            string type = OutputVariable.Substring(twodot);

            if (!type.Contains("*"))
            {
                if (type.Contains("N") || type.Contains("Z"))
                {
                    code += "( int " + value + " )";
                    code += "\n\t\t{\n" + "\t\t\tConsole.Write(\"Ket qua la: {0}," + value + "\");\n\t\t}\n";
                }
                if (type.Contains("R") || type.Contains("Q"))
                {
                    code += "( float " + value + " )";
                    code += "\n\t\t{\n" + "\t\t\tConsole.Write(\"Ket qua la: {0}," + value + "\");\n\t\t}\n";
                }
                if (type.Contains("B"))
                {
                    code += "( bool " + value + " )";
                    code += "\n\t\t{\n" + "\t\t\tif (" + value + ")\n\t\t\t{\n";
                    code += "\t\t\t\tConsole.Write(\"Dung\");\n\t\t\t}";
                    code += " else Console.Write(\"Sai\");" + "\n\t\t}\n";
                }
            }
            else if(type.Contains("*"))
            {
                if (type.Contains("N") || type.Contains("Z"))
                {
                    code += "( []int " + value + " )";
                    code += "\n\t\t{"+"\n\t\t\tfor(int i=0;i<" + value + ".Length;" + "i++)\n";
                    code += "\t\t\t{\n" + "\t\t\t\tConsole.WriteLine(" + value+"[i].ToString());\n\t\t\t}\n";
                    code += "\t\t}\n";
                }
                if (type.Contains("R") || type.Contains("Q"))
                {
                    code += "( []float " + value + " )";
                    code += "\n\t\t{" + "\n\t\t\tfor(int i=0;i<" + value + ".Length;" + "i++)\n";
                    code += "\t\t\t{\n" + "\t\t\t\tConsole.WriteLine(" + value + "[i].ToString());\n\t\t\t}\n";
                    code += "\t\t}\n";
                }
                if (type.Contains("char"))
                {
                    code += "( []char " + value + " )";
                    code += "\n\t\t{" + "\n\t\t\tfor(int i=0;i<" + value + ".Length;" + "i++)\n";
                    code += "\t\t\t{\n" + "\t\t\t\tConsole.WriteLine(" + value + "[i]);\n\t\t\t}\n";
                    code += "\t\t}\n";
                }
            }

            return code;
        }

        private string GeneratePre(string content, string FunctionName, string param)
        {
            string code = "\n\t\tpublic int KiemTra_"+FunctionName+"("+param+")\n"+"\t\t{\n";
            if (content != "")
            {
                content = RemoveAllBracket(content);
                code += "\t\t\tif (" + content + ")\n\t\t\t{\n" + "\t\t\t\treturn 1;\n" + "\t\t\t} else return 0;\n"+"\t\t}\n";
            }
            else code += "\t\t\treturn 1;\n"+"\t\t}\n";
            return code;
        }

        private string GenerateFunction(string content, string Output, string FunctionName, string param)
        {
            string code = "";
            int TwoDot = Output.IndexOf(":");
            string type = Output.Substring(TwoDot);

            if (!type.Contains("*"))
            {
                if (type.Contains("N") || type.Contains("Z"))
                {
                    code = "\n\t\tpublic int " + FunctionName + "(" + param + ")" + "\n\t\t{\n";
                    code += FunctionExcute(content) + "\n\t\t}\n";
                }
                else if (type.Contains("R") || type.Contains("Q"))
                {
                    code = "\n\t\tpublic float " + FunctionName + "(" + param + ")" + "\n\t\t{\n";
                    code += FunctionExcute(content) + "\n\t\t}\n";
                }
                else if (type.Contains("B"))
                {
                    code = "\n\t\tpublic bool " + FunctionName + "(" + param + ")" + "\n\t\t{\n";
                    code += FunctionExcute(content) + "\n\t\t}\n";
                }
            }

            else if (type.Contains("*"))
            {
                code = "\n\t\tpublic int " + FunctionName + "(" + param + ")" + "\n\t\t{\n";
                code += FunctionExcute(content) + "\n\t\t}\n";
            }

<<<<<<< HEAD
            return code;
        }

        private string GenerateMain(string OutputVariable,string MainInputCode, string FunctionName, string InputFunctionCall, string PreFunctionalCall, string FunctionalCall, string OutputFunctionCall)
        {
            int TwoDot = OutputVariable.IndexOf(":");
            string Value = OutputVariable.Substring(0,TwoDot);

            string code = "\n\t\tpublic static void Main (string[] args)"+ "\n\t\t{\n";
            code += MainInputCode + "\t\t\t" + FunctionName + " p = new " + FunctionName + "();\n"
                + "\t\t\tp." + InputFunctionCall + ";\n"
                + "\t\t\tif(p." + PreFunctionalCall + "==1)\n" 
                + "\t\t\t{\n" +"\t\t\t\t"+Value +"=p."+FunctionalCall+";\n"
                + "\t\t\t\tp."+OutputFunctionCall+"("+Value+");\n" + "\t\t\t}\n"
                + "\t\t\telse Console.Writeline(\"Thong tin nhap khong hop le\");\n"
                + "\n\t\t\tConsole.Readline();\n"
                + "\t\t}\n"
                + "\t}\n}";
            
            return code;
        }

=======
            //MessageBox.Show(arr[2]);
            GenerateInput(arr[2], Count);
        }
        
>>>>>>> 80648667ced217d9fd0d2f3ac635b3a33437fd8a
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
                    Func += "\t\t\tfor (int " + variable + " = " + head + "; " + variable + " >= " + tail + "; " + variable + "--)\n";
                else Func += "\t\t\tfor (int " + variable + " = " + head + "; " + variable + " <= " + tail + "; " + variable + "++)\n";

                //Chinh tab
                for (int j = 0; j < i; j++)
                {
                    Func += "\t";
                }
                Func += "\t\t\t{\n";
                for (int j = 0; j <= i; j++)
                {
                    Func += "\t\t\t\t";
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
                    Func += "\t\t";
                }
                Func += "return false;\n";
                for (int i = 0; i < lstCondition.Count-1; i++)
                {
                    Func += "\t\t\t";
                }
                if (lstCondition.Count == 3)
                    Func += "\t}\n\t}\n\t\t\t}\n\t\t\treturn true;";
                else Func += "\t}\n\t\t\t}\n\t\t\treturn true;";
            } //TT TT hoac TT
            else if (lstCondition.Count == 2 && lstCondition[0].Contains("TT")//Chi co 1 for
                || lstCondition.Count == 3 && lstCondition[0].Contains("TT") && lstCondition[1].Contains("TT")) //Co 2 for  va ca 2 deu la Ton Tai
            {
                Func += "\t\tif (" + condition + "){\n";
                for (int i = 0; i < lstCondition.Count; i++)
                {
                    Func += "\t";
                }
                Func += "\treturn true;\n";
                for (int i = 0; i < lstCondition.Count - 1; i++)
                {
                    Func += "\t";
                }
                if (lstCondition.Count == 3)
                    Func += "\t\t}\n\t\t\t}\n}\n\t\treturn false;";
                else Func += "}\n}\nreturn false;";
            }
            else if (lstCondition.Count == 3 && lstCondition[0].Contains("VM") && lstCondition[1].Contains("TT")) //Co 2 for va MV TT
            {
                Func += "\t\tif (" + condition + "){" +
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
                condition = "\t\t\tif (" + item + ")\n"
                    + "\t\t\t{\n\t\t\t\treturn " + sentenceReturn + ";\n\t\t\t}";
            else condition = "\t\t\treturn " + sentenceReturn + ";";
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
            while (index >= 0)
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

        #region
        private void HighlightAllCode()
        {
            Regex r = new Regex("\\n");
            string[] lines = r.Split(txtOutput.Text);

            int index = 0;
            foreach (string line in lines)
            {
                HighLightCode(line, index);
                index = index + line.Length + 1;
            }
        }

        private void HighLightCode(string line, int startPositon)
        {
            Regex r = new Regex("([ \\t{}();,:.><=*])");
            int index = startPositon;
            //Cat dong hien tai ra thanh cac tu rieng biet de so sanh voi keyword
            string[] words = r.Split(line);
            foreach (string word in words)
            {
                if (codeKeywords.Contains(word))
                {
                    //MessageBox.Show(word);
                    txtOutput.SelectionStart = index;
                    txtOutput.SelectionLength = word.Length;
                    txtOutput.SelectionColor = Color.Blue;
                    txtOutput.SelectionFont = new Font("Courier New", 11.25f, FontStyle.Bold);
                }
                else if (function.Contains(word))
                {
                    //MessageBox.Show(word); 
                    txtOutput.SelectionStart = index;
                    txtOutput.SelectionLength = word.Length;
                    txtOutput.SelectionColor = Color.BlueViolet;
                    txtOutput.SelectionFont = new Font("Courier New", 11.25f);
                }
                else if (txtClassName.Text.Equals(word))
                {
                    txtOutput.SelectionStart = index;
                    txtOutput.SelectionLength = word.Length;
                    txtOutput.SelectionColor = Color.Brown;
                    txtOutput.SelectionFont = new Font("Courier New", 11.25f, FontStyle.Bold);
                }

                index += word.Length;
            }
        }
        #endregion

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
            if (string.IsNullOrEmpty(JavaPath))
            {
                MessageBox.Show("Can't file Java JDK\nGo to More -> Enviroment to set JDK path",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            txtLanguage.Text = "Java";
            currentLanguage = Language.Java;
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

        private void btnBuild_Click(object sender, EventArgs e)
        {
            File.WriteAllText(FunctionName + ".cs", txtOutput.Text);

            string a = File.ReadAllText(FunctionName + ".cs");
            MessageBox.Show(a);
        }
        private void MenuAboutTeam_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        #endregion
        
    }
}
