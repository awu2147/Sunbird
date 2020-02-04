using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.Core;
using System.Xml.Serialization;

namespace Sunbird.Controllers
{
    public enum AnimationState
    {
        None,
        Once,
        Loop
    }

    public class SwitchAnimArgs
    {
        public int StartFrame { get; set; } = 0;
        public int FrameCount { get; set; } = 1;
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;

        public SwitchAnimArgs()
        {

        }

        public SwitchAnimArgs(int startFrame)
        {
            StartFrame = startFrame;
        }

        public SwitchAnimArgs(int startFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            StartFrame = startFrame;
            FrameCount = frameCount;
            FrameSpeed = frameSpeed;
            AnimState = animState;
        }
    }

    [Serializable]
    public class Animator
    {
        public SpriteSheet SpriteSheet { get; set; }
        public Vector2 Position { get; set; }
        public int FrameCount { get; set; } = 1;
        public int FrameCounter { get; set; }
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;

        [XmlIgnore]
        public Sprite Sender { get; set; }

        [XmlIgnore]
        public Dictionary<int, Point> PositionMap { get { return SpriteSheet.PositionMap; } }
        public int CurrentFrame { get; set; }
        public int StartFrame { get; set; }
        public Timer Timer { get; set; } = new Timer();

        private Animator()
        {

        }

        public Animator(SpriteSheet spriteSheet, Sprite sender)
        {
            this.Sender = sender;
            SpriteSheet = spriteSheet;
            Timer.OnCompleted = () =>
            {
                CurrentFrame++;
                FrameCounter++;
            };
        }

        public Animator(SpriteSheet spriteSheet, Sprite sender, int startFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            this.Sender = sender;
            SpriteSheet = spriteSheet;
            StartFrame = startFrame;
            FrameCount = frameCount;
            FrameSpeed = frameSpeed;
            AnimState = animState;
            Timer.OnCompleted = () =>
            {
                CurrentFrame++;
                FrameCounter++;
            };
        }

        public virtual void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            SpriteSheet.Texture = content.Load<Texture2D>(SpriteSheet.TexturePath);
            SpriteSheet.PositionMap = SpriteSheet.ConstructPositionMap();
            Timer.OnCompleted = () =>
            {
                CurrentFrame++;
                FrameCounter++;
            };
        }

        public Rectangle VisibleArea()
        {
            return new Rectangle(Position.ToPoint(), new Point(SpriteSheet.FrameWidth, SpriteSheet.FrameHeight));
        }

        public Rectangle ViewRectangle()
        {
            return new Rectangle(PositionMap[CurrentFrame].X, PositionMap[CurrentFrame].Y, SpriteSheet.FrameWidth, SpriteSheet.FrameHeight);
        }

        public void SwitchAnimation(int startframe, int framecount, float framespeed, AnimationState animState)
        {         
            StartFrame = startframe;
            CurrentFrame = startframe;
            FrameCounter = 0;
            FrameCount = framecount;
            FrameSpeed = framespeed;
            AnimState = animState;
            Timer.Reset();
        }

        public void SwitchAnimation(SwitchAnimArgs args)
        {
            SwitchAnimation(args.StartFrame, args.FrameCount, args.FrameSpeed, args.AnimState);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteSheet.Texture, Position,
            new Rectangle(PositionMap[CurrentFrame].X, PositionMap[CurrentFrame].Y,
            SpriteSheet.FrameWidth, SpriteSheet.FrameHeight), Color.White * 0.5f);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, float alpha)
        {
            spriteBatch.Draw(SpriteSheet.Texture, Position,
            new Rectangle(PositionMap[CurrentFrame].X, PositionMap[CurrentFrame].Y,
            SpriteSheet.FrameWidth, SpriteSheet.FrameHeight), Color.White * alpha);
        }

        public void Update(GameTime gameTime)
        {
            Position = Sender.Position + Sender.PositionOffset;
            if (AnimState == AnimationState.Loop)
            {
                Timer.WaitForSeconds(gameTime, FrameSpeed);
                if (FrameCounter >= FrameCount)
                {
                    CurrentFrame = StartFrame;
                    FrameCounter = 0;
                }
            }
        }
    }
}
