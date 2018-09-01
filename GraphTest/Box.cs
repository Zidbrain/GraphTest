using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraphTest
{
    public class SkyBox : IDrawable
    {
        private DynamicVertexBuffer _buffer;
        private readonly VertexPositionColorNormalTexture[] _vertexes;
        private readonly Texture2D[] _textures;

        public DrawingEffects DrawingEffects => DrawingEffects.BasicDrawing;

        public SkyBox()
        {
            _vertexes = GraphTest.ConstructCube(1f, Color.White);

            _buffer = new DynamicVertexBuffer(Program.GraphTest.GraphicsDevice, typeof(VertexPositionColorNormalTexture), 6, BufferUsage.WriteOnly);

            _textures = new Texture2D[]
            {
                Program.GraphTest.Load<Texture2D>("Skybox/back"),
                Program.GraphTest.Load<Texture2D>("Skybox/front"),
                Program.GraphTest.Load<Texture2D>("Skybox/right"),
                Program.GraphTest.Load<Texture2D>("Skybox/left"),
                Program.GraphTest.Load<Texture2D>("Skybox/top"),
                Program.GraphTest.Load<Texture2D>("Skybox/bottom"),
            };

        }

        public void Draw()
        {
            var ef = Program.GraphTest.Shader;
            var mat = Program.GraphTest.Matrix;

            Program.GraphTest.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            ef.TextureEnabled = true;
            ef.Matrix = Matrix.CreateTranslation(Program.GraphTest.CameraPosition) * mat;

            for (int i = 0; i < 6; i++)
            {
                ef.Texture = _textures[i];
                _buffer.SetData(_vertexes, i * 6, 6);
                Program.GraphTest.DrawVertexes(_buffer, ShaderInputType.Primitive);
            }

            ef.Matrix = mat;
            Program.GraphTest.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
    public class Box : IDrawable
    {
        private DynamicVertexBuffer _buffer;
        private VertexPositionColorNormalTexture[] _vertexes;
        private Vector3 _position;
        private readonly Vector3[] _offsets;

        public Texture2D Texture { get; set; }

        public bool IsTextureEnabled { get; set; }

        public DrawingEffects DrawingEffects => DrawingEffects.Standart;

        public Vector3 Posiiton
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

        public Box(float size)
        {
            _buffer = new DynamicVertexBuffer(Program.GraphTest.GraphicsDevice, typeof(VertexPositionColorNormalTexture), 36, BufferUsage.WriteOnly);
            _vertexes = GraphTest.ConstructCube(size, Color.White);
            _offsets = new Vector3[_vertexes.Length];
            for (int i = 0; i < _offsets.Length; i++)
                _offsets[i] = _vertexes[i].Position;
            _buffer.SetData(_vertexes);
        }

        public void Draw()
        {
            var ef = Program.GraphTest.Shader;

            ef.TextureEnabled = IsTextureEnabled;
            ef.Texture = Texture;

            Program.GraphTest.DrawVertexes(_buffer, ShaderInputType.Primitive);
        }
    }
}