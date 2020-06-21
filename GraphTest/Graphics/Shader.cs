using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraphTest
{
    public enum ShaderTechnique
    {
        Standart,
        Hot,
        WriteDepth,
        SoftShadows,
        Blur,
        ApplyLighting,
        Gamma,
        ApplyAmbient,
        Toon,
        ChromaticAbberation,
        RayTracing
    }

    public enum ShaderInputType
    {
        Primitive,
        Mesh
    }

    public class Shader
    {
        public Effect Effect { get; private set; }

        private ShaderTechnique _technique = ShaderTechnique.Standart;
        public ShaderTechnique Technique
        {
            get => _technique;
            set
            {
                if (!LockTechnique)
                {
                    _technique = value;
                    Effect.CurrentTechnique = Effect.Techniques[_shaderInputType.ToString() + value.ToString()];
                }
            }
        }

        private ShaderInputType _shaderInputType;
        public ShaderInputType InputType
        {
            get => _shaderInputType;
            set
            {
                _shaderInputType = value;
                Effect.CurrentTechnique = Effect.Techniques[value.ToString() + _technique.ToString()];
            }
        }

        public bool LockTechnique { get; set; }

        public Shader()
        {
            var gt = Program.GraphTest;
            Effect = gt.Load<Effect>("Shaders/shader");

            Effect.Parameters["_hotTexture"].SetValue(gt.Load<Texture2D>("heatMap"));
            Effect.Parameters["_screenSize"].SetValue(gt.ScreenSize);

            _buffer = new DynamicVertexBuffer(Program.GraphTest.GraphicsDevice, typeof(VertexPosition), 6, BufferUsage.WriteOnly);
            _buffer.SetData(new VertexPosition[] {
                new VertexPosition(new Vector3(-1,1,1)),
                new VertexPosition(new Vector3(-1,-1,1)),
                new VertexPosition(new Vector3(1,-1, 1)),
                new VertexPosition(new Vector3(1,-1, 1)),
                new VertexPosition(new Vector3(-1,1,1)),
                new VertexPosition(new Vector3(1,1,1))
            });
        }

        public void ApplyDraw(VertexPositionColorNormalTexture[] vertexes, ShaderInputType inputType, int bufferLength, int baseVertex = 0, int startIndex = 0)
        {
            InputType = inputType;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                if (inputType == ShaderInputType.Primitive)
                    Program.GraphTest.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexes, 0, vertexes.Length / 3);
                else
                    Program.GraphTest.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseVertex, startIndex, bufferLength);
            }
        }

        private readonly DynamicVertexBuffer _buffer;

        public void ApplyDrawRayTracing(Vector3[] vertexes)
        {
            InputType = ShaderInputType.Primitive;

            for (int i = 0; i < vertexes.Length; i++)
            {
                var done = Vector4.Transform(new Vector4(vertexes[i], 1), ModelTransform);
                vertexes[i] = new Vector3(done.X, done.Y, done.Z);
            }

            Normal = Vector3.Cross(vertexes[1] - vertexes[0], vertexes[2] - vertexes[0]);
            Effect.Parameters["_points"].SetValue(vertexes);
            Effect.Parameters["_texture"].SetValue(Program.GraphTest.United);
            Effect.CurrentTechnique.Passes[0].Apply();

            Program.GraphTest.GraphicsDevice.SetVertexBuffer(_buffer);
            Program.GraphTest.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            Program.GraphTest.Present();
        }

        public Vector3 Normal
        {
            get => Effect.Parameters["_normal"].GetValueVector3();
            set => Effect.Parameters["_normal"].SetValue(value);
        }

        public int AmountOfLights
        {
            get => Effect.Parameters["_amountOfLights"].GetValueInt32();
            set => Effect.Parameters["_amountOfLights"].SetValue(value);
        }

        public bool TextureEnabled
        {
            get => Effect.Parameters["_textureenabled"].GetValueBoolean();
            set => Effect.Parameters["_textureenabled"].SetValue(value);
        }

        public bool SpecularEnabled
        {
            get => Effect.Parameters["_specular"].GetValueBoolean();
            set => Effect.Parameters["_specular"].SetValue(value);
        }

        public Vector4 Color
        {
            get => Effect.Parameters["_color"].GetValueVector4();
            set => Effect.Parameters["_color"].SetValue(value);
        }

        public bool NormalMapEnabled
        {
            get => Effect.Parameters["_normalMapEnabled"].GetValueBoolean();
            set => Effect.Parameters["_normalMapEnabled"].SetValue(value);
        }

        public Texture2D Texture
        {
            get => Effect.Parameters["_texture"].GetValueTexture2D();
            set => Effect.Parameters["_texture"].SetValue(value);
        }

        public Matrix ModelTransform
        {
            get => Effect.Parameters["_modelTransform"].GetValueMatrix();
            set => Effect.Parameters["_modelTransform"].SetValue(value);
        }

        public Matrix Matrix
        {
            get => Effect.Parameters["_matrix"].GetValueMatrix();
            set => Effect.Parameters["_matrix"].SetValue(value);
        }

        public Texture2D NormalBuffer
        {
            get => Effect.Parameters["_normalBuffer"].GetValueTexture2D();
            set => Effect.Parameters["_normalBuffer"].SetValue(value);
        }

        public Texture2D PositionBuffer
        {
            get => Effect.Parameters["_positionBuffer"].GetValueTexture2D();
            set => Effect.Parameters["_positionBuffer"].SetValue(value);
        }

        public Vector3 ViewVector
        {
            get => Effect.Parameters["_viewVector"].GetValueVector3();
            set => Effect.Parameters["_viewVector"].SetValue(value);
        }

        public float Time
        {
            get => Effect.Parameters["_time"].GetValueSingle();
            set => Effect.Parameters["_time"].SetValue(value);
        }

        public Vector3 LightPosition
        {
            get => Effect.Parameters["_lightPosition"].GetValueVector3();
            set => Effect.Parameters["_lightPosition"].SetValue(value);
        }

        public float DiffuseIntensity
        {
            get => Effect.Parameters["_diffuseIntensity"].GetValueSingle();
            set => Effect.Parameters["_diffuseIntensity"].SetValue(value);
        }

        public Vector2 ChromaticAbbreationAmount
        {
            get => Effect.Parameters["_chromaticAbbreationAmount"].GetValueVector2();
            set => Effect.Parameters["_chromaticAbberationAmount"].SetValue(value);
        }

        public float DiffuseRadius
        {
            get => Effect.Parameters["_radius"].GetValueSingle();
            set => Effect.Parameters["_radius"].SetValue(value);
        }

        public Vector3 AmbientColor
        {
            get => Effect.Parameters["_ambientColor"].GetValueVector3();
            set => Effect.Parameters["_ambientColor"].SetValue(value);
        }

        public Matrix LightMatrix
        {
            get => Effect.Parameters["_lightMatrix"].GetValueMatrix();
            set => Effect.Parameters["_lightMatrix"].SetValue(value);
        }

        public Texture2D ShadowMap
        {
            get => Effect.Parameters["_shadowMap"].GetValueTexture2D();
            set => Effect.Parameters["_shadowMap"].SetValue(value);
        }

        public bool CheckDepth
        {
            get => Effect.Parameters["_checkDepth"].GetValueBoolean();
            set => Effect.Parameters["_checkDepth"].SetValue(value);
        }

        public Texture2D DepthBuffer
        {
            get => Effect.Parameters["_depthBuffer"].GetValueTexture2D();
            set => Effect.Parameters["_depthBuffer"].SetValue(value);
        }

        public Texture2D RenderTarget
        {
            get => Effect.Parameters["_renderTarget"].GetValueTexture2D();
            set => Effect.Parameters["_renderTarget"].SetValue(value);
        }

        public float DistortionFactor
        {
            get => Effect.Parameters["_distortionFactor"].GetValueSingle();
            set => Effect.Parameters["_distortionFactor"].SetValue(value);
        }

        public float RiseFactor
        {
            get => Effect.Parameters["_riseFactor"].GetValueSingle();
            set => Effect.Parameters["_riseFactor"].SetValue(value);
        }

        public bool VerticalBlur
        {
            get => Effect.Parameters["_vertical"].GetValueBoolean();
            set => Effect.Parameters["_vertical"].SetValue(value);
        }

        public bool WriteOnlyColor
        {
            get => Effect.Parameters["_writeOnlyColor"].GetValueBoolean();
            set => Effect.Parameters["_writeOnlyColor"].SetValue(value);
        }
    }
}
