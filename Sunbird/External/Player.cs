using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.Controllers;
using Sunbird.Serialization;
using Sunbird.GUI;

namespace Sunbird.External
{
    public class Player : Sprite
    {
        private Player()
        {

        }

        public Player(SpriteSheet spriteSheet, int startFrame, int frameCount, float frameSpeed, AnimationState animState) : base(spriteSheet, startFrame, frameCount, frameSpeed, animState)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Peripherals.KeyTapped(Keys.W))
            {
                Animator.SwitchAnimation(2, 2, 0.2f, AnimationState.Loop);
                Position += new Vector2(0, -10);
            }
            if (Peripherals.KeyTapped(Keys.A))
            {
                Animator.SwitchAnimation(0, 2, 0.2f, AnimationState.Loop);
                Position += new Vector2(-10, 0);
            }
            if (Peripherals.KeyTapped(Keys.S))
            {
                Animator.SwitchAnimation(4, 2, 0.2f, AnimationState.Loop);
                Position += new Vector2(0, 10);
            }
            if (Peripherals.KeyTapped(Keys.D))
            {
                Animator.SwitchAnimation(6, 2, 0.2f, AnimationState.Loop);
                Position += new Vector2(10, 0);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }
    }
}
