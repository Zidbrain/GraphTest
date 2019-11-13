using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraphTest
{
    public class Ground : IDrawable
    {
        private readonly VertexPositionColorNormalTexture[] _vertexes;
        private readonly Matrix _mat = Matrix.CreateTranslation(Vector3.Zero);
        private readonly Texture2D _normal;

        public DrawingEffects DrawingEffects => DrawingEffects.SeenThroughWindow | DrawingEffects.BasicDrawing | DrawingEffects.LightingEnabled;

        public Ground()
        {
            var vert = GraphTest.ConstructSquare(new Vector3(-10f, -0.25f, -10f), new Vector3(10f, -0.25f, 10f), true);
           _vertexes = new VertexPositionColorNormalTexture[vert.Length];

            for (var i = 0; i < vert.Length; i++)
            {
                _vertexes[i] = new VertexPositionColorNormalTexture(vert[i].Position, Color.Green, new Vector3(0f, 1f, 0f), vert[i].TextureCoordinate);
            }

            _normal = Program.GraphTest.Load<Texture2D>("normal");
        }

        public void Draw()
        {
            var effect = Program.GraphTest.Shader;
            effect.TextureEnabled = false;
            effect.ModelTransform = _mat;
            effect.NormalMapEnabled = true;
            effect.NormalBuffer = _normal;
            Program.GraphTest.DrawVertexes(_vertexes);
            effect.NormalMapEnabled = false;
        }
    }
}