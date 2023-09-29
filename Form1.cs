using System.Globalization;

namespace CPU6502
{
    public partial class Form1 : Form
    {
        RAM mem;
        CPU cpu;
        Label[,] dump;
        Label[] addr;
        ushort dumpStart;

        public Form1()
        {
            InitializeComponent();

            mem = new RAM();
            cpu = new CPU(mem);
            dump = new Label[16, 8];
            addr = new Label[8];

            InitializeMemoryDump();

            UpdateStatusDisplay();

        }

        private void InitializeMemoryDump()
        {
            Label[] header = { lbl00, lbl01, lbl02, lbl03, lbl04, lbl05, lbl06, lbl07, lbl08, lbl09, lbl0A, lbl0B, lbl0C, lbl0D, lbl0E, lbl0F };

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    dump[x, y] = new Label();
                    dump[x, y].Parent = pnlDump;
                    dump[x, y].Top = y * 40 + 80;
                    dump[x, y].Left = header[x].Left;
                    dump[x, y].Text = string.Format("00");
                    dump[x, y].AutoSize = true;
                    dump[x, y].Tag = new Point(x, y);
                    dump[x, y].Click += new EventHandler(DumpClicked);
                }
            }

            for (int y = 0; y < 8; y++)
            {
                addr[y] = new Label();
                addr[y].Parent = pnlDump;
                addr[y].Top = y * 40 + 80;
                addr[y].Left = 30;
                addr[y].AutoSize = true;
                addr[y].Text = "0000";
            }
        }

        private void DumpClicked(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                Label clicked = (Label)sender;

                ShowInputBox(clicked);
            }
        }

        private void ShowInputBox(Label clicked)
        {
            Point coord = (Point)clicked.Tag;
            ushort addr = (ushort)(dumpStart + coord.Y * 0x10 + coord.X);

            TextBox input = new TextBox();
            input.Parent = clicked.Parent;
            input.Location = clicked.Location;
            input.Width = 50;
            input.Height = 30;
            input.BringToFront();
            input.Tag = addr;
            input.Text = string.Format("{0:X2}", mem.Read(addr));
            input.Focus();
            input.SelectAll();
            input.KeyPress += ValueEntered;
            input.Leave += ExitValue;
            input.PreviewKeyDown += OnPreviewKeyDown;
        }

        private void DumpEdit(ushort addr)
        {
            ushort newDumpStart = dumpStart;

            if (addr >= dumpStart + 0x80)
            {
                newDumpStart = (ushort)(addr & 0xfff0);
                //  will that value cause us to display > 0xFFFF ?
                if (newDumpStart > (0xfff0 - 0x0080))
                {
                    newDumpStart = 0xfff0 - 0x0080;
                }
                //dumpStart = newDumpStart;

                //UpdateDump();
            }
            else if (addr < dumpStart)
            {
                newDumpStart = (ushort)(addr & 0xfff0);

            }

            vScrollBar1.Value = newDumpStart / 0x10;

            int x, y;
            y = ((addr & 0xfff0) - newDumpStart) / 0x10;
            x = addr & 0x000f;

            ShowInputBox(dump[x, y]);
        }

        private void ValueEntered(object? sender, KeyPressEventArgs e)
        {
            if (sender != null)
            {
                if (e.KeyChar == '\r')
                {
                    ExitValue(sender, e);
                }
            }

        }

        private void OnPreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab && sender != null)
            {
                TextBox box = (TextBox)sender;
                ushort nextAddr = ((ushort)((ushort)(box.Tag) + 1));
                ExitValue(sender, e);
                DumpEdit(nextAddr);
            }
        }

        private void ExitValue(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                TextBox input = (TextBox)sender;
                ushort addr = (ushort)(input.Tag);
                mem.Write(addr, (byte)Byte.Parse(input.Text, NumberStyles.HexNumber));
                input.Dispose();
                UpdateDump();
            }
        }

        public void UpdateStatusDisplay()
        {
            lblPC.Text = String.Format("0x{0:X4}", cpu.PC);
            lblOpCode.Text = cpu.Disassemble(cpu.PC);
            lblSP.Text = string.Format("0x{0:X2}", cpu.SP);

            // Flags
            SetFlag(cpu.F.N, lblN);
            SetFlag(cpu.F.O, lblO);
            SetFlag(cpu.F.B, lblB);
            SetFlag(cpu.F.D, lblD);
            SetFlag(cpu.F.I, lblI);
            SetFlag(cpu.F.Z, lblZ);
            SetFlag(cpu.F.C, lblC);

            lblA.Text = String.Format("0x{0:X2}", cpu.A);
            lblX.Text = String.Format("0x{0:X2}", cpu.X);
            lblY.Text = String.Format("0x{0:X2}", cpu.Y);

            UpdateDump();
        }

        private void UpdateDump()
        {
            for (int y = 0; y < 8; y++)
            {
                addr[y].Text = string.Format("{0:X4}", dumpStart + 0x10 * y);
                for (int x = 0; x < 16; x++)
                {
                    dump[x, y].Text = string.Format("{0:X2}", mem.Read((ushort)(dumpStart + 0x10 * y + x)));
                }
            }
        }

        private void SetFlag(bool value, Label lbl)
        {
            if (value)
            {
                lbl.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lbl.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            cpu.Fetch();
            cpu.Execute();

            UpdateStatusDisplay();
        }

        private void SetPC_Click(object sender, EventArgs e)
        {
            string working = lblPC.Text;

            if (working.StartsWith("0x"))
            {
                working = working.Substring(2);
            }

            ushort value = ushort.Parse(working, NumberStyles.HexNumber);
            cpu.PC = value;

            UpdateStatusDisplay();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string LoadAt = txtLoadAt.Text;
            if (LoadAt.StartsWith("0x"))
            {
                LoadAt = LoadAt.Substring(1);
            }

            ushort value = ushort.Parse(LoadAt, NumberStyles.HexNumber);

            byte[] file;

            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                file = File.ReadAllBytes(openFileDialog1.FileName);

                for (int i = 0; i < file.Length; i++)
                {
                    mem.Write((ushort)(value + i), file[i]);
                }

                UpdateDump();
            }
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            dumpStart = (ushort)(vScrollBar1.Value * 16);
            UpdateDump();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                if (txtSaveFrom.Text.ToString().StartsWith("0x"))
                {
                    txtSaveFrom.Text = txtSaveFrom.Text.Substring(2);
                }

                if (txtSaveTo.Text.ToString().StartsWith("0x"))
                {
                    txtSaveTo.Text = txtSaveTo.Text.Substring(2);
                }

                ushort begin = ushort.Parse(txtSaveFrom.Text, NumberStyles.HexNumber);
                ushort end = ushort.Parse(txtSaveTo.Text, NumberStyles.HexNumber);

                byte[] buf = new byte[end - begin + 1];
                for (int i = 0; i < buf.Length; i++)
                {
                    buf[i] = mem.Read((ushort)(begin + i));
                }

                File.WriteAllBytes(saveFileDialog1.FileName, buf);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            cpu.Reset();
            UpdateStatusDisplay();
            UpdateDump();
        }
    }
}