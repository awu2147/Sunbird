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
        Follow = 0,
        Drag = 1,
        Push = 2,
    }

    public class Camera
    {
        public Matrix CurrentTransform { get; set; } = Matrix.Identity;
        public CameraMode CurrentMode { get; set; }

        // Follow
        public Matrix FollowTransform { get; set; } = Matrix.Identity;
        private Vector2 FollowOffset { get; set; }

        // Drag
        public Matrix DragTransform { get; set; } = Matrix.Identity;
        private Vector2 DragTargetPosition { get; set; }
        private Point DragPositionChange { get; set; }
        private Point Anchor { get; set; }

        // TODO: Push
        public Matrix PushTransform { get; set; } = Matrix.Identity;
        private Direction PushDirection { get; set; }
        private float Counter { get; set; } = 3;

        private MainGame MainGame { get; set; }


        public Camera(MainGame sender)
        {
            MainGame = sender;
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
            else if (CurrentMode == CameraMode.Drag)
            {
                CurrentTransform = DragTransform;
            }
            Drag();
            Push();
        }

        public void Follow(Sprite target, Vector2 offset)
        {
            FollowTransform = Matrix.CreateTranslation((MainGame.Width / 2) / World.ZoomRatio - target.Position.X - offset.X, (MainGame.Height / 2) / World.ZoomRatio - target.Position.Y - offset.Y, 0) * Matrix.CreateScale(World.ZoomRatio);
            FollowOffset = offset;
            if (CurrentMode != CameraMode.Drag)
            {
                DragTargetPosition = target.Position;
            }
        }

        public void Drag()
        {
            if (CurrentMode != CameraMode.Drag)
            {
                DragTransform = FollowTransform;
            }
            else
            {
                if (Peripherals.currentMouseState.MiddleButton == ButtonState.Pressed)
                {
                    MainGame.SamplerState = SamplerState.AnisotropicClamp;
                    if (Peripherals.MouseTapped(Peripherals.currentMouseState.MiddleButton, Peripherals.previousMouseState.MiddleButton))
                    {                      
                        Peripherals.MiddleButtonReleased += peripherals_MiddleButtonReleased;
                        Anchor = Peripherals.GetMouseWindowPosition();
                    }
                    var currentPosition = Peripherals.GetMouseWindowPosition();
                    DragPositionChange = (currentPosition - Anchor) * new Point(World.Scale, World.Scale) / new Point(World.Zoom, World.Zoom);
                    DragTransform = CreateDragTransform();
                }
                else
                {
                    MainGame.SamplerState = SamplerState.PointClamp;
                }
            }
        }

        private void peripherals_MiddleButtonReleased(object sender, EventArgs e)
        {
            DragTargetPosition -= DragPositionChange.ToVector2();
            DragPositionChange = Point.Zero;
            Peripherals.MiddleButtonReleased -= peripherals_MiddleButtonReleased;
        }

        public Matrix CreateDragTransform()
        {
            return Matrix.CreateTranslation((MainGame.Width / 2) / World.ZoomRatio - DragTargetPosition.X - FollowOffset.X + DragPositionChange.X, 
                                            (MainGame.Height / 2) / World.ZoomRatio - DragTargetPosition.Y - FollowOffset.Y + DragPositionChange.Y, 0) * Matrix.CreateScale(World.ZoomRatio);
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
                MainGame.SamplerState = SamplerState.AnisotropicClamp;
                if (ms.X >= -100 && ms.X <= 100)
                {
                    Counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(3, 0, 0);
                    PushDirection = Direction.West;
                }
                else if (ms.X >= MainGame.Width - 100 && ms.X <= MainGame.Width + 100)
                {
                    Counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(-3, 0, 0);
                    PushDirection = Direction.East;
                }
                else if (ms.Y >= -100 && ms.Y <= 100)
                {
                    Counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(0, 3, 0);
                    PushDirection = Direction.North;
                }
                else if (ms.Y >= MainGame.Height - 100 && ms.Y <= MainGame.Height + 100)
                {
                    Counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(0, -3, 0);
                    PushDirection = Direction.South;
                }
                else
                {
                    if (PushDirection == Direction.West)
                    {
                        PushTransform *= Matrix.CreateTranslation(Counter, 0, 0);
                        Counter -= 0.1f;
                        if (Counter <= 0)
                        {
                            PushDirection = Direction.None;
                            Counter = 3;
                        }
                    }
                    else if (PushDirection == Direction.East)
                    {
                        PushTransform *= Matrix.CreateTranslation(-Counter, 0, 0);
                        Counter -= 0.1f;
                        if (Counter <= 0)
                        {
                            PushDirection = Direction.None;
                            Counter = 3;
                        }
                    }
                    else if (PushDirection == Direction.North)
                    {
                        PushTransform *= Matrix.CreateTranslation(0, Counter, 0);
                        Counter -= 0.1f;
                        if (Counter <= 0)
                        {
                            PushDirection = Direction.None;
                            Counter = 3;
                        }
                    }
                    else if (PushDirection == Direction.South)
                    {
                        PushTransform *= Matrix.CreateTranslation(0, -Counter, 0);
                        Counter -= 0.1f;
                        if (Counter <= 0)
                        {
                            PushDirection = Direction.None;
                            Counter = 3;
                        }
                    }

                    if (PushDirection == Direction.None)
                    {
                        MainGame.SamplerState = SamplerState.PointClamp;
                        PushTransform = Matrix.CreateTranslation(PushTransform.M41, PushTransform.M42, 0) * Matrix.CreateScale(World.ZoomRatio);
                    }

                }
            }
        }

    }
}
