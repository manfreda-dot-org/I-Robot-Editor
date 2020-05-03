using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using I_Robot.Roms;

namespace I_Robot.GameStructures.Playfield
{
    /// <summary>
    /// a Tile is the basic building block of the playfield
    /// each tile takes up two bytes of game memory
    /// </summary>
    public class Tile : Rom206Object
    {
        /// <summary>
        /// Size of a playfield tile, in world coordinates
        /// </summary>
        public const int SIZE = 128;

        static readonly string[] Hex = new string[16] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };

        /// <summary>
        /// Enumeration of the 16 types of tiles supported by the game
        /// </summary>
        public enum TYPE : byte
        {
            /// <summary>
            /// Indicates an empty tile on the playfield
            /// </summary>
            EMPTY_0 = 0,

            /// <summary>
            /// Behaves like a standard blue tile
            /// </summary>
            BLUE_1 = 1,

            /// <summary>
            /// Behaves like a standard blue tile, execpt
            /// that a jewel will be created above this tile
            /// </summary>
            BLUE_JEWEL_2 = 2,

            /// <summary>
            /// Tile that automatically rises and falls
            /// </summary>
            UP_DOWN_3 = 3,

            /// <summary>
            /// Special tile type that causes a bridge to be built
            /// When the bridge is complete the tile type will be set to BLUE_2
            /// </summary>
            BRIDGE_4 = 4,

            /// <summary>
            /// Standard red tile.
            /// When touched by robot, the eye is zapped and the tile turns to a regular blue tile
            /// </summary>
            RED_5 = 5,

            /// <summary>
            /// Black tile type.  Not sure how this is used in the game
            /// </summary>
            BLACK_6 = 6,

            /// <summary>
            /// Special tile that instantly kills the red eye and ends the level
            /// They eye can be kiled even if reds are still left on the game level
            /// </summary>
            KILL_EYE_7 = 7,

            /// <summary>
            /// Blue tile that is rendered as a slope
            /// The slope connects to the tiles to the right and forward
            /// </summary>
            BLUE_SLOPE_8 = 8,

            /// <summary>
            /// A dark blue destructable tile that the robot can shoot
            /// </summary>
            DESTRUCTABLE_9 = 9,

            /// <summary>
            /// A standard green "blocking" tile that prevents the robot from passing
            /// </summary>
            GREEN_10 = 10,

            /// <summary>
            /// Behaves like a standard blue tile, execpt that a pyramid enemy
            /// will be created above this tile
            /// The enemy moves in a forward/backward pattern
            /// </summary>
            BLUE_11 = 11,

            /// <summary>
            /// Behaves like a standard blue tile, execpt that a pyramid enemy
            /// will be created above this tile
            /// The enemy moves in a left/right pattern
            /// </summary>
            BLUE_12 = 12,

            /// <summary>
            /// Red tile that is rendered as a slope
            /// Acts like a red tile when touched by robot, and is converted to a blue slope tile
            /// </summary>
            RED_SLOPE_13 = 13,

            /// <summary>
            /// Standard yellow tile, typically only seen on landing zones
            /// </summary>
            YELLOW_14 = 14,

            /// <summary>
            /// Illegal tile type.  Will crash game
            /// </summary>
            ILLEGAL_15 = 15
        }

        /// <summary>
        /// Creates a new playfield tile from data in 136029-206
        /// </summary>
        /// <param name="rom">instance of ROM 136029-206</param>
        /// <param name="table_offset">offset into ROM tile table, must be a multiple of two</param>
        public Tile(Rom206 rom, int table_offset) : base(rom, Rom206.PLAYFIELD_TILE_TABLE + table_offset)
        {
        }

        public override int Size => 2;

        /// <summary>
        /// The height of the tile, in world coordinates
        /// </summary>
        public int Height
        {
            get { return (sbyte)this[0] * 4; }
        }

        /// <summary>
        /// Indicates whether the tile should be flashing.
        /// Used to indicate tiles that the robot can jump to
        /// </summary>
        public bool IsFlashing
        {
            get { return (this[1] & 0x80) != 0; }
        }

        /// <summary>
        /// Specifies the tile type
        /// There are 16 possible types
        /// Each type has different behavior (red tile, blue tile, sloped tile, etc)
        /// </summary>
        public TYPE Type
        {
            get { return (TYPE)(this[1] & 0xF); }
        }

        public bool IsSloped
        {
            get
            {
                switch (Type)
                {
                    case TYPE.BLUE_SLOPE_8:
                    case TYPE.RED_SLOPE_13: return true;
                }
                return false;
            }
        }

        public bool IsVisible
        {
            get
            {
                switch (Type)
                {
                    case TYPE.EMPTY_0:
                    case TYPE.BLACK_6:
                    case TYPE.ILLEGAL_15: return false;
                }
                return true;
            }
        }

