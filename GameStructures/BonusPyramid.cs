using I_Robot.Roms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I_Robot.GameStructures.BonusPyramid
{
    /// <summary>
    /// Holds information related to bonus pyramids that occur every 3 game levels
    /// </summary>
    public class Info : Rom206Object
    {
        public readonly Playfield.ChunkList Chunks;
        public readonly byte Byte1;
        public readonly byte Byte2;

        public Info(Level level, int address) : base(level.Rom, address)
        {
            System.Diagnostics.Debug.Assert(address > 0);
            Chunks = Rom.GetChunkListAt(this.Word(0));
            Byte1 = this[2];
            Byte2 = this[3];
        }

        /// <summary>
        /// The dimensions of the pyramid, in world units
        /// </summary>
        public System.Drawing.Size Dimensions => Chunks.Dimensions;

        public override int Size => 4;

        public void Print()
        {
            Chunks.Print();
        }
    }
}
