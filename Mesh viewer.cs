using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace I_Robot
{
    public partial class Mesh_viewer : Form
    {
        Mathbox.MeshList MeshList = new Mathbox.MeshList();
        Matrix Matrix; // world matrix
        Mathbox.Mesh mSelectedMesh = null;
        System.Drawing.Point Mouse;

        public Mesh_viewer()
        {
            InitializeComponent();

            XnaView1.MouseWheel += new MouseEventHandler(XnaView1_MouseWheel);

            foreach (Mathbox.Mesh mesh in MeshList)
                //listBox1.Items.Add("Mesh @ " + mesh.Address.ToString("X4"));
                listBox1.Items.Add(mesh);
        }

        public Mathbox.Mesh SelectedMesh
        {
            get { return mSelectedMesh; }
            set
            {
                if (mSelectedMesh != value)
                {
                    mSelectedMesh = value;

                    // reset world matrix
                    float scale = 1.0f / 1024;
                    Matrix = Matrix.CreateScale(scale, scale, scale);

                    listBox1.SelectedIndex = FindMeshInList(mSelectedMesh);
                }
            }
        }

        int FindMeshInList(Mathbox.Mesh mesh)
        {
            int i = listBox1.Items.Count;
            while (i-- > 0)
            {
                Mathbox.Mesh m = listBox1.Items[i] as Mathbox.Mesh;
                if (m.Address == mesh.Address)
                    return i;
            }
            return -1;
        }

        Mathbox.Mesh Find(UInt16 address)
        {
            foreach (Mathbox.Mesh mesh in MeshList)
            {
                if (mesh.Address == address)
                    return mesh;
            }
            return null;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedMesh = listBox1.Items[listBox1.SelectedIndex] as Mathbox.Mesh;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (SelectedMesh == null)
                return;

            // create object matrix
            XnaView1.PrimitiveBatch3D.ViewMatrix = Microsoft.Xna.Framework.Matrix.CreateLookAt(Vector3.Forward * 3, Vector3.Zero, Vector3.Down);
            XnaView1.PrimitiveBatch3D.WorldMatrix = Matrix;

            XnaView1.ClearColor = Microsoft.Xna.Framework.Graphics.Color.DimGray.ToVector4();
            XnaView1.BeginRender(true);
            SelectedMesh.Render(XnaView1);
            XnaView1.EndRender();
        }

        private void XnaView1_MouseHover(object sender, EventArgs e)
        {
            XnaView1.Focus();
        }

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
                    v.X = (Mouse.X - last.X) / 333.0f;
                    v.Y = (Mouse.Y - last.Y) / 333.0f;
                    Matrix = Matrix * Matrix.CreateTranslation(v);
                    break;
            }
        }

        private void XnaView1_MouseWheel(object sender, MouseEventArgs e)
        {
            float scale = (float)Math.Pow(2, e.Delta / 10.0 / 78);
            Matrix = Matrix.CreateScale(scale, scale, scale) * Matrix;
        }
    }
}
