using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GraphTest
{
    public class HUD
    {
        private readonly SpriteFont _font;
        private string _writtenText = "";
        private readonly string _helpString;

        public bool IsConsoleEnabled { get; private set; }

        public HUD()
        {
            _font = Program.GraphTest.Load<SpriteFont>("font");

            Program.GraphTest.Window.TextInput += delegate (object sender, TextInputEventArgs e)
            {
                if (IsConsoleEnabled && _font.Characters.Contains(e.Character))
                    _writtenText += e.Character;
            };

            _helpString = "dif <number> - Diffuse Intensity\n" +
                "rad <number> - Diffuse Lighting Radius\n" +
                "amb <number> - Ambient Color\n" +
                "pos - Set light position on current location\n" +
                "switch - Switch light mode to raytracing and back";
        }

        public void Update()
        {
            var gt = Program.GraphTest;
            var keys = gt.KeysPressedOnce;

            if (IsConsoleEnabled)
            {
                if (keys.Contains(Keys.Escape))
                {
                    IsConsoleEnabled = false;
                    _writtenText = null;
                }

                if (keys.Contains(Keys.Back) && _writtenText.Length != 0)
                    _writtenText = _writtenText.Remove(_writtenText.Length - 1);

                if (keys.Contains(Keys.Enter))
                {
                    try
                    {
                        if (_writtenText.StartsWith("dif "))
                            Program.GraphTest.LightEngine.Lights[0].DiffuseIntensity = Convert.ToSingle(_writtenText.Remove(0, 4));
                        else if (_writtenText.StartsWith("rad "))
                            Program.GraphTest.LightEngine.Lights[0].Radius = Convert.ToSingle(_writtenText.Remove(0, 4));
                        else if (_writtenText.StartsWith("amb "))
                            Program.GraphTest.Shader.AmbientColor = new Vector3(Convert.ToSingle(_writtenText.Remove(0, 4)));
                        else if (_writtenText.StartsWith("pos "))
                        {
                            var index = Convert.ToInt32(_writtenText.Substring(4));

                            Program.GraphTest.LightEngine.Lights[index].Position = Program.GraphTest.CameraPosition;
                        }
                        else if (_writtenText.StartsWith("switch"))
                            Program.GraphTest.LightEngine.LightMode = ~Program.GraphTest.LightEngine.LightMode & LightMode.RayTracing;
                    }
                    catch (FormatException) { }

                    IsConsoleEnabled = false;
                    _writtenText = "";
                }
            }
            else
            {
                if (keys.Contains(Keys.OemTilde))
                    IsConsoleEnabled = true;
            }

        }

        public void Draw()
        {
            var gt = Program.GraphTest;
            gt.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, gt.SamplerState, DepthStencilState.DepthRead, gt.RasterizerState);

            if (IsConsoleEnabled)
            {
                gt.SpriteBatch.DrawString(_font, _helpString, new Vector2(gt.ScreenSize.X, 0) - new Vector2(_font.MeasureString(_helpString).X, 0f), Color.White);

                var consoleText = _writtenText + (gt.GameTime.TotalGameTime.TotalMilliseconds % 1000 > 500 ? "_" : "");
                gt.SpriteBatch.DrawString(_font, consoleText, Vector2.Zero, Color.White);
            }

            var text = $"X: {gt.CameraPosition.X} Y: {gt.CameraPosition.Y} Z: {gt.CameraPosition.Z}";
            gt.SpriteBatch.DrawString(_font, text, gt.ScreenSize - _font.MeasureString(text), Color.White);
            var text1 = $"Diffuse Intensity: {gt.Shader.DiffuseIntensity}\n" +
                $"Diffuse Radius: {gt.Shader.DiffuseRadius}\n" +
                $"Ambient Color: {gt.Shader.AmbientColor.X}\n" +
                $"FPS: {gt.FPS:F0}";
            gt.SpriteBatch.DrawString(_font, text1, new Vector2(0, gt.ScreenSize.Y) - new Vector2(0, _font.MeasureString(text1).Y), Color.White);

            gt.SpriteBatch.End();
        }
    }
}
