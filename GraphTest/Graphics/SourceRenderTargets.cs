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

            Color = new RenderTarget2D(gt.GraphicsDevice, 1920, 1080, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 4, RenderTargetUsage.PreserveContents) { Name = "Color" };
            Position = new RenderTarget2D(gt.GraphicsDevice, 1920, 1080, false, SurfaceFormat.Vector4, DepthFormat.None, 4, RenderTargetUsage.DiscardContents) { Name = "Position" };
            Normal = new RenderTarget2D(gt.GraphicsDevice, 1920, 1080, false, SurfaceFormat.Vector4, DepthFormat.None, 4, RenderTargetUsage.DiscardContents) { Name = "Normal" };
            DepthMask = new RenderTarget2D(gt.GraphicsDevice, 1920, 1080, false, SurfaceFormat.Vector2, DepthFormat.None, 4, RenderTargetUsage.DiscardContents) { Name = @"Depth/Mask" };

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
