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

    public class Animator
    {
        public SpriteSheet SpriteSheet { get; set; }

        public float loopTimer = 0f;

        public Vector2 Position { get; set; }

        public int FrameCount { get; set; } = 1;

        public int FrameCounter { get; set; }

        public float FrameSpeed { get; set; } = 0.1f;

        public AnimationState AnimState { get; set; } = AnimationState.None;

        [XmlIgnore]
        public Sprite Sender { get; set; }

        private Dictionary<int, Point> PositionMap { get { return SpriteSheet.PositionMap; } }

        public int CurrentFrame { get; set; }

        public int StartFrame { get; set; } = 0;

        private Animator()
        {

        }

        public Animator(SpriteSheet spriteSheet, Sprite sender)
        {
            this.Sender = sender;
            SpriteSheet = spriteSheet;
        }

        public Animator(SpriteSheet spriteSheet, Sprite sender, int startFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            this.Sender = sender;
            SpriteSheet = spriteSheet;
            StartFrame = startFrame;
            FrameCount = frameCount;
            FrameSpeed = frameSpeed;
            AnimState = animState;
        }

        public virtual void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            SpriteSheet.Texture = content.Load<Texture2D>(SpriteSheet.TexturePath);
            SpriteSheet.PositionMap = SpriteSheet.ConstructPositionMap();
        }

        public void SwitchAnimation(int startframe, int framecount, float framespeed, AnimationState animState)
        {         
            CurrentFrame = startframe;
            StartFrame = startframe;              
            FrameCount = framecount;
            FrameSpeed = framespeed;
            FrameCounter = 0;
            loopTimer = 0;
            AnimState = animState;          
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteSheet.Texture, Position,
            new Rectangle(PositionMap[CurrentFrame].X, PositionMap[CurrentFrame].Y,
            SpriteSheet.FrameWidth, SpriteSheet.FrameHeight), Color.White);
        }

        public void Update(GameTime gameTime)
        {
            Position = Sender.Position;
            if (AnimState == AnimationState.Loop)
            {
                loopTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (loopTimer > FrameSpeed)
                {
                    loopTimer = 0f;

                    CurrentFrame++;
                    FrameCounter++;

                    if (FrameCounter >= FrameCount)
                    {
                        CurrentFrame = StartFrame;
                        FrameCounter = 0;
                    }
                }
            }
        }
    }
}
