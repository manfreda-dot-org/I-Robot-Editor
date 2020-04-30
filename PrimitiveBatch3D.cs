using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    // XnaPrimitiveBatch is a class that handles efficient rendering automatically for its
    // users, in a similar way to SpriteBatch. PrimitiveBatch can render lines, points,
    // and triangles to the screen. In this sample, it is used to draw a spacewars
    // retro scene.
    public class PrimitiveBatch3D
    {
        public Matrix ViewMatrix
        {
            get { return Effect.View; }
            set { Effect.View = value; Effect.CommitChanges(); }
        }

        public Matrix WorldMatrix
        {
            get { return Effect.World; }
            set { Effect.World = value; Effect.CommitChanges(); }
        }

        public Matrix ProjectionMatrix
        {
            get { return Effect.Projection; }
            set { Effect.Projection = value; Effect.CommitChanges(); }
        }

        // a basic effect, which contains the shaders that we will use to draw our
        // primitives.
        public BasicEffect Effect;

        // the device that we will issue draw calls to.
        GraphicsDevice device;


        // the constructor creates a new XnaPrimitiveBatch and sets up all of the internals
        // that XnaPrimitiveBatch will need.
        public PrimitiveBatch3D(GraphicsDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            this.device = device;

            // set up a new basic effect, and enable vertex colors.
            Effect = new BasicEffect(device, null);
            Effect.VertexColorEnabled = true;

//            basicEffect.LightingEnabled = true;
            Effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
            Effect.DirectionalLight0.Enabled = true;
            Effect.DirectionalLight0.DiffuseColor = Color.Gray.ToVector3();
            Effect.DirectionalLight0.SpecularColor = Color.Black.ToVector3();
            Effect.DirectionalLight0.Direction = new Vector3(0, -1, -1);
            Effect.DirectionalLight0.Direction.Normalize();

            WorldMatrix = Microsoft.Xna.Framework.Matrix.Identity;
            ViewMatrix = Microsoft.Xna.Framework.Matrix.CreateLookAt(Vector3.Backward*10 + Vector3.Up*10, Vector3.Zero,- Vector3.Up);

            // projection uses CreateOrthographicOffCenter to create 2d projection
            // matrix with 0,0 in the upper left.
            ProjectionMatrix = Microsoft.Xna.Framework.Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, (float)device.Viewport.Width / device.Viewport.Height, 1.0f, 10.0f);
        }

        // Begin is called to tell the XnaPrimitiveBatch what kind of primitives will be
        // drawn, and to prepare the graphics card to render those primitives.
        bool DepthEnabled = false;
        public void Begin(VertexDeclaration declaration)
        {
            // prepare the graphics device for drawing by setting the vertex declaration
            // and telling our basic effect to begin.
            //device.RenderState.CullMode = CullMode.CullClockwiseFace;
            device.RenderState.CullMode = CullMode.None;
            DepthEnabled = device.RenderState.DepthBufferEnable;
            device.RenderState.DepthBufferEnable = true;
            device.VertexDeclaration = declaration;

            Effect.Begin();
            Effect.CurrentTechnique.Passes[0].Begin();
        }

        // End is called once all the primitives have been drawn using AddVertex.
        // it will call Flush to actually submit the draw call to the graphics card, and
        // then tell the basic effect to end.
        public void End()
        {
            // and then tell basic effect that we're done.
            Effect.CurrentTechnique.Passes[0].End();
            Effect.End();
            device.RenderState.DepthBufferEnable = DepthEnabled;
        }
    }
}
