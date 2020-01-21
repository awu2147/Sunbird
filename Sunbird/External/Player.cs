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

    public class Player : Sprite, IControllable
    {

        public Direction Direction { get; set; } = Direction.South;
        public Movement Movement { get; set; } = Movement.Standing;

        [XmlIgnore]
        public Peripherals Peripherals { get; set; }
        private Config Config { get; set; }

        private List<Keys> MovementKeyList => new List<Keys>() { Config.North, Config.East, Config.South, Config.West };
        public float Speed { get; set; } = 3;

        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_North => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_North, Config.North); };
        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_West => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_West, Config.West); };
        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_South => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_South, Config.South); };
        public EventHandler<KeyReleasedEventArgs> MovementKeyReleased_East => delegate (object sender, KeyReleasedEventArgs e) { MovementKeyReleased(sender, e, MovementKeyReleased_East, Config.East); };

        private Player()
        {

        }

        public Player(Animator animator, Config config, Peripherals peripherals)
        {
            Animator = animator;
            Animator.Sender = this;
            Config = config;
            Peripherals = peripherals;
        }

        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Animator.LoadContent(mainGame, graphicsDevice, content);
            Animator.Sender = this;
            Config = mainGame.Config;
            Peripherals = mainGame.Peripherals;
        }

        public override void Update(GameTime gameTime)
        {
            MoveUpdate();
            MainGame.Camera.Follow(this);
            base.Update(gameTime);
        }

        private void MoveUpdate()
        {
            if (Peripherals.currentPressedKeys.Contains(Config.North))
            {
                if (Peripherals.KeyTapped(Config.North))
                {
                    Peripherals.KeyReleased += MovementKeyReleased_North;
                }
                if (Movement == Movement.Standing || Direction != Direction.North)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.North;
                    Animator.SwitchAnimation(2, 2, 0.2f, AnimationState.Loop);
                }
                Position += new Vector2(0, -Speed);
            }

            if (Peripherals.currentPressedKeys.Contains(Config.West))
            {
                if (Peripherals.KeyTapped(Config.West))
                {
                    Peripherals.KeyReleased += MovementKeyReleased_West;
                }
                if (Movement == Movement.Standing || Direction != Direction.West)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.West;
                    Animator.SwitchAnimation(0, 2, 0.2f, AnimationState.Loop);
                }
                Position += new Vector2(-Speed, 0);
            }

            if (Peripherals.currentPressedKeys.Contains(Config.South))
            {
                if (Peripherals.KeyTapped(Config.South))
                {
                    Peripherals.KeyReleased += MovementKeyReleased_South;
                }
                if (Movement == Movement.Standing || Direction != Direction.South)
                {
                    Movement = Movement.Walking;
                    Direction = Direction.South;
                    Animator.SwitchAnimation(4, 2, 0.2f, AnimationState.Loop);
                }
                Position += new Vector2(0, Speed);
            }

            if (Peripherals.currentPressedKeys.Contains(Config.East))
            {
                if (Peripherals.KeyTapped(Config.East))
                {
                    Peripherals.KeyReleased += MovementKeyReleased_East;
                }
                if (Movement == Movement.Standing || Direction != Direction.East)
                {
                    Movement = Movement.Walking;
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
