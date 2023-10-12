using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
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
        public int StopAtMemoryRead = -1;
        public int StopAtMemoryWrite = -1;
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

        enum opCodes : byte
        {
            PHP = 0x08,
            ORA_IMM = 0x09,
            ORA_ABS = 0x0D,
            BPL = 0x10,
            ASL_ZP_X = 0x16,
            CLC = 0x18,
            JSR = 0x20,
            BIT_ZP=0x24,
            PLP = 0x28,
            AND_IMM = 0x29,
            ROL = 0x2A,
            BMI_REL = 0x30,
            SEC = 0x38,
            LSR_ZP = 0x46,
            PHA = 0x48,
            EOR_IMM = 0x49,
            JMP_ABS = 0x4C,
            LSR_ZP_X = 0x56,
            CLI = 0x58,
            RTS = 0x60,
            ADC_ZP = 0x65,
            PLA = 0x68,
            JMP_IND = 0x6C,
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
            LDX_ZP = 0xA6,
            TAY = 0xA8,
            LDA_IMM = 0xA9,
            TAX = 0xAA,
            LDY_ABS = 0xAC,
            LDA_ABS = 0xAD,
            LDX_ABS = 0xAE,
            BCS_REL = 0xB0,
            LDA_IND_Y = 0xB1,
            LDY_ZP_X = 0xB4,
            LDA_ZP_X = 0xB5,
            LDA_ABS_Y = 0xB9,
            LDA_ABS_X = 0xBD,
            CPY_ZP = 0xC4,
            CMP_ZP = 0xC5,
            DEC_ZP = 0xC6,
            INY = 0xC8,
            CMP_IMM = 0xC9,
            DEX = 0xCA,
            DEC_ABS = 0xCE,
            BNE_REL = 0xD0,
            CMP_IND_Y = 0xD1,
            CLD = 0xD8,
            CMP_ABS_X = 0xDD,
            CPX_IMM = 0xE0,
            SBC_IND_X=0xE1,
            SBC_ZP = 0xE5,
            INC_ZP = 0xE6,
            INX = 0xE8,
            SBC_IMM = 0xE9,
            SBC_ABS = 0xED,
            INC_ABS = 0xEE,
            BEQ_REL = 0xF0,
            SBC_IND_Y=0xF1,
            SBC_ZP_X = 0xF5,
            SBC_ABS_Y=0xF9,
            SBC_ABS_X = 0xFD
        }

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
                        return (ushort)((mem.Read(addr++) + X) & 0xFF);
                    }
                case AddressingMode.ZP_Y:
                    {
                        return (ushort)((mem.Read(addr++) + Y) & 0xFF);
                    }
                case AddressingMode.IND:
                    {
                        ushort Interim = (ushort)(mem.Read(addr++) + (mem.Read(addr++) << 8));
                        return (ushort)(mem.Read(Interim) + (mem.Read(Interim + 1) << 8));
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
                        mem.Write((ushort)(FetchAddress(ref PC, AddressingMode.ZP)), X);
                        break;
                    }
                case opCodes.STY_ZP:
                    {
                        mem.Write((ushort)(FetchAddress(ref PC, AddressingMode.ZP)), Y);
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
                case opCodes.LDX_ZP:
                    {
                        X = FetchValue(ref PC, AddressingMode.ZP);
                        F.Z = (X == 0);
                        F.N = (X & 0x80) != 0;
                        break;
                    }
                case opCodes.LDY_ZP_X:
                    {
                        Y = FetchValue(ref PC, AddressingMode.ZP_X);
                        F.Z = (Y == 0);
                        F.N = (Y & 0x80) != 0;
                        break;
                    }
                case opCodes.BMI_REL:
                    {
                        if (F.N)
                        {
                            PC = FetchAddress(ref PC, AddressingMode.REL);
                        }
                        else
                        {
                            PC++;
                        }
                        break;
                    }
                case opCodes.CLI:
                    {
                        F.I = false;
                        break;
                    }
                case opCodes.JMP_IND:
                    {
                        PC = FetchAddress(ref PC, AddressingMode.IND);
                        break;
                    }
                case opCodes.SEC:
                    {
                        F.C = true;
                        break;
                    }
                case opCodes.LDX_ABS:
                    {
                        X = FetchValue(ref PC, AddressingMode.ABS);
                        F.Z = (X == 0);
                        F.N = (X & 0x80) != 0;
                        break;
                    }
                case opCodes.LDY_ABS:
                    {
                        Y = FetchValue(ref PC, AddressingMode.ABS);
                        F.Z = (Y == 0);
                        F.N = (Y & 0x80) != 0;
                        break;
                    }
                case opCodes.CPY_ZP:
                    {
                        sbyte value = (sbyte)(FetchValue(ref PC, AddressingMode.ZP));
                        sbyte cmp = (sbyte)((sbyte)Y - value);
                        F.Z = (cmp == 0);
                        F.N = (cmp & 0x80) != 0;
                        F.C = value > Y;
                        break;
                    }
                case opCodes.CMP_ZP:
                    {
                        sbyte value = (sbyte)(FetchValue(ref PC, AddressingMode.ZP));
                        sbyte cmp = (sbyte)((sbyte)A - value);
                        F.Z = (cmp == 0);
                        F.N = (cmp & 0x80) != 0;
                        F.C = value > A;
                        break;
                    }
                case opCodes.ADC_ZP:
                    {
                        int result = A + FetchValue(ref PC, AddressingMode.ZP) + (F.C ? 1 : 0);
                        F.C = (result > 0xFF);
                        F.O = (result < -128 || result > 127);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        A = (byte)result;
                        break;
                    }
                case opCodes.CMP_IMM:
                    {
                        sbyte value = (sbyte)(FetchValue(ref PC, AddressingMode.IMM));
                        sbyte cmp = (sbyte)((sbyte)A - value);
                        F.Z = (cmp == 0);
                        F.N = (cmp & 0x80) != 0;
                        F.C = value > A;
                        break;
                    }
                case opCodes.SBC_IMM:
                    {
                        int result = A - FetchValue(ref PC, AddressingMode.IMM) - (!F.C ? 1 : 0);
                        F.C = (result > 0xFF);
                        F.O = (result < -128 || result > 127);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        A = (byte)result;
                        break;
                    }
                case opCodes.PHP:
                    {
                        byte flagByte = (byte)(F.ToByte() | 0x30);  // set BRK and bit 5
                        Push(flagByte);
                        break;
                    }
                case opCodes.PHA:
                    {
                        Push(A);
                        break;
                    }
                case opCodes.PLA:
                    {
                        A = Pop();
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.PLP:
                    {
                        F.FromByte((byte)(Pop() & 0xCF));      // ignore BRK and bit 5
                        break;
                    }
                case opCodes.DEC_ZP:
                    {
                        ushort address = FetchAddress(ref PC, AddressingMode.ZP); // need to retain address
                        byte value = (byte)(mem.Read(address) - 1); // don't use GetValue as it inteprets the address as an operand
                        mem.Write(address, value);
                        F.Z = (value == 0);
                        F.N = (value & 0x80) != 0;
                        break;
                    }
                case opCodes.DEC_ABS:
                    {
                        ushort address = FetchAddress(ref PC, AddressingMode.ABS); // need to retain address
                        byte value = (byte)(mem.Read(address) - 1); // don't use GetValue as it inteprets the address as an operand
                        mem.Write(address, value);
                        F.Z = (value == 0);
                        F.N = (value & 0x80) != 0;
                        break;
                    }
                case opCodes.INC_ABS:
                    {
                        ushort address = FetchAddress(ref PC, AddressingMode.ABS); // need to retain address
                        byte value = (byte)(mem.Read(address) + 1); // don't use GetValue as it inteprets the address as an operand
                        mem.Write(address, value);
                        F.Z = (value == 0);
                        F.N = (value & 0x80) != 0;
                        break;
                    }
                case opCodes.ASL_ZP_X:
                    {
                        ushort addr = FetchAddress(ref PC, AddressingMode.ZP_X);
                        int value = mem.Read(addr) << 1;
                        F.C = (value & 0x100) != 0;
                        F.Z = (value & 0xFF) == 0;
                        F.N = (value & 0x80) != 0;
                        mem.Write(addr, (byte)value);
                        break;
                    }
                case opCodes.LSR_ZP_X:
                    {
                        ushort addr = FetchAddress(ref PC, AddressingMode.ZP_X);
                        int value = mem.Read(addr);
                        F.C = (value & 0x01) != 0;
                        F.Z = (value & 0xFE) == 0;
                        F.N = false;
                        mem.Write(addr, (byte)(value >> 1));
                        break;
                    }
                case opCodes.LSR_ZP:
                    {
                        ushort addr = FetchAddress(ref PC, AddressingMode.ZP);
                        int value = mem.Read(addr);
                        F.C = (value & 0x01) != 0;
                        F.Z = (value & 0xFE) == 0;
                        F.N = false;
                        mem.Write(addr, (byte)(value >> 1));
                        break;
                    }
                case opCodes.EOR_IMM:
                    {
                        A ^= FetchValue(ref PC, AddressingMode.IMM);
                        F.Z = (A == 0);
                        F.N = (A & 0x80) != 0;
                        break;
                    }
                case opCodes.SBC_ZP:
                    {
                        int result = A - FetchValue(ref PC, AddressingMode.ZP) - (!F.C ? 1 : 0);
                        F.C = (result > 0xFF);
                        F.O = (result < -128 || result > 127);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        A = (byte)result;
                        break;
                    }
                case opCodes.SBC_ZP_X:
                    {
                        int result = A - FetchValue(ref PC, AddressingMode.ZP_X) - (!F.C ? 1 : 0);
                        F.C = (result > 0xFF);
                        F.O = (result < -128 || result > 127);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        A = (byte)result;
                        break;
                    }
                case opCodes.SBC_ABS:
                    {
                        int result = A - FetchValue(ref PC, AddressingMode.ABS) - (!F.C ? 1 : 0);
                        F.C = (result > 0xFF);
                        F.O = (result < -128 || result > 127);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        A = (byte)result;
                        break;
                    }
                case opCodes.SBC_ABS_X:
                    {
                        int result = A - FetchValue(ref PC, AddressingMode.ABS_X) - (!F.C ? 1 : 0);
                        F.C = (result > 0xFF);
                        F.O = (result < -128 || result > 127);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        A = (byte)result;
                        break;
                    }
                case opCodes.SBC_ABS_Y:
                    {
                        int result = A - FetchValue(ref PC, AddressingMode.ABS_Y) - (!F.C ? 1 : 0);
                        F.C = (result > 0xFF);
                        F.O = (result < -128 || result > 127);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        A = (byte)result;
                        break;
                    }
                case opCodes.SBC_IND_X:
                    {
                        int result = A - FetchValue(ref PC, AddressingMode.X_IND) - (!F.C ? 1 : 0);
                        F.C = (result > 0xFF);
                        F.O = (result < -128 || result > 127);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        A = (byte)result;
                        break;
                    }
                case opCodes.SBC_IND_Y:
                    {
                        int result = A - FetchValue(ref PC, AddressingMode.IND_Y) - (!F.C ? 1 : 0);
                        F.C = (result > 0xFF);
                        F.O = (result < -128 || result > 127);
                        F.Z = (result == 0);
                        F.N = (result & 0x80) != 0;
                        A = (byte)result;
                        break;
                    }
                case opCodes.BIT_ZP:
                    {

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

        public string Disassemble(int Addr)
        {
            opCodes OpCode = (opCodes)mem.Read(Addr);
            string Assembler = "";

            if (OpCode.ToString().Length >= 3) // might throw if opcode not supported
            {
                Assembler = OpCode.ToString().Substring(0, 3);
            }

            switch (OpCode)
            {
                // IMM
                case opCodes.LDX_IMM:
                case opCodes.LDA_IMM:
                case opCodes.AND_IMM:
                case opCodes.ORA_IMM:
                case opCodes.LDY_IMM:
                case opCodes.CMP_IMM:
                case opCodes.SBC_IMM:
                case opCodes.CPX_IMM:
                case opCodes.ADC_IMM:
                case opCodes.EOR_IMM:
                    {
                        Assembler += DisassembleOperand(Addr + 1, AddressingMode.IMM);
                        break;
                    }


                // IMP
                case opCodes.SEI:
                case opCodes.TXS:
                case opCodes.CLD:
                case opCodes.RTS:
                case opCodes.DEX:
                case opCodes.TAY:
                case opCodes.INY:
                case opCodes.TAX:
                case opCodes.ROL:
                case opCodes.TXA:
                case opCodes.TYA:
                case opCodes.CLC:
                case opCodes.DEY:
                case opCodes.CLI:
                case opCodes.PHP:
                case opCodes.PHA:
                case opCodes.PLP:
                case opCodes.SEC:
                case opCodes.INX:
                    {
                        break;
                    }


                // ABS
                case opCodes.JSR:
                case opCodes.STX_ABS:
                case opCodes.STA_ABS:
                case opCodes.LDA_ABS:
                case opCodes.JMP_ABS:
                case opCodes.STY_ABS:
                case opCodes.ORA_ABS:
                case opCodes.LDX_ABS:
                case opCodes.LDY_ABS:
                case opCodes.DEC_ABS:
                case opCodes.INC_ABS:
                case opCodes.SBC_ABS:
                    {
                        Assembler += DisassembleOperand(Addr + 1, AddressingMode.ABS);
                        break;
                    }


                // ABS,X
                case opCodes.LDA_ABS_X:
                case opCodes.CMP_ABS_X:
                case opCodes.STA_ABS_X:
                case opCodes.SBC_ABS_X:
                    {
                        Assembler += DisassembleOperand(Addr + 1, AddressingMode.ABS_X); //string.Format("LDA ${0:X4}+X  ${1:X4}", operand, operand + X);
                        break;
                    }


                // ABS_Y
                case opCodes.STA_ABS_Y:
                case opCodes.LDA_ABS_Y:
                case opCodes.SBC_ABS_Y:
                    {
                        Assembler += DisassembleOperand(Addr + 1, AddressingMode.ABS_Y);
                        break;
                    }


                // REL
                case opCodes.BNE_REL:
                case opCodes.BEQ_REL:
                case opCodes.BPL:
                case opCodes.BCS_REL:
                case opCodes.BMI_REL:
                case opCodes.BCC_REL:
                    {
                        Assembler += DisassembleOperand(Addr + 1, AddressingMode.REL);
                        break;
                    }


                // ZP
                case opCodes.STA_ZP:
                case opCodes.STX_ZP:
                case opCodes.STY_ZP:
                case opCodes.INC_ZP:
                case opCodes.LDY_ZP:
                case opCodes.LDX_ZP:
                case opCodes.LDA_ZP:
                case opCodes.CPY_ZP:
                case opCodes.CMP_ZP:
                case opCodes.ADC_ZP:
                case opCodes.DEC_ZP:
                case opCodes.LSR_ZP:
                case opCodes.SBC_ZP:
                case opCodes.BIT_ZP:
                    {
                        Assembler += DisassembleOperand(Addr + 1, AddressingMode.ZP);
                        break;
                    }


                // ZP_X
                case opCodes.STY_ZP_X:
                case opCodes.LDY_ZP_X:
                case opCodes.LDA_ZP_X:
                case opCodes.STA_ZP_X:
                case opCodes.ASL_ZP_X:
                case opCodes.LSR_ZP_X:
                case opCodes.SBC_ZP_X:
                    {
                        Assembler += DisassembleOperand(Addr + 1, AddressingMode.ZP_X);
                        break;
                    }


                // IND_Y
                case opCodes.LDA_IND_Y:
                case opCodes.STA_IND_Y:
                case opCodes.CMP_IND_Y:
                case opCodes.SBC_IND_Y:
                    {
                        Assembler += DisassembleOperand(Addr + 1, AddressingMode.IND_Y);

                        break;
                    }


                // IND
                case opCodes.JMP_IND:
                    {
                        Assembler += DisassembleOperand(Addr + 1, AddressingMode.IND);
                        break;
                    }
                default:
                    {
                        Assembler = String.Format("??? {0:X2} {1:X2} {2:X2}", (byte)OpCode, mem.Read((ushort)(Addr + 1)), mem.Read((ushort)(Addr + 2)));
                        break;
                    }
            }

            return Assembler;
        }

        string DisassembleOperand(int Addr, AddressingMode Mode)
        {
            switch (Mode)
            {
                case AddressingMode.IMM:
                    {
                        return string.Format(" #${0:X2}", mem.Read(Addr));
                    }
                case AddressingMode.ABS:
                    {
                        return string.Format(" ${0:X4}", mem.Read(Addr) + (mem.Read(Addr + 1) << 8));
                    }
                case AddressingMode.ABS_X:
                    {
                        int addr = mem.Read(Addr) + (mem.Read(Addr + 1) << 8);
                        return string.Format(" ${0:X4},X   ${1:X4}", addr, addr + X);
                    }
                case AddressingMode.ABS_Y:
                    {
                        int addr = mem.Read(Addr) + (mem.Read(Addr + 1) << 8);
                        return string.Format(" ${0:X4},Y   ${1:X4}", addr, addr + Y);
                    }
                case AddressingMode.ZP:
                    {
                        return string.Format(" ${0:X2}", mem.Read(Addr));
                    }
                case AddressingMode.REL:
                    {
                        return string.Format(" ${0:X4}", Addr + 1 + (sbyte)mem.Read(Addr));
                    }
                case AddressingMode.IND_Y:
                    {
                        ushort zpAddress = (ushort)(mem.Read(Addr));
                        ushort addr = (ushort)(mem.Read(zpAddress) + (mem.Read(zpAddress + 1) << 8));
                        return string.Format(" ${0:X4},Y   ${1:X4}", addr, addr + Y);
                    }
                case AddressingMode.ZP_X:
                    {
                        ushort zpAddress = (ushort)(mem.Read(Addr));
                        return string.Format(" ${0:X2},X   ${1:X2}", zpAddress, ((zpAddress + X) & 0xFF));
                    }
                case AddressingMode.ZP_Y:
                    {
                        ushort zpAddress = (ushort)(mem.Read(Addr));
                        return string.Format(" ${0:X2},Y   ${1:X2}", zpAddress, ((zpAddress + Y) & 0xFF));
                    }
                case AddressingMode.IND:
                    {
                        ushort interim = (ushort)(mem.Read(Addr) + (mem.Read(Addr + 1) << 8));
                        return string.Format(" $({0:X4})    ${1:X4}", interim, (mem.Read(interim) + (mem.Read(interim + 1) << 8)));
                    }
                default:
                    return string.Format("{0:X2} {1:X2}", mem.Read(Addr), mem.Read(Addr + 1));
            }
        }
    }
}
