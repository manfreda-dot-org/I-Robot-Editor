using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using I_Robot.GameStructures.Playfield;

namespace I_Robot.Roms
{
    public class ROM
    {
        public readonly String Filename;
        public readonly UInt32 Checksum;
        public readonly byte[] Data = null;

        // hide the constructor - force user to call TryLoad() method
        ROM(String filename)
        {
            Filename = filename;

            Data = System.IO.File.ReadAllBytes(filename);

            // calculate checksum
            for (int n = 0; n < Data.Length; n++)
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
    }

    /// <summary>
    /// Represents an instance of the I,Robot game rom 136029.206
    /// </summary>
    public class Rom206
    {
        /// <summary>
        /// This is the base address of level pointer table
        /// It is a table of 52 pointers to LevelInfo strucutres
        /// Represents the first 52 levels
        /// </summary>
        public const UInt16 LEVEL_TABLE_ADDRESS = 0x5510;

        /// <summary>
        /// Base address of the PlayfieldInfo table
        /// The table is 26 entries long
        /// And contains 24 game levels (2 levels are duplicated)
        /// plus 2 unused levels left over in the game
        /// </summary>
        public const UInt16 PLAYFIELD_INFO_ADDRESS = 0x531E;

        /// <summary>
        /// Base address of the playfield row table
        /// The table is 251 entries long, corresponding to 251 predefined row templates
        /// Each entry in the table is 16 bytes, corresponding to 16 tiles in a row
        /// The predefined rows are combined together to build chunks
        /// And the chunks are combined to build levels
        /// </summary>
        public const UInt16 PLAYFIELD_ROW_TABLE = 0x436E;

        /// <summary>
        /// Base address of the Tile table
        /// There are 64 Tile stuctures in the ROM
        /// The actual table starts at 0x4000, however the
        /// game program uses 0x4010 as an offset, so that
        /// it can use negative indexes into the table
        /// </summary>
        public const UInt16 PLAYFIELD_TILE_TABLE = 0x4010;
        public const UInt16 PLAYFIELD_TILE_TABLE_BASE = 0x4000;

        /// <summary>
        /// This is the base address of the bonus pyramid pointer table
        /// It is a table of 52 pointers to BonusPyramidInfo structures
        /// Pointers are null if there is no bonus pyramid for the level
        /// </summary>
        public const UInt16 BONUS_PYRAMID_TABLE_ADDRESS = 0x5884;


        byte[] Data = new byte[0x2000];
        Tile[] Tile = new Tile[64];
        Row[] Row = new Row[251];
        public Dictionary<UInt16, Chunk> Chunks = new Dictionary<ushort, Chunk>();
        public Dictionary<UInt16, ChunkList> ChunkList = new Dictionary<ushort, ChunkList>();

        /// <summary>
        /// Private constructor, use TryLoad() to create a new instance
        /// </summary>
        private Rom206()
        {
            // load the ROM
            ROM.TryLoad("136029-206.bin", out ROM rom);
            System.Diagnostics.Debug.Assert(rom.Data.Length == 0x4000);
            Array.Copy(rom.Data, 0x2000, Data, 0, Data.Length);

            // read all tiles
            for (int n = 0; n < Tile.Length; n++)
                Tile[n] = new Tile(this, (n - 8) * 2);

            // read all the rows
            for (int n = 0; n < Row.Length; n++)
                Row[n] = new Row(this, n + 1);
        }

        /// <summary>
        /// Attempts to load the 136029-206.bin ROM
        /// </summary>
        /// <param name="rom"></param>
        /// <returns></returns>
        static public bool TryLoad(out Rom206 rom)
        {
            rom = null;
            try
            {
                rom = new Rom206();
                return true;
            }
            catch
            {
                Log.LogMessage("Failed to load 136029-206.bin");
                return false;
            }
        }


        public Tile GetTileAt(int offset)
        {
            return Tile[offset / 2 + 8];
        }

        public Row GetRowAt(int index)
        {
            return Row[index - 1];
        }

        public Chunk GetChunkAt(UInt16 address)
        {
            if (!Chunks.ContainsKey(address))
                Chunks.Add(address, new Chunk(this, address));
            return Chunks[address];
        }

        public ChunkList GetChunkListAt(UInt16 address)
        {
            if (!ChunkList.ContainsKey(address))
                ChunkList.Add(address, new ChunkList(this, address));
            return ChunkList[address];
        }

        public byte this[int index]
        {
            get
            {
                return Data[index - 0x4000];
            }
        }

        public UInt16 Word(int index)
        {
            return (UInt16)((this[index] << 8) + this[index + 1]);
        }
    }

    /// <summary>
    /// Represents a generic object/structure inside a Rom
    /// </summary>
    public class Rom206Reference
    {
        public readonly Rom206 Rom;
        public readonly UInt16 Address;

        public Rom206Reference(Rom206 rom, int address)
        {
            Rom = rom;
            Address = (UInt16)address;
        }

        protected byte Byte(int index)
        {
            return Rom[Address + index];
        }

        protected UInt16 Word(int index)
        {
            return (UInt16)((Byte(index) << 8) + Byte(index + 1));
        }
    }

}
