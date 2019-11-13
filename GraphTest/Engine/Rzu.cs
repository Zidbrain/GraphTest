using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraphTest
{
    public class Rzu : IDrawable
    {
        private readonly VertexPositionColorNormalTexture[] _vertexes;
        private readonly Texture2D _texture;
        private readonly Model _model;
        private Vector3 _position;
        private Matrix _posMat;
        private readonly Vector3[] _offsets;

        public DrawingEffects DrawingEffects => DrawingEffects.Standart;

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                _posMat = Matrix.CreateTranslation(_position);
            }
        }
        public float Size { get; private set; }

        public Rzu(Vector3 position, float size)
        {
            Size = size;

            _vertexes = GraphTest.ConstructCube(size, Color.White);
            _offsets = new Vector3[_vertexes.Length];
            for (var i = 0; i < _offsets.Length; i++)
                _offsets[i] = _vertexes[i].Position;
            _posMat = Matrix.CreateTranslation(Vector3.Zero);

            _texture = Program.GraphTest.Load<Texture2D>("lol");
            _model = Program.GraphTest.Load<Model>("thonk");

            Position = position;
        }

        public void Draw()
        {
            _model.Root.Transform = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(_position + new Vector3(0.1f, 0.2f, 0.3f));

            var effect = Program.GraphTest.Shader;
            var mat = effect.Matrix;

            effect.TextureEnabled = true;
            effect.Texture = _texture;
            effect.Matrix = _posMat * mat;
            effect.ModelTransform = _posMat;

            effect.Technique = ShaderTechnique.Standart;
            Program.GraphTest.DrawVertexes(_vertexes);

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
                effect.Technique = ShaderTechnique.Standart;
                Program.GraphTest.DrawVertexes(mesh);

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
