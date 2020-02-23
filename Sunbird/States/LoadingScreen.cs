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
    public class LoadingScreen: State, ILoadingScreen
    {
        public Color BackgroundColor { get; set; } = Color.CornflowerBlue;
        public Texture2D Background { get; set; }
        public List<Sprite> spriteList { get; set; } = new List<Sprite>();
        public LoadingBar LoadingBar { get; set; }

        public LoadingScreen(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content) : base(mainGame, graphicsDevice, content)
        {
            LoadContent();
        }

        public virtual void LoadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
            foreach (var sprite in spriteList)
            {
                sprite.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteBatch spriteBatchShadow, SpriteBatch spriteBatchLighting, SpriteBatch spriteBatchLightingStencil)
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

        public override void DrawOverlay(GameTime gameTime, SpriteBatch spriteBatch)
        {

        }

    }
}
