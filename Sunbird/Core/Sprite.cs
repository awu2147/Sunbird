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
using System.Xml.Schema;

namespace Sunbird.Core
{
    public class Sprite
    {
        public Vector2 Position { get; set; }

        public Animator Animator { get; set; }

        public Sprite()
        {

        }

        public Sprite(SpriteSheet spriteSheet)
        {
            Animator = new Animator(spriteSheet, this);
        }

        public Sprite(SpriteSheet spriteSheet, int startFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            Animator = new Animator(spriteSheet, this, startFrame, frameCount, frameSpeed, animState);
        }

        public virtual void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Animator.LoadContent(mainGame, graphicsDevice, content);
            Animator.Sender = this;
        }

        public virtual void Update(GameTime gameTime)
        {
            Animator.Update(gameTime);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Animator.Draw(gameTime, spriteBatch);
        }
    }
}
