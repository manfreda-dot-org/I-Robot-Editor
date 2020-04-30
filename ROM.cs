using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I_Robot
{
    class ROM
    {
        public readonly String Filename;
        public readonly UInt32 Checksum;
        public readonly byte[] Data = null;

        // hide the constructor - force user to call Load() method
        ROM(String filename)
        {
            Filename = filename;

            Data = System.IO.File.ReadAllBytes(filename);

            // calculate checksum
            for (int n = 0; n < Size; n++)
                Checksum += Data[n];
        }

        public byte this[int index]
        {
            get { return Data[index]; }
        }

        public int Size => Data.Length;

        static String ChecksumString(UInt32 checksum)
        {
            return "$" + checksum.ToString("X").PadLeft(8, '0');
        }

        static public bool TryLoad(String filename, out ROM rom)
        {
            try
            {
                ROM r = new ROM(filename);
                rom = r;
                return true;
            }
            catch
            {
                Log.LogMessage("Failed to load ROM file \"" + filename + "\".");
                rom = null;
                return false;
            }
        }

        static public bool TryLoad(String filename, int size, out ROM rom)
        {
            if (TryLoad(filename, out rom))
            {
                if (rom.Size != size)
                {
                    System.Diagnostics.Debug.WriteLine("ROM \"" + filename + "\" has incorrect size (expected = " + size.ToString() + " bytes, actual = " + rom.Size.ToString() + " bytes)");
                    rom = null;
                }
            }

            return rom != null;
        }

        static public bool TryLoad(String filename, int size, UInt32 checksum, out ROM rom)
        {
            if (TryLoad(filename, size, out rom))
            {
                if (rom.Checksum != checksum)
                {
                    System.Diagnostics.Debug.WriteLine("ROM \"" + filename + "\" has incorrect checksum. (expected = " + ChecksumString(checksum) + ", actual = " + ChecksumString(rom.Checksum) + ")");
                    rom = null;
                }
            }
            return rom != null;
        }

        static public ROM LoadEither(String filename1, String filename2, int size)
        {
            if (!TryLoad(filename1, size, out ROM rom))
                TryLoad(filename2, size, out rom);
            return rom;
        }

        static public ROM LoadEither(String filename1, String filename2, int size, UInt32 checksum)
        {
            if (!TryLoad(filename1, size, checksum, out ROM rom))
                TryLoad(filename2, size, checksum, out rom);
            return rom;
        }
    }

}
