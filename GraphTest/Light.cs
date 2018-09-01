using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static Microsoft.Xna.Framework.MathHelper;

namespace GraphTest
{
    public class LightEngine
    {
        public RenderTargetBinding[] _softShadows;
        private DynamicVertexBuffer _vertexes;

        public List<Light> Lights { get; private set; }

        public LightEngine()
        {
            _softShadows = new RenderTargetBinding[]
           {
                new RenderTargetBinding(new RenderTarget2D(Program.GraphTest.GraphicsDevice,1920,1080, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 4, RenderTargetUsage.PreserveContents) { Name = "SoftShadows" }),
               //new RenderTargetBinding(new RenderTarget2D(Program.GraphTest.GraphicsDevice, 1920, 1080, false, SurfaceFormat.Single, DepthFormat.None, 4, RenderTargetUsage.DiscardContents) { Name = "EmptyForSoftShadows" })
           };

            Lights = new List<Light>();

            _vertexes = new DynamicVertexBuffer(Program.GraphTest.GraphicsDevice, typeof(VertexPositionColorNormalTexture), 6, BufferUsage.WriteOnly);
            var vert = new VertexPositionColorNormalTexture[]
            {
                new VertexPositionColorNormalTexture(new Vector3(-1,1,1), Color.White, Vector3.Zero, Vector2.Zero),
                new VertexPositionColorNormalTexture(new Vector3(-1,-1,1), Color.White, Vector3.Zero, new Vector2(0f, 1f)),
                new VertexPositionColorNormalTexture(new Vector3(1,-1, 1), Color.White, Vector3.Zero, Vector2.One),
                new VertexPositionColorNormalTexture(new Vector3(1,-1, 1), Color.White, Vector3.Zero, Vector2.One),
                new VertexPositionColorNormalTexture(new Vector3(-1,1,1), Color.White, Vector3.Zero, Vector2.Zero),
                new VertexPositionColorNormalTexture(new Vector3(1,1,1), Color.White, Vector3.Zero, new Vector2(1f,0f))
            };
            _vertexes.SetData(vert);
        }

        public void Draw()
        {
            var gd = Program.GraphTest.GraphicsDevice;
            var gt = Program.GraphTest;

            gd.SetRenderTargets(_softShadows);
            gd.Clear(Color.White);
            gt.Shader.DepthBuffer = (Texture2D)_softShadows[0].RenderTarget;

            foreach (var light in Lights)
            {
                light.AppendShadow(_softShadows);
            }
            gt.Present();

            gt.Shader.Texture = (Texture2D)_softShadows[0].RenderTarget;
            gt.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            gt.Shader.Technique = ShaderTechnique.Blur;
            gt.Shader.Matrix = Matrix.Identity;
            for (int i = 0; i < 4; i++)
            {
                gt.Shader.VerticalBlur = false;
                gt.DrawVertexes(_vertexes, ShaderInputType.Primitive);
                gt.Present();
                gt.Shader.VerticalBlur = true;
                gt.DrawVertexes(_vertexes, ShaderInputType.Primitive);
                gt.Present();
            }

            gt.Shader.Texture = (Texture2D)gt.RenderTarget[0].RenderTarget;
            gt.Shader.ShadowMap = (Texture2D)_softShadows[0].RenderTarget;
            gt.Shader.PositionBuffer = (Texture2D)gt.RenderTarget[1].RenderTarget;
            gt.Shader.NormalBuffer = (Texture2D)gt.RenderTarget[2].RenderTarget;
            gt.Shader.DepthBuffer = (Texture2D)gt.RenderTarget[3].RenderTarget;

            gd.SetRenderTargets(gt.RenderTarget);
            gt.Shader.Technique = ShaderTechnique.Gamma;
            gt.DrawVertexes(_vertexes, ShaderInputType.Primitive);

            gd.SetRenderTarget(gt.United);

            gt.Shader.Technique = ShaderTechnique.ApplyAmbient;
            gt.DrawVertexes(_vertexes, ShaderInputType.Primitive);
            gt.Present();
            gt.Shader.Texture = gt.United;
            gt.Shader.RenderTarget = (Texture2D)gt.RenderTarget[0].RenderTarget;

            gt.Shader.Technique = ShaderTechnique.ApplyLighting;

            foreach (var light in Lights)
            {
                gt.Shader.LightPosition = light.Position;
                gt.Shader.DiffuseRadius = light.Radius;
                gt.DrawVertexes(_vertexes, ShaderInputType.Primitive);
                gt.Present();
            }

            gt.DrawingQueue.InsideCall = false;
            gt.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            gt.Shader.Matrix = gt.Matrix;
            gt.Shader.Technique = ShaderTechnique.Standart;
        }
    }

    public class Light
    {
        public RenderTargetBinding[] _shadowMap;
        private Box _box = new Box(0.25f);
        private Vector3[] _sides;
        private Vector3[] _ups;

        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public float Radius { get; set; } = 1f;

        public Light()
        {
            _shadowMap = new RenderTargetBinding[]
{
                new RenderTargetBinding(new RenderTarget2D(Program.GraphTest.GraphicsDevice,2048,2048, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 4, RenderTargetUsage.DiscardContents) {Name = "ShadowMap" }),
};
            _sides = new Vector3[]
            {
                Vector3.Forward,
                Vector3.Backward,
                Vector3.Down,
                Vector3.Up,
                Vector3.Left,
                Vector3.Right
            };
            _ups = new Vector3[]
            {
                Vector3.Up,
                Vector3.Up,
                Vector3.Forward,
                Vector3.Backward,
                Vector3.Up,
                Vector3.Up
            };
        }

        public void AppendShadow(RenderTargetBinding[] shadowsTarget)
        {
            for (int i = 0; i < 2; i++)
            {
                var gt = Program.GraphTest;
                var mat = gt.Matrix;
                var lightmat = Matrix.CreateLookAt(Position, Position + _sides[i], _ups[i]) *
                        Matrix.CreatePerspectiveFieldOfView(ToRadians(90f), 1f, 0.01f, 100f);

                // initial shadowm map
                gt.Shader.Technique = ShaderTechnique.WriteDepth;
                gt.DrawingQueue.InsideCall = true;

                gt.GraphicsDevice.SetRenderTargets(_shadowMap);
                gt.GraphicsDevice.Clear(Color.Black);
                gt.Matrix = lightmat;
                gt.Shader.LightMatirix = gt.Matrix;
                gt.Shader.Matrix = gt.Matrix;
                gt.DrawingQueue.Draw(DrawingEffects.CastsShadow);
                _box.Posiiton = gt.CameraPosition;
                _box.Draw();

                // shadow map from the perspective of the camera
                gt.GraphicsDevice.SetRenderTargets(shadowsTarget);
                gt.Shader.ShadowMap = (Texture2D)_shadowMap[0].RenderTarget;
                gt.Shader.Matrix = mat;
                gt.Matrix = mat;
                gt.Shader.Technique = ShaderTechnique.SoftShadows;
                gt.DrawingQueue.Draw(DrawingEffects.LightingEnabled);
            }
        }
    }
}
