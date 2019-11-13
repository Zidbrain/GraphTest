using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using static Microsoft.Xna.Framework.MathHelper;
using static System.Math;

namespace GraphTest
{
    public class GraphTest : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private Rzu _rzu;
        private Rzu _rzu1;
        private Wall _wall;
        private Vector2 _yawpitch;
        private KeyboardState _state;
        private Ground _ground;
        private ModelManipulate _table;
        private HUD _hud;
        private List<Keys> _keys;
        private SkyBox _skybox;

        public Shader Shader { get; private set; }
        public Matrix Matrix { get; set; }
        public SourceRenderTargets RenderTargets { get; private set; }
        public SamplerState SamplerState { get; private set; }
        public RasterizerState RasterizerState { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public Vector3 CameraPosition { get; private set; }
        public DrawingQueue DrawingQueue { get; private set; }
        public LightEngine LightEngine { get; private set; }
        public List<Keys> KeysPressedOnce => new List<Keys>(_keys);
        public Vector3 CameraDirection { get; private set; } = new Vector3(0f, 0f, 1f);
        public float FPS { get; private set; }

        public Vector2 ScreenSize { get; private set; }

        public BlendState BlendState { get; private set; }

        public VertexPositionColorNormalTexture[] StaticVertexes { get; private set; }

        public GraphTest() : base()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferMultiSampling = false,
                GraphicsProfile = GraphicsProfile.HiDef,
                HardwareModeSwitch = false,
                SynchronizeWithVerticalRetrace = false,
            };
            IsFixedTimeStep = false;
        }

        public static VertexPositionColorNormalTexture[] ConstructSquare(Vector3 leftUpper, Vector3 rightDown, bool b = false)
        {
            var pos = new Vector3[]
            {
                leftUpper,
                new Vector3(leftUpper.X, rightDown.Y, b ? rightDown.Z : leftUpper.Z),
                rightDown,
                rightDown,
                leftUpper,
                new Vector3(rightDown.X, leftUpper.Y, b ?  leftUpper.Z : rightDown.Z)
            };

            var tex = new Vector2[]
            {
                Vector2.Zero,
                new Vector2(0f, 1f),
                Vector2.One,
                Vector2.One,
                Vector2.Zero,
                new Vector2(1f,0f)
            };

            var ret = new VertexPositionColorNormalTexture[6];
            for (var i = 0; i < 6; i++)
                ret[i] = new VertexPositionColorNormalTexture(pos[i], Color.White, Vector3.Zero, tex[i]);

            return ret;
        }

        public static VertexPositionColorNormalTexture[] ConstructCube(float sized, Color color)
        {
            var size = sized / 2f;

            var ret = new List<VertexPositionColorNormalTexture>();
            ret.AddRange(ConstructSquare(new Vector3(-size, size, -size), new Vector3(size, -size, -size))); // back
            ret.AddRange(ConstructSquare(new Vector3(size, size, size), new Vector3(-size, -size, size))); // front
            ret.AddRange(ConstructSquare(new Vector3(-size, size, size), new Vector3(-size, -size, -size))); // left
            ret.AddRange(ConstructSquare(new Vector3(size, size, -size), new Vector3(size, -size, size))); // right
            ret.AddRange(ConstructSquare(new Vector3(size, size, -size), new Vector3(-size, size, size), true)); // top
            ret.AddRange(ConstructSquare(new Vector3(-size, -size, size), new Vector3(size, -size, -size), true)); // bottom

            var normals = new Vector3[6]
            {
                new Vector3(0f, 0f, -1f),
                new Vector3(0f,0f,1f),
                new Vector3(-1f,0f,0f),
                new Vector3(1f,0f,0f),
                new Vector3(0f,1f,0f),
                new Vector3(0f,-1f,0f)
            };

            var retr = new VertexPositionColorNormalTexture[ret.Count];
            for (var i = 0; i < retr.Length; i++)
            {
                retr[i] = new VertexPositionColorNormalTexture(ret[i].Position, color, normals[i / 6], ret[i].TextureCoordinate);
            }

            return retr;
        }

        public bool DrawRayTracing { get; set; }

        public T Load<T>(string path) =>
            Content.Load<T>(path);

        private DynamicVertexBuffer _buffer;

        public void DrawVertexes(VertexPositionColorNormalTexture[] buffer)
        {
            for (int i = 0; i < buffer.Length / 6; i++)
            {
                var temp = new VertexPositionColorNormalTexture[6];
                for (int j = 0; j < 6; j++)
                    temp[j] = buffer[i * 6 + j];
                if (DrawRayTracing)
                {
                    Shader.PointOnPlane = temp[0].Position;
                    Shader.Normal = Vector3.Cross(temp[1].Position - temp[0].Position, temp[2].Position - temp[0].Position);
                    Shader.Technique = ShaderTechnique.RayTracing;

                    _buffer.SetData(StaticVertexes);
                    Shader.ApplyDraw(ShaderInputType.Primitive, 6);
                }
                else
                {
                    _buffer.SetData(temp);
                    Shader.ApplyDraw(ShaderInputType.Primitive, 6);
                }
            }
        }

        public void DrawVertexes(ModelMesh mesh)
        {
            if (DrawRayTracing)
                foreach (var part in mesh.MeshParts)
                {
                    var buffer = new VertexPositionColorNormalTexture[part.NumVertices];
                    part.VertexBuffer.GetData(buffer);

                    Shader.PointOnPlane = buffer[0].Position;
                    Shader.Normal = Vector3.Cross(buffer[1].Position - buffer[0].Position, buffer[2].Position - buffer[0].Position);
                    Shader.Technique = ShaderTechnique.RayTracing;

                    _buffer.SetData(StaticVertexes);
                    Shader.ApplyDraw(ShaderInputType.Primitive, 6);
                }
            else
                foreach (var part in mesh.MeshParts)
                {
                    GraphicsDevice.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);
                    GraphicsDevice.Indices = part.IndexBuffer;
                    Shader.ApplyDraw(ShaderInputType.Mesh, part.NumVertices);
                }

            GraphicsDevice.SetVertexBuffer(_buffer);
        }

        public void Present()
        {
            var target = GraphicsDevice.GetRenderTargets();
            GraphicsDevice.SetRenderTargets(null);
            GraphicsDevice.SetRenderTargets(target);
        }

        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";

            ScreenSize = new Vector2(GraphicsDevice.Adapter.CurrentDisplayMode.Width, GraphicsDevice.Adapter.CurrentDisplayMode.Height);

            Shader = new Shader();

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            SamplerState = new SamplerState()
            {
                Filter = TextureFilter.Point,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MaxAnisotropy = 0,
            };

            RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                MultiSampleAntiAlias = false,
            };

            BlendState = new BlendState()
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                AlphaSourceBlend = Blend.SourceAlpha,
                BlendFactor = Color.White,
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
            };

            RenderTargets = new SourceRenderTargets();

            United = CreateRenderTarget(false, SurfaceFormat.Color, DepthFormat.None, 2, RenderTargetUsage.PreserveContents);

            StaticVertexes = new VertexPositionColorNormalTexture[]
            {
                new VertexPositionColorNormalTexture(new Vector3(-1,1,1), Color.White, Vector3.Zero, Vector2.Zero),
                new VertexPositionColorNormalTexture(new Vector3(-1,-1,1), Color.White, Vector3.Zero, new Vector2(0f, 1f)),
                new VertexPositionColorNormalTexture(new Vector3(1,-1, 1), Color.White, Vector3.Zero, Vector2.One),
                new VertexPositionColorNormalTexture(new Vector3(1,-1, 1), Color.White, Vector3.Zero, Vector2.One),
                new VertexPositionColorNormalTexture(new Vector3(-1,1,1), Color.White, Vector3.Zero, Vector2.Zero),
                new VertexPositionColorNormalTexture(new Vector3(1,1,1), Color.White, Vector3.Zero, new Vector2(1f,0f))
            };

            _ground = new Ground();
            _wall = new Wall();
            _rzu = new Rzu(new Vector3(10f, 0.5f, -1.5f), 0.5f);
            _rzu1 = new Rzu(new Vector3(-0.5f, 0.5f, -1.5f), 0.5f);
            Mouse.SetPosition(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);

            _table = new ModelManipulate("untitled") { Position = new Vector3(-2f, 0.25f, -4f), Size = new Vector3(0.3f), SpecularEnabled = true };

            DrawingQueue = new DrawingQueue();

            Shader.DiffuseIntensity = 1f;
            Shader.DiffuseRadius = 3f;
            Shader.AmbientColor = new Vector3(0.5f);

            _hud = new HUD();
            _keys = new List<Keys>();
            _skybox = new SkyBox();

            LightEngine = new LightEngine() { Enabled = true, LightMode = LightMode.Default };
            LightEngine.Lights.Add(new Light() { Position = new Vector3(5f, 0f, 5f), Radius = 20f, ShadowsEnabled = false });
            LightEngine.Lights.Add(new Light { Radius = 10 });

            _buffer = new DynamicVertexBuffer(GraphicsDevice, typeof(VertexPositionColorNormalTexture), 6, BufferUsage.WriteOnly);
            GraphicsDevice.SetVertexBuffer(_buffer);

            base.LoadContent();
        }

        public RenderTarget2D United { get; private set; }

        public Vector2 ToScreenCoordinates(Vector3 worldCoordinates, Matrix transform)
        {
            var convert = (Vector3.Transform(worldCoordinates, transform));
            return new Vector2(convert.X / convert.Z + 1f, -convert.Y / convert.Z + 1f) * new Vector2(Window.ClientBounds.Width / 2f, Window.ClientBounds.Height / 2f);
        }

        private void UpdateCameraDirection()
        {
            if (IsActive)
            {
                var dif = (Mouse.GetState().Position - new Point(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2)).ToVector2() / 500f;
                _yawpitch -= dif;
                CameraDirection = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(_yawpitch.X, _yawpitch.Y, 0f));

                Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            }
        }

        private void UpdateCameraPosition()
        {
            var factor = (float)GameTime.ElapsedGameTime.TotalMilliseconds / (1000f / 60f);
            var boost = Keyboard.GetState().IsKeyDown(Keys.LeftShift);
            var speed = CameraDirection * 0.1f * factor;
            var right = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(_yawpitch.X - PiOver2, 0f, 0f)) * 0.1f * factor;
            var up = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(_yawpitch.X, _yawpitch.Y + PiOver2, 0f)) * 0.1f * factor;
            var vector = Vector3.Zero;
            foreach (var key in Keyboard.GetState().GetPressedKeys())
            {
                switch (key)
                {
                    case Keys.W:
                        vector += speed;
                        break;
                    case Keys.S:
                        vector += -speed;
                        break;
                    case Keys.D:
                        vector += right;
                        break;
                    case Keys.A:
                        vector += -right;
                        break;
                    case Keys.Space:
                        vector += up;
                        break;
                    case Keys.LeftControl:
                        vector += -up;
                        break;
                }
            }
            CameraPosition += boost ? vector * 2f : vector;
        }

        public GameTime GameTime { get; private set; }

        protected override void Update(GameTime gameTime)
        {
            GameTime = gameTime;

            FPS = 1000f / (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            UpdateCameraDirection();
            if (!_hud.IsConsoleEnabled) UpdateCameraPosition();

            Matrix = Matrix.CreateLookAt(CameraPosition, CameraPosition + CameraDirection, Vector3.UnitY) *
                    Matrix.CreatePerspectiveFieldOfView(ToRadians(45f), ScreenSize.X / ScreenSize.Y, 0.01f, 100f);
            Shader.Matrix = Matrix;
            Shader.ViewVector = CameraDirection;
            Shader.Time = (float)gameTime.TotalGameTime.TotalMilliseconds * 0.001f;

            _keys.Clear();
            var state = Keyboard.GetState();
            if (state != _state)
            {
                if (!_hud.IsConsoleEnabled)
                {
                    if (state.IsKeyDown(Keys.F))
                        _graphics.ToggleFullScreen();
                    if (state.IsKeyDown(Keys.Escape))
                        Exit();
                }

                _keys.AddRange(state.GetPressedKeys());
            }

            LightEngine.Lights[0].Position = new Vector3((float)Cos(gameTime.TotalGameTime.TotalMilliseconds / 3000d) * 6f, 0f, (float)Sin(gameTime.TotalGameTime.TotalMilliseconds / 3000d) * 6f);
            LightEngine.Lights[0].Radius = (float)Cos(gameTime.TotalGameTime.TotalMilliseconds / 1000d) * 3f + 3.1f;
            LightEngine.Lights[1].Position = new Vector3((float)Cos(gameTime.TotalGameTime.TotalMilliseconds / 3000d + PI) * 6f, 2.5f, (float)Sin(gameTime.TotalGameTime.TotalMilliseconds / 3000d + PI) * 6f);

            _state = Keyboard.GetState();

            _hud.Update();

            _rzu.Position = new Vector3(Lerp(0f, 10f, (float)Sin(gameTime.TotalGameTime.TotalMilliseconds / 1500)), 0.5f, -1.5f);

            base.Update(gameTime);
        }

        public void ClearTargets()
        {
            GraphicsDevice.SetRenderTarget(RenderTargets.Color);
            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.SetRenderTarget(RenderTargets.DepthMask);
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(RenderTargets.Normal);
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.SetRenderTargets(RenderTargets);
        }

        public void DrawBatch(Texture2D texture)
        {
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState, DepthStencilState.Default, RasterizerState);
            SpriteBatch.Draw(texture, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            SpriteBatch.End();
        }

        protected override void Draw(GameTime gameTime)
        {
            ClearTargets();

            DrawingQueue.Add(_skybox, -1);
            DrawingQueue.Add(_ground, 0);
            DrawingQueue.Add(_rzu, 0);
            DrawingQueue.Add(_rzu1, 0);
            DrawingQueue.Add(_wall, -1);
            DrawingQueue.Add(_table, 0);
            DrawingQueue.Draw(DrawingEffects.Standart);
            Present();

            LightEngine.Draw();
            DrawingQueue.Clear();

            _hud.Draw();

            GraphicsDevice.SetRenderTargets(null);

            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState, DepthStencilState.None, RasterizerState);
            SpriteBatch.Draw(United, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            SpriteBatch.End();

            base.Draw(gameTime);
        }

        public RenderTarget2D CreateRenderTarget(bool mipMap, SurfaceFormat surfaceFormat, DepthFormat depthFormat, int multisample, RenderTargetUsage preservationParameter) =>
            new RenderTarget2D(GraphicsDevice, GraphicsDevice.Adapter.CurrentDisplayMode.Width, GraphicsDevice.Adapter.CurrentDisplayMode.Height, mipMap, surfaceFormat, depthFormat, multisample, preservationParameter);
    }
}