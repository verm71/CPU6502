using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CPU6502
{
    public partial class Display : Form
    {
        public Graphics graphics;

        public Display()
        {
            InitializeComponent();

            graphics = panel1.CreateGraphics();
        }

        private void Display_Load(object sender, EventArgs e)
        {
            this.Left = Screen.GetBounds(new Point(this.Left, this.Top)).Width - this.Width ;
        }
    }
}
