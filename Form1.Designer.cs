namespace CPU6502
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            label1 = new Label();
            lblSP = new Label();
            label3 = new Label();
            lblN = new Label();
            lblO = new Label();
            lblC = new Label();
            lblZ = new Label();
            lblI = new Label();
            lblD = new Label();
            lblB = new Label();
            lblY = new Label();
            label10 = new Label();
            lblX = new Label();
            label12 = new Label();
            lblA = new Label();
            label14 = new Label();
            lblOpCode = new Label();
            lblPC = new TextBox();
            SetPC = new Button();
            txtLoadAt = new TextBox();
            btnLoad = new Button();
            label2 = new Label();
            openFileDialog1 = new OpenFileDialog();
            panel1 = new Panel();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(192, 255, 192);
            button1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            button1.Location = new Point(6, 6);
            button1.Margin = new Padding(2, 1, 2, 1);
            button1.Name = "button1";
            button1.Size = new Size(101, 30);
            button1.TabIndex = 0;
            button1.Text = "STEP";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(111, 13);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(25, 15);
            label1.TabIndex = 1;
            label1.Text = "PC:";
            // 
            // lblSP
            // 
            lblSP.AutoSize = true;
            lblSP.BorderStyle = BorderStyle.FixedSingle;
            lblSP.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblSP.Location = new Point(331, 35);
            lblSP.Margin = new Padding(2, 0, 2, 0);
            lblSP.Name = "lblSP";
            lblSP.Size = new Size(35, 17);
            lblSP.TabIndex = 4;
            lblSP.Text = "0xFF";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(246, 35);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(73, 15);
            label3.TabIndex = 3;
            label3.Text = "SP: 0x0100 +";
            // 
            // lblN
            // 
            lblN.AutoSize = true;
            lblN.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblN.Location = new Point(486, 13);
            lblN.Margin = new Padding(2, 0, 2, 0);
            lblN.Name = "lblN";
            lblN.Size = new Size(16, 15);
            lblN.TabIndex = 5;
            lblN.Text = "N";
            // 
            // lblO
            // 
            lblO.AutoSize = true;
            lblO.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblO.Location = new Point(507, 13);
            lblO.Margin = new Padding(2, 0, 2, 0);
            lblO.Name = "lblO";
            lblO.Size = new Size(16, 15);
            lblO.TabIndex = 6;
            lblO.Text = "O";
            // 
            // lblC
            // 
            lblC.AutoSize = true;
            lblC.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblC.Location = new Point(601, 13);
            lblC.Margin = new Padding(2, 0, 2, 0);
            lblC.Name = "lblC";
            lblC.Size = new Size(14, 15);
            lblC.TabIndex = 7;
            lblC.Text = "C";
            // 
            // lblZ
            // 
            lblZ.AutoSize = true;
            lblZ.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblZ.Location = new Point(581, 13);
            lblZ.Margin = new Padding(2, 0, 2, 0);
            lblZ.Name = "lblZ";
            lblZ.Size = new Size(14, 15);
            lblZ.TabIndex = 8;
            lblZ.Text = "Z";
            // 
            // lblI
            // 
            lblI.AutoSize = true;
            lblI.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblI.Location = new Point(566, 13);
            lblI.Margin = new Padding(2, 0, 2, 0);
            lblI.Name = "lblI";
            lblI.Size = new Size(11, 15);
            lblI.TabIndex = 9;
            lblI.Text = "I";
            // 
            // lblD
            // 
            lblD.AutoSize = true;
            lblD.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblD.Location = new Point(546, 13);
            lblD.Margin = new Padding(2, 0, 2, 0);
            lblD.Name = "lblD";
            lblD.Size = new Size(16, 15);
            lblD.TabIndex = 10;
            lblD.Text = "D";
            // 
            // lblB
            // 
            lblB.AutoSize = true;
            lblB.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblB.Location = new Point(528, 13);
            lblB.Margin = new Padding(2, 0, 2, 0);
            lblB.Name = "lblB";
            lblB.Size = new Size(15, 15);
            lblB.TabIndex = 11;
            lblB.Text = "B";
            // 
            // lblY
            // 
            lblY.AutoSize = true;
            lblY.BorderStyle = BorderStyle.FixedSingle;
            lblY.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblY.Location = new Point(218, 54);
            lblY.Margin = new Padding(2, 0, 2, 0);
            lblY.Name = "lblY";
            lblY.Size = new Size(37, 17);
            lblY.TabIndex = 13;
            lblY.Text = "0x00";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(198, 54);
            label10.Margin = new Padding(2, 0, 2, 0);
            label10.Name = "label10";
            label10.Size = new Size(17, 15);
            label10.TabIndex = 12;
            label10.Text = "Y:";
            // 
            // lblX
            // 
            lblX.AutoSize = true;
            lblX.BorderStyle = BorderStyle.FixedSingle;
            lblX.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblX.Location = new Point(139, 54);
            lblX.Margin = new Padding(2, 0, 2, 0);
            lblX.Name = "lblX";
            lblX.Size = new Size(37, 17);
            lblX.TabIndex = 15;
            lblX.Text = "0x00";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(111, 54);
            label12.Margin = new Padding(2, 0, 2, 0);
            label12.Name = "label12";
            label12.Size = new Size(17, 15);
            label12.TabIndex = 14;
            label12.Text = "X:";
            // 
            // lblA
            // 
            lblA.AutoSize = true;
            lblA.BorderStyle = BorderStyle.FixedSingle;
            lblA.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblA.Location = new Point(139, 35);
            lblA.Margin = new Padding(2, 0, 2, 0);
            lblA.Name = "lblA";
            lblA.Size = new Size(37, 17);
            lblA.TabIndex = 17;
            lblA.Text = "0x00";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(111, 35);
            label14.Margin = new Padding(2, 0, 2, 0);
            label14.Name = "label14";
            label14.Size = new Size(24, 15);
            label14.TabIndex = 16;
            label14.Text = "Ac:";
            // 
            // lblOpCode
            // 
            lblOpCode.AutoSize = true;
            lblOpCode.BorderStyle = BorderStyle.FixedSingle;
            lblOpCode.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblOpCode.Location = new Point(246, 14);
            lblOpCode.Margin = new Padding(2, 0, 2, 0);
            lblOpCode.Name = "lblOpCode";
            lblOpCode.Size = new Size(77, 17);
            lblOpCode.TabIndex = 18;
            lblOpCode.Text = "JMP 0x1234";
            // 
            // lblPC
            // 
            lblPC.Location = new Point(136, 12);
            lblPC.Margin = new Padding(2, 1, 2, 1);
            lblPC.Name = "lblPC";
            lblPC.Size = new Size(70, 23);
            lblPC.TabIndex = 19;
            // 
            // SetPC
            // 
            SetPC.BackColor = Color.FromArgb(192, 255, 192);
            SetPC.Location = new Point(207, 10);
            SetPC.Margin = new Padding(2, 1, 2, 1);
            SetPC.Name = "SetPC";
            SetPC.Size = new Size(35, 23);
            SetPC.TabIndex = 20;
            SetPC.Text = "Set";
            SetPC.UseVisualStyleBackColor = false;
            SetPC.Click += SetPC_Click;
            // 
            // txtLoadAt
            // 
            txtLoadAt.Location = new Point(488, 44);
            txtLoadAt.Margin = new Padding(2, 1, 2, 1);
            txtLoadAt.Name = "txtLoadAt";
            txtLoadAt.Size = new Size(70, 23);
            txtLoadAt.TabIndex = 21;
            // 
            // btnLoad
            // 
            btnLoad.BackColor = Color.FromArgb(192, 255, 192);
            btnLoad.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnLoad.Location = new Point(403, 39);
            btnLoad.Margin = new Padding(2, 1, 2, 1);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(57, 30);
            btnLoad.TabIndex = 22;
            btnLoad.Text = "Load...";
            btnLoad.UseVisualStyleBackColor = false;
            btnLoad.Click += btnLoad_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(465, 47);
            label2.Name = "label2";
            label2.Size = new Size(17, 15);
            label2.TabIndex = 23;
            label2.Text = "at";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // panel1
            // 
            panel1.Location = new Point(42, 95);
            panel1.Name = "panel1";
            panel1.Size = new Size(354, 140);
            panel1.TabIndex = 24;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(688, 351);
            Controls.Add(panel1);
            Controls.Add(label2);
            Controls.Add(btnLoad);
            Controls.Add(txtLoadAt);
            Controls.Add(SetPC);
            Controls.Add(lblPC);
            Controls.Add(lblOpCode);
            Controls.Add(lblA);
            Controls.Add(label14);
            Controls.Add(lblX);
            Controls.Add(label12);
            Controls.Add(lblY);
            Controls.Add(label10);
            Controls.Add(lblB);
            Controls.Add(lblD);
            Controls.Add(lblI);
            Controls.Add(lblZ);
            Controls.Add(lblC);
            Controls.Add(lblO);
            Controls.Add(lblN);
            Controls.Add(lblSP);
            Controls.Add(label3);
            Controls.Add(label1);
            Controls.Add(button1);
            Margin = new Padding(2, 1, 2, 1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label label1;
        private Label lblSP;
        private Label label3;
        private Label lblN;
        private Label lblO;
        private Label lblC;
        private Label lblZ;
        private Label lblI;
        private Label lblD;
        private Label lblB;
        private Label lblY;
        private Label label10;
        private Label lblX;
        private Label label12;
        private Label lblA;
        private Label label14;
        private Label lblOpCode;
        private TextBox lblPC;
        private Button SetPC;
        private TextBox txtLoadAt;
        private Button btnLoad;
        private Label label2;
        private OpenFileDialog openFileDialog1;
        private Panel panel1;
    }
}