﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Sunbird.States
{
    /// <summary>
    /// A MainGame State; MainGame can only be in one State at a time. 
    /// </summary>
    public abstract class State
    {
        protected ContentManager Content { get; set; }
        protected GraphicsDevice GraphicsDevice { get; set; }
        protected MainGame MainGame { get; set; } // Create a reference to MainGame to avoid singleton pattern.

        /// <summary>
        /// Only used for serialization.
        /// </summary>
        public State()
        {

        }

        public State(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Content = content;
            GraphicsDevice = graphicsDevice;
            MainGame = mainGame;
        }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

    }
}
