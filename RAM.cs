using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU6502
{
    internal class RAM
    {
        private byte[] mem = new byte[65536];
        private byte[] BASICROM = new byte[0xc000 - 0xa000];
        static readonly string BASICROM_FILENAME = "ROMS\\basic";

        private byte[] KERNALROM = new byte[0x10000 - 0xe000];
        static readonly string KERNALROM_FILENAME = "ROMS\\kernal";

        private byte[] CHARROM = new byte[0xe000 - 0xd000];
        static readonly string CHARROM_FILENAME = "ROMS\\chargen";

        public RAM()
        {
            // Setup I/O and mapping registers
            mem[0] = 0xff;
            mem[1] = 0x1f;

            BASICROM = File.ReadAllBytes(BASICROM_FILENAME);
            KERNALROM = File.ReadAllBytes(KERNALROM_FILENAME);
            CHARROM = File.ReadAllBytes(CHARROM_FILENAME);
        }

        public byte Read(ushort addr)
        {
            if (addr >= 0xa000 && addr <= 0xbfff && (mem[1] & 0x1) != 0)
            {
                return BASICROM[addr - 0xa000]; // return BASIC ROM
            }
            else if (addr >= 0xe000 && addr <= 0xffff && (mem[1] & 0x2) != 0)
            {
                return KERNALROM[addr - 0xe000]; // return KERNAL ROM
            }
            else if (addr >= 0xd000 && addr <= 0xdfff && (mem[1] & 0x4) != 0)
            {
                return CHARROM[addr - 0xd000];
            }
            else
            {
                return mem[addr];
            }
        }

        public void Write(ushort addr, byte value)
        { mem[addr] = value; }

    }
}
