using System.Diagnostics;
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
        Task? CPUTask = null;

        public Form1()
        {
            InitializeComponent();
            mem = new RAM();
            cpu = new CPU(mem);
            mem.cpu = cpu;
            dump = new Label[16, 8];
            addr = new Label[8];

            InitializeMemoryDump();

            UpdateStatusDisplay();
            rb1M.Checked = true;
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
            if (cpu.run)
            {
                cpu.run = false;
            }
            else
            {
                cpu.FetchOpCode();
                cpu.Execute();
            }
            mem.vic.UpdateDisplay = true;
            UpdateStatusDisplay();
        }

        private void SetPC_Click(object sender, EventArgs e)
        {
            string working = lblPC.Text;

            if (working.StartsWith("0x"))
            {
                working = working.Substring(2);
            }

            ushort value;

            try
            {
                value = ushort.Parse(working, NumberStyles.HexNumber);
            }
            catch
            {
                lblPC.Text = "0x0000";
                value = 0;
            }
            cpu.PC = value;

            UpdateStatusDisplay();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string LoadAt = txtLoadAt.Text;
            ushort value = 0;

            if (!cbUseHeader.Checked)
            {
                if (LoadAt.StartsWith("0x"))
                {
                    LoadAt = LoadAt.Substring(1);
                }

                value = ushort.Parse(LoadAt, NumberStyles.HexNumber);
            }

            byte[] file;

            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                file = File.ReadAllBytes(openFileDialog1.FileName);
                int startRead = 0;

                if (cbUseHeader.Checked)
                {
                    value = (ushort)(file[0] + (file[1] << 8));
                    startRead = 2;
                }

                for (int i = startRead; i < file.Length; i++)
                {
                    mem._mem[(ushort)(value + i - startRead)] = file[i];
                }

                cpu.PC = value;
                UpdateStatusDisplay();
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

                byte[] buf = new byte[end - begin + 3];  // include possible start address vector

                byte offset = (byte)(cbUseHeader.Checked ? 2 : 0);
                if (cbUseHeader.Checked)
                {
                    buf[0] = (byte)(begin & 0xff);
                    buf[1] = (byte)(begin >> 8);
                }

                for (int i = 0; i < buf.Length - offset; i++)
                {
                    buf[i + offset] = mem.Read((ushort)(begin + i));
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

        private void cbUseHeader_CheckedChanged(object sender, EventArgs e)
        {
            txtLoadAt.Enabled = !cbUseHeader.Checked;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (!cpu.run)
            {
                if (cbStopAt.Checked)
                {
                    string working = txtStopAt.Text;
                    if (working.StartsWith("0x"))
                    {
                        working = working.Substring(2);
                    }
                    cpu.StopAt = ushort.Parse(working, NumberStyles.HexNumber);
                }
                else
                {
                    cpu.StopAt = 0;
                }

                if (cbStopOnMemory.Checked)
                {
                    string working = txtStopOnMemory.Text;
                    if (working.StartsWith("0x"))
                    {
                        working = working.Substring(2);
                    }

                    if (rbMemoryWrite.Checked)
                    {
                        cpu.StopAtMemoryWrite = ushort.Parse(working, NumberStyles.HexNumber);
                    }
                    else
                    {
                        cpu.StopAtMemoryWrite = -1;
                    }

                    if (rbMemoryRead.Checked)
                    {
                        cpu.StopAtMemoryRead = ushort.Parse(working, NumberStyles.HexNumber);
                    }
                    else
                    {
                        cpu.StopAtMemoryRead = -1;
                    }
                }
                else
                {
                    cpu.StopAtMemoryRead = -1;
                    cpu.StopAtMemoryWrite = -1;
                }

                mem.vic.UpdateDisplay = true;
                cpu.run = true;
                CPUTask = Task.Run(cpu.Run);
                UpdateTimer.Enabled = true;
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateStatusDisplay();

            if (!cpu.run && (CPUTask != null))
            {
                UpdateStatusDisplay();
                if (CPUTask.IsCompleted)
                {
                    CPUTask.Dispose();
                    UpdateTimer.Enabled = false;
                };
            }
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cpu.run)
            {
                cpu.run = false;

                if (CPUTask != null && (CPUTask.Status == TaskStatus.Running))
                {
                    await CPUTask;
                    CPUTask.Dispose();
                    Debug.WriteLine("CPU Thread stopped by program termination.");
                }
            }
        }

        private void rbZero_CheckedChanged(object sender, EventArgs e)
        {
            if (rbZero.Checked) { cpu.OpCodePauseNanoseconds = 0; }
        }

        private void rb10K_CheckedChanged(object sender, EventArgs e)
        {
            if (rb10K.Checked) { cpu.OpCodePauseNanoseconds = 10000; }
        }

        private void rb100K_CheckedChanged(object sender, EventArgs e)
        {
            if (rb100K.Checked) { cpu.OpCodePauseNanoseconds = 100000; }
        }

        private void rb1M_CheckedChanged(object sender, EventArgs e)
        {
            if (rb1M.Checked) { cpu.OpCodePauseNanoseconds = 1000000; }
        }

        private void rb100M_CheckedChanged(object sender, EventArgs e)
        {
            if (rb100M.Checked) { cpu.OpCodePauseNanoseconds = 100000000; }
        }

        private void cbStopAt_CheckedChanged(object sender, EventArgs e)
        {
            txtStopAt.Enabled = cbStopAt.Checked;
        }

        private void cbStopOnMemory_CheckedChanged(object sender, EventArgs e)
        {
            txtStopOnMemory.Enabled = cbStopOnMemory.Checked;

        }
    }
}