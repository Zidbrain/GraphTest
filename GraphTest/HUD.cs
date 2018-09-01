using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GraphTest
{
    public class HUD
    {
        private SpriteFont _font;
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

            _helpString = "dif <number> - Diffuse Intensity\nrad <number> - Diffuse Lighting Radius\namb <number> - Ambient Color\npos - Set light position on current location";
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
                            Program.GraphTest.Shader.DiffuseIntensity = Convert.ToSingle(_writtenText.Remove(0, 4));
                        else if (_writtenText.StartsWith("rad "))
                            Program.GraphTest.LightEngine.Lights[0].Radius = Convert.ToSingle(_writtenText.Remove(0, 4));
                        else if (_writtenText.StartsWith("amb "))
                            Program.GraphTest.Shader.AmbientColor = new Vector3(Convert.ToSingle(_writtenText.Remove(0, 4)));
                        else if (_writtenText.StartsWith("pos "))
                        {
                            var index = Convert.ToInt32(_writtenText.Substring(4));

                            Program.GraphTest.LightEngine.Lights[index].Position = Program.GraphTest.CameraPosition;
                            Program.GraphTest.LightEngine.Lights[index].Direction = Program.GraphTest.CameraDirection;
                        }
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
                gt.SpriteBatch.DrawString(_font, _helpString, new Vector2(1920, 0) - new Vector2(_font.MeasureString(_helpString).X, 0f), Color.White);

                var consoleText = _writtenText + (gt.GameTime.TotalGameTime.TotalMilliseconds % 1000 > 500 ? "_" : "");
                gt.SpriteBatch.DrawString(_font, consoleText, Vector2.Zero, Color.White);
            }

            var text = $"X: {gt.CameraPosition.X} Y: {gt.CameraPosition.Y} Z:{gt.CameraPosition.Z}";
            gt.SpriteBatch.DrawString(_font, text, new Vector2(1920, 1080) - _font.MeasureString(text), Color.White);
            var text1 = $"Diffuse Intensity: {gt.Shader.DiffuseIntensity}\n" +
                $"Diffuse Radius: {gt.Shader.DiffuseRadius}\n" +
                $"Ambient Color: {gt.Shader.AmbientColor.X}\n" +
                $"FPS: {gt.FPS.ToString("F0")}";
            gt.SpriteBatch.DrawString(_font, text1, new Vector2(0, 1080) - new Vector2(0, _font.MeasureString(text1).Y), Color.White);

            gt.SpriteBatch.End();
        }
    }
}
