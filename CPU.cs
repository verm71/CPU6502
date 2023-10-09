using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static CPU6502.RAM;

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
        opCodes OpCode;
        ushort LastFetchAddr;
        //ulong Cycles = 0;
        public bool run = false;
        public ushort StopAt = 0;
        public int OpCodePauseNanoseconds = 100000;

        public enum AddressingMode
        {
            ABS,
            ABS_X,
            ABS_Y,
            IMM,
            IND,
            X_IND,
            IND_Y,
            REL,
            ZP,
            ZP_X,
            ZP_Y
        }


        // *************************************************
        enum opCodes : byte
        {
            ORA_IMM = 0x09,
            ORA_ABS = 0x0D,
            BPL = 0x10,
            CLC = 0x18,
            JSR = 0X20,
            AND_IMM = 0x29,
            ROL = 0x2A,
            JMP_ABS = 0x4C,
            RTS = 0x60,
            ADC_IMM = 0x69,
            SEI = 0x78,
            STY_ZP = 0x84,
            STA_ZP = 0x85,
            STX_ZP = 0x86,
            DEY = 0x88,
            TXA = 0x8A,
            STY_ABS = 0x8C,
            STA_ABS = 0x8D,
            STX_ABS = 0x8E,
            BCC_REL = 0x90,
            STA_IND_Y = 0x91,
            STY_ZP_X = 0x94,
            STA_ZP_X = 0x95,
            TYA = 0x98,
            STA_ABS_Y = 0x99,
            TXS = 0x9A,
            STA_ABS_X = 0x9D,
            LDY_IMM = 0xA0,
            LDX_IMM = 0XA2,
            LDY_ZP = 0xA4,
            LDA_ZP = 0xA5,
            TAY = 0xA8,
            LDA_IMM = 0xA9,
            TAX = 0xAA,
            LDA_ABS = 0xAD,
            BCS_REL = 0xB0,
            LDA_IND_Y = 0xB1,
            LDA_ZP_X = 0xB5,
            LDA_ABS_Y = 0xB9,
            LDA_ABS_X = 0xBD,
            DEX = 0xCA,
            INY = 0xC8,
            BNE_REL = 0xD0,
            CMP_IND_Y = 0xD1,
            CLD = 0xD8,
            CMP_ABS_X = 0xDD,
            CPX_IMM = 0xE0,
            INC_ZP = 0xE6,
            INX = 0xE8,
            BEQ_REL = 0xF0
        }
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

        public void FetchOpCode()
        {
            LastFetchAddr = PC;
            OpCode = (opCodes)mem.Read(PC++);
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
                FetchOpCode();
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

        public ushort FetchAddress(ref ushort addr, AddressingMode Mode)//, bool IncrementAddress)
        {
            switch (Mode)
            {
                case AddressingMode.ABS:
                    {
                        return (ushort)(mem.Read(addr++) + (mem.Read(addr++) << 8));
                    }
                case AddressingMode.REL:
                    {
                        sbyte offset = (sbyte)mem.Read(addr++);
                        return (ushort)(addr + offset);
                    }
                case AddressingMode.ZP:
                    {
                        return (ushort)(mem.Read(addr++));
                    }
                case AddressingMode.ABS_X:
                    {
                        return (ushort)(mem.Read(addr++) + (mem.Read(addr++) << 8) + X);
                    }
                case AddressingMode.ABS_Y:
                    {
                        return (ushort)(mem.Read(addr++) + (mem.Read(addr++) << 8) + Y);
                    }
                case AddressingMode.IND_Y:
                    {
                        ushort zpAddress = (ushort)(mem.Read(addr++));
                        return (ushort)(FetchAddress(ref zpAddress, AddressingMode.ABS) + Y); // doesn't matter if zpAddress gets incremented                        
                    }
                case AddressingMode.ZP_X:
                    {
                        return (ushort)((mem.Read(addr++) + X) % 0xFF);
                    }
                case AddressingMode.ZP_Y:
                    {
                        return (ushort)((mem.Read(addr++) + Y) & 0xFF);
                    }
                default:
                    {
                        Debug.WriteLine("     ****");
                        Debug.WriteLine("     **** Unimplemented addressing mode {0} for address {1:X4}.", Mode, addr);
                        Debug.WriteLine("     ****");
                        Console.Beep(250, 100);
                        Console.Beep(250, 100);
                        run = false;
                        return 0xFF;
                    }
            }
        }

        public byte FetchValue(ref ushort addr, AddressingMode Mode)//, bool IncrementAddress)
        {
            // for the operand
            switch (Mode)
            {
                case AddressingMode.IMM:
                    {
                        return mem.Read(addr++);
                    }
                default:
                    {
                        return mem.Read(FetchAddress(ref addr, Mode));
                    }
            }
        }

        public void Execute()
        {
            switch (OpCode)
            {
                case opCodes.LDX_IMM:
                    {
                        this.X = FetchValue(ref PC, AddressingMode.IMM);
                        this.F.Z = (X == 0);
                        this.F.N = (X & 0x80) != 0;
                        break;
                    }
                case opCodes.SEI:
                    {
                        this.F.I = true;
                        break;
                    }
                case opCodes.TXS:
                    {
                        SP = X;
                        break;
                    }
                case opCodes.CLD:
                    {
                        F.D = false;
                        break;
                    }
                case opCodes.JSR:
                    {
                        Push((byte)(((PC + 2) & 0xFF00) >> 8));
                        Push((byte)(((PC + 2) & 0x00FF)));
                        PC = FetchAddress(ref PC, AddressingMode.ABS);
                        break;
                    }
                case opCodes.LDA_ABS_X:
                    {
                        A = FetchValue(ref PC, AddressingMode.ABS_X);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;

                        break;
                    }
                case opCodes.CMP_ABS_X:
                    {
                        byte operand = FetchValue(ref PC, AddressingMode.ABS_X);
                        sbyte result = (sbyte)(A - operand);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        F.C = (operand > A);
                        break;
                    }
                case opCodes.BNE_REL:
                    {
                        if (!F.Z)
                        {
                            PC = FetchAddress(ref PC, AddressingMode.REL);
                        }
                        else
                        {
                            PC++;
                        }
                        break;
                    }
                case opCodes.RTS:
                    {
                        PC = (ushort)(Pop() | (Pop() << 8));
                        break;
                    }
                case opCodes.STX_ABS:
                    {
                        mem.Write((ushort)FetchAddress(ref PC, AddressingMode.ABS), X);
                        break;
                    }
                case opCodes.DEX:
                    {
                        X--;
                        F.Z = (X == 0);
                        F.N = ((X & 0x80) != 0);

                        break;
                    }
                case opCodes.LDA_IMM:
                    {
                        A = FetchValue(ref PC, AddressingMode.IMM);
                        //Cycles += 2;
                        this.F.Z = (A == 0);
                        this.F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.STA_ABS:
                    {
                        mem.Write(FetchAddress(ref PC, AddressingMode.ABS), A);
                        break;
                    }
                case opCodes.STA_ZP:
                    {
                        mem.Write(FetchAddress(ref PC, AddressingMode.ZP), A);
                        break;
                    }
                case opCodes.LDA_ABS:
                    {
                        A = FetchValue(ref PC, AddressingMode.ABS);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.BEQ_REL:
                    {
                        if (F.Z)
                        {
                            PC = FetchAddress(ref PC, AddressingMode.REL);
                        }
                        else
                        {
                            PC++;
                        }
                        break;
                    }
                case opCodes.JMP_ABS:
                    {
                        PC = FetchAddress(ref PC, AddressingMode.ABS);
                        break;
                    }
                case opCodes.AND_IMM:
                    {
                        A = (byte)(A & FetchValue(ref PC, AddressingMode.IMM));
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.ORA_IMM:
                    {
                        A = (byte)(A | FetchValue(ref PC, AddressingMode.IMM));
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.TAY:
                    {
                        Y = A;
                        F.Z = (Y == 0);
                        F.N = (Y & 0x80) != 0;
                        break;
                    }
                case opCodes.STA_ABS_Y:
                    {
                        mem.Write(FetchAddress(ref PC, AddressingMode.ABS_Y), A);
                        break;
                    }
                case opCodes.INY:
                    {
                        Y++;
                        F.N = (Y & 0x80) != 0;
                        F.Z = Y == 0;
                        break;
                    }
                case opCodes.LDY_IMM:
                    {
                        Y = FetchValue(ref PC, AddressingMode.IMM);
                        F.N = (Y & 0x80) != 0;
                        F.Z = Y == 0;
                        break;
                    }
                case opCodes.STX_ZP:
                    {
                        mem.Write((ushort)(FetchValue(ref PC, AddressingMode.ZP)), X);
                        break;
                    }
                case opCodes.STY_ZP:
                    {
                        mem.Write((ushort)(FetchValue(ref PC, AddressingMode.ZP)), Y);
                        break;
                    }
                case opCodes.INC_ZP:
                    {
                        ushort address = FetchAddress(ref PC, AddressingMode.ZP); // need to retain address
                        byte value = (byte)(mem.Read(address) + 1); // don't use GetValue as it inteprets the address as an operand
                        mem.Write(address, value);
                        F.Z = (value == 0);
                        F.N = (value & 0x80) != 0;
                        break;
                    }
                case opCodes.LDA_IND_Y:
                    {
                        A = FetchValue(ref PC, AddressingMode.IND_Y);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.TAX:
                    {
                        X = A;
                        F.Z = (X == 0);
                        F.N = (X & 0x80) != 0;
                        break;
                    }
                case opCodes.STA_IND_Y:
                    {
                        mem.Write((ushort)(FetchAddress(ref PC, AddressingMode.IND_Y)), A);
                        break;
                    }
                case opCodes.CMP_IND_Y:
                    {
                        sbyte value = (sbyte)(FetchValue(ref PC, AddressingMode.IND_Y));
                        sbyte cmp = (sbyte)((sbyte)A - value);
                        F.Z = (cmp == 0);
                        F.N = (cmp & 0x80) != 0;
                        F.C = value > A;
                        break;
                    }
                case opCodes.ROL:
                    {
                        F.C = (A & 0x80) != 0;     // C <- [76543210]
                        A <<= 1;                   // ROL A
                        A += (byte)(F.C ? 1 : 0);  // [76543210] <- C
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.TXA:
                    {
                        A = X;
                        F.N = (A & 0x80) != 0;
                        F.Z = (A == 0);
                        break;
                    }
                case opCodes.TYA:
                    {
                        A = Y;
                        F.N = (A & 0x80) != 0;
                        F.Z = (A == 0);
                        break;
                    }
                case opCodes.LDY_ZP:
                    {
                        Y = FetchValue(ref PC, AddressingMode.ZP);
                        F.Z = Y == 0;
                        F.N = (Y & 0x80) != 0;
                        break;
                    }
                case opCodes.CLC:
                    {
                        F.C = false;
                        break;
                    }
                case opCodes.STY_ABS:
                    {
                        mem.Write(FetchAddress(ref PC, AddressingMode.ABS), Y);
                        break;
                    }
                case opCodes.LDA_ABS_Y:
                    {
                        A = FetchValue(ref PC, AddressingMode.ABS_Y);
                        F.Z = A == 0;
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.BCS_REL:
                    {
                        if (F.C)
                        {
                            PC = FetchAddress(ref PC, AddressingMode.REL);
                        }
                        else
                        {
                            PC++;
                        }
                        break;
                    }
                case opCodes.DEY:
                    {
                        Y--;
                        F.Z = Y == 0;
                        F.N = (Y & 0x80) != 0;
                        break;
                    }
                case opCodes.BPL:
                    {
                        if (!F.N)
                        {
                            PC = FetchAddress(ref PC, AddressingMode.REL);
                        }
                        else
                        {
                            PC++;
                        }
                        break;
                    }
                case opCodes.STA_ABS_X:
                    {
                        mem.Write(FetchAddress(ref PC, AddressingMode.ABS_X), A);
                        break;
                    }
                case opCodes.STY_ZP_X:
                    {
                        mem.Write(FetchAddress(ref PC, AddressingMode.ZP_X), Y);
                        break;
                    }
                case opCodes.ADC_IMM:
                    {
                        int result = A + FetchValue(ref PC, AddressingMode.IMM) + (F.C ? 1 : 0);
                        F.C = (result > 0xFF);
                        F.O = (result < -128 || result > 127);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        A = (byte)result;
                        break;
                    }
                case opCodes.BCC_REL:
                    {
                        if (!F.C)
                        {
                            PC = FetchAddress(ref PC, AddressingMode.REL);
                        }
                        else
                        {
                            PC++;
                        }
                        break;
                    }
                case opCodes.INX:
                    {
                        X++;
                        F.Z = X == 0;
                        F.N = (X & 0x80) != 0;
                        break;
                    }
                case opCodes.CPX_IMM:
                    {
                        sbyte value = (sbyte)(FetchValue(ref PC, AddressingMode.IMM));
                        sbyte cmp = (sbyte)((sbyte)X - value);
                        F.Z = (cmp == 0);
                        F.N = (cmp & 0x80) != 0;
                        F.C = value > X;
                        break;
                    }
                case opCodes.STA_ZP_X:
                    {
                        mem.Write(FetchAddress(ref PC, AddressingMode.ZP_X), A);
                        break;
                    }
                case opCodes.LDA_ZP_X:
                    {
                        A = FetchValue(ref PC, AddressingMode.ZP_X);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.ORA_ABS:
                    {
                        A = (byte)(A | FetchValue(ref PC, AddressingMode.ABS));
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.LDA_ZP:
                    {
                        A = FetchValue(ref PC, AddressingMode.ZP);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                default:
                    {
                        Debug.WriteLine(String.Format("**** {1:X4}: OP Code {0:X2} not implemented.", (byte)OpCode, LastFetchAddr));
                        run = false;
                        PC--;
                        Console.Beep(1000, 1000);
                        break;
                    }
            }
        }

        public string Disassemble(ushort Addr)
        {
            opCodes OpCode = (opCodes)mem.Read(Addr);
            string Assembler = String.Format("??? {0:X2} {1:X2} {2:X2}", (byte)OpCode, mem.Read((ushort)(Addr + 1)), mem.Read((ushort)(Addr + 2)));

            switch (OpCode)
            {
                case opCodes.LDX_IMM:
                    {
                        Assembler = String.Format("LDX #{0:X2}", mem.Read((ushort)(Addr + 1)));
                        break;
                    }
                case opCodes.SEI:
                    {
                        Assembler = "SEI";
                        break;
                    }
                case opCodes.TXS:
                    {
                        Assembler = "TXS";
                        break;
                    }
                case opCodes.CLD:
                    {
                        Assembler = "CLD";
                        break;
                    }
                case opCodes.JSR:
                    {
                        Assembler = String.Format("JSR ${0:X4}", mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        break;
                    }
                case opCodes.LDA_ABS_X:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("LDA ${0:X4}+X  ${1:X4}", operand, operand + X);
                        break;
                    }
                case opCodes.CMP_ABS_X:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("CMP ${0:X4}+X  ${1:X4}", operand, operand + X);
                        break;
                    }
                case opCodes.BNE_REL:
                    {
                        sbyte rel = (sbyte)mem.Read((ushort)(Addr + 1));
                        Assembler = String.Format("BNE {0:X4}", Addr + 2 + rel);
                        break;
                    }
                case opCodes.RTS:
                    {
                        Assembler = "RTS";
                        break;
                    }
                case opCodes.STX_ABS:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("STX ${0:X4}", operand);
                        break;
                    }
                case opCodes.DEX:
                    {
                        Assembler = "DEX";
                        break;
                    }
                case opCodes.LDA_IMM:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("LDA #{0:X2}", operand);
                        break;
                    }
                case opCodes.STA_ABS:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("STA ${0:X4}", operand);
                        break;
                    }
                case opCodes.STA_ZP:
                    {
                        byte zp = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("STA ${0:X2}", zp);
                        break;
                    }
                case opCodes.LDA_ABS:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("LDA ${0:X4}", operand);
                        break;
                    }
                case opCodes.BEQ_REL:
                    {
                        sbyte rel = (sbyte)mem.Read((ushort)(Addr + 1));
                        Assembler = String.Format("BEQ {0:X4}", Addr + 2 + rel);
                        break;
                    }
                case opCodes.JMP_ABS:
                    {
                        ushort target = (ushort)(mem.Read((ushort)(Addr + 1)) | (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("JMP ${0:X4}", target);
                        break;
                    }
                case opCodes.AND_IMM:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("AND #{0:X2}", operand);
                        break;
                    }
                case opCodes.ORA_IMM:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("ORA #{0:X2}", operand);
                        break;
                    }
                case opCodes.TAY:
                    {
                        Assembler = "TAY";
                        break;
                    }
                case opCodes.STA_ABS_Y:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)) + (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("STA ${0:X4}+Y  {1:X4}", operand, operand + Y);
                        break;
                    }
                case opCodes.INY:
                    {
                        Assembler = "INY";
                        break;
                    }
                case opCodes.LDY_IMM:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("LDY #{0:X2}", operand);
                        break;
                    }
                case opCodes.STX_ZP:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("STX {0:X4}", operand);
                        break;
                    }
                case opCodes.STY_ZP:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("STY {0:X4}", operand);
                        break;
                    }
                case opCodes.INC_ZP:
                    {
                        byte operand = mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("INC {0:X4}", operand);
                        break;
                    }
                case opCodes.LDA_IND_Y:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)));
                        ushort address = (ushort)(mem.Read(operand) + mem.Read((ushort)(operand + 1)) << 8);
                        Assembler = string.Format("LDA ({0:X2}),Y  {1:X4}", operand, address + Y);
                        break;
                    }
                case opCodes.TAX:
                    {
                        Assembler = "TAX";
                        break;
                    }
                case opCodes.STA_IND_Y:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)));
                        ushort address = (ushort)(mem.Read(operand) + mem.Read((ushort)(operand + 1)) << 8);
                        Assembler = string.Format("STA ({0:X2}),Y  {1:X4}", operand, address);
                        break;
                    }
                case opCodes.CMP_IND_Y:
                    {
                        ushort operand = (ushort)(mem.Read((ushort)(Addr + 1)));
                        ushort address = (ushort)(mem.Read(operand) + mem.Read((ushort)(operand + 1)) << 8);
                        Assembler = string.Format("CMP ({0:X2}),Y  {1:X4}", operand, address);
                        break;
                    }
                case opCodes.ROL:
                    {
                        Assembler = "ROL";
                        break;
                    }
                case opCodes.TXA:
                    {
                        Assembler = "TXA";
                        break;
                    }
                case opCodes.TYA:
                    {
                        Assembler = "TYA";
                        break;
                    }
                case opCodes.LDY_ZP:
                    {
                        byte operand = (byte)(mem.Read((byte)(Addr + 1)));
                        Assembler = string.Format("LDY ${0:X4}", operand);
                        break;
                    }
                case opCodes.CLC:
                    {
                        Assembler = "CLC";
                        break;
                    }
                case opCodes.STY_ABS:
                    {
                        ushort addr = (ushort)(mem.Read((ushort)(Addr + 1)) + (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("STY {0:X4}", addr);
                        break;
                    }
                case opCodes.LDA_ABS_Y:
                    {
                        ushort addr = (ushort)(mem.Read((ushort)(Addr + 1)) + (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("LDA {0:X4},Y   {1:X4}", addr, addr + Y);
                        break;
                    }
                case opCodes.BCS_REL:
                    {
                        sbyte operand = (sbyte)mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("BCS ${0:X4}", Addr + operand + 2);
                        break;
                    }
                case opCodes.DEY:
                    {
                        Assembler = "DEY";
                        break;
                    }
                case opCodes.BPL:
                    {
                        sbyte operand = (sbyte)mem.Read((ushort)(Addr + 1));
                        Assembler = string.Format("BPL ${0:X4}", Addr + operand + 2);
                        break;
                    }
                case opCodes.STA_ABS_X:
                    {
                        ushort addr = (ushort)(mem.Read((ushort)(Addr + 1)) + (mem.Read((ushort)(Addr + 2)) << 8));
                        Assembler = string.Format("STA ${0:X4},X  {1:X4}", addr, addr + X);
                        break;
                    }
                case opCodes.STY_ZP_X:
                    {
                        ushort addr = (ushort)(mem.Read((ushort)(Addr + 1)) + X);
                        ushort lookup = (ushort)(mem.Read(addr) + mem.Read((ushort)(addr + 1)) << 8);
                        Assembler = string.Format("STY ${0:X2},X  {1:X4}", addr + X, lookup);
                        break;
                    }
            }

            return Assembler;

        }
    }
}
