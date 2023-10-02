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
        public void Write(ushort Addr, byte Value)
        {
            switch (Addr)
            {
                default:
                    {
                        Debug.WriteLine("Unhandled write to CIA2 at {0:X4} value {1:X2}", Addr, Value);
                        break;
                    }
            }
        }
    }
}