        public byte ColorIndex
        {
            get
            {
                switch (Type)
                {
                    case Tile.TYPE.EMPTY_0: return 0;
                    case Tile.TYPE.BLUE_1: return 0x37;
                    case Tile.TYPE.BLUE_JEWEL_2: return 0x37;
                    case Tile.TYPE.UP_DOWN_3: return 0x17;
                    case Tile.TYPE.BRIDGE_4: return 0x39;
                    case Tile.TYPE.RED_5: return 0x0F;
                    case Tile.TYPE.BLACK_6: return 0;
                    case Tile.TYPE.KILL_EYE_7: return 0x0F; 
                    case Tile.TYPE.BLUE_SLOPE_8: return 0x30;
                    case Tile.TYPE.DESTRUCTABLE_9: return 0x34; 
                    case Tile.TYPE.GREEN_10: return 0x25; 
                    case Tile.TYPE.BLUE_11: return 0x37; 
                    case Tile.TYPE.BLUE_12: return 0x37; 
                    case Tile.TYPE.RED_SLOPE_13: return 0x8;
                    case Tile.TYPE.YELLOW_14: return 0x1F; 
                    case Tile.TYPE.ILLEGAL_15: return 0;
                }
                return 0;
            }
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
    /// Playfields are built by joining TileRow objects together
    /// </summary>
    public class TileRow : Rom206Object, IReadOnlyList<Tile>
    {
        public const int NUM_COLUMNS = 16;

        readonly Tile[] Tile = new Tile[NUM_COLUMNS];

        public TileRow(Rom206 rom, int index) : base(rom, Rom206.PLAYFIELD_ROW_TABLE + (index - 1) * 16)
        {
            for (int n = 0; n < Tile.Length; n++)
            {
                int offset = (sbyte)rom[Address + n];
                Tile[n] = rom.GetTileAt(offset);
            }
        }

        public override int Size => 16;

        new public Tile this[int index] => ((IReadOnlyList<Tile>)Tile)[index];
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
    /// A list of TileRow objects that comprise a "chunk"
    /// Chunks can be strung together to make larger playfields
    /// </summary>
    public class Chunk : Rom206Object, IReadOnlyList<TileRow>
    {
        readonly List<TileRow> List = new List<TileRow>();

        public Chunk(Rom206 rom, int address) : base(rom, address)
        {
            System.Diagnostics.Debug.Assert(Rom.Word(address) != 0x0080);
            do
            {
                int index = Rom[address++];
                List.Add(Rom.GetRowAt(index));
            } while (Rom.Word(address) != 0x0080);
        }

        public override int Size => List.Count + 2;
        new public TileRow this[int index] => List[index];
        public int Count => List.Count;
        public IEnumerator<TileRow> GetEnumerator() { return List.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return List.GetEnumerator(); }

        public void Print()
        {
            int n = List.Count;
            while (n > 0)
                System.Diagnostics.Debug.WriteLine(List[--n].ToString());
        }
    }

    /// <summary>
    /// A list of Chunk objects that, when combined together,
    /// will create a complete playfield
    /// </summary>
    public class ChunkList : Rom206Object, IReadOnlyList<Chunk>
    {
        readonly List<Chunk> Chunks = new List<Chunk>();

        public ChunkList(Rom206 rom, int address) : base(rom, address)
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

        public override int Size => (Chunks.Count + 1) * 2;

        public Tile GetTileAt(int row, int column)
        {
            foreach (Chunk chunk in Chunks)
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
                foreach (Chunk rowlist in Chunks)
                    count += rowlist.Count;
                return count;
            }
        }

        public int NumColumns
        {
            get
            {
                return TileRow.NUM_COLUMNS;
            }
        }

        public System.Windows.Size Dimensions
        {
            get
            {
                return new System.Windows.Size(NumColumns * Tile.SIZE, NumRows * Tile.SIZE);
            }
        }

        new public Chunk this[int index] => Chunks[index];
        public int Count => Chunks.Count;
        public IEnumerator<Chunk> GetEnumerator() { return ((IReadOnlyList<Chunk>)Chunks).GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return ((IReadOnlyList<Chunk>)Chunks).GetEnumerator(); }

        public void Print()
        {
            int n = Chunks.Count;
            while (n > 0)
                Chunks[--n].Print();
        }
    }

    /// <summary>
    /// Holds information needed to create the main game playfield area
    /// Does not include the portion of the playfield inside a bonus pyramid 
    /// </summary>
    public class Info : Rom206Object
    {
        public const int NUM_COLUMNS = TileRow.NUM_COLUMNS;
        public const int MAX_ROWS = 64;

        public readonly Level Level;
        public readonly ChunkList Chunks;
        public readonly int RowsToPyramid;
        public readonly int NumRedsThisLevel;
        public readonly int NumEmptyRowsInFrontOfPyramid;

        public Info(Level level, int address) : base(level.Rom, address)
        {
            Level = level;
            Chunks = Rom.GetChunkListAt(this.Word(0));
            RowsToPyramid = this[2];
            NumRedsThisLevel = this[3];
            NumEmptyRowsInFrontOfPyramid = this[4];
        }

        public override int Size => 5;

        public int NumRows => Chunks.NumRows;
        public int NumColumns => Chunks.NumColumns;
        public System.Windows.Size Dimensions => Chunks.Dimensions;

        public void Print()
        {
            Chunks.Print();
        }
    }

}