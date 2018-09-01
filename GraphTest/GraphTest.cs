using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static System.Math;
using static Microsoft.Xna.Framework.MathHelper;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
using System.Diagnostics;

namespace GraphTest
{
    public class GraphTest : Game
    {
        private GraphicsDeviceManager _graphics;
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
        public RenderTargetBinding[] RenderTarget { get; private set; }
        public SamplerState SamplerState { get; private set; }
        public RasterizerState RasterizerState { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public Vector3 CameraPosition { get; private set; }
        public DrawingQueue DrawingQueue { get; private set; }
        public LightEngine LightEngine { get; private set; }
        public List<Keys> KeysPressedOnce => new List<Keys>(_keys);
        public Vector3 CameraDirection { get; private set; } = new Vector3(0f, 0f, 1f);
        public float FPS { get; private set; }

        public GraphTest() : base()
        {
            _graphics = new GraphicsDeviceManager(this) { PreferMultiSampling = false, GraphicsProfile = GraphicsProfile.HiDef, HardwareModeSwitch = false, SynchronizeWithVerticalRetrace = false };
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
            for (int i = 0; i < 6; i++)
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
            for (int i = 0; i < retr.Length; i++)
            {
                retr[i] = new VertexPositionColorNormalTexture(ret[i].Position, color, normals[i / 6], ret[i].TextureCoordinate);
            }

            return retr;
        }

        private static Vector2 ToVector2(Vector3 input) =>
            new Vector2(input.X, input.Y);

        public T Load<T>(string path) =>
            Content.Load<T>(path);

        public void DrawVertexes(DynamicVertexBuffer buffer, ShaderInputType inputType)
        {
            GraphicsDevice.SetVertexBuffer(buffer);
            Shader.Apply(inputType);
            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, buffer.VertexCount);
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

            Shader = new Shader();

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            SamplerState = new SamplerState()
            {
                Filter = TextureFilter.Anisotropic,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MaxAnisotropy = 16,
            };

            RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                MultiSampleAntiAlias = true,
            };

            RenderTarget = new RenderTargetBinding[]
            {
                new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 4, RenderTargetUsage.PreserveContents) {Name = "Color" }),
                new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Vector4, DepthFormat.None, 4, RenderTargetUsage.PreserveContents) {Name = "Position"}),
                new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Color, DepthFormat.None, 4, RenderTargetUsage.PreserveContents) {Name = "Normal"}),
                new RenderTargetBinding(new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Single, DepthFormat.None, 4, RenderTargetUsage.PreserveContents) { Name = "Depth" }),
            };

            United = new RenderTarget2D(GraphicsDevice, 1920, 1080, false, SurfaceFormat.Color, DepthFormat.None, 4, RenderTargetUsage.PreserveContents);

            _ground = new Ground();
            _wall = new Wall();
            _rzu = new Rzu(new Vector3(10f, 0.5f, -1.5f), 0.5f);
            _rzu1 = new Rzu(new Vector3(-0.5f, 0.5f, -1.5f), 0.5f);
            Mouse.SetPosition(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);

            _table = new ModelManipulate("untitled") { Position = new Vector3(0f, 0.25f, -2f) };

            DrawingQueue = new DrawingQueue();

            Shader.DiffuseIntensity = 1f;
            Shader.DiffuseRadius = 3f;
            Shader.AmbientColor = new Vector3(0.5f);

            _hud = new HUD();
            _keys = new List<Keys>();
            _skybox = new SkyBox();

            LightEngine = new LightEngine();
            LightEngine.Lights.Add(new Light());
            //LightEngine.Lights.Add(new Light());

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
                    Matrix.CreatePerspectiveFieldOfView(ToRadians(45f), 1920f / 1080f, 0.01f, 100f);
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

            LightEngine.Lights[0].Position = new Vector3((float)Cos(gameTime.TotalGameTime.TotalMilliseconds / 3000d) * 3f, 2.5f, (float)Sin(gameTime.TotalGameTime.TotalMilliseconds / 3000d) * 3f);
            LightEngine.Lights[0].Direction = Vector3.Zero - LightEngine.Lights[0].Position;
            //LightEngine.Lights[1].Position = new Vector3((float)Cos(gameTime.TotalGameTime.TotalMilliseconds / 3000d + 1.5d) * 3f, 2.5f, (float)Sin(gameTime.TotalGameTime.TotalMilliseconds / 3000d + 1.5d) * 3f);
            //LightEngine.Lights[1].Direction = Vector3.Zero - LightEngine.Lights[1].Position;

            _state = Keyboard.GetState();

            _hud.Update();

            _rzu.Position = new Vector3(Lerp(0f, 10f, (float)Sin(gameTime.TotalGameTime.TotalMilliseconds / 1500)), 0.5f, -1.5f);

            base.Update(gameTime);
        }

        public void ClearTargets()
        {
            GraphicsDevice.SetRenderTarget((RenderTarget2D)RenderTarget[0].RenderTarget);
            GraphicsDevice.Clear(Color.SkyBlue);
            GraphicsDevice.SetRenderTarget((RenderTarget2D)RenderTarget[3].RenderTarget);
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.SetRenderTargets(RenderTarget);
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

            DrawingQueue.Add(_skybox, -3);
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

            GraphicsDevice.Clear(Color.TransparentBlack);

           SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState, DepthStencilState.Default, RasterizerState);
            SpriteBatch.Draw(United, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            SpriteBatch.Draw((Texture2D)LightEngine._softShadows[0].RenderTarget, new Rectangle(1920 - 480, 0, 480, 270), Color.White);
            SpriteBatch.Draw((Texture2D)LightEngine.Lights[0]._shadowMap[0].RenderTarget, new Rectangle(1920 - 480 * 2, 0, 480, 270), Color.White);
            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}