using I_Robot.GameStructures;
using I_Robot.GameStructures.Playfield;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace I_Robot
{
    /// <summary>
    /// Represents a 3D model of the playfield
    /// </summary>
    class PlayfieldModel3D : ModelBuilder
    {
        Level mLevel = null;

        public PlayfieldModel3D()
        {
        }

        public PlayfieldModel3D(Level level)
        {
            Level = level;
        }

        /// <summary>
        /// Gets/sets the level that is used to build this model
        /// </summary>
        public Level Level
        {
            get { return mLevel; }
            set
            {
                // is the level changing?
                if (value != null && mLevel != value)
                {
                    mLevel = value;

                    // choose the correct color group based on the level number
                    Palette.SetColorGroup((value.LevelNum - 1) / 26);

                    // clear the group
                    Group.Children.Clear();

                    // initialize at bottom left of playfield
                    Point3D p = new Point3D();

                    // build all tiles in all chunks
                    BuildChunkList(value.PlayfieldInfo.Chunks, ref p);
                    if (value.BonusPyramid != null)
                        BuildChunkList(value.BonusPyramid.Chunks, ref p);

                    // render the robot at the starting location
                    double rows = value.PlayfieldInfo.Chunks[0].Count + 0.5;
                    AddMathboxObject(
                        Mathbox.Mesh.ROBOT_ARMS_DOWN, 
                        new Point3D(GameStructures.Playfield.Tile.SIZE * 8.5f, 0, GameStructures.Playfield.Tile.SIZE * rows),
                        90, 0, 0);
                }
            }
        }

        void BuildChunkList(ChunkList chunks, ref Point3D p)
        {
            int num_rows = chunks.NumRows;// + level.BonusPyramid?.NumRows ?? 0;
            int num_cols = chunks.NumColumns;
            for (int r = 0; r < num_rows; r++)
            {
                p.X = 0;
                for (int c = 0; c < num_cols; c++, p.X += Tile.SIZE)
                    BuildTile3D(chunks, r, c, p);
                p.Z += Tile.SIZE;
            }
        }

        void BuildTile3D(ChunkList chunks, int row, int col, Point3D p)
        {
            Tile tile = chunks.GetTileAt(row, col);

            int color = 0;
            switch (tile.Type)
            {
                case Tile.TYPE.EMPTY_0: return;
                case Tile.TYPE.BLUE_1: color = 0x37; break;
                case Tile.TYPE.BLUE_JEWEL_2: color = 0x37; AddMathboxObject(Mathbox.Mesh.JEWEL, p + new Vector3D(Tile.SIZE / 2, -p.Y - Tile.SIZE, Tile.SIZE / 2)); break;
                case Tile.TYPE.UP_DOWN_3: color = 0x17; break;
                case Tile.TYPE.BRIDGE_4: color = 0x39; break;
                case Tile.TYPE.RED_5: color = 0x0F; break;
                case Tile.TYPE.BLACK_6: return;
                case Tile.TYPE.KILL_EYE_7: color = 0x0F; break;
                case Tile.TYPE.BLUE_SLOPE_8: AddSlopedTile(p, chunks, row, col, 0x30); return;
                case Tile.TYPE.DESTRUCTABLE_9: color = 0x34; break;
                case Tile.TYPE.GREEN_10: color = 0x25; break;
                case Tile.TYPE.BLUE_11: color = 0x37; break;
                case Tile.TYPE.BLUE_12: color = 0x37; break;
                case Tile.TYPE.RED_SLOPE_13: AddSlopedTile(p, chunks, row, col, 0x8); return;
                case Tile.TYPE.YELLOW_14: color = 0x1F; break;
                case Tile.TYPE.ILLEGAL_15: return;
            }

            AddFlatTile(chunks, row, col, p, tile);
        }

        void AddFlatTile(ChunkList chunks, int row, int col, Point3D p, Tile tile)
        {
            p.Y = tile.Height;
            double bottom = Tile.SIZE * 2;

            Point3D p2 = p + new Vector3D(Tile.SIZE, 0, 0);
            Point3D p3 = p2 + new Vector3D(0, 0, Tile.SIZE);
            Point3D p4 = p + new Vector3D(0, 0, Tile.SIZE);
            Point3D p5 = new Point3D(p.X, bottom, p.Z);
            Point3D p6 = p5 + new Vector3D(Tile.SIZE, 0, 0);
            Point3D p7 = p6 + new Vector3D(0, 0, Tile.SIZE);
            Point3D p8 = p5 + new Vector3D(0, 0, Tile.SIZE);

            // limit "darkest" color to be base color + intensity 1
            int color_index = tile.ColorIndex;
            int min_color = (color_index & 0xF8) + 1;

            // top
            Material m = Palette.DiffuseMaterial[Math.Max(color_index, min_color)];
            AddRectangle(p, p2, p3, p4, m);

            // left/right
            //            m = Palette.DiffuseMaterial[Math.Max(color_index - 2, min_color)];

            bool left = true;
            if (col > 0)
            {
                Tile t = chunks.GetTileAt(row, col - 1);
                if (t.IsVisible && !t.IsSloped && t.Height <= p.Y)
                    left = false;
            }
            if (left)
                AddRectangle(p, p4, p8, p5, m);

            bool right = true;
            if (col < 15)
            {
                Tile t = chunks.GetTileAt(row, col + 1);
                if (t.IsVisible && !t.IsSloped && t.Height <= p.Y)
                    right = false;
            }
            if (right)
                AddRectangle(p2, p6, p7, p3, m);

            // front / back
            //            m = Palette.DiffuseMaterial[Math.Max(color_index - 4, min_color)];
            bool front = true;
            if (row < chunks.NumRows-1)
            {
                Tile t = chunks.GetTileAt(row + 1, col);
                if (t.IsVisible && !t.IsSloped && t.Height <= p.Y)
                    front = false;
            }
            if (front)
                AddRectangle(p3, p4, p8, p7, m);
            bool back = true;
            if (row > 0)
            {
                Tile t = chunks.GetTileAt(row-1, col);
                if (t.IsVisible && !t.IsSloped && t.Height <= p.Y)
                    back = false;
            }
            if (back)
                AddRectangle(p, p5, p6, p2, m);

            // bottom
            //m = Palette.DiffuseMaterial[Math.Max(color_index - 6, min_color)];
            AddRectangle(p5, p8, p7, p6, m);
        }

        void AddSlopedTile(Point3D p, ChunkList chunks, int row, int col, int color_index)
        {
            double x2 = p.X + Tile.SIZE;
            double z2 = p.Z + Tile.SIZE;
            int y11 = chunks.GetTileAt(row, col).Height;
            int y21 = chunks.GetTileAt(row + 1, col).Height;

            int y12 = y11;
            int y22 = y21;
            if (col < TileRow.NUM_COLUMNS - 1)
            {
                y12 = chunks.GetTileAt(row, col + 1).Height;
                y22 = chunks.GetTileAt(row + 1, col + 1).Height;
            }

            // adjust slope color to reflect tile height
            int ymax = Math.Max(Math.Max(Math.Max(Math.Abs(y11), Math.Abs(y12)), Math.Abs(y21)), Math.Abs(y22));
            color_index = (int)(color_index + Math.Abs(ymax) / 84 + 1.5);

            // limit "darkest" color to be base color + intensity 1
            int min_color = (color_index & 0xF8) + 1;

            color_index = Math.Max(color_index, min_color);

            AddRectangle(
                new Point3D(p.X, y11, p.Z),
                new Point3D(x2, y12, p.Z),
                new Point3D(x2, y22, z2),
                new Point3D(p.X, y21, z2),
                Palette.DiffuseMaterial[color_index]);
        }

        void AddMathboxObject(UInt16 addr, Point3D v, double yaw=0, double pitch = 0, double roll = 0)
        {
            if (MathboxModel3D.TryGetModel(addr, out var model))
            {
                Model3DGroup g = new Model3DGroup();
                g.Children.Add(model);

                MatrixTransform3D matrix = new MatrixTransform3D();
                Matrix3D m = Matrix3D.Identity;
                m.Rotate(new Quaternion(new Vector3D(0, 0, 1), roll));
                m.Rotate(new Quaternion(new Vector3D(1, 0, 0) * m, pitch));
                m.Rotate(new Quaternion(new Vector3D(0, 1, 0) * m, yaw));
                m.Translate(new Vector3D(v.X, v.Y, v.Z));
                matrix.Matrix = m;
                g.Transform = matrix;
                Group.Children.Add(g);
            }
        }

    }
}
