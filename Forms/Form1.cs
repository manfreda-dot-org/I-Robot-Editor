using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;

namespace I_Robot
{
    public partial class Form1 : Form
    {
        PlayfieldRenderer Renderer = new PlayfieldRenderer();
        Matrix Matrix; // world matrix

        GameStructures.LevelCollection Levels;
        GameStructures.Level mSelectedLevel;
        Mathbox.Mesh Robot;

        public Form1()
        {
            InitializeComponent();
            XnaView1.MouseWheel += new MouseEventHandler(XnaView1_MouseWheel);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Robot = Mathbox.Mesh.GetMeshAt(0x2958);

            Levels = new GameStructures.LevelCollection();

            foreach (GameStructures.Level level in Levels)
                    listBox1.Items.Add(level.Name);

            SelectedLevel = Levels[0];

            toolStripStatusLabel1.Text = $"Total levels = {Levels.Count}";
        }


        GameStructures.Level SelectedLevel
        {
            get { return mSelectedLevel; }
            set
            {
                if (value != null && mSelectedLevel != value)
                {
                    mSelectedLevel = value;
#if DEBUG
                    value.Print();
#endif
                    Palette.SetColorGroup((value.LevelNum - 1) / 26);

                    // reset world matrix
                    float scale = 1.0f / GameStructures.Playfield.Tile.SIZE;
                    Matrix = Matrix.CreateTranslation(new Vector3(-128 / 2 - value.PlayfieldInfo.Dimensions.Width / 2, 400, -128 / 2 - value.PlayfieldInfo.Dimensions.Height / 2));
                    Matrix = Matrix * Matrix.CreateScale(scale, scale * 232 / 256, scale);

                    int index = listBox1.FindString(value.Name);
                    if (listBox1.SelectedIndex != index)
                        listBox1.SelectedIndex = index;

                    //                    toolStripStatusLabel2.Text = $"Current object = 0x{value.Address.ToString("X4")}";
                    //                    toolStripStatusLabel3.Text = $"Vertices = 0x{value.VertexBase.ToString("X4")}";
                    //                    toolStripStatusLabel4.Text = $"Polygons = {value.Polygons.Count}";
                    timer1_Tick(null, null);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (Levels != null)
                    SelectedLevel = Levels[listBox1.SelectedIndex];
            }
            catch
            {
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (SelectedLevel == null)
                return;

            Palette.CycleColors();

            // create object matrix
            XnaView1.PrimitiveBatch3D.ViewMatrix = Microsoft.Xna.Framework.Matrix.CreateLookAt(Vector3.Forward * 3, Vector3.Zero, Vector3.Down);
            XnaView1.PrimitiveBatch3D.WorldMatrix = Matrix * Matrix.CreateTranslation(new Vector3(0, 0, 20));

            XnaView1.ClearColor = Microsoft.Xna.Framework.Graphics.Color.Black.ToVector4();
            XnaView1.BeginRender(true);

            Renderer.RenderPlayfield(XnaView1, SelectedLevel);

            float rows = SelectedLevel.PlayfieldInfo.Chunks[0].Count + 0.5f;
            XnaView1.PrimitiveBatch3D.WorldMatrix = Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2,0,0) * Matrix.CreateTranslation(new Vector3(GameStructures.Playfield.Tile.SIZE * 8.5f, 0, GameStructures.Playfield.Tile.SIZE * rows)) * XnaView1.PrimitiveBatch3D.WorldMatrix;
            Robot.Render(XnaView1);

            XnaView1.EndRender();
        }

        System.Drawing.Point Mouse;
        private void XnaView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Mouse = e.Location;
            }
        }

        private void XnaView1_MouseMove(object sender, MouseEventArgs e)
        {
            System.Drawing.Point last = Mouse;
            Mouse = e.Location;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    float y = last.X - Mouse.X;
                    float p = Mouse.Y - last.Y;
                    float scale = (float)(Math.PI / 180);
                    Matrix = Matrix * Matrix.CreateFromYawPitchRoll(y * scale, p * scale, 0);
                    break;
                case MouseButtons.Middle:
                case MouseButtons.Right:
                    Vector3 v = new Vector3(0, 0, 0);
                    v.X = (Mouse.X- last.X) / 30.0f;
                    v.Y = (Mouse.Y - last.Y) / 30.0f;
                    Matrix = Matrix * Matrix.CreateTranslation(v);
                    break;
            }
        }

        private void XnaView1_MouseWheel(object sender, MouseEventArgs e)
        {
#if false
            float scale = (float)Math.Pow(2, e.Delta / 10.0 / 78);
            Matrix = Matrix * Matrix.CreateScale(scale, scale, scale);
#else
            Vector3 v = new Vector3(0, 0, e.Delta / -100f);
            Matrix = Matrix * Matrix.CreateTranslation(v);
#endif
        }

        private void XnaView1_MouseHover(object sender, EventArgs e)
        {
            XnaView1.Focus();
        }

        private void newMeshViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var window = new Mesh_viewer();
            window.SelectedMesh = Mathbox.Mesh.GetMeshAt(0x3578);
            window.Show();
        }

        private void XnaView1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Right: Matrix = Matrix * Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2 / -18, 0, 0); break;
                case Keys.Left: Matrix = Matrix * Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2 / 18, 0, 0); break;
                case Keys.Up: Matrix = Matrix * Matrix.CreateFromYawPitchRoll(0, MathHelper.PiOver2 / -18, 0); break;
                case Keys.Down: Matrix = Matrix * Matrix.CreateFromYawPitchRoll(0, MathHelper.PiOver2 / 18, 0); break;
                case Keys.A: Matrix = Matrix * Matrix.CreateTranslation(-1, 0, 0); break;
                case Keys.D: Matrix = Matrix * Matrix.CreateTranslation(+1, 0, 0); break;
                case Keys.W: Matrix = Matrix * Matrix.CreateTranslation(0, -1, 0); break;
                case Keys.S: Matrix = Matrix * Matrix.CreateTranslation(0, +1, 0); break;
            }
        }
    }
}