using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace I_Robot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameStructures.LevelCollection Levels = new GameStructures.LevelCollection();
        PlayfieldModel3D PlayfieldModel = new PlayfieldModel3D();

        AmbientLight AmbientLight = new AmbientLight(Color.FromRgb(16, 16, 16));
        DirectionalLight DirectionalLight = new DirectionalLight(Color.FromRgb(255, 255, 255), new Vector3D(0.1, -0.5, -1));

        Point Mouse;

        const double MODEL_SCALE = 1.0 / 16 / GameStructures.Playfield.Tile.SIZE;
        readonly Transform3DGroup ModelTransformGroup = new Transform3DGroup();
        readonly ScaleTransform3D ModelScale = new ScaleTransform3D(MODEL_SCALE, MODEL_SCALE * 232 / 256, MODEL_SCALE);
        readonly MatrixTransform3D WorldMatrix = new MatrixTransform3D();
        readonly MatrixTransform3D RotMatrix = new MatrixTransform3D();

        public MainWindow()
        {
            InitializeComponent();

            ModelTransformGroup.Children.Add(WorldMatrix);
            ModelTransformGroup.Children.Add(ModelScale);

            ModelVisual3D starfield = new StarfieldModel3D();
            starfield.Transform = RotMatrix;
            Viewport.Children.Add(starfield);

            Viewport.Children.Add(new ModelVisual3D() { Content = AmbientLight });
            Viewport.Children.Add(new ModelVisual3D() { Content = DirectionalLight });

            //ModelVisual3D big_bro = Mathbox.Model3D.GetModelAt(0x3578);
            //big_bro.Transform = ModelTransformGroup;
            //Viewport.Children.Add(big_bro);

            ModelVisual3D playfield = PlayfieldModel;
            playfield.Transform = ModelTransformGroup;
            Viewport.Children.Add(playfield);

            foreach (var level in Levels)
                lbLevels.Items.Add(level);
            lbLevels.SelectedIndex = 0;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.05);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Palette.CycleColors();
        }
        
        private void lbLevels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedLevel = lbLevels.SelectedItem as GameStructures.Level;
        }

        GameStructures.Level SelectedLevel
        {
            get { return PlayfieldModel.Level; }
            set
            {
                if (value != null && PlayfieldModel.Level != value)
                {
                    PlayfieldModel.Level = value;
                    //PlayfieldModel.Transform = ModelTransformGroup;

                    // reset model rotation matrix
                    Matrix3D m = Matrix3D.Identity;
                    int size = GameStructures.Playfield.Tile.SIZE;
                    m.Translate(new Vector3D((value.PlayfieldInfo.Dimensions.Width + size) / -2, size * 3, size * 3));
                    WorldMatrix.Matrix = m;
                }
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //            mCamera.Position = new System.Windows.Media.Media3D.Point3D(
            //                mCamera.Position.X,
            //                mCamera.Position.Y,
            //                mCamera.Position.Z + e.Delta / 250D);

            Matrix3D m = WorldMatrix.Matrix;
            m.Translate(new Vector3D(0, 0, -2 * e.Delta));
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
                    m.Rotate(new Quaternion(new Vector3D(1, 0, 0), p*scale));
                    m.Rotate(new Quaternion(new Vector3D(0, 1, 0) * m, y*scale));
                    //m.Rotate(new Quaternion(new Vector3D(0, 0, 1) * m, z));
                    WorldMatrix.Matrix = m;

                    m = RotMatrix.Matrix;
                    m.Rotate(new Quaternion(new Vector3D(1, 0, 0), p * scale));
                    m.Rotate(new Quaternion(new Vector3D(0, 1, 0) * m, y * scale));
                    //m.Rotate(new Quaternion(new Vector3D(0, 0, 1) * m, z));
                    RotMatrix.Matrix = m;
                }
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    Vector delta = Point.Subtract(Mouse, last);
                    Matrix3D m = WorldMatrix.Matrix;
                    m.Translate(new Vector3D(delta.X * 3, delta.Y * 3, 0));
                    WorldMatrix.Matrix = m;
                }
            }
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ShowMathboxModels_Click(object sender, RoutedEventArgs e)
        {
            MathboxModelWindow window = new MathboxModelWindow();
            window.Owner = this;

            if (MathboxModel3D.TryGetModel(Mathbox.Mesh.BIG_BROTHER_MOUTH_CLOSED, out var model))
                window.SelectedModel = model;

            window.Show();
        }
    }
}
