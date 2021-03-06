﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static Microsoft.Xna.Framework.MathHelper;

namespace GraphTest
{
    public enum LightMode
    {
        Default,
        RayTracing
    }

    public class LightEngine
    {
        private readonly RenderTargetBinding[] _softShadows;

        public List<Light> Lights { get; private set; }
        public bool Enabled { get; set; } = true;

        public LightEngine()
        {
            _softShadows = new RenderTargetBinding[]
            {
                new RenderTargetBinding(Program.GraphTest.CreateRenderTarget(false, SurfaceFormat.Single, DepthFormat.None, 2, RenderTargetUsage.PreserveContents))
            };

            Lights = new List<Light>();
        }

        private void DefaultLighting()
        {
            var gd = Program.GraphTest.GraphicsDevice;
            var gt = Program.GraphTest;

            if (Enabled)
            {
                gt.Shader.AmountOfLights = Lights.Count;

                // Append shadows to shared shadows buffer (perspective from camera)
                gd.SetRenderTargets(_softShadows);
                gd.Clear(Color.White);
                gt.Shader.DepthBuffer = (Texture2D)_softShadows[0].RenderTarget;
                gt.Shader.PositionBuffer = gt.RenderTargets.Position;

                foreach (var light in Lights)
                {
                    light.AppendShadow(_softShadows);
                }
                gt.Present();

                // Blur the shadows buffer
                gt.Shader.Texture = (Texture2D)_softShadows[0].RenderTarget;
                gt.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                gt.Shader.Technique = ShaderTechnique.Blur;
                gt.Shader.Matrix = Matrix.Identity;
                for (var i = 0; i < 4; i++)
                {
                    gt.Shader.VerticalBlur = false;
                    gt.DrawVertexes(gt.StaticVertexes);
                    gt.Present();
                    gt.Shader.VerticalBlur = true;
                    gt.DrawVertexes(gt.StaticVertexes);
                    gt.Present();
                }

            }
            // Apply gamma to source render target (can do that since LightEngine draws itself only at the end of a frame)
            gt.Shader.Texture = gt.RenderTargets.Color;
            gt.Shader.ShadowMap = (Texture2D)_softShadows[0].RenderTarget;
            gt.Shader.NormalBuffer = gt.RenderTargets.Normal;
            gt.Shader.DepthBuffer = gt.RenderTargets.DepthMask;

            gt.Shader.Technique = ShaderTechnique.Gamma;
            if (Enabled)
            {
                gd.SetRenderTargets(gt.RenderTargets);
                gt.DrawVertexes(gt.StaticVertexes);
            }
            else
            {
                gd.SetRenderTarget(gt.United);
                gd.Clear(Color.Black);
                gt.Shader.Matrix = Matrix.Identity;
                gt.DrawVertexes(gt.StaticVertexes);
            }

            if (Enabled)
            {
                // Apply ambient lighting
                gd.SetRenderTarget(gt.United);
                gt.Shader.Technique = ShaderTechnique.ApplyAmbient;
                gt.DrawVertexes(gt.StaticVertexes);
                gt.Present();
                gt.Shader.Texture = gt.United;
                gt.Shader.RenderTarget = gt.RenderTargets.Color;

                // Apply diffuse and specular lighting for each light
                gt.Shader.Technique = ShaderTechnique.ApplyLighting;
                foreach (var light in Lights)
                {
                    gt.Shader.LightPosition = light.Position;
                    gt.Shader.DiffuseRadius = light.Radius;
                    gt.Shader.DiffuseIntensity = light.DiffuseIntensity;
                    gt.DrawVertexes(gt.StaticVertexes);
                    gt.Present();
                }

                gt.Shader.Technique = ShaderTechnique.ChromaticAbberation;
                gt.Shader.ChromaticAbbreationAmount = new Vector2(5);
                gt.Shader.Texture = gt.United;
                gt.Shader.TextureEnabled = true;
                gt.DrawVertexes(gt.StaticVertexes);
            }

            // Restore parameters and continiue
            gt.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            gt.Shader.Matrix = gt.Matrix;
            gt.Shader.Technique = ShaderTechnique.Standart;
        }

