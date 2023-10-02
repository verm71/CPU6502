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
        CIA1 c1;
        CIA2 c2;
        RAM mem;

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
