﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GraphTest
{
    public class Wall : IDrawable
    {
        private readonly VertexPositionColorNormalTexture[] _vertexes;
        private readonly Texture2D _texture;
        private readonly Matrix _mat = Matrix.CreateTranslation(Vector3.Zero);

        public DrawingEffects DrawingEffects => DrawingEffects.BasicDrawing | DrawingEffects.LightingEnabled;

        public Wall()
        {
            var pos = new List<VertexPositionColorNormalTexture>();
            pos.AddRange(GraphTest.ConstructSquare(new Vector3(-2f, 2f, -1.5f), new Vector3(2f, -0.25f, -1.5f)));
            pos.AddRange(GraphTest.ConstructSquare(new Vector3(2f, 2f, -1.5f), new Vector3(4f, -0.25f, 0f)));
            pos.AddRange(GraphTest.ConstructSquare(new Vector3(-2f, 2f, -1.5f), new Vector3(-4f, -0.25f, 0f)));

            var normals = new Vector3[3]
            {
                new Vector3(0f, 0f,1f),
                Vector3.Normalize(new Vector3(-1f,0f,1f)),
                Vector3.Normalize(new Vector3(1f,0f,1f))
            };

            _vertexes = new VertexPositionColorNormalTexture[pos.Count];
            for (var i = 0; i < _vertexes.Length; i++)
            {
                _vertexes[i] = new VertexPositionColorNormalTexture(pos[i].Position, new Color(Color.White, 0.5f), normals[i / 6], pos[i].TextureCoordinate);
            }

            _texture = Program.GraphTest.Load<Texture2D>("lol");
        }

        public void Draw()
        {
            var gt = Program.GraphTest;
            var effect = Program.GraphTest.Shader;
            var gd = gt.GraphicsDevice;

            if (gt.Shader.LockTechnique)
            {
                gt.DrawVertexes(_vertexes);
                return;
            }

            gd.SetRenderTarget(gt.RenderTargets.DepthMask);
            gd.Clear(Color.Black);
            gt.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            effect.Technique = ShaderTechnique.WriteDepth;
            gt.DrawVertexes(_vertexes);

            gd.SetRenderTarget(gt.RenderTargets.Color);
            gt.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            gt.Shader.Technique = ShaderTechnique.Standart;
            effect.CheckDepth = true;
            effect.DepthBuffer = Program.GraphTest.RenderTargets.DepthMask;
            gt.DrawingQueue.Draw(DrawingEffects.SeenThroughWindow);
            gt.Present();

            effect.Technique = ShaderTechnique.Hot;
            effect.CheckDepth = false;
            effect.Texture = _texture;
            effect.DistortionFactor = 0.1f;
            effect.RiseFactor = 0.8f;
            effect.ModelTransform = _mat;
            effect.RenderTarget = gt.RenderTargets.Color;
            gd.SetRenderTargets(gt.RenderTargets);
            gt.DrawVertexes(_vertexes);
            effect.Technique = ShaderTechnique.Standart;
        }
    }
}
