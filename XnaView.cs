using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public struct VertexPositionColorNormal
    {
        public Vector3 Position;
        public Microsoft.Xna.Framework.Graphics.Color Color;
        public Vector3 Normal;

        public static int SizeInBytes = 7 * 4;
        public static VertexElement[] VertexElements = new VertexElement[]
             {
                 new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
                 new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0 ),
                 new VertexElement( 0, sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
             };
    }

    public partial class XnaView : UserControl, IGraphicsDeviceService
    {
        Vector4 clearColor = new Vector4(0, 0, 0, 1);
        RenderTarget2D mRenderTarget = null;
        DepthStencilBuffer depthStencil = null;
        PrimitiveBatch2D mPrimitiveBatch2D = null;
        PrimitiveBatch3D mPrimitiveBatch3D = null;
        Texture2D texture = null;

        class XnaGraphics : IServiceProvider
        {
            static object CriticalSection = new object();
            static GraphicsDevice mDevice = null;
            static DepthStencilBuffer mDefaultDepthStencil;
            static SpriteBatch mSpriteBatch = null;
            static ContentManager ContentManager = null;

            public XnaGraphics(Control owner)
            {
                lock (CriticalSection)
                {
                    if (mDevice != null)
                        return;

                    while (owner.Parent != null)
                        owner = owner.Parent;

                    PresentationParameters presentation = new PresentationParameters();
                    presentation.AutoDepthStencilFormat = DepthFormat.Depth24;
                    presentation.BackBufferCount = 1;
                    presentation.BackBufferFormat = SurfaceFormat.Color;
                    presentation.BackBufferWidth = Screen.PrimaryScreen.Bounds.Width;
                    presentation.BackBufferHeight = Screen.PrimaryScreen.Bounds.Height;
                    presentation.DeviceWindowHandle = owner.Handle;
                    presentation.EnableAutoDepthStencil = true;
                    presentation.FullScreenRefreshRateInHz = 0;
                    presentation.IsFullScreen = false;
                    presentation.MultiSampleQuality = 0;
                    presentation.MultiSampleType = MultiSampleType.None;
                    presentation.PresentationInterval = PresentInterval.One;
                    presentation.PresentOptions = PresentOptions.None;
                    presentation.SwapEffect = SwapEffect.Discard;
                    presentation.RenderTargetUsage = RenderTargetUsage.DiscardContents;

                    //            gfxDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, DeviceType.Hardware, this.Handle,
                    //                CreateOptions.HardwareVertexProcessing, presentation);
                    mDevice = new GraphicsDevice(
                        GraphicsAdapter.DefaultAdapter,
                        DeviceType.Hardware,
                        owner.Handle,
                        presentation);

                    // initialize the content manager
                    ContentManager = new ContentManager(this);

                    mDefaultDepthStencil = mDevice.DepthStencilBuffer;
                    
                    mSpriteBatch = new SpriteBatch(mDevice);
                }
            }

            public GraphicsDevice Device { get { return mDevice; } }
            public DepthStencilBuffer DefaultDepthStencil { get { return mDefaultDepthStencil; } }
            public SpriteBatch SpriteBatch { get { return mSpriteBatch; } }

            #region IServiceProvider Members

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService))
                {
                    return this;
                }
                else
                {
                    return null;
                }
            }

            #endregion
        }
        XnaGraphics Graphics = null;
        public GraphicsDevice Device { get { return Graphics.Device; } }
        public DepthStencilBuffer DefaultDepthStencil { get { return Graphics.DefaultDepthStencil; } }
        public PrimitiveBatch2D PrimitiveBatch2D { get { return mPrimitiveBatch2D; } }
        public PrimitiveBatch3D PrimitiveBatch3D { get { return mPrimitiveBatch3D; } }
        public SpriteBatch SpriteBatch { get { return Graphics.SpriteBatch; } }

        [Category("Appearance"), Description("Clear color for the control."), Browsable(true)]
        public Vector4 ClearColor
        {
            get { return clearColor; }
            set { clearColor = value; if (DesignMode) Invalidate();  }
        }

        public RenderTarget2D RenderTarget
        {
            get
            {
                if (mRenderTarget == null)
                {
                    int w = RenderArea.Width * 1;
                    int h = RenderArea.Height * 1;
                    mRenderTarget = new RenderTarget2D(Graphics.Device, w, h, 1, SurfaceFormat.Color);
                    depthStencil = new DepthStencilBuffer(Graphics.Device, w, h, DepthFormat.Depth24);

                    // Create camera and projection matrix
                    PrimitiveBatch3D.ProjectionMatrix = Microsoft.Xna.Framework.Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)w/h*232/256, 1.0f, 10000.0f);
                }
                return mRenderTarget;
            }
        }

        public DepthStencilBuffer DepthStencil
        {
            get { return depthStencil; }
        }

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        Rectangle mRenderArea = new Rectangle(0, 0, 0, 0);
        public Rectangle RenderArea { get { return mRenderArea; } }

        public XnaView()
        {
            InitializeComponent();

            Graphics = new XnaGraphics(this);
            mPrimitiveBatch2D = new PrimitiveBatch2D(Graphics.Device);
            mPrimitiveBatch3D = new PrimitiveBatch3D(Graphics.Device);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                    return true;
                case Keys.Shift | Keys.Right:
                case Keys.Shift | Keys.Left:
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        // don't show control background
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Texture == null)
            {
                System.Drawing.Color c = System.Drawing.Color.FromArgb(
                    (byte)(clearColor.X * 255),
                    (byte)(clearColor.Y * 255),
                    (byte)(clearColor.Z * 255));
                using (System.Drawing.Brush b = new System.Drawing.SolidBrush(c))
                {
                    e.Graphics.FillRectangle(b, ClientRectangle);
                }

                float max = ClearColor.X;
                if (ClearColor.Y > max) max = ClearColor.Y;
                if (ClearColor.Z > max) max = ClearColor.Z;

                System.Drawing.StringFormat f = new System.Drawing.StringFormat();
                f.LineAlignment = System.Drawing.StringAlignment.Center;
                f.Alignment = System.Drawing.StringAlignment.Center;
                e.Graphics.DrawString(
                    this.Name,
                    this.Font,
                    (max > 0.5) ? System.Drawing.Brushes.Black : System.Drawing.Brushes.White,
                    ClientRectangle, f);
            }
        }

        // paint the texture to the control
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Texture != null)
                ShowTexture();
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            base.OnSizeChanged(e);

            int w = Width;
            if (w < 1) w = 1;
            if (w > Graphics.Device.PresentationParameters.BackBufferWidth) w = Graphics.Device.PresentationParameters.BackBufferWidth;
            int h = Height;
            if (h < 1) h = 1;
            if (h > Graphics.Device.PresentationParameters.BackBufferHeight) h = Graphics.Device.PresentationParameters.BackBufferHeight;
            mRenderArea.Width = w;
            mRenderArea.Height = h;

            mRenderTarget = null; // force render target to be re-created
        }

        public void BeginRender(bool clear)
        {
            // enable rendering on our surface
            Graphics.Device.SetRenderTarget(0, RenderTarget);
            Graphics.Device.DepthStencilBuffer = DepthStencil;

            // clear the buffer
            if (clear)
            {
                ClearOptions options = ClearOptions.Target | ClearOptions.DepthBuffer;
                float depth = 1;
                int stencil = 128;
                Graphics.Device.Clear(options, ClearColor, depth, stencil);
            }
        }

        public void EndRender()
        {           
            // end rendering
            Graphics.Device.SetRenderTarget(0, null);
            Graphics.Device.DepthStencilBuffer = Graphics.DefaultDepthStencil;

            // get the renderer output
            Texture = RenderTarget.GetTexture();

            // show it on the screen
            ShowTexture();
        }

        void ShowTexture()
        {
            Graphics.Device.RenderState.MultiSampleAntiAlias = true;
            Graphics.Device.RenderState.FillMode = FillMode.Solid;
            Graphics.Device.Clear(Color.White);
            SpriteBatch.Begin();
            SpriteBatch.Draw(Texture, RenderArea, Microsoft.Xna.Framework.Graphics.Color.White);
            SpriteBatch.End();

            try
            {
                Graphics.Device.Present(RenderArea, null, Handle);
            }
            catch
            {
            }
        }

        #region IGraphicsDeviceService Members

        public event EventHandler DeviceCreated;
        public event EventHandler DeviceDisposing;
        public event EventHandler DeviceReset;
        public event EventHandler DeviceResetting;

        public GraphicsDevice GraphicsDevice
        {
            get { return this.Device; }
        }

        #endregion
    }

    // this class could serve as a base class for an XNA form
    public class EditorControls : GameComponent
    {
        Form windowsGameForm;
        IGraphicsDeviceService Service;
        GraphicsDevice Device;

        Panel RenderPanel;
        MenuStrip MainMenu;

        public EditorControls(Game game) : base(game) { }

        public override void Initialize()
        {
            Service = Game.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
            Device = Service.GraphicsDevice;
            windowsGameForm = Control.FromHandle(Game.Window.Handle) as Form;
            InitializeComponent();

            Service.DeviceResetting += new EventHandler(OnDeviceReset);
            Service.DeviceCreated += new EventHandler(OnDeviceCreated);
            Device.Reset();
        }

        void InitializeComponent()
        {
            MainMenu = new MenuStrip();
            RenderPanel = new Panel();
            MainMenu.SuspendLayout();
            windowsGameForm.SuspendLayout();
            MainMenu.Location = new System.Drawing.Point(0, 0);
            MainMenu.Name = "MainMenu";
            MainMenu.Size = new System.Drawing.Size(741, 24);
            MainMenu.TabIndex = 0;
            RenderPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            RenderPanel.Location = new System.Drawing.Point(0, 49);
            RenderPanel.Name = "RenderPanel";
            RenderPanel.Size = new System.Drawing.Size(800, 600);
            RenderPanel.TabIndex = 2;
            windowsGameForm.Controls.Add(MainMenu);
            windowsGameForm.Controls.Add(RenderPanel);
            MainMenu.ResumeLayout(false);
            MainMenu.PerformLayout();
            windowsGameForm.ResumeLayout(false);
            windowsGameForm.PerformLayout();
        }

        void OnDeviceCreated(object sender, EventArgs e)
        {
            Device = Service.GraphicsDevice;
            Device.Reset();
        }

        void OnDeviceReset(object sender, EventArgs e)
        {
            Device.PresentationParameters.DeviceWindowHandle = RenderPanel.Handle;
            Device.PresentationParameters.BackBufferWidth = RenderPanel.Width;
            Device.PresentationParameters.BackBufferHeight = RenderPanel.Height;
        }
    }

}
