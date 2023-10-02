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
            components = new System.ComponentModel.Container();
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
            pnlDump = new Panel();
            vScrollBar1 = new VScrollBar();
            lbl0F = new Label();
            lbl0E = new Label();
            lbl0D = new Label();
            lbl0C = new Label();
            lbl0B = new Label();
            lbl0A = new Label();
            lbl09 = new Label();
            lbl08 = new Label();
            lbl07 = new Label();
            lbl06 = new Label();
            lbl05 = new Label();
            lbl04 = new Label();
            lbl03 = new Label();
            lbl02 = new Label();
            lbl01 = new Label();
            lbl00 = new Label();
            txtSaveTo = new TextBox();
            txtSaveFrom = new TextBox();
            label4 = new Label();
            btnSave = new Button();
            saveFileDialog1 = new SaveFileDialog();
            btnReset = new Button();
            cbUseHeader = new CheckBox();
            btnRun = new Button();
            UpdateTimer = new System.Windows.Forms.Timer(components);
            pnlDump.SuspendLayout();
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
            button1.Click += btnStep_Click;
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
            txtLoadAt.Location = new Point(488, 37);
            txtLoadAt.Margin = new Padding(2, 1, 2, 1);
            txtLoadAt.Name = "txtLoadAt";
            txtLoadAt.Size = new Size(70, 23);
            txtLoadAt.TabIndex = 21;
            // 
            // btnLoad
            // 
            btnLoad.BackColor = Color.FromArgb(192, 255, 192);
            btnLoad.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnLoad.Location = new Point(403, 32);
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
            label2.Location = new Point(465, 40);
            label2.Name = "label2";
            label2.Size = new Size(17, 15);
            label2.TabIndex = 23;
            label2.Text = "at";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // pnlDump
            // 
            pnlDump.Controls.Add(vScrollBar1);
            pnlDump.Controls.Add(lbl0F);
            pnlDump.Controls.Add(lbl0E);
            pnlDump.Controls.Add(lbl0D);
            pnlDump.Controls.Add(lbl0C);
            pnlDump.Controls.Add(lbl0B);
            pnlDump.Controls.Add(lbl0A);
            pnlDump.Controls.Add(lbl09);
            pnlDump.Controls.Add(lbl08);
            pnlDump.Controls.Add(lbl07);
            pnlDump.Controls.Add(lbl06);
            pnlDump.Controls.Add(lbl05);
            pnlDump.Controls.Add(lbl04);
            pnlDump.Controls.Add(lbl03);
            pnlDump.Controls.Add(lbl02);
            pnlDump.Controls.Add(lbl01);
            pnlDump.Controls.Add(lbl00);
            pnlDump.Location = new Point(42, 94);
            pnlDump.Name = "pnlDump";
            pnlDump.Size = new Size(484, 245);
            pnlDump.TabIndex = 24;
            // 
            // vScrollBar1
            // 
            vScrollBar1.Location = new Point(0, 23);
            vScrollBar1.Maximum = 4097;
            vScrollBar1.Name = "vScrollBar1";
            vScrollBar1.Size = new Size(27, 183);
            vScrollBar1.TabIndex = 16;
            vScrollBar1.ValueChanged += vScrollBar1_ValueChanged;
            // 
            // lbl0F
            // 
            lbl0F.AutoSize = true;
            lbl0F.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl0F.Location = new Point(442, 10);
            lbl0F.Name = "lbl0F";
            lbl0F.Size = new Size(20, 15);
            lbl0F.TabIndex = 15;
            lbl0F.Text = "0F";
            // 
            // lbl0E
            // 
            lbl0E.AutoSize = true;
            lbl0E.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl0E.Location = new Point(417, 10);
            lbl0E.Name = "lbl0E";
            lbl0E.Size = new Size(20, 15);
            lbl0E.TabIndex = 14;
            lbl0E.Text = "0E";
            // 
            // lbl0D
            // 
            lbl0D.AutoSize = true;
            lbl0D.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl0D.Location = new Point(392, 10);
            lbl0D.Name = "lbl0D";
            lbl0D.Size = new Size(23, 15);
            lbl0D.TabIndex = 13;
            lbl0D.Text = "0D";
            // 
            // lbl0C
            // 
            lbl0C.AutoSize = true;
            lbl0C.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl0C.Location = new Point(367, 10);
            lbl0C.Name = "lbl0C";
            lbl0C.Size = new Size(21, 15);
            lbl0C.TabIndex = 12;
            lbl0C.Text = "0C";
            // 
            // lbl0B
            // 
            lbl0B.AutoSize = true;
            lbl0B.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl0B.Location = new Point(342, 10);
            lbl0B.Name = "lbl0B";
            lbl0B.Size = new Size(22, 15);
            lbl0B.TabIndex = 11;
            lbl0B.Text = "0B";
            // 
            // lbl0A
            // 
            lbl0A.AutoSize = true;
            lbl0A.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl0A.Location = new Point(317, 10);
            lbl0A.Name = "lbl0A";
            lbl0A.Size = new Size(22, 15);
            lbl0A.TabIndex = 10;
            lbl0A.Text = "0A";
            // 
            // lbl09
            // 
            lbl09.AutoSize = true;
            lbl09.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl09.Location = new Point(292, 10);
            lbl09.Name = "lbl09";
            lbl09.Size = new Size(21, 15);
            lbl09.TabIndex = 9;
            lbl09.Text = "09";
            // 
            // lbl08
            // 
            lbl08.AutoSize = true;
            lbl08.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl08.Location = new Point(267, 10);
            lbl08.Name = "lbl08";
            lbl08.Size = new Size(21, 15);
            lbl08.TabIndex = 8;
            lbl08.Text = "08";
            // 
            // lbl07
            // 
            lbl07.AutoSize = true;
            lbl07.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl07.Location = new Point(242, 10);
            lbl07.Name = "lbl07";
            lbl07.Size = new Size(21, 15);
            lbl07.TabIndex = 7;
            lbl07.Text = "07";
            // 
            // lbl06
            // 
            lbl06.AutoSize = true;
            lbl06.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl06.Location = new Point(217, 10);
            lbl06.Name = "lbl06";
            lbl06.Size = new Size(21, 15);
            lbl06.TabIndex = 6;
            lbl06.Text = "06";
            // 
            // lbl05
            // 
            lbl05.AutoSize = true;
            lbl05.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl05.Location = new Point(192, 10);
            lbl05.Name = "lbl05";
            lbl05.Size = new Size(21, 15);
            lbl05.TabIndex = 5;
            lbl05.Text = "05";
            // 
            // lbl04
            // 
            lbl04.AutoSize = true;
            lbl04.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl04.Location = new Point(167, 10);
            lbl04.Name = "lbl04";
            lbl04.Size = new Size(21, 15);
            lbl04.TabIndex = 4;
            lbl04.Text = "04";
            // 
            // lbl03
            // 
            lbl03.AutoSize = true;
            lbl03.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl03.Location = new Point(142, 10);
            lbl03.Name = "lbl03";
            lbl03.Size = new Size(21, 15);
            lbl03.TabIndex = 3;
            lbl03.Text = "03";
            // 
            // lbl02
            // 
            lbl02.AutoSize = true;
            lbl02.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl02.Location = new Point(117, 10);
            lbl02.Name = "lbl02";
            lbl02.Size = new Size(21, 15);
            lbl02.TabIndex = 2;
            lbl02.Text = "02";
            // 
            // lbl01
            // 
            lbl01.AutoSize = true;
            lbl01.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl01.Location = new Point(92, 10);
            lbl01.Name = "lbl01";
            lbl01.Size = new Size(21, 15);
            lbl01.TabIndex = 1;
            lbl01.Text = "01";
            // 
            // lbl00
            // 
            lbl00.AutoSize = true;
            lbl00.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lbl00.Location = new Point(67, 10);
            lbl00.Name = "lbl00";
            lbl00.Size = new Size(21, 15);
            lbl00.TabIndex = 0;
            lbl00.Text = "00";
            // 
            // txtSaveTo
            // 
            txtSaveTo.Location = new Point(586, 69);
            txtSaveTo.Margin = new Padding(2, 1, 2, 1);
            txtSaveTo.Name = "txtSaveTo";
            txtSaveTo.Size = new Size(70, 23);
            txtSaveTo.TabIndex = 25;
            txtSaveTo.Text = "0xFFFF";
            // 
            // txtSaveFrom
            // 
            txtSaveFrom.Location = new Point(488, 69);
            txtSaveFrom.Margin = new Padding(2, 1, 2, 1);
            txtSaveFrom.Name = "txtSaveFrom";
            txtSaveFrom.Size = new Size(70, 23);
            txtSaveFrom.TabIndex = 26;
            txtSaveFrom.Text = "0x0000";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(563, 72);
            label4.Name = "label4";
            label4.Size = new Size(18, 15);
            label4.TabIndex = 27;
            label4.Text = "to";
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(192, 255, 192);
            btnSave.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnSave.Location = new Point(403, 64);
            btnSave.Margin = new Padding(2, 1, 2, 1);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(57, 30);
            btnSave.TabIndex = 28;
            btnSave.Text = "Save...";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            // 
            // btnReset
            // 
            btnReset.BackColor = Color.FromArgb(255, 192, 192);
            btnReset.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnReset.Location = new Point(403, 0);
            btnReset.Margin = new Padding(2, 1, 2, 1);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(57, 30);
            btnReset.TabIndex = 29;
            btnReset.Text = "RESET";
            btnReset.UseVisualStyleBackColor = false;
            btnReset.Click += btnReset_Click;
            // 
            // cbUseHeader
            // 
            cbUseHeader.AutoSize = true;
            cbUseHeader.Location = new Point(569, 38);
            cbUseHeader.Margin = new Padding(2, 1, 2, 1);
            cbUseHeader.Name = "cbUseHeader";
            cbUseHeader.Size = new Size(131, 19);
            cbUseHeader.TabIndex = 30;
            cbUseHeader.Text = "with address header";
            cbUseHeader.UseVisualStyleBackColor = true;
            cbUseHeader.CheckedChanged += cbUseHeader_CheckedChanged;
            // 
            // btnRun
            // 
            btnRun.BackColor = Color.FromArgb(192, 255, 192);
            btnRun.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnRun.Location = new Point(6, 40);
            btnRun.Margin = new Padding(2, 1, 2, 1);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(101, 30);
            btnRun.TabIndex = 31;
            btnRun.Text = "RUN";
            btnRun.UseVisualStyleBackColor = false;
            btnRun.Click += btnRun_Click;
            // 
            // UpdateTimer
            // 
            UpdateTimer.Tick += UpdateTimer_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(762, 351);
            Controls.Add(btnRun);
            Controls.Add(cbUseHeader);
            Controls.Add(btnReset);
            Controls.Add(btnSave);
            Controls.Add(label4);
            Controls.Add(txtSaveFrom);
            Controls.Add(txtSaveTo);
            Controls.Add(pnlDump);
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
            StartPosition = FormStartPosition.Manual;
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            FormClosed += Form1_FormClosed;
            pnlDump.ResumeLayout(false);
            pnlDump.PerformLayout();
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
        private Panel pnlDump;
        private Label lbl01;
        private Label lbl00;
        private Label lbl0F;
        private Label lbl0E;
        private Label lbl0D;
        private Label lbl0C;
        private Label lbl0B;
        private Label lbl0A;
        private Label lbl09;
        private Label lbl08;
        private Label lbl07;
        private Label lbl06;
        private Label lbl05;
        private Label lbl04;
        private Label lbl03;
        private Label lbl02;
        private VScrollBar vScrollBar1;
        private TextBox txtSaveTo;
        private TextBox txtSaveFrom;
        private Label label4;
        private Button btnSave;
        private SaveFileDialog saveFileDialog1;
        private Button btnReset;
        private CheckBox cbUseHeader;
        private Button btnRun;
        private System.Windows.Forms.Timer UpdateTimer;
    }
}