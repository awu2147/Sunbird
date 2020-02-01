using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.Controllers;
using Sunbird.Serialization;
using System.Xml.Schema;

namespace Sunbird.Core
{
    public class GhostMarker : Sprite
    {
        public Sprite Image;

        public bool DrawDefaultMarker;

        private GhostMarker()
        {

        }

        public GhostMarker(SpriteSheet spriteSheet) : base(spriteSheet)
        {
            Alpha = 0.3f;
        }

        public void MorphImage(Sprite image, MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Serializer.WriteXML<Sprite>(image, "DynamicCache.xml", new Type[] { typeof(Cube) });
            Image = Serializer.ReadXML<Sprite>("DynamicCache.xml", new Type[] { typeof(Cube) });
            Image.LoadContent(mainGame, graphicsDevice, content);
        }

        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            base.LoadContent(mainGame, graphicsDevice, content);
            Image.LoadContent(mainGame, graphicsDevice, content);
        }

        public override void Update(GameTime gameTime)
        {
            // Do we always want to update here? I guess we need to update so that the animator position updates.
            base.Update(gameTime);
            Image.Position = Position;
            Image.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (DrawDefaultMarker == true)
            {
                base.Draw(gameTime, spriteBatch);
            }
            else
            {
                Image.Alpha = 0.3f;
                Image.Draw(gameTime, spriteBatch);
            }
        }

    }
}
