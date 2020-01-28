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
using System.Xml.Schema;

namespace Sunbird.Core
{
    public enum Alignment
    {
        TopLeft,
        TopRight,
        Center,
        BottomLeft,
        BottomRight
    }

    public class Sprite
    {
        public Animator Animator { get; set; }

        public float Alpha { get; set; } = 1f;

        public Vector2 Position { get; set; }

        public Coord Coords { get; set; }

        public int Altitude { get; set; }

        public int DrawAltitude { get { return Altitude + DrawPriority; } }

        public int DrawPriority { get; set; }

        public bool IsHidden { get; set; }


        public Sprite()
        {

        }

        public Sprite(SpriteSheet spriteSheet)
        {
            Animator = new Animator(spriteSheet, this);
        }

        public Sprite(SpriteSheet spriteSheet, Vector2 position)
        {
            Position = position;
            Animator = new Animator(spriteSheet, this);
        }

        public Sprite(SpriteSheet spriteSheet, Vector2 position, Alignment alignment)
        {
            if (alignment == Alignment.TopLeft)
            {
                Position = position;
            }
            else if (alignment == Alignment.TopRight)
            {
                Position = position + new Vector2(-spriteSheet.Texture.Width, 0);
            }
            else if (alignment == Alignment.Center)
            {
                Position = position + new Vector2(-spriteSheet.Texture.Width / 2, -spriteSheet.Texture.Height / 2);
            }
            else if (alignment == Alignment.BottomLeft)
            {
                Position = position + new Vector2(0, -spriteSheet.Texture.Height);
            }
            else if (alignment == Alignment.BottomRight)
            {
                Position = position + new Vector2(-spriteSheet.Texture.Width, -spriteSheet.Texture.Height);
            }
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

        public void ReplaceSpriteSheet(SpriteSheet newSheet)
        {
            Animator.SpriteSheet = newSheet;
        }

        public void ReconfigureAnimator()
        {
            ReconfigureAnimator(0, 1, 0.133f, AnimationState.None);
        }

        public void ReconfigureAnimator(int startFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            Animator.StartFrame = startFrame;
            Animator.CurrentFrame = startFrame;
            Animator.FrameCount = frameCount;
            Animator.FrameSpeed = frameSpeed;
            Animator.AnimState = animState;
        }

        public virtual void Update(GameTime gameTime)
        {
            Animator.Update(gameTime);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsHidden == false)
            {
                Animator.Draw(gameTime, spriteBatch, Alpha);
            }
        }
    }
}
