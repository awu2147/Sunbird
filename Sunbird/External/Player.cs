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

    public enum Movement
    {
        Standing,
        Walking,
        Running
    }

    public class Player : Sprite
    {
        private MainGame MainGame { get; set; }
        public Direction Direction { get; set; } = Direction.South;
        public Movement Movement { get; set; } = Movement.Standing;
        private Config Config { get; set; }

        HashSet<Keys> CurrentMovementKeys { get; set; } = new HashSet<Keys>();

        private List<Keys> MovementKeyList => new List<Keys>() { Config.North, Config.East, Config.South, Config.West };
        public float Speed { get; set; } = 3;

        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_North => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_North, Config.North); };
        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_West => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_West, Config.West); };
        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_South => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_South, Config.South); };
        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_East => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_East, Config.East); };

        private Player()
        {

        }

        public Player(MainGame mainGame, Animator animator)
        {
            Animator = animator;
            Animator.Sender = this;
            MainGame = mainGame;
            Config = mainGame.Config;
        }

        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Animator.LoadContent(mainGame, graphicsDevice, content);
            Animator.Sender = this;
            MainGame = mainGame;
            Config = mainGame.Config;
        }

        public override void Update(GameTime gameTime)
        {
            MoveUpdate();
            MainGame.Camera.Follow(this, new Vector2(12 * 3, 15 * 3));
            base.Update(gameTime);
        }

        private void MoveUpdate()
        {
            if (Peripherals.KeyTapped(Config.North))
            {
                Peripherals.KeyReleased += MovementKeyReleased_North;
                CurrentMovementKeys.Add(Config.North);
            }
            if (Peripherals.KeyTapped(Config.East))
            {
                Peripherals.KeyReleased += MovementKeyReleased_East;
                CurrentMovementKeys.Add(Config.East);
            }
            if (Peripherals.KeyTapped(Config.South))
            {
                Peripherals.KeyReleased += MovementKeyReleased_South;
                CurrentMovementKeys.Add(Config.South);
            }
            if (Peripherals.KeyTapped(Config.West))
            {
                CurrentMovementKeys.Add(Config.West);
                Peripherals.KeyReleased += MovementKeyReleased_West;
            }

            if (Peripherals.KeysPressed(Config.North, Config.East))
            {
                if (Movement == Movement.Standing || Direction != Direction.NorthEast)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.NorthEast;
                    Animator.SwitchAnimation(4, 4, 0.133f, AnimationState.Loop);
                }
                Position += new Vector2(Speed, -Speed / 2);
            }
            else if (Peripherals.KeysPressed(Config.North, Config.West))
            {
                if (Movement == Movement.Standing || Direction != Direction.NorthWest)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.NorthWest;
                    Animator.SwitchAnimation(12, 4, 0.133f, AnimationState.Loop);
                }
                Position += new Vector2(-Speed, -Speed / 2);
            }
            else if (Peripherals.KeysPressed(Config.South, Config.East))
            {
                if (Movement == Movement.Standing || Direction != Direction.SouthEast)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.SouthEast;
                    Animator.SwitchAnimation(4, 4, 0.133f, AnimationState.Loop);
                }
                Position += new Vector2(Speed, Speed / 2);
            }
            else if (Peripherals.KeysPressed(Config.South, Config.West))
            {
                if (Movement == Movement.Standing || Direction != Direction.SouthWest)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.SouthWest;
                    Animator.SwitchAnimation(12, 4, 0.133f, AnimationState.Loop);
                }
                Position += new Vector2(-Speed, Speed / 2);
            }
            else if (Peripherals.KeyPressed(Config.North))
            {
                if (Movement == Movement.Standing || Direction != Direction.North)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.North;
                    Animator.SwitchAnimation(0, 4, 0.133f, AnimationState.Loop);
                }
                Position += new Vector2(0, -Speed);
            }
            else if (Peripherals.KeyPressed(Config.West))
            {
                if (Movement == Movement.Standing || Direction != Direction.West)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.West;
                    Animator.SwitchAnimation(12, 4, 0.133f, AnimationState.Loop);
                }
                Position += new Vector2(-Speed, 0);
            }
            else if (Peripherals.KeyPressed(Config.South))
            {
                if (Movement == Movement.Standing || Direction != Direction.South)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.South;
                    Animator.SwitchAnimation(8, 4, 0.133f, AnimationState.Loop);
                }
                Position += new Vector2(0, Speed);
            }
            else if (Peripherals.KeyPressed(Config.East))
            {
                if (Movement == Movement.Standing || Direction != Direction.East)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.East;
                    Animator.SwitchAnimation(4, 4, 0.133f, AnimationState.Loop);
                }
                Position += new Vector2(Speed, 0);
            }

            ApplyMotionBlur(new List<Movement>() { Movement.Walking });
            Coords = World.TopFace_PositionToRelativeCoord(Position + new Vector2(12*3, 25*3), Altitude);

            if (!MovementKeyList.Any(x => Peripherals.currentPressedKeys.Contains(x)))
            {
                Movement = Movement.Standing;
                if (Direction == Direction.North)
                {
                    Animator.SwitchAnimation(0, 1, 0.2f, AnimationState.None);
                }
                else if (Direction == Direction.West)
                {
                    Animator.SwitchAnimation(12, 1, 0.2f, AnimationState.None);
                }
                else if (Direction == Direction.South)
                {
                    Animator.SwitchAnimation(8, 1, 0.2f, AnimationState.None);
                }
                else if (Direction == Direction.East)
                {
                    Animator.SwitchAnimation(4, 1, 0.2f, AnimationState.None);
                }
            }
        }

        private void ApplyMotionBlur(List<Movement> movements)
        {
            if (movements.Contains(Movement))
            {
                MainGame.SamplerState = SamplerState.AnisotropicClamp;
            }
            else
            {
                MainGame.SamplerState = SamplerState.PointClamp;
            }
        }

        private void MovementKeyReleased(object sender, KeyReleasedEventArgs e, EventHandler<KeyReleasedEventArgs> self, Keys key)
        {
            if (key == e.Key)
            {
                CurrentMovementKeys.Remove(key);
                Peripherals.KeyReleased -= self;
                if (!MovementKeyList.Any(x => Peripherals.currentPressedKeys.Contains(x)))
                {
                    Movement = Movement.Standing;
                    if (Direction == Direction.North)
                    {
                        Animator.SwitchAnimation(0, 1, 0.2f, AnimationState.None);
                    }
                    else if (Direction == Direction.West || Direction == Direction.NorthWest || Direction == Direction.SouthWest)
                    {
                        Animator.SwitchAnimation(12, 1, 0.2f, AnimationState.None);
                    }
                    else if (Direction == Direction.South)
                    {
                        Animator.SwitchAnimation(8, 1, 0.2f, AnimationState.None);
                    }
                    else if (Direction == Direction.East || Direction == Direction.NorthEast || Direction == Direction.SouthEast)
                    {
                        Animator.SwitchAnimation(4, 1, 0.2f, AnimationState.None);
                    }
                }
                Debug.Print(e.Key.ToString() + " key released. Movement = " + Movement.ToString());
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }
    }
}
