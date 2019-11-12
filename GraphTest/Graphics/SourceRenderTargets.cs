using Microsoft.Xna.Framework.Graphics;

namespace GraphTest
{
    public class SourceRenderTargets
    {
        public RenderTargetBinding[] Targets { get; private set; }

        public RenderTarget2D Color { get; private set; }
        public RenderTarget2D Position { get; private set; }
        public RenderTarget2D Normal { get; private set; }
        public RenderTarget2D DepthMask { get; private set; }

        public void Set(GraphicsDevice graphics) => graphics.SetRenderTargets(Targets);

        public SourceRenderTargets()
        {
            Color = Program.GraphTest.CreateRenderTarget(false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8,2, RenderTargetUsage.PreserveContents);
            Position = Program.GraphTest.CreateRenderTarget(false, SurfaceFormat.Vector4, DepthFormat.None, 2, RenderTargetUsage.DiscardContents);
            Normal = Program.GraphTest.CreateRenderTarget(false, SurfaceFormat.Vector4, DepthFormat.None, 2, RenderTargetUsage.DiscardContents);
            DepthMask = Program.GraphTest.CreateRenderTarget(false, SurfaceFormat.Vector2, DepthFormat.None, 2, RenderTargetUsage.PreserveContents);

            Targets = new RenderTargetBinding[]
            {
                new RenderTargetBinding(Color),
                new RenderTargetBinding(Position),
                new RenderTargetBinding(Normal),
                new RenderTargetBinding(DepthMask),
            };
        }

    }
}
