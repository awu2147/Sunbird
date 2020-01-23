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
        Push = 1,
        Drag = 2,
    }

    public class Camera
    {
        public Matrix FollowTransform { get; set; } = Matrix.Identity;

        public Matrix PushTransform { get; set; } = Matrix.Identity;

        public Matrix DragTransform { get; set; } = Matrix.Identity;

        public Matrix CurrentTransform { get; set; } = Matrix.Identity;

        public CameraMode CurrentMode { get; set; }

        private Direction PushDirection { get; set; }

        private MainGame MainGame { get; set; }

        public SamplerState SamplerState { get; set; }

        private float counter = 3;

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
            Push();
            Drag();
        }

        public Point GetMousePosition()
        {
            MouseState state = Mouse.GetState();
            return new Point(state.X, state.Y);
        }

        public void Follow(Sprite target)
        {
            FollowTransform = Matrix.CreateTranslation(-target.Position.X + MainGame.Width / 2, -target.Position.Y + MainGame.Height / 2, 0);            
        }

        private Point anchor;

        private Point lastDrag = Point.Zero;

        private Point dragPositionChange;

        public void Drag()
        {
            if (CurrentMode != CameraMode.Drag)
            {
                DragTransform = FollowTransform;
                lastDrag.X = (int)FollowTransform.M41;
                lastDrag.Y = (int)FollowTransform.M42;
            }
            else
            {
                var peripherals = MainGame.Peripherals;
                if (peripherals.currentMouseState.MiddleButton == ButtonState.Pressed)
                {
                    MainGame.SamplerState = SamplerState.AnisotropicClamp;
                    if (peripherals.MouseTapped(peripherals.currentMouseState.MiddleButton, peripherals.previousMouseState.MiddleButton))
                    {                      
                        peripherals.MiddleButtonReleased += peripherals_MiddleButtonReleased;
                        anchor = GetMousePosition();
                    }
                    var currentPosition = GetMousePosition();
                    dragPositionChange = currentPosition - anchor;
                    DragTransform = Matrix.CreateTranslation(lastDrag.X + dragPositionChange.X, lastDrag.Y + dragPositionChange.Y, 0);
                }
                else
                {
                    MainGame.SamplerState = SamplerState.PointClamp;
                }
            }
        }

        private void peripherals_MiddleButtonReleased(object sender, EventArgs e)
        {
            lastDrag += dragPositionChange;
            dragPositionChange = Point.Zero;
            MainGame.Peripherals.MiddleButtonReleased -= peripherals_MiddleButtonReleased;
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
                    counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(3, 0, 0);
                    PushDirection = Direction.West;
                }
                else if (ms.X >= MainGame.Width - 100 && ms.X <= MainGame.Width + 100)
                {
                    counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(-3, 0, 0);
                    PushDirection = Direction.East;
                }
                else if (ms.Y >= -100 && ms.Y <= 100)
                {
                    counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(0, 3, 0);
                    PushDirection = Direction.North;
                }
                else if (ms.Y >= MainGame.Height - 100 && ms.Y <= MainGame.Height + 100)
                {
                    counter = 3;
                    PushTransform = PushTransform * Matrix.CreateTranslation(0, -3, 0);
                    PushDirection = Direction.South;
                }
                else
                {
                    if (PushDirection == Direction.West)
                    {
                        PushTransform *= Matrix.CreateTranslation(counter, 0, 0);
                        counter -= 0.1f;
                        if (counter <= 0)
                        {
                            PushDirection = Direction.None;
                            counter = 3;
                        }
                    }
                    else if (PushDirection == Direction.East)
                    {
                        PushTransform *= Matrix.CreateTranslation(-counter, 0, 0);
                        counter -= 0.1f;
                        if (counter <= 0)
                        {
                            PushDirection = Direction.None;
                            counter = 3;
                        }
                    }
                    else if (PushDirection == Direction.North)
                    {
                        PushTransform *= Matrix.CreateTranslation(0, counter, 0);
                        counter -= 0.1f;
                        if (counter <= 0)
                        {
                            PushDirection = Direction.None;
                            counter = 3;
                        }
                    }
                    else if (PushDirection == Direction.South)
                    {
                        PushTransform *= Matrix.CreateTranslation(0, -counter, 0);
                        counter -= 0.1f;
                        if (counter <= 0)
                        {
                            PushDirection = Direction.None;
                            counter = 3;
                        }
                    }

                    if (PushDirection == Direction.None)
                    {
                        MainGame.SamplerState = SamplerState.PointClamp;
                        PushTransform = Matrix.CreateTranslation(PushTransform.M41, PushTransform.M42, 0);
                    }

                }
            }
        }

    }
}
