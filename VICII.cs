using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace CPU6502
{
    internal class VICII
    {
        CIA1 cia1;
        CIA2 cia2;
        RAM mem;
        Display display;
        public bool UpdateDisplay = false;
        int CurrentRaster;
        public byte bank;
        int FramePauseNanoseconds = 10000;
        Color[] palette = { Color.Black, Color.White, Color.Red, Color.Cyan, Color.Purple, Color.Green, Color.Blue, Color.Yellow, Color.Orange, Color.Brown, Color.Pink, Color.Gray, Color.DarkGray, Color.LightGreen, Color.LightBlue, Color.LightGray };

         ushort _BaseMemory
        {
            get
            {
                return (ushort)(bank * 0x4000);
            }
        }
         ushort _VideoMatrixAddressInternal
        {
            get
            {
                return (ushort)(VideoMatrixBaseAddress * 64);
            }
        }

         ushort _CharacterMemoryInternal
        {
            get
            {
                return (ushort)(CharacterDotDataBaseAddress * 1024);
            }
        }

        // Control Register 1
        bool BlankScreenToBorderColor;
        bool Twenty5Rows;

        // Control Rogister 2: D016
        bool MultiColorMode;
        bool FortyColumns;
        byte SmoothScrollX;

        // D018
        byte VideoMatrixBaseAddress;
        byte CharacterDotDataBaseAddress;

        public VICII(ref CIA1 Cia1, ref CIA2 Cia2, RAM Ram)
        {
            Cia1 = cia1 = new CIA1(this, Ram);
            Cia2 = cia2 = new CIA2(this, Ram);

            mem = Ram;
            display = new Display();
            display.Show();
            Task.Run(DisplayRefresh);
        }

        public void Write(ushort Addr, byte Value)
        {
            mem._mem[Addr] = Value;

            switch (Addr)
            {
                case 0xD016:
                    {
                        MultiColorMode = ((byte)(Value & 0x10) != 0);
                        FortyColumns = ((byte)(Value & 0x08) != 0);
                        SmoothScrollX = ((byte)(Value & 0x07));
                        mem._mem[Addr] = Value;
                        break;
                    }

                case 0xD018:
                    {
                        VideoMatrixBaseAddress = (byte)(Value & 0xF0);
                        CharacterDotDataBaseAddress = (byte)(Value & 0x0E);
                        break;
                    }

                default:
                    {
                        Debug.WriteLine("Unhandled write to VIC-II at {0:X4} value {1:X2}", Addr, Value);
                        break;
                    }
            }
        }

        byte ReadVic(int IntAddr)   // using internal Vic maping
        {
            switch (bank)
            {
                case 0:
                case 2:
                    {
                        if (IntAddr < 0x200)   // color RAM?
                        {
                            return (byte)(mem._mem[0xd800 + IntAddr] & 0x0F);
                        }
                        if (IntAddr < 0x1000) // regular ram
                        {
                            return mem._mem[IntAddr + _BaseMemory];
                        }
                        if (IntAddr < 0x2000) // char rom
                        {
                            return mem.CHARROM[IntAddr - 0x1000];
                        }
                        return mem._mem[IntAddr + _BaseMemory];
                    }
            }

            return mem._mem[IntAddr + _BaseMemory];
        }

        void DisplayRefresh()
        {
            SolidBrush background = new(Color.Black);
            Bitmap scr = new(320, 200);
            Rectangle scale = new(0, 0, 1000, 800);

            while (!display.IsDisposed)
            {
                if (UpdateDisplay)
                {
                    int videoAddress = _VideoMatrixAddressInternal;

                    for (int c = 0; c < 40; c++)
                    {
                        byte ch = ReadVic(_VideoMatrixAddressInternal + (CurrentRaster / 8) * 40 + c);

                        byte bm = ReadVic(_CharacterMemoryInternal + ch * 8 + CurrentRaster % 8);
                        for (int b = 7; b >= 0; b--)
                        {
                            if ((bm & 0x01) != 0)
                            {
                                scr.SetPixel(c * 8 + b, CurrentRaster, palette[mem._mem[(CurrentRaster / 8) * 40 + c + 0xD800]]);
                            }
                            else
                            {
                                scr.SetPixel(c * 8 + b, CurrentRaster, palette[mem._mem[0xD021] & 0x0F]);
                            }
                            bm >>= 1;
                        }
                    }

                    CurrentRaster++;
                    CurrentRaster = (byte)(CurrentRaster % 200);
                    mem._mem[0xd012] = (byte)(CurrentRaster & 0xFF);
                    mem._mem[0xd011] = (byte)(((CurrentRaster & 0x100) >> 1) | (mem._mem[0xd011] & 0x7F));


                    if (CurrentRaster == 0)
                    {
                        display.graphics.DrawImage(scr, scale);
                    }

                    DateTime end = DateTime.Now + new TimeSpan(FramePauseNanoseconds / 100);
                    while (DateTime.Now < end)
                    {
                        Thread.Yield();
                    }
                }
            }

            Debug.WriteLine("Display thread stopped by disposal of screen.");
            Console.Beep(500, 500);
        }
    }
}
