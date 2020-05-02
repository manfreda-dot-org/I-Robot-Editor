using I_Robot.Roms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I_Robot.GameStructures
{
    /// <summary>
    /// Represents a game level, as stored in ROM 136029-206
    /// </summary>
    public class Level : Rom206Object
    {
        public readonly string Name;
        public readonly int LevelNum;

        public readonly Playfield.Info PlayfieldInfo;
        public readonly int DefaultBonusTimerSec;
        public readonly byte LevelFlags;
        public readonly int DefaultBestTimeSec;

        /// <summary>
        /// Pointer to the bonus pyramid associated with this level
        /// Set to null if there is no bonus pyramid for this level
        /// </summary>
        public readonly BonusPyramid.Info BonusPyramid;

        public Level(string name, int level_num, Rom206 rom, int address, int? force_playfield_info = null) : base(rom, address)
        {
            Name = name;
            LevelNum = level_num;
            PlayfieldInfo = new Playfield.Info(this, force_playfield_info ?? Rom206.PLAYFIELD_INFO_ADDRESS + this[0]);
            DefaultBonusTimerSec = this[1];
            LevelFlags = this[4];
            DefaultBestTimeSec = this[14];

            // read the BonusPyramidInfo pointer from the table in ROM
            int pyramid_addr = Rom.Word(Rom206.BONUS_PYRAMID_TABLE_ADDRESS + (level_num - 1) * 2);
            BonusPyramid = (pyramid_addr > 0) ? new BonusPyramid.Info(this, pyramid_addr) : null;
        }

        public override int Size => 15;

        public void Print()
        {
            System.Diagnostics.Debug.WriteLine(Name);
            System.Diagnostics.Debug.WriteLine($"Rows: {PlayfieldInfo.NumRows}   Columns: {PlayfieldInfo.NumColumns}");
            System.Diagnostics.Debug.WriteLine(PlayfieldInfo.Dimensions.ToString());

            BonusPyramid?.Print();
            PlayfieldInfo.Print();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class LevelCollection : IReadOnlyList<Level>
    {
        public const int DEFAULT_NUMBER_OF_LEVELS = 52;

        readonly Rom206 Rom;
        List<Level> List = new List<Level>();

        public LevelCollection()
        {
            Rom206.TryLoad(out Rom);
            if (Rom == null)
                return;

            for (int n = 0; n < DEFAULT_NUMBER_OF_LEVELS; n++)
            {
                // read the LevelInfo pointer from the table in ROM
                int address = Rom.Word(Rom206.LEVEL_TABLE_ADDRESS + n * 2);

                // create a level at this address
                Level level = new Level($"Level {n + 1} @ {address.ToString("X4")}", n + 1, Rom, address);

                // and add to our collection
                List.Add(level);
            }

            // add the unused levels from the rom
            List.Add(new Level($"Unused playfield @ 5323", -1, Rom, 0x56C2, 0x5323));
            List.Add(new Level($"Unused playfield @ 5378", -1, Rom, 0x56C2, 0x5378));
        }

        public Level this[int index] => ((IReadOnlyList<Level>)List)[index];
        public int Count => ((IReadOnlyList<Level>)List).Count;
        public IEnumerator<Level> GetEnumerator() { return ((IReadOnlyList<Level>)List).GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return ((IReadOnlyList<Level>)List).GetEnumerator(); }
    }
}
