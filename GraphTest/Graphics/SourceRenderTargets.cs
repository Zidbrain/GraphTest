using Microsoft.Xna.Framework.Graphics;

namespace GraphTest
{
    public class SourceRenderTargets
    {
        private readonly RenderTargetBinding[] _targets;

        public RenderTarget2D Color { get; private set; }
        public RenderTarget2D Position { get; private set; }
        public RenderTarget2D Normal { get; private set; }
        public RenderTarget2D DepthMask { get; private set; }

        public void Set(GraphicsDevice graphics) => graphics.SetRenderTargets(_targets);

        public SourceRenderTargets()
        {
            var gt = Program.GraphTest;

            Color = Program.GraphTest.CreateRenderTarget(false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8,2, RenderTargetUsage.PreserveContents);
            Position = Program.GraphTest.CreateRenderTarget(false, SurfaceFormat.Vector4, DepthFormat.None, 2, RenderTargetUsage.DiscardContents);
            Normal = Program.GraphTest.CreateRenderTarget(false, SurfaceFormat.Vector4, DepthFormat.None, 2, RenderTargetUsage.DiscardContents);
            DepthMask = Program.GraphTest.CreateRenderTarget(false, SurfaceFormat.Vector2, DepthFormat.None, 2, RenderTargetUsage.PreserveContents);

            _targets = new RenderTargetBinding[4]
            {
                new RenderTargetBinding(Color),
                new RenderTargetBinding(Position),
                new RenderTargetBinding(Normal),
                new RenderTargetBinding(DepthMask),
            };
        }

    }
}
