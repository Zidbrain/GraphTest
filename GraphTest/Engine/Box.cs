using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraphTest
{
    public class SkyBox : IDrawable
    {
        private readonly VertexPositionColorNormalTexture[] _vertexes;
        private readonly Texture2D[] _textures;

        public DrawingEffects DrawingEffects => DrawingEffects.BasicDrawing | DrawingEffects.SeenThroughWindow;

        public SkyBox()
        {
            _vertexes = GraphTest.ConstructCube(1f, Color.White);

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
            var prev = ef.CheckDepth;

            Program.GraphTest.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            ef.WriteOnlyColor = true;
            ef.TextureEnabled = true;
            ef.CheckDepth = false;
            ef.Matrix = Matrix.CreateTranslation(Program.GraphTest.CameraPosition) * mat;

            var temp = new VertexPositionColorNormalTexture[6];
            for (var i = 0; i < 6; i++)
            {
                ef.Texture = _textures[i];
                for (int j = 0; j < 6; j++)
                    temp[j] = _vertexes[i * 6 + j];
                Program.GraphTest.DrawVertexes(temp);
            }

            ef.WriteOnlyColor = false;
            ef.Matrix = mat;
            ef.CheckDepth = prev;
            Program.GraphTest.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
    public class Box : IDrawable
    {
        private readonly VertexPositionColorNormalTexture[] _vertexes;
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

                for (var i = 0; i < _vertexes.Length; i++)
                {
                    _vertexes[i] = new VertexPositionColorNormalTexture(_position + _offsets[i], _vertexes[i].Color, _vertexes[i].Normal, _vertexes[i].TextureCoordinate);
                }
            }
        }

        public Box(float size)
        {
            _vertexes = GraphTest.ConstructCube(size, Color.White);
            _offsets = new Vector3[_vertexes.Length];
            for (var i = 0; i < _offsets.Length; i++)
                _offsets[i] = _vertexes[i].Position;
        }

        public void Draw()
        {
            var ef = Program.GraphTest.Shader;

            ef.TextureEnabled = IsTextureEnabled;
            ef.Texture = Texture;

            Program.GraphTest.DrawVertexes(_vertexes);
        }
    }
}