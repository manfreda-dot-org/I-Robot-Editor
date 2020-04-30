using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace I_Robot
{
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
        public Dictionary<UInt16, PlayfieldChunk> Chunks = new Dictionary<ushort, PlayfieldChunk>();
        public Dictionary<UInt16, PlayfieldChunkList> ChunkList = new Dictionary<ushort, PlayfieldChunkList>();

        public Rom206()
        {
            // load the ROM
            byte[] rom = System.IO.File.ReadAllBytes(@".\136029.206");
            System.Diagnostics.Debug.Assert(rom.Length == 0x4000);
            Array.Copy(rom, 0x2000, Data, 0, Data.Length);

            // read all tiles
            for (int n = 0; n < Tile.Length; n++)
                Tile[n] = new Tile(this, (n - 8) * 2);

            // read all the rows
            for (int n = 0; n < Row.Length; n++)
                Row[n] = new Row(this, n+1);
        }

        public Tile GetTileAt(int offset)
        {
            return Tile[offset / 2 + 8];
        }

        public Row GetRowAt(int index)
        {
            return Row[index - 1];
        }

        public PlayfieldChunk GetChunkAt(UInt16 address)
        {
            if (!Chunks.ContainsKey(address))
                Chunks.Add(address, new PlayfieldChunk(this, address));
            return Chunks[address];
        }

        public PlayfieldChunkList GetChunkListAt(UInt16 address)
        {
            if (!ChunkList.ContainsKey(address))
                ChunkList.Add(address, new PlayfieldChunkList(this, address));
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
    public class RomObject
    {
        public readonly Rom206 Rom;
        public readonly UInt16 Address;

        public RomObject(Rom206 rom, int address)
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

    /// <summary>
    /// a Tile is the basic building block of the playfield
    /// </summary>
    public class Tile : RomObject
    {
        public const int SIZE = 128;

        static readonly string[] Hex = new string[16] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };

        public enum TYPE : byte
        {
            EMPTY_0 = 0,
            BLUE_1 = 1,
            BLUE_JEWEL_2 = 2,
            UP_DOWN_3 = 3,
            BRIDGE_4 = 4,
            RED_5 = 5,
            BLACK_6 = 6,
            KILL_EYE_7 = 7,
            BLUE_SLOPE_8 = 8,
            DESTRUCTABLE_9 = 9,
            GREEN_10 = 10,
            BLUE_11 = 11,
            BLUE_12 = 12,
            RED_SLOPE_13 = 13,
            YELLOW_14 = 14,
            ILLEGAL_15 = 15
        }

        public Tile(Rom206 rom, int table_offset) : base(rom, Rom206.PLAYFIELD_TILE_TABLE + table_offset)
        {
        }

        public int Height
        {
            get { return (sbyte)this.Byte(0) * 4; }
        }

        public bool Flash
        {
            get { return (this.Byte(1) & 0x80) != 0; }
        }

        public TYPE Type
        {
            get { return (TYPE)(this.Byte(1) & 0xF); }
        }

        public override string ToString()
        {
            if (Type == TYPE.EMPTY_0)
                return " ";
            else
                return Hex[(byte)Type];
        }
    }

    /// <summary>
    /// Represents a row of 16 Tile objects
    /// </summary>
    public class Row : RomObject, IReadOnlyList<Tile>
    {
        public const int NUM_COLUMNS = 16;

        readonly Tile[] Tile = new Tile[NUM_COLUMNS];

        public Row(Rom206 rom, int index) : base(rom, Rom206.PLAYFIELD_ROW_TABLE + (index-1) * 16)
        {
            for (int n = 0; n < Tile.Length; n++)
            {
                int offset = (sbyte)rom[Address + n];
                Tile[n] = rom.GetTileAt(offset);
            }
        }

        public Tile this[int index] => ((IReadOnlyList<Tile>)Tile)[index];
        public int Count => Tile.Length;
        public IEnumerator<Tile> GetEnumerator() { return ((IReadOnlyList<Tile>)Tile).GetEnumerator(); }

        public override string ToString()
        {
            return $"{Tile[0]}{Tile[1]}{Tile[2]}{Tile[3]}{Tile[4]}{Tile[5]}{Tile[6]}{Tile[7]}{Tile[8]}{Tile[9]}{Tile[10]}{Tile[11]}{Tile[12]}{Tile[13]}{Tile[14]}{Tile[15]}";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Tile>)Tile).GetEnumerator();
        }
    }

    /// <summary>
    /// Represents a cunk of Row objects
    /// The rows are grouped together in the ROM as sort of a "block of rows" that can be used to build larger playfields
    /// </summary>
    public class PlayfieldChunk : RomObject, IReadOnlyList<Row>
    {
        readonly List<Row> List = new List<Row>();

        public PlayfieldChunk(Rom206 rom, int address) : base(rom, address)
        {
            System.Diagnostics.Debug.Assert(Rom.Word(address) != 0x0080);
            do
            {
                int index = Rom[address++];
                List.Add(Rom.GetRowAt(index));
            } while (Rom.Word(address) != 0x0080);
        }

        public Row this[int index] => List[index];
        public int Count => List.Count;
        public IEnumerator<Row> GetEnumerator() { return List.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return List.GetEnumerator(); }

        public void Print()
        {
            int n = List.Count;
            while (n > 0)
                System.Diagnostics.Debug.WriteLine(List[--n].ToString());
        }
    }

    public class PlayfieldChunkList : RomObject, IReadOnlyList<PlayfieldChunk>
    {
        readonly List<PlayfieldChunk> Chunks = new List<PlayfieldChunk>();

        public PlayfieldChunkList(Rom206 rom, int address) : base(rom, address)
        {
            System.Diagnostics.Debug.Assert(Rom.Word(address) != 0x0080);

            int pChunk = 0;
            System.Diagnostics.Debug.Assert(this.Word(pChunk) != 0);
            do
            {
                Chunks.Add(Rom.GetChunkAt(this.Word(pChunk)));
                pChunk += 2;
            } while (this.Word(pChunk) != 0);
        }

        public Tile GetTileAt(int row, int column)
        {
            foreach (PlayfieldChunk chunk in Chunks)
            {
                if (row >= chunk.Count)
                    row -= chunk.Count;
                else
                    return chunk[row][column];
            }
            return null;
        }

        public int NumRows
        {
            get
            {
                int count = 0;
                foreach (PlayfieldChunk rowlist in Chunks)
                    count += rowlist.Count;
                return count;
            }
        }

        public int NumColumns
        {
            get
            {
                return Row.NUM_COLUMNS;
            }
        }

        public System.Drawing.Size Dimensions
        {
            get
            {
                return new System.Drawing.Size(NumColumns * Tile.SIZE, NumRows * Tile.SIZE);
            }
        }

        public PlayfieldChunk this[int index] => Chunks[index];
        public int Count => Chunks.Count;
        public IEnumerator<PlayfieldChunk> GetEnumerator() { return ((IReadOnlyList<PlayfieldChunk>)Chunks).GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return ((IReadOnlyList<PlayfieldChunk>)Chunks).GetEnumerator(); }

        public void Print()
        {
            int n = Chunks.Count;
            while (n > 0)
                Chunks[--n].Print();
        }
    }

    /// <summary>
    /// Holds information needed to create a playfield
    /// </summary>
    public class PlayfieldInfo : RomObject
    {
        public const int MAX_COLUMNS = Row.NUM_COLUMNS;
        public const int MAX_ROWS = 64;

        public readonly LevelInfo Level;
        public readonly PlayfieldChunkList Chunks;
        public readonly int RowsToPyramid;
        public readonly int NumRedsThisLevel;
        public readonly int NumEmptyRowsInFrontOfPyramid;

        public PlayfieldInfo(LevelInfo level, int address) : base(level.Rom, address)
        {
            Level = level;
            Chunks = Rom.GetChunkListAt(this.Word(0));
            RowsToPyramid = Byte(2);
            NumRedsThisLevel = Byte(3);
            NumEmptyRowsInFrontOfPyramid = Byte(4);
        }

        public int NumRows => Chunks.NumRows;
        public int NumColumns => Chunks.NumColumns;
        public System.Drawing.Size Dimensions => Chunks.Dimensions;

        public void Print()
        {
            Chunks.Print();
        }
    }

    public class BonusPyramidInfo : RomObject
    {
        public readonly PlayfieldChunkList Chunks;
        public readonly byte Byte1;
        public readonly byte Byte2;

        public BonusPyramidInfo(LevelInfo level, int address) : base(level.Rom, address)
        {
            System.Diagnostics.Debug.Assert(address > 0);
            Chunks = Rom.GetChunkListAt(this.Word(0));
            Byte1 = this.Byte(2);
            Byte2 = this.Byte(3);
        }

        public System.Drawing.Size Dimensions => Chunks.Dimensions;

        public void Print()
        {
            Chunks.Print();
        }
    }

    public class LevelInfo : RomObject
    {
        public readonly string Name;
        public readonly int LevelNum;
        public readonly PlayfieldInfo PlayfieldInfo;
        public readonly int DefaultBonusTimerSec;
        public readonly byte LevelFlags;
        public readonly int DefaultBestTimeSec;

        /// <summary>
        /// Pointer to the bonus pyramid associated with this level
        /// Set to null if there is no bonus pyramid for this level
        /// </summary>
        public readonly BonusPyramidInfo BonusPyramid;

        public LevelInfo(string name, int level_num, Rom206 rom, int address, int? force_playfield_info = null) : base(rom, address)
        {
            Name = name;
            LevelNum = level_num;
            PlayfieldInfo = new PlayfieldInfo(this, force_playfield_info ?? Rom206.PLAYFIELD_INFO_ADDRESS + Byte(0));
            DefaultBonusTimerSec = Byte(1);
            LevelFlags = Byte(4);
            DefaultBestTimeSec = Byte(14);

            // read the BonusPyramidInfo pointer from the table in ROM
            int pyramid_addr = Rom.Word(Rom206.BONUS_PYRAMID_TABLE_ADDRESS + (level_num - 1) * 2);
            BonusPyramid = (pyramid_addr > 0) ? new BonusPyramidInfo(this, pyramid_addr) : null;
        }

        public void Print()
        {
            System.Diagnostics.Debug.WriteLine(Name);
            System.Diagnostics.Debug.WriteLine($"Rows: {PlayfieldInfo.NumRows}   Columns: {PlayfieldInfo.NumColumns}");
            System.Diagnostics.Debug.WriteLine(PlayfieldInfo.Dimensions.ToString());

            BonusPyramid?.Print();
            PlayfieldInfo.Print();
        }
    }

    public class LevelCollection : IReadOnlyList<LevelInfo>
    {
        public const int DEFAULT_NUMBER_OF_LEVELS = 52;

        Rom206 Rom = new Rom206();
        List<LevelInfo> List = new List<LevelInfo>();

        public LevelCollection()
        {
            for (int n = 0; n < DEFAULT_NUMBER_OF_LEVELS; n++)
            {
                // read the LevelInfo pointer from the table in ROM
                int address = Rom.Word(Rom206.LEVEL_TABLE_ADDRESS + n * 2);

                // create a level at this address
                LevelInfo level = new LevelInfo($"Level {n + 1} @ {address.ToString("X4")}", n+1, Rom, address);

                // and add to our collection
                List.Add(level);
            }

            // add the unused levels from the rom
            List.Add(new LevelInfo($"Unused playfield @ 5323", -1, Rom, 0x56C2, 0x5323));
            List.Add(new LevelInfo($"Unused playfield @ 5378", -1, Rom, 0x56C2, 0x5378));

        }

        public LevelInfo this[int index] => ((IReadOnlyList<LevelInfo>)List)[index];
        public int Count => ((IReadOnlyList<LevelInfo>)List).Count;
        public IEnumerator<LevelInfo> GetEnumerator() { return ((IReadOnlyList<LevelInfo>)List).GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return ((IReadOnlyList<LevelInfo>)List).GetEnumerator(); }
    }
}
