using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using I_Robot.Roms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace I_Robot.GameStructures.Playfield
{
    /// <summary>
    /// a Tile is the basic building block of the playfield
    /// </summary>
    public class Tile : Rom206Reference
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
    public class Row : Rom206Reference, IReadOnlyList<Tile>
    {
        public const int NUM_COLUMNS = 16;

        readonly Tile[] Tile = new Tile[NUM_COLUMNS];

        public Row(Rom206 rom, int index) : base(rom, Rom206.PLAYFIELD_ROW_TABLE + (index - 1) * 16)
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
    public class Chunk : Rom206Reference, IReadOnlyList<Row>
    {
        readonly List<Row> List = new List<Row>();

        public Chunk(Rom206 rom, int address) : base(rom, address)
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

    public class ChunkList : Rom206Reference, IReadOnlyList<Chunk>
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

        public Chunk this[int index] => Chunks[index];
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
    /// Holds information needed to create a playfield
    /// </summary>
    public class PlayfieldInfo : Rom206Reference
    {
        public const int MAX_COLUMNS = Row.NUM_COLUMNS;
        public const int MAX_ROWS = 64;

        public readonly Level Level;
        public readonly ChunkList Chunks;
        public readonly int RowsToPyramid;
        public readonly int NumRedsThisLevel;
        public readonly int NumEmptyRowsInFrontOfPyramid;

        public PlayfieldInfo(Level level, int address) : base(level.Rom, address)
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

    public class BonusPyramidInfo : Rom206Reference
    {
        public readonly ChunkList Chunks;
        public readonly byte Byte1;
        public readonly byte Byte2;

        public BonusPyramidInfo(Level level, int address) : base(level.Rom, address)
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

}