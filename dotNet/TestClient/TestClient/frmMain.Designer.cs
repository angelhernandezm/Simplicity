namespace TestClient {
	partial class frmMain {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabManual = new System.Windows.Forms.TabPage();
			this.tabRandom = new System.Windows.Forms.TabPage();
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtOperand1 = new System.Windows.Forms.TextBox();
			this.txtOperand2 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cboOperation = new System.Windows.Forms.ComboBox();
			this.btnManualCalculate = new System.Windows.Forms.Button();
			this.btnRandomCalculate = new System.Windows.Forms.Button();
			this.txtOutputRandom = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtOperationCount = new System.Windows.Forms.TextBox();
			this.chkRunInParallel = new System.Windows.Forms.CheckBox();
			this.tabControl1.SuspendLayout();
			this.tabManual.SuspendLayout();
			this.tabRandom.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabManual);
			this.tabControl1.Controls.Add(this.tabRandom);
			this.tabControl1.Location = new System.Drawing.Point(12, 12);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(433, 246);
			this.tabControl1.TabIndex = 0;
			// 
			// tabManual
			// 
			this.tabManual.Controls.Add(this.btnManualCalculate);
			this.tabManual.Controls.Add(this.cboOperation);
			this.tabManual.Controls.Add(this.txtOperand2);
			this.tabManual.Controls.Add(this.label2);
			this.tabManual.Controls.Add(this.txtOperand1);
			this.tabManual.Controls.Add(this.label1);
			this.tabManual.Controls.Add(this.txtOutput);
			this.tabManual.Location = new System.Drawing.Point(4, 22);
			this.tabManual.Name = "tabManual";
			this.tabManual.Padding = new System.Windows.Forms.Padding(3);
			this.tabManual.Size = new System.Drawing.Size(425, 220);
			this.tabManual.TabIndex = 0;
			this.tabManual.Text = "Manual";
			this.tabManual.UseVisualStyleBackColor = true;
			// 
			// tabRandom
			// 
			this.tabRandom.Controls.Add(this.chkRunInParallel);
			this.tabRandom.Controls.Add(this.txtOperationCount);
			this.tabRandom.Controls.Add(this.label3);
			this.tabRandom.Controls.Add(this.btnRandomCalculate);
			this.tabRandom.Controls.Add(this.txtOutputRandom);
			this.tabRandom.Location = new System.Drawing.Point(4, 22);
			this.tabRandom.Name = "tabRandom";
			this.tabRandom.Padding = new System.Windows.Forms.Padding(3);
			this.tabRandom.Size = new System.Drawing.Size(425, 220);
			this.tabRandom.TabIndex = 1;
			this.tabRandom.Text = "Random";
			this.tabRandom.UseVisualStyleBackColor = true;
			// 
			// txtOutput
			// 
			this.txtOutput.Location = new System.Drawing.Point(9, 101);
			this.txtOutput.Multiline = true;
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ReadOnly = true;
			this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtOutput.Size = new System.Drawing.Size(404, 106);
			this.txtOutput.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(17, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(60, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Operand 1:";
			// 
			// txtOperand1
			// 
			this.txtOperand1.Location = new System.Drawing.Point(82, 19);
			this.txtOperand1.Name = "txtOperand1";
			this.txtOperand1.Size = new System.Drawing.Size(100, 20);
			this.txtOperand1.TabIndex = 2;
			// 
			// txtOperand2
			// 
			this.txtOperand2.Location = new System.Drawing.Point(82, 45);
			this.txtOperand2.Name = "txtOperand2";
			this.txtOperand2.Size = new System.Drawing.Size(100, 20);
			this.txtOperand2.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(17, 49);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Operand 2:";
			// 
			// cboOperation
			// 
			this.cboOperation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboOperation.FormattingEnabled = true;
			this.cboOperation.Items.AddRange(new object[] {
            "Sum",
            "Subtract",
            "Multiply",
            "Divide"});
			this.cboOperation.Location = new System.Drawing.Point(255, 17);
			this.cboOperation.Name = "cboOperation";
			this.cboOperation.Size = new System.Drawing.Size(140, 21);
			this.cboOperation.TabIndex = 5;
			// 
			// btnManualCalculate
			// 
			this.btnManualCalculate.Location = new System.Drawing.Point(286, 49);
			this.btnManualCalculate.Name = "btnManualCalculate";
			this.btnManualCalculate.Size = new System.Drawing.Size(75, 23);
			this.btnManualCalculate.TabIndex = 6;
			this.btnManualCalculate.Text = "&Calculate";
			this.btnManualCalculate.UseVisualStyleBackColor = true;
			this.btnManualCalculate.Click += new System.EventHandler(this.btnManualCalculate_Click);
			// 
			// btnRandomCalculate
			// 
			this.btnRandomCalculate.Location = new System.Drawing.Point(304, 31);
			this.btnRandomCalculate.Name = "btnRandomCalculate";
			this.btnRandomCalculate.Size = new System.Drawing.Size(75, 23);
			this.btnRandomCalculate.TabIndex = 8;
			this.btnRandomCalculate.Text = "&Calculate";
			this.btnRandomCalculate.UseVisualStyleBackColor = true;
			this.btnRandomCalculate.Click += new System.EventHandler(this.btnRandomCalculate_Click);
			// 
			// txtOutputRandom
			// 
			this.txtOutputRandom.Location = new System.Drawing.Point(10, 83);
			this.txtOutputRandom.Multiline = true;
			this.txtOutputRandom.Name = "txtOutputRandom";
			this.txtOutputRandom.ReadOnly = true;
			this.txtOutputRandom.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtOutputRandom.Size = new System.Drawing.Size(404, 106);
			this.txtOutputRandom.TabIndex = 7;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(10, 21);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(114, 13);
			this.label3.TabIndex = 9;
			this.label3.Text = "Number of operations: ";
			// 
			// txtOperationCount
			// 
			this.txtOperationCount.Location = new System.Drawing.Point(130, 18);
			this.txtOperationCount.Name = "txtOperationCount";
			this.txtOperationCount.Size = new System.Drawing.Size(78, 20);
			this.txtOperationCount.TabIndex = 10;
			this.txtOperationCount.Text = "100";
			// 
			// chkRunInParallel
			// 
			this.chkRunInParallel.AutoSize = true;
			this.chkRunInParallel.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.chkRunInParallel.Location = new System.Drawing.Point(13, 48);
			this.chkRunInParallel.Name = "chkRunInParallel";
			this.chkRunInParallel.Size = new System.Drawing.Size(99, 17);
			this.chkRunInParallel.TabIndex = 11;
			this.chkRunInParallel.Text = "Run in parallel?";
			this.chkRunInParallel.UseVisualStyleBackColor = true;
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(457, 270);
			this.Controls.Add(this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.Text = "Simplicity Test Client";
			this.tabControl1.ResumeLayout(false);
			this.tabManual.ResumeLayout(false);
			this.tabManual.PerformLayout();
			this.tabRandom.ResumeLayout(false);
			this.tabRandom.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabManual;
		private System.Windows.Forms.TabPage tabRandom;
		private System.Windows.Forms.TextBox txtOutput;
		private System.Windows.Forms.TextBox txtOperand2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtOperand1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnManualCalculate;
		private System.Windows.Forms.ComboBox cboOperation;
		private System.Windows.Forms.CheckBox chkRunInParallel;
		private System.Windows.Forms.TextBox txtOperationCount;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnRandomCalculate;
		private System.Windows.Forms.TextBox txtOutputRandom;
	}
}

