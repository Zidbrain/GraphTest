using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraphTest
{
    public class Ground : IDrawable
    {
        private readonly DynamicVertexBuffer _buffer;
        private readonly Matrix _mat = Matrix.CreateTranslation(Vector3.Zero);

        public DrawingEffects DrawingEffects => DrawingEffects.SeenThroughWindow | DrawingEffects.BasicDrawing | DrawingEffects.LightingEnabled;

        public Ground()
        {
            _buffer = new DynamicVertexBuffer(Program.GraphTest.GraphicsDevice, typeof(VertexPositionColorNormalTexture), 6, BufferUsage.WriteOnly);
            var vert = GraphTest.ConstructSquare(new Vector3(-10f, -0.25f, -10f), new Vector3(10f, -0.25f, 10f), true);
            var vertexes = new VertexPositionColorNormalTexture[vert.Length];

            for (var i = 0; i < vert.Length; i++)
            {
                vertexes[i] = new VertexPositionColorNormalTexture(vert[i].Position, Color.Green, new Vector3(0f, 1f, 0f), vert[i].TextureCoordinate);
            }
            _buffer.SetData(vertexes);
        }

        public void Draw()
        {
            var effect = Program.GraphTest.Shader;
            effect.TextureEnabled = false;
            effect.ModelTransform = _mat;
            Program.GraphTest.DrawVertexes(_buffer, ShaderInputType.Primitive);
        }
    }
}