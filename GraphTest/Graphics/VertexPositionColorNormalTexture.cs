using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GraphTest
{
    public struct VertexPositionColorNormalTexture : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;

        private static readonly VertexDeclaration s_vertexDeclaration
            = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(16 + 12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                );

        public VertexPositionColorNormalTexture(Vector3 position, Color color, Vector3 normal, Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
        }

        public VertexDeclaration VertexDeclaration => s_vertexDeclaration;
    }
}