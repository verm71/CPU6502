﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU6502
{
    internal class VICII
    {
        CIA1 c1;
        CIA2 c2;
        RAM mem;

        // Control Register 1
        bool BlankScreenToBorderColor;
        bool Twenty5Rows;

        // Control Rogister 2
        bool MultiColorMode;
        bool FortyColumns;        
        byte SmoothScrollX;

        public VICII(CIA1 cia1, CIA2 cia2, RAM ram)
        {
            c1 = cia1;
            c2 = cia2;
            mem = ram;
        }

        public void Write(ushort Addr, byte Value)
        {
            switch (Addr)
            {
                case 0xD016:
                    {
                        MultiColorMode = ((byte)(Value & 0x10) !=0);
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
    }
}
