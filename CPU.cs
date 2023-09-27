using System;
using System.Collections.Generic;
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
        public ulong PC;
        public Flags F = new Flags();
        RAM mem;
        byte OpCode;
        ulong Cycles = 0;


        // *************************************************
        const byte JSR = 0X20;
        const byte SEI = 0x78;
        const byte TXS = 0x9A;
        const byte LDX_IMM = 0XA2;
        const byte LDA_ABS_X = 0xBD;
        const byte CLD = 0xD8;
        const byte CMP_ABS_X = 0xDD;
        // *************************************************

        public CPU(RAM m)
        {
            mem = m;
            this.Reset();
        }

        public void Reset()
        {
            F.Reset();
            PC = (ulong)(mem.Read(0xfffc) + (mem.Read(0xfffd) << 8));
        }

        public void Fetch()
        {
            OpCode = mem.Read(PC);
            PC++;
        }

        public void Push(byte b)
        {
            mem.Write((ulong)(0x0100 + SP--), b);
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
                        PC = (ulong)(mem.Read(PC++) | (mem.Read(PC++) << 8));
                        break;
                    }

                case LDA_ABS_X:
                    {
                        ulong addr = (ulong)(mem.Read(PC++) | (mem.Read(PC++) << 8) + X + (F.C?1:0)) ;
                        A = mem.Read(addr);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;

                        break;
                    }

                case CMP_ABS_X:
                    {
                        ulong addr = (ulong)(mem.Read(PC++) | (mem.Read(PC++) << 8) + X + (F.C ? 1 : 0));
                        byte operand=mem.Read(addr);
                        short result = (short)(A - operand);
                        if (result==0)
                        {
                            F.N = false;
                            F.C = false;
                            F.Z = true;
                        } else if (result < 0)
                        {
                            F.N = true;

                        }
                    }


                default:
                    {
                        throw new NotImplementedException(String.Format("OP Code {0:X2} not implemented.", OpCode));
                    }
            }
        }

        public string Disassemble(ulong addr)
        {
            byte OpCode = mem.Read(addr);
            string Assembler = String.Format("??? {0:X2} {1:X2}", OpCode, mem.Read(addr + 1));

            switch (OpCode)
            {
                case LDX_IMM:
                    {
                        Assembler = String.Format("LDX #{0:X2}", mem.Read(addr + 1));
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
                        Assembler = String.Format("JSR {0:X4}", mem.Read(addr + 1) | (mem.Read(addr + 2) << 8));
                        break;
                    }

                case LDA_ABS_X:
                    {
                        ulong operand = (ulong)(mem.Read(addr + 1) | (mem.Read(addr + 2) << 8));
                        Assembler = string.Format("LDA {0:X4}+X+C  {1:X4}", operand, operand + X + (ulong)(F.C?1:0)));
                        break;
                    }

                case CMP_ABS_X:
                    {

                    }
            }

            return Assembler;

        }
    }
}
