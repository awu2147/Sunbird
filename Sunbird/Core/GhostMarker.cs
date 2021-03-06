﻿using System;
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
        public Sprite Image { get; set; }
        public bool DrawDefaultMarker { get; set; }

        private GhostMarker() { }

        public GhostMarker(MainGame mainGame, SpriteSheet spriteSheet) : base(mainGame, spriteSheet) { Alpha = 0.3f; }

        public void MorphImage(Sprite image, MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Serializer.WriteXML<Sprite>(SpriteSerializer, image, "DynamicCache.xml");
            Image = Serializer.ReadXML<Sprite>(SpriteSerializer, "DynamicCache.xml");
            Image.IsHidden = IsHidden;
            Image.Position = Position;
            Image.SafeLoadContent(mainGame, graphicsDevice, content);
            // AntiShadow and SelfShadow texture are null here but this should be fine.
        }

        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            base.LoadContent(mainGame, graphicsDevice, content);
            Image.LoadContent(mainGame, graphicsDevice, content);
        }

        public override void Update(GameTime gameTime)
        {
            //base.Update(gameTime); // This is not really needed since animator position updated through getter, and TopFaceMarker is not animated.
            Image.Update(gameTime);
            Image.IsHidden = IsHidden;
            Image.Position = Position;
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
