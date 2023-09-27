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
    }
}