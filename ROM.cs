using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using I_Robot.GameStructures.Playfield;

namespace I_Robot.Roms
{
    /// <summary>
    /// interface for a byte addressable ROM
    /// </summary>
    public interface IRom8
    {
        int Size { get; }

        byte this[int index] { get; }
    }

    public static class RomExtensions
    {
        public static UInt16 Word(this IRom8 rom, int index)
        {
            return (UInt16)((rom[index] << 8) + rom[index + 1]);
        }
    }


    public class GameRom : IRom8
    {
        readonly byte[] Memory = new byte[0x10000];

        private GameRom(ROM r405, ROM r206, ROM r207, ROM r208, ROM r209, ROM r210)
        {
            // TBD
        }

        public byte this[int index] => Memory[index];

        public int Size => Memory.Length;

        static public bool TryCreate(out GameRom rom)
        {
            rom = null;
            if (!ROM.TryLoad("136029-405.bin", 0x4000, 0x150A97, out ROM r405)) return false;
            if (!ROM.TryLoad("136029-206.bin", 0x4000, 0x174942, out ROM r206)) return false;
            if (!ROM.TryLoad("136029-207.bin", 0x4000, 0x17384C, out ROM r207)) return false;
            if (!ROM.TryLoad("136029-208.bin", 0x2000, 0x0D5E26, out ROM r208)) return false;
            if (!ROM.TryLoad("136029-209.bin", 0x4000, 0x1A1B59, out ROM r209)) return false;
            if (!ROM.TryLoad("136029-210.bin", 0x4000, 0x179092, out ROM r210)) return false;
            rom = new GameRom(r405, r206, r207, r208, r209, r210);
            return true;
        }
    }

    public class ROM : IRom8
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

            System.Diagnostics.Debug.WriteLine($"ROM {filename} loaded, checksum = {ChecksumString(Checksum)}");
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
                Log.LogMessage($"Failed to load ROM file: {filename}");
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
                    Log.LogMessage($"ROM {filename} has incorrect size: expected = {size} bytes, actual = {rom.Size} bytes");
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
                    Log.LogMessage($"ROM {filename} has incorrect checksum: expected = {ChecksumString(checksum)}, actual = {ChecksumString(rom.Checksum)}");
                    rom = null;
                }
            }
            return rom != null;
        }
    }

    /// <summary>
    /// Represents an instance of the I,Robot game rom 136029.206
    /// </summary>
    public class Rom206 : IRom8
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
        TileRow[] Row = new TileRow[251];
        public Dictionary<UInt16, Chunk> Chunks = new Dictionary<ushort, Chunk>();
        public Dictionary<UInt16, ChunkList> ChunkList = new Dictionary<ushort, ChunkList>();

        /// <summary>
        /// Private constructor, use TryLoad() to create a new instance
        /// </summary>
        private Rom206()
        {
            GameRom.TryCreate(out GameRom r);

            // load the ROM
            ROM.TryLoad("136029-206.bin", 0x4000, 0x174942, out ROM rom);
            System.Diagnostics.Debug.Assert(rom.Data.Length == 0x4000);
            Array.Copy(rom.Data, 0x2000, Data, 0, Data.Length);

            // read all tiles
            for (int n = 0; n < Tile.Length; n++)
                Tile[n] = new Tile(this, (n - 8) * 2);

            // read all the rows
            for (int n = 0; n < Row.Length; n++)
                Row[n] = new TileRow(this, n + 1);
        }

        public int Size => Data.Length;

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

        public TileRow GetRowAt(int index)
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
    }

    /// <summary>
    /// Represents a generic object/structure inside a Rom
    /// </summary>
    public abstract class Rom206Object : IRom8
    {
        public readonly Rom206 Rom;
        public readonly UInt16 Address;

        public Rom206Object(Rom206 rom, int address)
        {
            Rom = rom;
            Address = (UInt16)address;
        }

        public byte this[int index] => Rom[Address + index];

        abstract public int Size { get; }
    }

}
