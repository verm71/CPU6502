using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU6502
{
    internal class CIA1
    {
        public VICII vic;
        RAM mem;

        // 0D CIA Interrupt Control Register
        bool IRQFlag;
        bool Flag1IRQ;
        bool SerialPortINT;
        bool TODClockINT;
        bool TimerBINT;
        bool TimerAINT;

        public CIA1(VICII Vic,RAM Mem)
        {
            this.vic = Vic;
            this.mem = Mem;
        }

        public void Write(ushort Addr, byte Value)
        {
            mem._mem[Addr] = Value;

            switch (Addr & 0x000F)
            {
                case 0x0D:
                    {
                        IRQFlag = (Value & 0x80) != 0;
                        Flag1IRQ = (Value & 0x10) != 0;
                        SerialPortINT = (Value & 0x08) != 0;
                        TODClockINT = (Value & 0x04) != 0;
                        TimerBINT = (Value & 0x02) != 0;
                        TimerAINT = (Value & 0x01) != 0;
                        break;
                    }

                default:
                    {
                        Debug.WriteLine("Unhandled write to CIA1 at {0:X4} value {1:X2}", Addr, Value);
                        break;
                    }
            }
        }
    }
}
