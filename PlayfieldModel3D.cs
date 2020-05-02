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
        Point3D P = new Point3D();

        public PlayfieldModel3D()
        {
        }

        /// <summary>
        /// Gets/sets the level that is used to build this model
        /// </summary>
        public Level Level
        {
            get { return mLevel; }
            set
            {
                if (mLevel != value)
                {
                    mLevel = value;

                    Palette.SetColorGroup((value.LevelNum - 1) / 26);

                    // clear the group
                    Group.Children.Clear();

                    // initialize at bottom left of playfield
                    P.X = P.Y = P.Z = 0;

                    // build all tiles in all chunks
                    BuildChunkList(value.PlayfieldInfo.Chunks);
                    if (value.BonusPyramid != null)
                        BuildChunkList(value.BonusPyramid.Chunks);

                    // render the robot at the starting location
                    double rows = value.PlayfieldInfo.Chunks[0].Count + 0.5;
                    RenderObjectAtPosition(0x2958, new Point3D(GameStructures.Playfield.Tile.SIZE * 8.5f, 0, GameStructures.Playfield.Tile.SIZE * rows), 90, 0, 0);
//                    XnaView1.PrimitiveBatch3D.WorldMatrix = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2, 0, 0) * Matrix.CreateTranslation(new Vector3(GameStructures.Playfield.Tile.SIZE * 8.5f, 0, GameStructures.Playfield.Tile.SIZE * rows)) * XnaView1.PrimitiveBatch3D.WorldMatrix;
//                    Robot.Render(XnaView1);
//                    Robot = Mathbox.Mesh.GetMeshAt(0x2958);
                }
            }
        }

        void BuildChunkList(ChunkList chunks)
        {
            int num_rows = chunks.NumRows;// + level.BonusPyramid?.NumRows ?? 0;
            int num_cols = chunks.NumColumns;
            for (int r = 0; r < num_rows; r++)
            {
                P.X = 0;
                for (int c = 0; c < num_cols; c++, P.X += Tile.SIZE)
                    BuildTile3D(chunks, r, c);
                P.Z += Tile.SIZE;
            }
        }

        void BuildTile3D(ChunkList chunks, int row, int col)
        {
            Tile tile = chunks.GetTileAt(row, col);

            int color = 0;
            switch (tile.Type)
            {
                case Tile.TYPE.EMPTY_0: return;
                case Tile.TYPE.BLUE_1: color = 0x37; break;
                case Tile.TYPE.BLUE_JEWEL_2: color = 0x37; RenderObjectAtPosition(0x728d, P + new Vector3D(Tile.SIZE / 2, -P.Y - Tile.SIZE, Tile.SIZE / 2)); break;
                case Tile.TYPE.UP_DOWN_3: color = 0x17; break;
                case Tile.TYPE.BRIDGE_4: color = 0x39; break;
                case Tile.TYPE.RED_5: color = 0x0F; break;
                case Tile.TYPE.BLACK_6: return;
                case Tile.TYPE.KILL_EYE_7: color = 0x0F; break;
                case Tile.TYPE.BLUE_SLOPE_8: AddSlopedTile(chunks, row, col, 0x30); return;
                case Tile.TYPE.DESTRUCTABLE_9: color = 0x34; break;
                case Tile.TYPE.GREEN_10: color = 0x25; break;
                case Tile.TYPE.BLUE_11: color = 0x37; break;
                case Tile.TYPE.BLUE_12: color = 0x37; break;
                case Tile.TYPE.RED_SLOPE_13: AddSlopedTile(chunks, row, col, 0x8); return;
                case Tile.TYPE.YELLOW_14: color = 0x1F; break;
                case Tile.TYPE.ILLEGAL_15: return;
            }

            AddFlatTile(tile.Height, color);
        }

        void AddFlatTile(float y, int color_index)
        {
            P.Y = y;
            double bottom = Tile.SIZE * 2;

            Point3D p2 = P + new Vector3D(Tile.SIZE, 0, 0);
            Point3D p3 = p2 + new Vector3D(0, 0, Tile.SIZE);
            Point3D p4 = P + new Vector3D(0, 0, Tile.SIZE);
            Point3D p5 = new Point3D(P.X, bottom, P.Z);
            Point3D p6 = p5 + new Vector3D(Tile.SIZE, 0, 0);
            Point3D p7 = p6 + new Vector3D(0, 0, Tile.SIZE);
            Point3D p8 = p5 + new Vector3D(0, 0, Tile.SIZE);

            // limit "darkest" color to be base color + intensity 1
            int min_color = (color_index & 0xF8) + 1;

            // top
            Material m = Palette.DiffuseMaterial[Math.Max(color_index, min_color)];
            AddRectangle(P, p2, p3, p4, m);

            // left/right
//            m = Palette.DiffuseMaterial[Math.Max(color_index - 2, min_color)];
            AddRectangle(P, p4, p8, p5, m);
            AddRectangle(p2, p6, p7, p3, m);

            // front / back
//            m = Palette.DiffuseMaterial[Math.Max(color_index - 4, min_color)];
            AddRectangle(P, p5, p6, p2, m);
            AddRectangle(p3, p4, p8, p7, m);

            // bottom
//            m = Palette.DiffuseMaterial[Math.Max(color_index - 6, min_color)];
            AddRectangle(p5, p8, p7, p6, m);
        }

        void AddSlopedTile(ChunkList chunks, int row, int col, int color_index)
        {
            double x2 = P.X + Tile.SIZE;
            double z2 = P.Z + Tile.SIZE;
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
                new Point3D(P.X, y11, P.Z),
                new Point3D(x2, y12, P.Z),
                new Point3D(x2, y22, z2),
                new Point3D(P.X, y21, z2),
                Palette.DiffuseMaterial[color_index]);
        }

        Model3DGroup RenderObjectAtPosition(UInt16 addr, Point3D v, double yaw=0, double pitch = 0, double roll = 0)
        {
            var mesh = Mathbox.Model3D.GetModelAt(addr);
            if (mesh != null)
            {
                Model3DGroup g = new Model3DGroup();
                g.Children.Add(mesh);

                MatrixTransform3D matrix = new MatrixTransform3D();
                Matrix3D m = Matrix3D.Identity;
                m.Rotate(new Quaternion(new Vector3D(0, 0, 1), roll));
                m.Rotate(new Quaternion(new Vector3D(1, 0, 0) * m, pitch));
                m.Rotate(new Quaternion(new Vector3D(0, 1, 0) * m, yaw));
                m.Translate(new Vector3D(v.X, v.Y, v.Z));
                matrix.Matrix = m;
                g.Transform = matrix;
                Group.Children.Add(g);
                return g;
            }
            return null;

        }

    }
}
