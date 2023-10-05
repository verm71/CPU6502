using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace CPU6502
{
    internal class VICII
    {
        CIA1 cia1;
        CIA2 cia2;
        RAM mem;
        Display display;
        byte CurrentRaster;
        public byte bank;

        public ushort _BaseMemory
        {
            get
            {
                return (ushort)(bank * 0x4000);
            }
        }
        public ushort _VideoMatrixAddress
        {
            get
            {
                return (ushort)(VideoMatrixBaseAddress * 64 + _BaseMemory);
            }
        }


        // Control Register 1
        bool BlankScreenToBorderColor;
        bool Twenty5Rows;

        // Control Rogister 2: D016
        bool MultiColorMode;
        bool FortyColumns;
        byte SmoothScrollX;

        // D018
        byte VideoMatrixBaseAddress;
        byte CharacterDotDataBaseAddress;

        public VICII(ref CIA1 Cia1, ref CIA2 Cia2, RAM ram)
        {
            Cia1=cia1 = new CIA1(this);
            Cia2=cia2 = new CIA2(this);

            mem = ram;
            display = new Display();
            display.Show();
            Task.Run(UpdateDisplay);
        }

        public void Write(ushort Addr, byte Value)
        {
            switch (Addr)
            {
                case 0xD016:
                    {
                        MultiColorMode = ((byte)(Value & 0x10) != 0);
                        FortyColumns = ((byte)(Value & 0x08) != 0);
                        SmoothScrollX = ((byte)(Value & 0x07));
                        mem._mem[Addr] = Value;
                        break;
                    }

                case 0xD018:
                    {
                        VideoMatrixBaseAddress = (byte)(Value & 0xF0 >> 4);
                        CharacterDotDataBaseAddress = (byte)(Value & 0x0E);
                        break;
                    }

                default:
                    {
                        Debug.WriteLine("Unhandled write to VIC-II at {0:X4} value {1:X2}", Addr, Value);
                        break;
                    }
            }
        }

        void UpdateDisplay()
        {
            while (!display.IsDisposed)
            {


                CurrentRaster++;
                CurrentRaster = (byte)(CurrentRaster % 200);
                Thread.Sleep(1);
            }

            Debug.WriteLine("Display thread stopped by disposal of screen.");
            Console.Beep(500, 500);
        }
    }
}
