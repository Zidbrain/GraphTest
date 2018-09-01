using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Math;

namespace GraphTest
{
    public class Rzu : IDrawable
    {
        private DynamicVertexBuffer _buffer;
        private VertexPositionColorNormalTexture[] _vertexes;
        private readonly Texture2D _texture;
        private Model _model;
        private Vector3 _position;
        private readonly Vector3[] _offsets;

        public DrawingEffects DrawingEffects => DrawingEffects.Standart;

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                for (int i = 0; i < _vertexes.Length; i++)
                {
                    _vertexes[i] = new VertexPositionColorNormalTexture(_position + _offsets[i], _vertexes[i].Color, _vertexes[i].Normal, _vertexes[i].TextureCoordinate);
                }
                _buffer.SetData(_vertexes);
            }
        }
        public float Size { get; private set; }

        public Rzu(Vector3 position, float size)
        {
            Size = size;

            _vertexes = GraphTest.ConstructCube(size, Color.White);
            _offsets = new Vector3[_vertexes.Length];
            for (int i = 0; i < _offsets.Length; i++)
                _offsets[i] = _vertexes[i].Position;

            _texture = Program.GraphTest.Load<Texture2D>("lol");
            _model = Program.GraphTest.Load<Model>("thonk");

            _buffer = new DynamicVertexBuffer(Program.GraphTest.GraphicsDevice, typeof(VertexPositionColorNormalTexture), _vertexes.Length, BufferUsage.WriteOnly);
            Position = position;
        }

        public void Draw()
        {
            _model.Root.Transform = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(_vertexes[0].Position + new Vector3(0.3f, -0.1f, 0.5f));

            var effect = Program.GraphTest.Shader;
            var gd = Program.GraphTest.GraphicsDevice;

            effect.TextureEnabled = true;
            effect.Texture = _texture;

            Program.GraphTest.DrawVertexes(_buffer, ShaderInputType.Primitive);

            effect.TextureEnabled = false;
            effect.InputType = ShaderInputType.Mesh;
            effect.SpecularEnabled = true;

            foreach (var mesh in _model.Meshes)
            {
                var meshEffect = (BasicEffect)mesh.Effects[0];

                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = effect.Effect;
                }
                effect.Color = new Vector4(meshEffect.DiffuseColor, 1f);
                var matrix = _model.Root.Transform * mesh.ParentBone.Transform;
                effect.Matrix = matrix * Program.GraphTest.Matrix;
                effect.ModelTransform = matrix;

                mesh.Draw();

                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = meshEffect;
                }
            }

            effect.Matrix = Program.GraphTest.Matrix;
            effect.SpecularEnabled = false;
        }
    }
}
