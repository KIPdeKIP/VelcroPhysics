using System;
using System.Globalization;
using FarseerGames.FarseerPhysicsDemos.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.FarseerPhysicsDemos.Components
{
    public class FrameRateCounter : DrawableGameComponent
    {
        private readonly NumberFormatInfo _format;
        private readonly ScreenManager _screenManager;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private int _frameCounter;
        private int _frameRate;

        public FrameRateCounter(ScreenManager screenManager)
            : base(screenManager.Game)
        {
            _screenManager = screenManager;
            _format = new NumberFormatInfo();
            _format.NumberDecimalSeparator = ".";
        }


        public override void Update(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime;

            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                _elapsedTime -= TimeSpan.FromSeconds(1);
                _frameRate = _frameCounter;
                _frameCounter = 0;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _frameCounter++;

            string fps = string.Format(_format, "fps: {0}", _frameRate);

            _screenManager.SpriteBatch.Begin();
            _screenManager.SpriteBatch.DrawString(_screenManager.SpriteFonts.FrameRateCounterFont, fps,
                                                  new Vector2(100, 80), Color.White);
            _screenManager.SpriteBatch.End();
        }
    }
}