using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace I_Robot
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MathboxModelWindow : Window
    {
        const double MODEL_SCALE = 1.0 / 16 / GameStructures.Playfield.Tile.SIZE;

        DirectionalLight DirectionalLight = new DirectionalLight(Color.FromRgb(255, 255, 255), new Vector3D(0.1, -0.5, -1));
        readonly Transform3DGroup ModelTransformGroup = new Transform3DGroup();
        readonly ScaleTransform3D ModelScale = new ScaleTransform3D(MODEL_SCALE, MODEL_SCALE * 232 / 256, MODEL_SCALE);
        readonly MatrixTransform3D WorldMatrix = new MatrixTransform3D();
        readonly TranslateTransform3D Translate = new TranslateTransform3D();

        MathboxModel3D mSelectedModel = null;
        Point Mouse;

        public MathboxModelWindow()
        {
            InitializeComponent();

            ModelTransformGroup.Children.Add(Translate);
            ModelTransformGroup.Children.Add(WorldMatrix);
            ModelTransformGroup.Children.Add(ModelScale);

            foreach (var model in Mathbox.Mesh.MeshList)
                lbModels.Items.Add(model);
        }

        public MathboxModel3D SelectedModel
        {
            get { return mSelectedModel; }
            set
            {
                if (mSelectedModel != value)
                {
                    if (mSelectedModel != null)
                        Viewport.Children.Clear();
                    mSelectedModel = value;

                    Translate.OffsetX = Translate.OffsetY = Translate.OffsetZ = 0;
                    WorldMatrix.Matrix = Matrix3D.Identity;
                    Visual3D  model = value;
                    model.Transform = ModelTransformGroup;
                    Viewport.Children.Add(model);
                    Viewport.Children.Add(new ModelVisual3D() { Content = DirectionalLight });

                    lbModels.ScrollIntoView(value);
                    lbModels.SelectedItem = value;
                }
            }
        }

        private void lbModels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MathboxModel3D.TryGetModel(lbModels.SelectedItem as Mathbox.Mesh, out var model))
                SelectedModel = model;
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scale = 1 + e.Delta / 500D;
            Matrix3D m = WorldMatrix.Matrix;
            m.Scale(new Vector3D(scale, scale, scale));
            WorldMatrix.Matrix = m;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point m = e.GetPosition(Viewport);
            if (m.X >= 0 && m.X <= Viewport.ActualWidth && m.Y >= 0 && m.Y <= ActualHeight)
            {
                Mouse = m;
                System.Windows.Input.Mouse.Capture(Viewport);
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Input.Mouse.Capture(null);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (System.Windows.Input.Mouse.Captured == Viewport)
            {
                Point last = Mouse;
                Mouse = e.GetPosition(Viewport);

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    double y = last.X - Mouse.X;
                    double p = Mouse.Y - last.Y;
                    double scale = Math.PI / 5;

                    Matrix3D m = WorldMatrix.Matrix;
                    m.Rotate(new Quaternion(new Vector3D(1, 0, 0), p * scale));
                    m.Rotate(new Quaternion(new Vector3D(0, 1, 0) * m, y * scale));
                    //m.Rotate(new Quaternion(new Vector3D(0, 0, 1) * m, z));
                    WorldMatrix.Matrix = m;
                }
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    Vector delta = Point.Subtract(Mouse, last);
//                    Translate.OffsetX += delta.X * 3;
//                    Translate.OffsetY += delta.Y * 3;
                    Matrix3D m = WorldMatrix.Matrix;
                    m.Translate(new Vector3D(delta.X * 3, delta.Y * 3, 0));
                    WorldMatrix.Matrix = m;
                }
            }
        }
    }
}
