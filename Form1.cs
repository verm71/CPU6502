using System.Globalization;

namespace CPU6502
{
    public partial class Form1 : Form
    {
        RAM mem;
        CPU cpu;

        public Form1()
        {
            InitializeComponent();

            mem = new RAM();
            cpu = new CPU(mem);

            UpdateStatusDisplay();

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

        private void button1_Click(object sender, EventArgs e)
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
                working = working.Substring(1);
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

                mem.Write(1, 0); // no mapping of ROM. All RAM available.
            }
        }
    }
}