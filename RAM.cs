using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CPU6502
{
    internal class RAM
    {
        public byte[] _mem = new byte[65536]; // All reads and writes except directly from I/O chips should go through .Read and .Write to obey mapping rules.
        private byte[] BASICROM = new byte[0xc000 - 0xa000];
        static readonly string BASICROM_FILENAME = "ROMS\\basic";

        private byte[] KERNALROM = new byte[0x10000 - 0xe000];
        static readonly string KERNALROM_FILENAME = "ROMS\\kernal";

        public byte[] CHARROM = new byte[0xe000 - 0xd000];
        static readonly string CHARROM_FILENAME = "ROMS\\chargen";

        Mapping[] MapMode;
        enum Mapping
        {
            NOMAP,
            MEM,
            ROM,
            IO,
            CART_ROM_HI,
            CART_ROM_LO
        }

        Mapping[,] Map = {
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM},            // 0
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM},            // 1
            { Mapping.MEM,Mapping.MEM,Mapping.MEM, Mapping.CART_ROM_HI,Mapping.MEM,Mapping.ROM,Mapping.ROM},   // 2
            { Mapping.MEM,Mapping.MEM,Mapping.CART_ROM_LO,Mapping.CART_ROM_HI,Mapping.MEM,Mapping.ROM,Mapping.ROM},            // 3
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM},            // 4
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.IO,Mapping.MEM},            // 5
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.CART_ROM_HI,Mapping.MEM,Mapping.MEM,Mapping.ROM},    // 6
            { Mapping.MEM,Mapping.MEM,Mapping.CART_ROM_LO,Mapping.CART_ROM_HI,Mapping.MEM,Mapping.IO,Mapping.ROM}, // 7
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM},            // 8
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.ROM,Mapping.MEM},            // 9
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.ROM,Mapping.ROM},            // 10
            { Mapping.MEM,Mapping.MEM,Mapping.CART_ROM_LO,Mapping.ROM,Mapping.MEM,Mapping.ROM,Mapping.ROM},    // 11
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM},            // 12
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.IO,Mapping.MEM},            // 13
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.IO,Mapping.ROM},            // 14
            { Mapping.MEM,Mapping.MEM,Mapping.CART_ROM_LO,Mapping.ROM,Mapping.MEM,Mapping.IO,Mapping.ROM},      // 15
            { Mapping.MEM,Mapping.NOMAP,Mapping.CART_ROM_LO,Mapping.NOMAP,Mapping.NOMAP,Mapping.IO,Mapping.CART_ROM_HI},// 16
            { Mapping.MEM,Mapping.NOMAP,Mapping.CART_ROM_LO,Mapping.NOMAP,Mapping.NOMAP,Mapping.IO,Mapping.CART_ROM_HI},// 17
            { Mapping.MEM,Mapping.NOMAP,Mapping.CART_ROM_LO,Mapping.NOMAP,Mapping.NOMAP,Mapping.IO,Mapping.CART_ROM_HI},// 18
            { Mapping.MEM,Mapping.NOMAP,Mapping.CART_ROM_LO,Mapping.NOMAP,Mapping.NOMAP,Mapping.IO,Mapping.CART_ROM_HI},// 19
            { Mapping.MEM,Mapping.NOMAP,Mapping.CART_ROM_LO,Mapping.NOMAP,Mapping.NOMAP,Mapping.IO,Mapping.CART_ROM_HI},// 20
            { Mapping.MEM,Mapping.NOMAP,Mapping.CART_ROM_LO,Mapping.NOMAP,Mapping.NOMAP,Mapping.IO,Mapping.CART_ROM_HI},// 21
            { Mapping.MEM,Mapping.NOMAP,Mapping.CART_ROM_LO,Mapping.NOMAP,Mapping.NOMAP,Mapping.IO,Mapping.CART_ROM_HI},// 22
            { Mapping.MEM,Mapping.NOMAP,Mapping.CART_ROM_LO,Mapping.NOMAP,Mapping.NOMAP,Mapping.IO,Mapping.CART_ROM_HI},// 23
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM},            // 24
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.ROM,Mapping.MEM},            // 25
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.ROM,Mapping.ROM},            // 26
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.ROM,Mapping.MEM,Mapping.ROM,Mapping.ROM},            // 27
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM},            // 28
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.IO,Mapping.MEM},            // 29
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.IO,Mapping.ROM},            // 30
            { Mapping.MEM,Mapping.MEM,Mapping.MEM,Mapping.ROM,Mapping.MEM,Mapping.IO,Mapping.ROM}             // 31
        };

        private CIA1 cia1;
        private CIA2 cia2;
        private VICII vic;

        public RAM()
        {
            // Setup I/O and mapping registers
            MapMode = new Mapping[7];

            Write(0, 0xff);
            Write(1, 0x1f);

            vic = new VICII(ref cia1, ref cia2, this);

            BASICROM = File.ReadAllBytes(BASICROM_FILENAME);
            KERNALROM = File.ReadAllBytes(KERNALROM_FILENAME);
            CHARROM = File.ReadAllBytes(CHARROM_FILENAME);
        }

        public void Write(ushort addr, byte value)
        {
            if (addr == 1)
            {
                for (int i = 0; i < 7; i++)
                {
                    MapMode[i] = Map[value & 0x1F, i];
                }
            }

            // I/O?
            if (addr >= 0xD000 && addr < 0xE000 && MapMode[5] == Mapping.IO)
            {
                byte page = (byte)(addr >> 8); // pages are easier to work with

                if (page >= 0xD0 && page < 0xD4) // VIC-II
                {
                    vic.Write(addr, value);                    
                }
                else if (page >= 0xD4 && page < 0xD8)
                {
                    // SID
                    Debug.WriteLine(string.Format("SID write at {0:X4} value {1:X2}", addr, value));

                }
                else if (page >= 0xD8 && page < 0xDC)
                {
                    // COLOR RAM
                    _mem[addr] = (byte)(value & 0x0F);
                    //Debug.WriteLine(string.Format("CRAM write at {0:X4} value {1:X2}", addr, value));

                }
                else if (page >= 0xDC && page < 0xDD) // CIA1
                {
                    cia1.Write(addr, value);

                }
                else if (page >= 0xDD && page < 0xDE) // CIA2
                {
                    cia2.Write(addr, value);

                }
                else if (page >= 0xDE)
                {
                    // I/O 1&2
                    Debug.WriteLine(string.Format("I/O write at {0:X4} value {1:X2}", addr, value));

                }

            }


            else
            {
                _mem[addr] = value;
            }
        }

        public byte Read(ushort addr)
        {
            ushort page = (ushort)(addr >> 8);

            if (page >= 0x00 && page <= 15)
            {
                return _mem[addr];
            }
            else if (page >= 0x10 && page <= 127)
            {
                if (MapMode[1] == Mapping.MEM)
                {
                    return _mem[addr];
                }
                else
                {
                    return 0;
                }
            }
            else if (page >= 0x80 && page <= 159)
            {
                switch (MapMode[2])
                {
                    case Mapping.MEM:
                        {
                            return _mem[addr];
                        }
                    case Mapping.CART_ROM_LO:
                        {
                            return 0xF0;
                            //throw new NotImplementedException("Cartridge ROM LO not implemented.");
                        }
                }
            }
            else if (page >= 0xA0 && page <= 191)
            {
                switch (MapMode[3])
                {
                    case Mapping.MEM:
                        {
                            return _mem[addr];
                        }
                    case Mapping.ROM:
                        {
                            return BASICROM[addr - 0xa000];
                        }
                    case Mapping.NOMAP:
                        {
                            return 0xFF;
                        }
                    case Mapping.CART_ROM_HI:
                        {
                            return 0x0F;
                            //throw new NotImplementedException("Cartridge ROM HI not implemented.");
                        }
                }
            }
            else if (page >= 0xC0 && page <= 207)
            {
                switch (MapMode[4])
                {
                    case Mapping.MEM:
                        {
                            return _mem[addr];
                        }
                    case Mapping.NOMAP:
                        {
                            return 0xFF;
                        }
                }
            }
            else if (page >= 0xD0 && page <= 0xDF)
            {
                switch (MapMode[5])
                {
                    case Mapping.MEM:
                        {
                            return _mem[addr];
                        }
                    case Mapping.ROM:
                        {
                            return CHARROM[addr - 0xd000];
                        }
                    case Mapping.IO:
                        {
                            return _mem[addr];    // not implemented
                        }
                }
            }
            else if (page >= 0xE0 && page <= 255)
            {
                switch (MapMode[6])
                {
                    case Mapping.MEM:
                        {
                            return _mem[addr];
                        }
                    case Mapping.ROM:
                        {
                            return KERNALROM[addr - 0xe000];
                        }
                    case Mapping.CART_ROM_HI:
                        {
                            return 0x0F;
                            //throw new NotImplementedException("Cartridge ROM HI not implemented.");
                        }
                }
            }

            throw new Exception(string.Format("Unsupported memory map mode {0:X2} for address {1:X4}", _mem[1], addr));
        }
    }
}
