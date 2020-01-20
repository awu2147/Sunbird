using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Linq;
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

    public enum Direction
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    public enum Movement
    {
        Standing,
        Walking,
        Running
    }

    public class Player : Sprite
    {

        public Direction Direction { get; set; } = Direction.South;
        public Movement Movement { get; set; } = Movement.Standing;
        private List<Keys> MovementKeyList { get; set; } = new List<Keys>() { Keys.W, Keys.A, Keys.D, Keys.S };
        public float Speed { get; set; } = 3;

        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_North => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_North, Keys.W); };
        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_West => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_West, Keys.A); };
        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_South => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_South, Keys.S); };
        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_East => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_East, Keys.D); };

        private Player()
        {

        }

        public Player(SpriteSheet spriteSheet, int startFrame, int frameCount, float frameSpeed, AnimationState animState) : base(spriteSheet, startFrame, frameCount, frameSpeed, animState)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Peripherals.currentPressedKeys.Contains(Keys.W))
            {
                if (Peripherals.KeyTapped(Keys.W))
                {
                    Peripherals.KeyReleased += MovementKeyReleased_North;
                }
                if (Movement == Movement.Standing)
                {
                    Movement = Movement.Walking;
                    Animator.SwitchAnimation(2, 2, 0.2f, AnimationState.Loop);
                }
                if (Direction != Direction.North)
                {
                    Direction = Direction.North;
                    Animator.SwitchAnimation(2, 2, 0.2f, AnimationState.Loop);
                }
                Position += new Vector2(0, -Speed);
            }

            if (Peripherals.currentPressedKeys.Contains(Keys.A))
            {
                if (Peripherals.KeyTapped(Keys.A))
                {
                    Peripherals.KeyReleased += MovementKeyReleased_West;
                }
                if (Movement == Movement.Standing)
                {
                    Movement = Movement.Walking;
                    Animator.SwitchAnimation(0, 2, 0.2f, AnimationState.Loop);
                }
                if (Direction != Direction.West)
                {
                    Direction = Direction.West;
                    Animator.SwitchAnimation(0, 2, 0.2f, AnimationState.Loop);
                }
                Position += new Vector2(-Speed, 0);
            }

            if (Peripherals.currentPressedKeys.Contains(Keys.S))
            {
                if (Peripherals.KeyTapped(Keys.S))
                {
                    Peripherals.KeyReleased += MovementKeyReleased_South;
                }
                if (Movement == Movement.Standing)
                {
                    Movement = Movement.Walking;
                    Animator.SwitchAnimation(4, 2, 0.2f, AnimationState.Loop);
                }
                if (Direction != Direction.South)
                {
                    Direction = Direction.South;
                    Animator.SwitchAnimation(4, 2, 0.2f, AnimationState.Loop);
                }
                Position += new Vector2(0, Speed);
            }

            if (Peripherals.currentPressedKeys.Contains(Keys.D))
            {
                if (Peripherals.KeyTapped(Keys.D))
                {
                    Peripherals.KeyReleased += MovementKeyReleased_East;
                }
                if (Movement == Movement.Standing)
                {
                    Movement = Movement.Walking;
                    Animator.SwitchAnimation(6, 2, 0.2f, AnimationState.Loop);
                }
                if (Direction != Direction.East)
                {
                    Direction = Direction.East;
                    Animator.SwitchAnimation(6, 2, 0.2f, AnimationState.Loop);
                }
                Position += new Vector2(Speed, 0);
            }
        }

        private void MovementKeyReleased(object sender, KeyReleasedEventArgs e, EventHandler<KeyReleasedEventArgs> self, Keys key)
        {
            if (key == e.key)
            {
                Peripherals.KeyReleased -= self;
                if (!MovementKeyList.Any(x => Peripherals.currentPressedKeys.Contains(x)))
                {
                    Movement = Movement.Standing;
                    if (Direction == Direction.North)
                    {
                        Animator.SwitchAnimation(2, 1, 0.2f, AnimationState.None);
                    }
                    else if (Direction == Direction.West)
                    {
                        Animator.SwitchAnimation(0, 1, 0.2f, AnimationState.None);
                    }
                    else if (Direction == Direction.South)
                    {
                        Animator.SwitchAnimation(4, 1, 0.2f, AnimationState.None);
                    }
                    else if (Direction == Direction.East)
                    {
                        Animator.SwitchAnimation(6, 1, 0.2f, AnimationState.None);
                    }
                }
                Debug.Print(e.key.ToString() + " key released. Movement = " + Movement.ToString());
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }
    }
}
