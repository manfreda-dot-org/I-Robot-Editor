using I_Robot.Roms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace I_Robot.Mathbox
{
    // Mathbox addresses 16-bit data mapped from $0000-????
    //
    //  address   description
    // 0000-0FFF  RAM shared with CPU
    // 1000-1FFF  RAM shared with video processor IC ???
    // 2000-2FFF  Mathbox ROM data bank 1
    // 3000-3FFF  Mathbox ROM data bank 2
    // 4000-4FFF  Mathbox ROM data bank 3
    // 5000-5FFF  Mathbox ROM data bank 4
    // 6000-6FFF  Mathbox ROM data bank 5
    // 7000-7FFF  Mathbox ROM data bank 6
    class Memory
    {
        /// <summary>
        /// Represents a banked address used by the CPU when accessing the Mathbox ROMs
        /// </summary>
        public struct CpuBankAddress
        {
            public readonly byte Bank;
            public readonly UInt16 Address;

            public CpuBankAddress(byte bank, UInt16 address)
            {
                Bank = bank;
                Address = address;

                CpuBankAddress temp = new CpuBankAddress(MathboxAddress);
                System.Diagnostics.Debug.Assert(Bank == temp.Bank && Address == temp.Address);
            }

            public CpuBankAddress(UInt16 mb_address)
            {
                Bank = (byte)((mb_address / 0x1000) + 1);
                Address = (UInt16)(0x2000 + ((mb_address * 2) & 0x1FFF));

                System.Diagnostics.Debug.Assert(mb_address == MathboxAddress);
            }

            public UInt16 MathboxAddress
            {
                get
                {
                    UInt16 addr = (UInt16)(0x1000 * (Bank - 1));
                    addr += (UInt16)((Address - 0x2000) / 2);
                    return addr;
                }
            }

            public override string ToString()
            {
                return $"{Bank}:{Address.ToString("X4")}";
            }
        }

        // memory accessible directly by mathbox functions (low endian format)
        UInt16[] Data = new UInt16[0x8000];

        /// <summary>
        /// Creates a new Mathbox memory object, using data from I,Robot ROMs
        /// </summary>
        public Memory()
        {
            try
            {
                // unpack ROMs into our memory map, starting at address $2000
                int address = 0x2000;

                ROM.TryLoad("136029-103.bin", 0x2000, 0x6A797, out ROM low);
                ROM.TryLoad("136029-104.bin", 0x2000, 0x43382, out ROM high);
                for (int s = 0; s < 0x2000; s++)
                    Data[address++] = (UInt16)(high[s] * 256 + low[s]);

                ROM.TryLoad("136029-101.bin", 0x4000, 0x150247, out low);
                ROM.TryLoad("136029-102.bin", 0x4000, 0xF557F, out high);
                for (int s = 0; s < 0x4000; s++)
                    Data[address++] = (UInt16)(high[s] * 256 + low[s]);

#if false
                byte[] file = new byte[0x6000 * 2];
                int addr = 0;
                for (int n = 0x2000; n <= 0x7FFF; n++)
                { 
                    file[addr++] = (byte)(Data[n] >> 8);
                    file[addr++] = (byte)(Data[n] >> 0);
                }
                System.IO.File.WriteAllBytes("mathbox.bin", file);

                byte[] bank = new byte[0x2000];
                addr = 0;
                for (int b = 3; b <= 8; b++)
                {
                    Array.Copy(file, (b - 3) * 0x2000, bank, 0, 0x2000);
                    System.IO.File.WriteAllBytes("2000." + b.ToString() + ".bin", bank);
                }
#endif
            }
            catch
            {
                Log.LogMessage("Failed to load mathbox ROMs");
            }
        }

        public UInt16 this[int index]
        {
            get { return Data[index]; }
            set
            {
                if (index < 0x2000)
                    Data[index] = value;
                else
                    new System.Data.ReadOnlyException();
            }
        }
    }
}
