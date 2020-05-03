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
        readonly ScaleTransform3D ModelScale = new ScaleTransform3D(MODEL_SCALE, MODEL_SCALE * 232 / 256, MODEL_SCALE);
        readonly TranslateTransform3D ModelTranslate = new TranslateTransform3D();
        readonly MatrixTransform3D ModelRotate = new MatrixTransform3D();
        readonly MatrixTransform3D StarfieldRotMatrix = new MatrixTransform3D();

        public MainWindow()
        {
            InitializeComponent();

            // add the starfield to the background
            ModelVisual3D starfield = new StarfieldModel3D();
            starfield.Transform = StarfieldRotMatrix;
            Viewport.Children.Add(starfield);

            // add standard light sources
            Viewport.Children.Add(new ModelVisual3D() { Content = AmbientLight });
            Viewport.Children.Add(new ModelVisual3D() { Content = DirectionalLight });

            // add the playfield model
            ModelVisual3D playfield = PlayfieldModel;
            Transform3DGroup xform_group = new Transform3DGroup();
            xform_group.Children.Add(ModelRotate);
            xform_group.Children.Add(ModelTranslate);
            xform_group.Children.Add(ModelScale);
            playfield.Transform = xform_group;
            Viewport.Children.Add(playfield);

#if DEBUG
            // throw in big brother
            if (MathboxModel3D.TryGetModel(Mathbox.Mesh.BIG_BROTHER_MOUTH_CLOSED, out var big_bro))
            {
                ModelVisual3D b = new ModelVisual3D();
                b.Content = big_bro;
                b.Transform = xform_group;
                Viewport.Children.Add(b);
            }
#endif

            // populate the list view with available levels
            foreach (var level in Levels)
                lbLevels.Items.Add(level);
            lbLevels.SelectedIndex = 0;

            // start our timer
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

                    // reset model rotation matrix
                    ModelTranslate.OffsetX = ModelTranslate.OffsetY = ModelTranslate.OffsetZ = 0;
                    Matrix3D m = Matrix3D.Identity;
                    int size = GameStructures.Playfield.Tile.SIZE;
                    m.Translate(new Vector3D((value.PlayfieldInfo.Dimensions.Width + size) / -2, size * 3, size * 3));
                    ModelRotate.Matrix = m;

                    StatusText.Text = $"{value.Name}    Num reds: {value.PlayfieldInfo.NumRedsThisLevel}    Flags: 0x{value.LevelFlags.ToString("X2")}    Bonus time: {value.DefaultBonusTimerSec} sec    Best time: {value.DefaultBestTimeSec} sec    Bonus pyramid: {(value.BonusPyramid != null ? "Yes" : "No")}";
                }
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ModelTranslate.OffsetZ -= 2 * e.Delta;
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

                    Matrix3D m = ModelRotate.Matrix;

                    Vector3D v = new Vector3D(0, 0, -2000);
                    m.Translate(v);


                    m.Rotate(new Quaternion(new Vector3D(1, 0, 0), p*scale));
                    m.Rotate(new Quaternion(new Vector3D(0, 1, 0) * m, y*scale));
                    //m.Rotate(new Quaternion(new Vector3D(0, 0, 1) * m, z));

                    m.Translate(-v);

                    ModelRotate.Matrix = m;

                    m = StarfieldRotMatrix.Matrix;
                    m.Rotate(new Quaternion(new Vector3D(1, 0, 0), p * scale));
                    m.Rotate(new Quaternion(new Vector3D(0, 1, 0) * m, y * scale));
                    //m.Rotate(new Quaternion(new Vector3D(0, 0, 1) * m, z));
                    StarfieldRotMatrix.Matrix = m;
                }
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    Vector delta = Point.Subtract(Mouse, last);
                    Matrix3D m = ModelRotate.Matrix;
                    m.Translate(new Vector3D(delta.X * 3, delta.Y * 3, 0));
//                    ModelRotate.Matrix = m;

                    ModelTranslate.OffsetX += delta.X * 3;
                    ModelTranslate.OffsetY += delta.Y * 3;
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
