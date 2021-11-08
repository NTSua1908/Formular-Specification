
namespace Formular_Specification
{
    partial class CompilerEnvironment
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCPP = new System.Windows.Forms.Button();
            this.txtCppPath = new System.Windows.Forms.TextBox();
            this.txtCsPath = new System.Windows.Forms.TextBox();
            this.btnCS = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(155, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "Compiler Enviroment path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(25, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(126, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "C++ Compiler MinGW";
            // 
            // btnCPP
            // 
            this.btnCPP.Location = new System.Drawing.Point(440, 81);
            this.btnCPP.Name = "btnCPP";
            this.btnCPP.Size = new System.Drawing.Size(75, 23);
            this.btnCPP.TabIndex = 2;
            this.btnCPP.Text = "Browse";
            this.btnCPP.UseVisualStyleBackColor = true;
            this.btnCPP.Click += new System.EventHandler(this.btnCPP_Click);
            // 
            // txtCppPath
            // 
            this.txtCppPath.Location = new System.Drawing.Point(170, 81);
            this.txtCppPath.Name = "txtCppPath";
            this.txtCppPath.ReadOnly = true;
            this.txtCppPath.Size = new System.Drawing.Size(253, 20);
            this.txtCppPath.TabIndex = 3;
            // 
            // txtCsPath
            // 
            this.txtCsPath.Location = new System.Drawing.Point(170, 148);
            this.txtCsPath.Name = "txtCsPath";
            this.txtCsPath.ReadOnly = true;
            this.txtCsPath.Size = new System.Drawing.Size(253, 20);
            this.txtCsPath.TabIndex = 7;
            // 
            // btnCS
            // 
            this.btnCS.Location = new System.Drawing.Point(440, 148);
            this.btnCS.Name = "btnCS";
            this.btnCS.Size = new System.Drawing.Size(75, 23);
            this.btnCS.TabIndex = 6;
            this.btnCS.Text = "Browse";
            this.btnCS.UseVisualStyleBackColor = true;
            this.btnCS.Click += new System.EventHandler(this.btnCS_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(25, 148);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "C# .NET";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 179);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(301, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Example: C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(28, 115);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(123, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Example: C:\\MinGW\\bin";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(314, 232);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(440, 232);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 11;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // CompilerEnvironment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 276);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtCsPath);
            this.Controls.Add(this.btnCS);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtCppPath);
            this.Controls.Add(this.btnCPP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "CompilerEnvironment";
            this.Text = "Environment";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCPP;
        private System.Windows.Forms.TextBox txtCppPath;
        private System.Windows.Forms.TextBox txtCsPath;
        private System.Windows.Forms.Button btnCS;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
    }
}