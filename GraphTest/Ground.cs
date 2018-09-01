using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using static System.Math;

namespace GraphTest
{
    public class Ground : IDrawable
    {
        private DynamicVertexBuffer _buffer;

        public DrawingEffects DrawingEffects => DrawingEffects.SeenThroughWindow | DrawingEffects.BasicDrawing | DrawingEffects.LightingEnabled;

        public Ground()
        {
            _buffer = new DynamicVertexBuffer(Program.GraphTest.GraphicsDevice, typeof(VertexPositionColorNormalTexture), 6, BufferUsage.WriteOnly);
            var vert = GraphTest.ConstructSquare(new Vector3(-10f, -0.25f, -10f), new Vector3(10f, -0.25f, 10f), true);
            var vertexes = new VertexPositionColorNormalTexture[vert.Length];

            for (int i = 0; i < vert.Length; i++)
            {
                vertexes[i] = new VertexPositionColorNormalTexture(vert[i].Position, Color.Green, new Vector3(0f, 1f, 0f), vert[i].TextureCoordinate);
            }
            _buffer.SetData(vertexes);
        }

        public void Draw()
        {
            var effect = Program.GraphTest.Shader;
            effect.TextureEnabled = false;
            Program.GraphTest.DrawVertexes(_buffer, ShaderInputType.Primitive);
        }
    }
}