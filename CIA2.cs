using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU6502
{
    internal class CIA2
    {
        public VICII vic;

        // 00
        bool SerialBUSIn;
        bool SerialBusClockIn;
        bool SerialBusOut;
        bool SerialBusClockOut;
        bool SerialBusATNOut;
        bool RS232Out;
        byte VICBankSelect;


        // 0D CIA Interrupt Control Register
        bool NMIFlag;
        bool Flag1NMI;
        
        bool SerialPortINT;
        bool TimerBINT;
        bool TimerAINT;

        public void Write(ushort Addr, byte Value)
        {
            switch (Addr & 0x000F)
            {
                case 0x00:
                    {
                        SerialBUSIn = (Value & 0x80) != 0;
                        SerialBusClockIn = (Value & 0x40) != 0;
                        SerialBusOut = (Value & 0x20) != 0;
                        SerialBusClockOut = (Value & 0x10) != 0;
                        SerialBusATNOut = (Value & 0x08) != 0;
                        RS232Out = (Value & 0x04) != 0;
                        VICBankSelect = (byte)(Value & 0x03);
                        vic.SetBank(VICBankSelect);
                        break;
                    }

                case 0x0D:
                    {
                        NMIFlag = (Value & 0x80) != 0;
                        Flag1NMI = (Value & 0x10) != 0;
                        SerialPortINT = (Value & 0x08) != 0;                        
                        TimerBINT = (Value & 0x02) != 0;
                        TimerAINT = (Value & 0x01) != 0;
                        break;
                    }

                default:
                    {
                        Debug.WriteLine("Unhandled write to CIA2 at {0:X4} value {1:X2}", Addr, Value);
                        break;
                    }
            }
        }
    }
}
