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
        none,
        once,
        loop
    }

    public class Animator
    {
        public SpriteSheet SpriteSheet { get; set; }

        public float loopTimer = 0f;

        public Vector2 Position { get; set; }

        public int FrameCount { get; set; }

        public float FrameSpeed { get; set; } = 0.1f;

        public AnimationState AnimState { get; set; } = AnimationState.none;

        [XmlIgnore]
        public Sprite sender;

        private Dictionary<int, Point> PositionMap { get { return SpriteSheet.PositionMap; } }

        public int CurrentFrame { get; set; }

        private Animator()
        {

        }

        public Animator(SpriteSheet spriteSheet, int startFrame, int framecount, float framespeed, AnimationState animState, Sprite sender)
        {
            this.sender = sender;
            SpriteSheet = spriteSheet;
        }

        public virtual void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            SpriteSheet.Texture = content.Load<Texture2D>(SpriteSheet.texturePath);
            SpriteSheet.ConstructPositionMap();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteSheet.Texture, Position,
            new Rectangle(PositionMap[CurrentFrame].X, PositionMap[CurrentFrame].Y,
            SpriteSheet.FrameWidth, SpriteSheet.FrameHeight), Color.White);
        }

        public void Update(GameTime gametime)
        {
            Position = sender.Position;
        }
    }
}
