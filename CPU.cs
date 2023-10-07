using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CPU6502
{
    internal class Flags
    {
        public bool C, Z, I, D, B, O, N;  // Carry, Zero, INT Mask, BCD, BRK, Overflow, Negative

        public byte ToByte()
        {
            return (byte)((C ? 0x1 : 0) + (Z ? 0x2 : 0) + (I ? 0x4 : 0) + (D ? 0x8 : 0) + (B ? 0x10 : 0) + (O ? 0x40 : 0) + (N ? 0x80 : 0));
        }

        public void FromByte(byte b)
        {
            C = (b & 0x1) != 0;
            Z = (b & 0x2) != 0;
            I = (b & 0x4) != 0;
            D = (b & 0x8) != 0;
            B = (b & 0x10) != 0;
            O = (b & 0x40) != 0;
            N = (b & 0x80) != 0;
        }

        public void Reset()
        {
            this.FromByte(0x04); // all off except interrupt            
        }
    }
    internal class CPU
    {
        public byte A, X, Y, SP;
        public ushort PC;
        public Flags F = new Flags();
        RAM mem;
        byte OpCode;
        ushort LastFetchAddr;
        ulong Cycles = 0;
        public bool run = false;
        public ushort StopAt = 0;
        public int OpCodePauseNanoseconds = 100000;


        // *************************************************
        const byte ORA_IMM = 0x09;
        const byte BPL = 0x10;
        const byte CLC = 0x18;
        const byte JSR = 0X20;
        const byte AND_IMM = 0x29;
        const byte ROL = 0x2A;
        const byte JMP_ABS = 0x4C;
        const byte RTS = 0x60;
        const byte SEI = 0x78;
        const byte STY_ZP = 0x84;
        const byte STA_ZP = 0x85;
        const byte STX_ZP = 0x86;
        const byte DEY = 0x88;
        const byte TXA = 0x8A;
        const byte STY_ABS = 0x8C;
        const byte STA_ABS = 0x8D;
        const byte STX_ABS = 0x8E;
        const byte STA_IND_Y = 0x91;
        const byte STY_ZP_X= 0x94;
        const byte TYA = 0x98;
        const byte STA_ABS_Y = 0x99;
        const byte TXS = 0x9A;
        const byte STA_ABS_X= 0x9D;
        const byte LDY_IMM = 0xA0;
        const byte LDX_IMM = 0XA2;
        const byte LDY_ZP = 0xA4;
        const byte TAY = 0xA8;
        const byte LDA_IMM = 0xA9;
        const byte TAX = 0xAA;
        const byte LDA_ABS = 0xAD;
        const byte BCS_REL = 0xB0;
        const byte LDA_IND_Y = 0xB1;
        const byte LDA_ABS_Y = 0xB9;
        const byte LDA_ABS_X = 0xBD;
        const byte DEX = 0xCA;
        const byte INY = 0xC8;
        const byte BNE_REL = 0xD0;
        const byte CMP_IND_Y = 0xD1;
        const byte CLD = 0xD8;
        const byte CMP_ABS_X = 0xDD;
        const byte INC_ZP = 0xE6;
        const byte BEQ_REL = 0xF0;
        // *************************************************

        public CPU(RAM m)
        {
            mem = m;
            this.Reset();
        }

        public void Reset()
        {
            run = false;
            F.Reset();
            PC = (ushort)(mem.Read(0xfffc) + (mem.Read(0xfffd) << 8));
        }

        public void Fetch()
        {
            LastFetchAddr = PC;
            OpCode = mem.Read(PC++);
        }

        public void Push(byte b)
        {
            mem.Write((ushort)(0x0100 + SP--), b);
        }

        public byte Pop()
        {
            return (mem.Read((ushort)(0x0100 + (++SP))));
        }

        public void Run()
        {
            string stopReason = "CPU Halted: RUN flag cleared.";

            while (run)
            {
                Fetch();
                Execute();

                DateTime end = DateTime.Now + new TimeSpan(OpCodePauseNanoseconds / 100);
                if (PC == StopAt)
                {
                    run = false;
                    stopReason = "CPU Halted: Stop point reached.";
                }
                while (DateTime.Now < end) { Thread.Yield(); }
            }

            Debug.WriteLine(stopReason);
        }

        public void Execute()
        {
            switch (OpCode)
            {
                case LDX_IMM:
                    {
                        this.X = mem.Read(PC++);
                        Cycles += 2;
                        this.F.Z = (X == 0);
                        this.F.N = (X & 0x80) != 0;
                        break;
                    }
                case SEI:
                    {
                        this.F.I = true;
                        break;
                    }
                case TXS:
                    {
                        SP = X;
                        break;
                    }
                case CLD:
                    {
                        F.D = false;
                        break;
                    }
                case JSR:
                    {
                        Push((byte)(((PC + 2) & 0xFF00) >> 8));
                        Push((byte)(((PC + 2) & 0x00FF)));
                        PC = (ushort)(mem.Read(PC++) | (mem.Read(PC++) << 8));
                        break;
                    }
                case LDA_ABS_X:
                    {
                        ushort addr = (ushort)((mem.Read(PC++) + X) | (mem.Read(PC++) << 8));
                        A = mem.Read(addr);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;

                        break;
                    }
                case CMP_ABS_X:
                    {
                        byte operand = mem.Read((ushort)((mem.Read(PC++) + X) | (mem.Read(PC++) << 8)));
                        sbyte result = (sbyte)(A - operand);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        F.C = (operand > A);
                        break;
                    }
                case BNE_REL:
                    {
                        ushort target = (ushort)(PC + 1 + (sbyte)mem.Read((ushort)(PC)));
                        PC++;
                        if (!F.Z)
                        {
                            PC = target;
                        }

                        break;
                    }
                case RTS:
                    {
                        PC = (ushort)(Pop() | (Pop() << 8));
                        break;
                    }
                case STX_ABS:
                    {
                        ushort addr = (ushort)(mem.Read(PC++) | (mem.Read(PC++) << 8));
                        mem.Write(addr, X);
                        break;
                    }
                case DEX:
                    {
                        X--;
                        F.Z = (X == 0);
                        F.N = ((X & 0x80) != 0);

                        break;
                    }
                case LDA_IMM:
                    {
                        A = mem.Read(PC++);
                        Cycles += 2;
                        this.F.Z = (A == 0);
                        this.F.N = (A & 0x80) != 0;
                        break;
                    }
                case STA_ABS:
                    {
                        ushort addr = (ushort)(mem.Read(PC++) | (mem.Read(PC++) << 8));
                        mem.Write(addr, A);
                        break;
                    }
                case STA_ZP:
                    {
                        byte zp = mem.Read(PC++);
                        mem.Write(zp, A);
                        break;
                    }
                case LDA_ABS:
                    {
                        ushort addr = (ushort)(mem.Read(PC++) | (mem.Read(PC++) << 8));
                        A = mem.Read(addr);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case BEQ_REL:
                    {
                        ushort target = (ushort)(PC + 1 + (sbyte)mem.Read((ushort)(PC)));
                        PC++;
                        if (F.Z)
                        {
                            PC = target;
                        }

                        break;
                    }
                case JMP_ABS:
                    {
                        ushort target = (ushort)(mem.Read(PC++) | (mem.Read(PC++) << 8));
                        PC = target;
                        break;
                    }
                case AND_IMM:
                    {
                        byte operand = mem.Read(PC++);
                        A = (byte)(A & operand);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case ORA_IMM:
                    {
                        byte operand = mem.Read(PC++);
                        A = (byte)(A | operand);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case TAY:
                    {
                        Y = A;
                        F.Z = (Y == 0);
                        F.N = (Y & 0x80) != 0;
                        break;
                    }
                case STA_ABS_Y:
                    {
                        ushort addr = (ushort)(mem.Read(PC++) + (mem.Read(PC++) << 8) + Y);
                        mem.Write(addr, A);
                        break;
                    }
                case INY:
                    {
                        Y++;
                        F.N = (Y & 0x80) != 0;
                        F.Z = Y == 0;
                        break;
                    }
                case LDY_IMM:
                    {
                        byte operand = mem.Read(PC++);
                        Y = operand;
                        F.N = (Y & 0x80) != 0;
                        F.Z = Y == 0;
                        break;
                    }
                case STX_ZP:
                    {
                        byte operand = mem.Read(PC++);
                        mem.Write((ushort)(operand), X);
                        break;
                    }
                case STY_ZP:
                    {
                        byte operand = mem.Read(PC++);
                        mem.Write((ushort)(operand), Y);
                        break;
                    }
                case INC_ZP:
                    {
                        byte operand = (byte)(mem.Read(PC++));
                        byte value = (byte)(mem.Read(operand) + 1);
                        mem.Write(operand, value);
                        F.Z = (value == 0);
                        F.N = (value & 0x80) != 0;
                        break;
                    }
                case LDA_IND_Y:
                    {
                        ushort operand = (ushort)(mem.Read(PC++));
                        ushort addr = (ushort)(mem.Read(operand) + mem.Read((ushort)(operand + 1)) << 8);
                        A = mem.Read((ushort)(addr + Y));
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case TAX:
                    {
                        X = A;
                        F.Z = (X == 0);
                        F.N = (X & 0x80) != 0;
                        break;
                    }
                case STA_IND_Y:
                    {
                        ushort operand = (ushort)(mem.Read(PC++));
                        ushort addr = (ushort)(mem.Read(operand) + mem.Read((ushort)(operand + 1)) << 8);
                        mem.Write((ushort)(addr + Y), A);
                        break;
                    }
                case CMP_IND_Y:
                    {
                        ushort operand = (ushort)(mem.Read(PC++));
                        ushort addr = (ushort)(mem.Read(operand) + mem.Read((ushort)(operand + 1)) << 8);
                        sbyte value = (sbyte)((sbyte)A - (sbyte)(mem.Read(addr)));
                        F.Z = (value == 0);
                        F.N = (value & 0x80) != 0;
                        F.C = mem.Read(addr) > A;
                        break;
                    }
                case ROL:
                    {
                        F.C = (A & 0x80) != 0;     // C <- [76543210]
                        A <<= 1;                   // ROL A
                        A += (byte)(F.C ? 1 : 0);  // [76543210] <- C
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case TXA:
                    {
                        A = X;
                        F.N = (A & 0x80) != 0;
                        F.Z = (A == 0);
                        break;
                    }
                case TYA:
                    {
                        A = Y;
                        F.N = (A & 0x80) != 0;
                        F.Z = (A == 0);
                        break;
                    }
                case LDY_ZP:
                    {
                        byte operand = mem.Read(PC++);
                        Y = mem.Read(operand);
                        F.Z = Y == 0;
                        F.N = (Y & 0x80) != 0;
                        break;
                    }
                case CLC:
                    {
                        F.C = false;
                        break;
                    }
                case STY_ABS:
                    {
                        ushort addr = (ushort)(mem.Read(PC++) + ((mem.Read(PC++) << 8)));
                        mem.Write(addr, Y);
                        break;
                    }
                case LDA_ABS_Y:
                    {
                        ushort addr = (ushort)(mem.Read(PC++) + ((mem.Read(PC++) << 8)));
                        A = mem.Read((ushort)(addr + Y));
                        F.Z = A == 0;
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case BCS_REL:
                    {
                        sbyte operand = (sbyte)(mem.Read(PC++));
                        if (F.C)
                        {
                            PC = (ushort)(PC + operand);
                        }
                        break;
                    }
                case DEY:
                    {
                        Y--;
                        F.Z = Y == 0;
                        F.N= (Y & 0x80) != 0;
                        break;
                    }
                case BPL:
                    {
                        sbyte operand = (sbyte)(mem.Read(PC++));
                        if (!F.N)
                        {
                            PC = (ushort)(PC+ operand);
                        }
                        break;
                    }
                case STA_ABS_X:
                    {
                        ushort addr = (ushort)(mem.Read(PC++) + (mem.Read(PC++) << 8));
                        mem.Write((ushort)(addr + X), A);
                        break;
                    }
                case STY_ZP_X:
                    {
                        ushort addr = (ushort)(mem.Read(PC++) + X);
                        ushort lookup = (ushort)(mem.Read(addr) + mem.Read((ushort)(addr + 1))<<8);
                        mem.Write(lookup, Y);
                        break;
                    }
                default:
                    {
                        Debug.WriteLine(String.Format("**** {1:X4}: OP Code {0:X2} not implemented.", OpCode, LastFetchAddr));
                        run = false;
                        PC--;
                        Console.Beep(1000, 1000);
                        break;
                    }
            }
        }

        public string Disassemble(ushort Addr)
        {
            byte OpCode = mem.Read(Addr);
            string Assembler = String.Format("??? {0:X2} {1:X2} {2:X2}", OpCode, mem.Read((ushort)(Addr + 1)), mem.Read((ushort)(Addr + 2)));

            switch (OpCode)
            {
                case LDX_IMM:
                    {
                        Assembler = String.Format("LDX #{0:X2}", mem.Read((ushort)(Addr + 1)));
                        break;
                    }
                case SEI:
                    {
                        Assembler = "SEI";
                        break;
                    }
                case TXS:
                    {
                        Assembler = "TXS";
                        break;
                    }
                case CLD:
                    {
                        Assembler = "CLD";
                        break;
                    }
                case JSR:
                    {
                        Assembler = String.Format("JSR ${0:X4}", mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        break;
                    }
                case LDA_ABS_X:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("LDA ${0:X4}+X  ${1:X4}", operand, operand + X);
                        break;
                    }
                case CMP_ABS_X:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("CMP ${0:X4}+X  ${1:X4}", operand, operand + X);
                        break;
                    }
                case BNE_REL:
                    {
                        sbyte rel = (sbyte)mem.Read((ushort)(Addr + 1));
                        Assembler = String.Format("BNE {0:X4}", Addr + 2 + rel);
                        break;
                    }
                case RTS:
                    {
                        Assembler = "RTS";
                        break;
                    }
                case STX_ABS:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("STX ${0:X4}", operand);
                        break;
                    }
                case DEX:
                    {
                        Assembler = "DEX";
                        break;
                    }
                case LDA_IMM:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("LDA #{0:X2}", operand);
                        break;
                    }
                case STA_ABS:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("STA ${0:X4}", operand);
                        break;
                    }
                case STA_ZP:
                    {
                        byte zp = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("STA ${0:X2}", zp);
                        break;
                    }
                case LDA_ABS:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("LDA ${0:X4}", operand);
                        break;
                    }
                case BEQ_REL:
                    {
                        sbyte rel = (sbyte)mem.Read((ushort)(Addr + 1));
                        Assembler = String.Format("BEQ {0:X4}", Addr + 2 + rel);
                        break;
                    }
                case JMP_ABS:
                    {
                        ushort target = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("JMP ${0:X4}", target);
                        break;
                    }
                case AND_IMM:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("AND #{0:X2}", operand);
                        break;
                    }
                case ORA_IMM:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("ORA #{0:X2}", operand);
                        break;
                    }
                case TAY:
                    {
                        Assembler = "TAY";
                        break;
                    }
                case STA_ABS_Y:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) + (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("STA ${0:X4}+Y  {1:X4}", operand, operand + Y);
                        break;
                    }
                case INY:
                    {
                        Assembler = "INY";
                        break;
                    }
                case LDY_IMM:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("LDY #{0:X2}", operand);
                        break;
                    }
                case STX_ZP:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("STX {0:X4}", operand);
                        break;
                    }
                case STY_ZP:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("STY {0:X4}", operand);
                        break;
                    }
                case INC_ZP:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("INC {0:X4}", operand);
                        break;
                    }
                case LDA_IND_Y:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)));
                        ushort address = (ushort)(mem.Read(operand) + mem.Read((ushort)(operand + 1)) << 8);
                        Assembler = string.Format("LDA ({0:X2}),Y  {1:X4}", operand, address + Y);
                        break;
                    }
                case TAX:
                    {
                        Assembler = "TAX";
                        break;
                    }
                case STA_IND_Y:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)));
                        ushort address = (ushort)(mem.Read(operand) + mem.Read((ushort)(operand + 1)) << 8);
                        Assembler = string.Format("STA ({0:X2}),Y  {1:X4}", operand, address);
                        break;
                    }
                case CMP_IND_Y:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)));
                        ushort address = (ushort)(mem.Read(operand) + mem.Read((ushort)(operand + 1)) << 8);
                        Assembler = string.Format("CMP ({0:X2}),Y  {1:X4}", operand, address);
                        break;
                    }
                case ROL:
                    {
                        Assembler = "ROL";
                        break;
                    }
                case TXA:
                    {
                        Assembler = "TXA";
                        break;
                    }
                case TYA:
                    {
                        Assembler = "TYA";
                        break;
                    }
                case LDY_ZP:
                    {
                        byte operand = (byte)(mem.Read((byte)(Addr + 1)));
                        Assembler = string.Format("LDY ${0:X4}", operand);
                        break;
                    }
                case CLC:
                    {
                        Assembler = "CLC";
                        break;
                    }
                case STY_ABS:
                    {
                        ushort addr = (ushort)(mem.Read((ushort)(Addr + 1)) + (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("STY {0:X4}", addr);
                        break;
                    }
                case LDA_ABS_Y:
                    {
                        ushort addr = (ushort)(mem.Read((ushort)(Addr + 1)) + (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("LDA {0:X4},Y   {1:X4}", addr, addr + Y);
                        break;
                    }
                case BCS_REL:
                    {
                        sbyte operand = (sbyte)mem.Read((ushort)(Addr+1));
                        Assembler = string.Format("BCS ${0:X4}", Addr + operand+2);
                        break;
                    }
                case DEY:
                    {
                        Assembler = "DEY";
                        break;
                    }
                case BPL:
                    {
                        sbyte operand = (sbyte)mem.Read((ushort)(Addr+1));
                        Assembler = string.Format("BPL ${0:X4}", Addr + operand+2);
                        break;
                    }
                case STA_ABS_X:
                    {
                        ushort addr = (ushort)(mem.Read((ushort)(Addr + 1)) + (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("STA ${0:X4},X  {1:X4}", addr, addr + X);
                        break;
                    }
                case STY_ZP_X:
                    {
                        ushort addr = (ushort)(mem.Read((ushort)(Addr+1)) + X);
                        ushort lookup = (ushort)(mem.Read(addr) + mem.Read((ushort)(addr + 1))<<8);
                        Assembler = string.Format("STY ${0:X2},X  {1:X4}", addr + X, lookup);
                        break;
                    }
            }

            return Assembler;

        }
    }
}