        private void RayTracingLighting()
        {
            var gd = Program.GraphTest.GraphicsDevice;
            var gt = Program.GraphTest;

            gd.DepthStencilState = DepthStencilState.None;
            gt.Shader.Texture = gt.RenderTargets.Color;
            gt.Shader.ShadowMap = (Texture2D)_softShadows[0].RenderTarget;
            gt.Shader.NormalBuffer = gt.RenderTargets.Normal;
            gt.Shader.PositionBuffer = gt.RenderTargets.Position;
            gt.Shader.Matrix = Matrix.Identity;
            gt.Shader.Technique = ShaderTechnique.Gamma;

            gd.SetRenderTarget(gt.United);
            gd.Clear(Color.Black);
            gt.DrawVertexes(gt.StaticVertexes);

            gd.SetRenderTarget(null);
            gd.SetRenderTarget(gt.United);

            gd.DepthStencilState = DepthStencilState.Default;

            gt.DrawRayTracing = true;
            gt.Shader.LightPosition = Lights[0].Position;
            gt.Shader.RenderTarget = gt.RenderTargets.Color;
            gt.Shader.LightMatrix = gt.Matrix;
            gt.DrawingQueue.Draw(DrawingEffects.CastsShadow);
            gt.DrawRayTracing = false;
            gd.DepthStencilState = DepthStencilState.Default;
            gt.Shader.Matrix = gt.Matrix;
        }
        
        public LightMode LightMode { get; set; }

        public void Draw()
        {
            switch (LightMode)
            {
                case LightMode.Default:
                    DefaultLighting();
                    break;
                case LightMode.RayTracing:
                    RayTracingLighting();
                    break;
            }
        }
    }

    public class Light
    {
        private readonly RenderTargetBinding[] _shadowMap;
        private readonly Box _box = new Box(0.25f);
        private static readonly Vector3[] s_sides;
        private static readonly Vector3[] s_ups;

        public Vector3 Position { get; set; }
        public float Radius { get; set; } = 3f;
        public float DiffuseIntensity { get; set; } = 1f;

        public bool ShadowsEnabled { get; set; } = true;

        static Light()
        {
            s_sides = new Vector3[]
            {
                Vector3.Forward,
                Vector3.Backward,
                Vector3.Down,
                Vector3.Up,
                Vector3.Left,
                Vector3.Right
            };
            s_ups = new Vector3[]
            {
                Vector3.Up,
                Vector3.Up,
                Vector3.Forward,
                Vector3.Backward,
                Vector3.Up,
                Vector3.Up
            };
        }

        public Light() => _shadowMap = new RenderTargetBinding[]
                {
                new RenderTargetBinding(new RenderTarget2D(Program.GraphTest.GraphicsDevice,1024,1024, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents) {Name = "ShadowMap" }),
                };

        public void AppendShadow(RenderTargetBinding[] shadowsTarget)
        {
            if (!ShadowsEnabled)
                return;

            for (var i = 0; i < 6; i++)
            {
                var gt = Program.GraphTest;
                var mat = gt.Matrix;
                var lightmat = Matrix.CreateLookAt(Position, Position + s_sides[i], s_ups[i]) *
                        Matrix.CreatePerspectiveFieldOfView(ToRadians(90f), 1f, 0.01f, Radius);

                // initial shadowm map
                gt.Shader.Technique = ShaderTechnique.WriteDepth;
                gt.Shader.LockTechnique = true;

                gt.GraphicsDevice.SetRenderTargets(_shadowMap);
                gt.GraphicsDevice.Clear(Color.Black);
                gt.Matrix = lightmat;
                gt.Shader.LightMatrix = gt.Matrix;
                gt.Shader.Matrix = gt.Matrix;
                gt.DrawingQueue.Draw(DrawingEffects.CastsShadow);
                _box.Posiiton = gt.CameraPosition;
                _box.Draw();

                // shadow map from the perspective of the camera
                gt.Shader.LockTechnique = false;
                gt.GraphicsDevice.SetRenderTargets(shadowsTarget);
                gt.Shader.ShadowMap = (Texture2D)_shadowMap[0].RenderTarget;
                gt.Shader.Matrix = Matrix.Identity;
                gt.Shader.Technique = ShaderTechnique.SoftShadows;
                gt.DrawVertexes(gt.StaticVertexes);

                gt.Matrix = mat;
                gt.Shader.Matrix = mat;
            }
        }
    }
}
