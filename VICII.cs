using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU6502
{
    internal class VICII
    {
        CIA1 cia1;
        CIA2 cia2;
        RAM mem;
        Display display;
        byte CurrentRaster;
        ushort _BaseMemory;


        // Control Register 1
        bool BlankScreenToBorderColor;
        bool Twenty5Rows;

        // Control Rogister 2
        bool MultiColorMode;
        bool FortyColumns;
        byte SmoothScrollX;

        public VICII(CIA1 Cia1, CIA2 Cia2, RAM ram)
        {
            cia1 = Cia1;
            cia2 = Cia2;
            cia1.vic = this; cia2.vic = this;

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
                default:
                    {
                        Debug.WriteLine("Unhandled write to VIC-II at {0:X4} value {1:X2}", Addr, Value);
                        break;
                    }
            }
        }

        public void SetBank(byte Bank)
        {
            switch (Bank)
            {
                case 0x00:
                    {
                        _BaseMemory = 0x0000;
                        break;
                    }
                case 0x01:
                    {
                        _BaseMemory = 0x4000;
                        break;
                    }
                case 0x02:
                    {
                        _BaseMemory = 0x8000;
                        break;
                    }
                case 0x03:
                    {
                        _BaseMemory = 0xc000;
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
                Thread.Sleep(1000 / 60);
            }

            Debug.WriteLine("Display thread stopped by disposal of screen.");
            Console.Beep(500, 500);
        }
    }
}
