using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraphTest
{
    public class ModelManipulate : IDrawable
    {
        private readonly Model _model;

        public DrawingEffects DrawingEffects => DrawingEffects.Standart;

        public Vector3 Position { get; set; }

        public Vector3 Size { get; set; } = Vector3.One;

        public bool SpecularEnabled { get; set; }

        public ModelManipulate(string modelName) =>
            _model = Program.GraphTest.Load<Model>(modelName);

        public void Draw()
        {
            _model.Root.Transform = Matrix.CreateScale(Size) * Matrix.CreateTranslation(Position);

            var ef = Program.GraphTest.Shader;

            ef.InputType = ShaderInputType.Mesh;
            if (SpecularEnabled)
                ef.SpecularEnabled = true;

            foreach (var mesh in _model.Meshes)
            {
                var effect = (BasicEffect)mesh.Effects[0];

                foreach (var part in mesh.MeshParts)
                    part.Effect = ef.Effect;

                ef.TextureEnabled = effect.TextureEnabled;
                ef.Color = new Vector4(effect.DiffuseColor, 1f);
                ef.Texture = effect.Texture;

                var matrix = _model.Root.Transform;
                ef.ModelTransform = matrix;
                ef.Matrix = matrix * Program.GraphTest.Matrix;

                mesh.Draw();

                foreach (var part in mesh.MeshParts)
                    part.Effect = effect;
            }

            ef.Matrix = Program.GraphTest.Matrix;
            ef.SpecularEnabled = false;
        }
    }

}
