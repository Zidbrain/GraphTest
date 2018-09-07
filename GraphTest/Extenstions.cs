using Microsoft.Xna.Framework.Graphics;

namespace GraphTest
{
    public static class Extenstions
    {
        public static void SetRenderTargets(this GraphicsDevice graphics, SourceRenderTargets targets) => targets.Set(graphics);
    }
}
