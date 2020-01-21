using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.GUI;

namespace Sunbird.External
{
    public class LoadingScreen1 : State, ILoadingScreen 
    {
        public Color BackgroundColor { get; set; } = Color.CornflowerBlue;
        public Texture2D Background { get; set; }
        public List<Sprite> spriteList { get; set; } = new List<Sprite>();
        public LoadingBar LoadingBar { get; set; }

        public LoadingScreen1(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content) : base(mainGame, graphicsDevice, content)
        {
            LoadContent();
            MainGame.Camera.PushTransform = Matrix.Identity;
        }

        private void LoadContent()
        {
            BackgroundColor = new Color(51, 57, 65);

            var centerIconTexture = Content.Load<Texture2D>("Temp/sunbird");
            var centerIconPosition = new Vector2(MainGame.Width / 2 - centerIconTexture.Width / 2, MainGame.Height / 2 - centerIconTexture.Height / 2 - 60);
            var centerIcon = new Sprite(new SpriteSheet(centerIconTexture, 1, 1)) { Position = centerIconPosition };
            spriteList.Add(centerIcon);

            var barEmpty = Content.Load<Texture2D>("Temp/bar1_empty");
            var barFull = Content.Load<Texture2D>("Temp/bar1_full");
            var barPosition = new Vector2(MainGame.Width / 2 - barEmpty.Width / 2, MainGame.Height / 2 - barEmpty.Height + 70);
            LoadingBar = new LoadingBar(barEmpty, barFull, barPosition);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var sprite in spriteList)
            {
                sprite.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(BackgroundColor);
            if (Background != null)
            {
                spriteBatch.Draw(Background, Vector2.Zero, Color.White);
            }
            foreach (var sprite in spriteList)
            {
                sprite.Draw(gameTime, spriteBatch);
            }
            LoadingBar.Draw(gameTime, spriteBatch);
        }

    }

    public class LoadingScreen1_Factory : ILoadingScreenFactory
    {
        public LoadingScreen1_Factory()
        {

        }

        public ILoadingScreen CreateLoadingScreen(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            return new LoadingScreen1(mainGame, graphicsDevice, content) as ILoadingScreen;
        }
    }
}
