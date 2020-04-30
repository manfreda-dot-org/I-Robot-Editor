using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using I_Robot.GameStructures;
using I_Robot.GameStructures.Playfield;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace I_Robot
{
    class PlayfieldRenderer
    {
        public enum PolygonType
        {
            Polygon = 0,
            Vector = 1,
            Dot = 2,
            Unknown = 3
        }

        XnaView View;
        public VertexPositionColor[] XnaVertices = new VertexPositionColor[0x10000];
        int Index = 0;
        Vector3 V = Vector3.Zero;

        public PlayfieldRenderer()
        {
        }

        // create a vertex declaration, which tells the graphics card what kind of
        // data to expect during a draw call. We're drawing using
        // VertexPositionColors, so we'll use those vertex elements.
        VertexDeclaration VertexDeclaration = null;

        int AddFlatTile(float y, int color_index)
        {
            V.Y = y;
            float bottom = Tile.SIZE * 2;

            // limit "darkest" color to be base color + intensity 1
            int min_color = (color_index & 0xF8) + 1;

            // top
            Color c = Palette.XnaColor[Math.Max(color_index, min_color)];

            XnaVertices[Index].Position = new Vector3(V.X, V.Y, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, V.Y, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X, V.Y, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;


            XnaVertices[Index].Position = new Vector3(V.X, V.Y, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, V.Y, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, V.Y, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;


            // left side
            c = Palette.XnaColor[Math.Max(color_index - 2, min_color)];

            XnaVertices[Index].Position = new Vector3(V.X, V.Y, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X, V.Y, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X, bottom, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X, V.Y, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X, bottom, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X, bottom, V.Z);
            XnaVertices[Index++].Color = c;

            // right side
            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, V.Y, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, bottom, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, V.Y, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, V.Y, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, bottom, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, bottom, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;


            // front
            c = Palette.XnaColor[Math.Max(color_index - 4, min_color)];

            XnaVertices[Index].Position.X = V.X;
            XnaVertices[Index].Position.Y = V.Y;
            XnaVertices[Index].Position.Z = V.Z;
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position.X = V.X + Tile.SIZE;
            XnaVertices[Index].Position.Y = bottom;
            XnaVertices[Index].Position.Z = V.Z;
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position.X = V.X + Tile.SIZE;
            XnaVertices[Index].Position.Y = V.Y;
            XnaVertices[Index].Position.Z = V.Z;
            XnaVertices[Index++].Color = c;


            XnaVertices[Index].Position.X = V.X;
            XnaVertices[Index].Position.Y = V.Y;
            XnaVertices[Index].Position.Z = V.Z;
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position.X = V.X;
            XnaVertices[Index].Position.Y = bottom;
            XnaVertices[Index].Position.Z = V.Z;
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position.X = V.X + Tile.SIZE;
            XnaVertices[Index].Position.Y = bottom;
            XnaVertices[Index].Position.Z = V.Z;
            XnaVertices[Index++].Color = c;

            // back
            XnaVertices[Index].Position.X = V.X;
            XnaVertices[Index].Position.Y = V.Y;
            XnaVertices[Index].Position.Z = V.Z + Tile.SIZE;
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position.X = V.X + Tile.SIZE;
            XnaVertices[Index].Position.Y = V.Y;
            XnaVertices[Index].Position.Z = V.Z + Tile.SIZE;
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position.X = V.X + Tile.SIZE;
            XnaVertices[Index].Position.Y = bottom;
            XnaVertices[Index].Position.Z = V.Z + Tile.SIZE;
            XnaVertices[Index++].Color = c;


            XnaVertices[Index].Position.X = V.X;
            XnaVertices[Index].Position.Y = V.Y;
            XnaVertices[Index].Position.Z = V.Z + Tile.SIZE;
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position.X = V.X + Tile.SIZE;
            XnaVertices[Index].Position.Y = bottom;
            XnaVertices[Index].Position.Z = V.Z + Tile.SIZE;
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position.X = V.X;
            XnaVertices[Index].Position.Y = bottom;
            XnaVertices[Index].Position.Z = V.Z + Tile.SIZE;
            XnaVertices[Index++].Color = c;


            // bottom
            c = Palette.XnaColor[Math.Max(color_index - 6, min_color)];
            XnaVertices[Index].Position = new Vector3(V.X, bottom, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X, bottom, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, bottom, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;


            XnaVertices[Index].Position = new Vector3(V.X, bottom, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, bottom, V.Z + Tile.SIZE);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X + Tile.SIZE, bottom, V.Z);
            XnaVertices[Index++].Color = c;

            return 36;
        }

        void AddSlopedTile(ChunkList chunks, int row, int col, int color_index)
        {
            float x2 = V.X + Tile.SIZE;
            float z2 = V.Z + Tile.SIZE;
            int y11 = chunks.GetTileAt(row, col).Height;
            int y21 = chunks.GetTileAt(row + 1, col).Height;

            int y12 = y11;
            int y22 = y21;
            if (col < Row.NUM_COLUMNS - 1)
            {
                y12 = chunks.GetTileAt(row, col + 1).Height;
                y22 = chunks.GetTileAt(row + 1, col + 1).Height;
            }

            // adjust slope color to reflect tile height
            int ymax = Math.Max(Math.Max(Math.Max(Math.Abs(y11), Math.Abs(y12)), Math.Abs(y21)), Math.Abs(y22));
            color_index = (int)(color_index + Math.Abs(ymax) / 84 + 1.5);

            // limit "darkest" color to be base color + intensity 1
            int min_color = (color_index & 0xF8) + 1;

            Color c = Palette.XnaColor[Math.Max(color_index, min_color)];

            // top
            XnaVertices[Index].Position = new Vector3(V.X, y11, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(x2, y22, z2);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X, y21, z2);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(V.X, y11, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(x2, y12, V.Z);
            XnaVertices[Index++].Color = c;

            XnaVertices[Index].Position = new Vector3(x2, y22, z2);
            XnaVertices[Index++].Color = c;
        }

        void RenderObjectAtTile(UInt16 addr, Vector3 v)
        {
            var mesh = Mathbox.Mesh.GetMeshAt(addr);
            if (mesh != null)
            {
                Matrix temp = View.PrimitiveBatch3D.WorldMatrix;
                View.PrimitiveBatch3D.WorldMatrix = Matrix.CreateFromYawPitchRoll(0, 0, 0) * Matrix.CreateTranslation(v) * View.PrimitiveBatch3D.WorldMatrix;
                mesh.Render(View);
                View.PrimitiveBatch3D.WorldMatrix = temp;
            }
        }

        void AddTileIndices(ChunkList chunks, int row, int col)
        {
            Tile tile = chunks.GetTileAt(row, col);

            int color = 0;
            switch (tile.Type)
            {
                case Tile.TYPE.EMPTY_0: return;
                case Tile.TYPE.BLUE_1: color = 0x37; break;
                case Tile.TYPE.BLUE_JEWEL_2: color = 0x37; RenderObjectAtTile(0x728d, V + new Vector3(Tile.SIZE/2, -V.Y - Tile.SIZE, Tile.SIZE/2)); break;
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

        public void RenderChunkList(ChunkList chunks)
        {
            int num_rows = chunks.NumRows;// + level.BonusPyramid?.NumRows ?? 0;
            int num_cols = chunks.NumColumns;
            for (int r = 0; r < num_rows; r++)
            {
                V.X = 0;
                for (int c = 0; c < num_cols; c++, V.X += Tile.SIZE)
                    AddTileIndices(chunks, r, c);
                V.Z += Tile.SIZE;
            }
        }

        public void RenderPlayfield(XnaView view, Level level)
        {
            if (VertexDeclaration?.GraphicsDevice != view.Device)
                VertexDeclaration = new VertexDeclaration(view.Device, VertexPositionColor.VertexElements);

            // fill vertex buffer
            View = view;
            Index = 0;
            V = Vector3.Zero;
            RenderChunkList(level.PlayfieldInfo.Chunks);
            if (level.BonusPyramid != null)
                RenderChunkList(level.BonusPyramid.Chunks);

            view.Device.RenderState.PointSpriteEnable = true;
            view.Device.RenderState.PointSize = 5;

            view.PrimitiveBatch3D.Begin(VertexDeclaration);

            // render unshaded
            view.PrimitiveBatch3D.Effect.LightingEnabled = false;
            view.PrimitiveBatch3D.Effect.CommitChanges();

            view.Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, XnaVertices, 0, Index / 3);

            view.PrimitiveBatch3D.End();

            view.Device.RenderState.PointSpriteEnable = false;
        }

    }
}
