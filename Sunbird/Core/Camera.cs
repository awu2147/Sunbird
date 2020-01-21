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
    public enum CameraMode
    {
        Follow,
        Push,
    }

    public class Camera
    {
        public Matrix FollowTransform { get; set; } = Matrix.Identity;

        public Matrix PushTransform { get; set; } = Matrix.Identity;

        public Matrix CurrentTransform { get; set; } = Matrix.Identity;

        public CameraMode CurrentMode;

        private Direction Direction { get; set; }

        private MainGame Sender { get; set; }

        private float counter = 3;

        public Camera(MainGame sender)
        {
            Sender = sender;
        }

        public void Update()
        {
            if (CurrentMode == CameraMode.Follow)
            {
                CurrentTransform = FollowTransform;
            }
            else if (CurrentMode == CameraMode.Push)
            {
                CurrentTransform = PushTransform;
            }
        }

        public void Follow(Sprite target)
        {
            FollowTransform = Matrix.CreateTranslation(-target.Position.X + Sender.Width / 2, -target.Position.Y + Sender.Height / 2, 0);            
        }

        public void Push()
        {
            if (CurrentMode != CameraMode.Push)
            {
                PushTransform = FollowTransform;
            }
            else
            {
                var ms = Mouse.GetState();
                if (ms.X >= -100 && ms.X <= 100)
                {
                    counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(3, 0, 0);
                    Direction = Direction.West;
                }
                else if (ms.X >= Sender.Width - 100 && ms.X <= Sender.Width + 100)
                {
                    counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(-3, 0, 0);
                    Direction = Direction.East;
                }
                else if (ms.Y >= -100 && ms.Y <= 100)
                {
                    counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(0, 3, 0);
                    Direction = Direction.North;
                }
                else if (ms.Y >= Sender.Height - 100 && ms.Y <= Sender.Height + 100)
                {
                    counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(0, -3, 0);
                    Direction = Direction.South;
                }
                else
                {
                    if (Direction == Direction.West)
                    {
                        PushTransform *= Matrix.CreateTranslation(counter, 0, 0);
                        counter -= 0.1f;
                        if (counter <= 0)
                        {
                            Direction = Direction.None;
                            counter = 3;
                        }
                    }
                    else if (Direction == Direction.East)
                    {
                        PushTransform *= Matrix.CreateTranslation(-counter, 0, 0);
                        counter -= 0.1f;
                        if (counter <= 0)
                        {
                            Direction = Direction.None;
                            counter = 3;
                        }
                    }
                    else if (Direction == Direction.North)
                    {
                        PushTransform *= Matrix.CreateTranslation(0, counter, 0);
                        counter -= 0.1f;
                        if (counter <= 0)
                        {
                            Direction = Direction.None;
                            counter = 3;
                        }
                    }
                    else if (Direction == Direction.South)
                    {
                        PushTransform *= Matrix.CreateTranslation(0, -counter, 0);
                        counter -= 0.1f;
                        if (counter <= 0)
                        {
                            Direction = Direction.None;
                            counter = 3;
                        }
                    }

                    if (Direction == Direction.None)
                    {
                        var x = (float)Math.Round(PushTransform.M41);
                        var y = (float)Math.Round(PushTransform.M42);
                        PushTransform = Matrix.CreateTranslation(x, y, 0);
                    }

                }
            }
        }

    }
}
