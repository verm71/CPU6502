using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
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


        // *************************************************
        const byte JSR = 0X20;
        const byte JMP_ABS = 0x4C;
        const byte RTS = 0x60;
        const byte SEI = 0x78;
        const byte STA_ZP = 0x85;
        const byte STA_ABS = 0x8D;
        const byte STX_ABS = 0x8E;
        const byte TXS = 0x9A;
        const byte LDX_IMM = 0XA2;
        const byte LDA_IMM = 0xA9;
        const byte LDA_ABS = 0xAD;
        const byte LDA_ABS_X = 0xBD;
        const byte DEX = 0xCA;
        const byte BNE_REL = 0xD0;
        const byte CLD = 0xD8;
        const byte CMP_ABS_X = 0xDD;
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
                        if (result == 0)
                        {
                            F.N = false;
                            F.C = true;
                            F.Z = true;
                        }
                        else if (result < 0)
                        {
                            F.N = true;
                            F.C = false;
                            F.Z = false;
                        }
                        else
                        {
                            F.N = false;
                            F.C = true;
                            F.Z = false;
                        }

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

                default:
                    {
                        Debug.WriteLine(String.Format("**** {1:X4}: OP Code {0:X2} not implemented.", OpCode, LastFetchAddr));
                        Console.Beep(1000, 1000);
                        run = false;
                        break;
                    }
            }
        }

        public string Disassemble(ushort addr)
        {
            byte OpCode = mem.Read(addr);
            string Assembler = String.Format("??? {0:X2} {1:X2} {2:X2}", OpCode, mem.Read((ushort)(addr + 1)), mem.Read((ushort)(addr + 2)));

            switch (OpCode)
            {
                case LDX_IMM:
                    {
                        Assembler = String.Format("LDX #{0:X2}", mem.Read((ushort)(addr + 1)));
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
                        Assembler = String.Format("JSR ${0:X4}", mem.Read((ushort)(addr + 1)) | (mem.Read((ushort)(addr + 2)) << 8));
                        break;
                    }

                case LDA_ABS_X:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(addr + 1)) | (mem.Read((ushort)(addr + 2)) << 8));
                        Assembler = string.Format("LDA ${0:X4}+X  ${1:X4}", operand, operand + X);
                        break;
                    }

                case CMP_ABS_X:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(addr + 1)) | (mem.Read((ushort)(addr + 2)) << 8));
                        Assembler = string.Format("CMP ${0:X4}+X  ${1:X4}", operand, operand + X);
                        break;
                    }

                case BNE_REL:
                    {
                        sbyte rel = (sbyte)mem.Read((ushort)(addr + 1));
                        Assembler = String.Format("BNE {0:X4}", addr + 2 + rel);
                        break;
                    }

                case RTS:
                    {
                        Assembler = "RTS";
                        break;
                    }

                case STX_ABS:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(addr + 1)) | (mem.Read((ushort)(addr + 2)) << 8));
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
                        byte operand = mem.Read((ushort)(addr + 1));
                        Assembler = string.Format("LDA #{0:X2}", operand);
                        break;
                    }

                case STA_ABS:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(addr + 1)) | (mem.Read((ushort)(addr + 2)) << 8));
                        Assembler = string.Format("STA ${0:X4}", operand);
                        break;
                    }

                case STA_ZP:
                    {
                        byte zp = mem.Read((ushort)(addr + 1));
                        Assembler = string.Format("STA ${0:X2}", zp);
                        break;
                    }

                case LDA_ABS:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(addr + 1)) | (mem.Read((ushort)(addr + 2)) << 8));
                        Assembler = string.Format("LDA ${0:X4}", operand);
                        break;
                    }

                case BEQ_REL:
                    {
                        sbyte rel = (sbyte)mem.Read((ushort)(addr + 1));
                        Assembler = String.Format("BEQ {0:X4}", addr + 2 + rel);
                        break;
                    }

                case JMP_ABS:
                    {
                        ushort target = (ushort)(mem.Read((ushort)(addr + 1)) | (mem.Read((ushort)(addr + 2)) << 8));
                        Assembler = string.Format("JMP ${0:X4}", target);
                        break;
                    }


            }

            return Assembler;

        }

        public void Run()
        {
            while (run)
            {
                Fetch();
                Execute();
                Thread.Sleep(100);
            }

            Debug.WriteLine("CPU Halted by RUN flag.");
        }
    }
}
