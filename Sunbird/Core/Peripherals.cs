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
    public class KeyReleasedEventArgs : EventArgs
    {
        public Keys Key { get; set; }

        public KeyReleasedEventArgs(Keys key)
        {
            Key = key;
        }
    }

    public static class Peripherals
    {
        public static MouseState currentMouseState { get; set; }
        public static MouseState previousMouseState { get; set; }
        public static KeyboardState currentKeyboardState { get; set; }
        public static KeyboardState previousKeyboardState { get; set; }
        public static Keys[] currentPressedKeys { get; set; }
        public static Keys[] previousPressedKeys { get; set; }

        public static event EventHandler<KeyReleasedEventArgs> KeyReleased;
        public static event EventHandler<EventArgs> MiddleButtonReleased;
        public static event EventHandler<EventArgs> LeftButtonReleased;
        public static event EventHandler<EventArgs> RightButtonReleased;

        public static void PreUpdate()
        {
            currentMouseState = Mouse.GetState();
            currentKeyboardState = Keyboard.GetState();
            currentPressedKeys = currentKeyboardState.GetPressedKeys();
        }

        public static void PostUpdate()
        {
            CheckForRelease();

            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;
            previousPressedKeys = currentPressedKeys;
        }

        public static void CheckForRelease()
        {
            if (previousPressedKeys != null)
            {
                foreach (var key in previousPressedKeys)
                {
                    if (currentPressedKeys.Contains(key) == false)
                    {
                        var args = new KeyReleasedEventArgs(key);
                        OnKeyReleased(args);
                    }
                }
            }
            if (previousMouseState.MiddleButton == ButtonState.Pressed && currentMouseState.MiddleButton == ButtonState.Released)
            {
                OnMiddleMouseButtonReleased();
            }
            if (previousMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Released)
            {
                OnLeftMouseButtonReleased();
            }
            if (previousMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Released)
            {
                OnRightMouseButtonReleased();
            }
            //Mouse.GetState().ScrollWheelValue
        }

        public static void OnKeyReleased(KeyReleasedEventArgs e)
        {
            EventHandler<KeyReleasedEventArgs> handler = KeyReleased;
            handler?.Invoke(null, e);
        }

        public static void OnMiddleMouseButtonReleased()
        {
            EventHandler<EventArgs> handler = MiddleButtonReleased;
            handler?.Invoke(null, null);
        }

        public static void OnLeftMouseButtonReleased()
        {
            EventHandler<EventArgs> handler = LeftButtonReleased;
            handler?.Invoke(null, null);
        }

        public static void OnRightMouseButtonReleased()
        {
            EventHandler<EventArgs> handler = RightButtonReleased;
            handler?.Invoke(null, null);
        }

        public static Point GetMouseWindowPosition()
        {
            MouseState state = Mouse.GetState();
            return new Point(state.X, state.Y);
        }

        public static Point GetCornerWorldPosition(Camera camera)
        {
            if (camera.CurrentMode == CameraMode.Follow)
            {
                return new Point(-(int)camera.FollowTransform.M41, -(int)camera.FollowTransform.M42);
            }
            else if (camera.CurrentMode == CameraMode.Push)
            {
                return new Point(-(int)camera.PushTransform.M41, -(int)camera.PushTransform.M42);
            }
            else if (camera.CurrentMode == CameraMode.Drag)
            {
                return new Point(-(int)camera.DragTransform.M41, -(int)camera.DragTransform.M42);
            }
            else
            {
                return Point.Zero;
            }
        }
        public static Point GetCornerWorldPosition(Camera camera, int x, int y)
        {
            var offset = new Point(x, y);
            if (camera.CurrentMode == CameraMode.Follow)
            {
                return new Point(-(int)camera.FollowTransform.M41, -(int)camera.FollowTransform.M42) + offset;
            }
            else if (camera.CurrentMode == CameraMode.Push)
            {
                return new Point(-(int)camera.PushTransform.M41, -(int)camera.PushTransform.M42) + offset;
            }
            else if (camera.CurrentMode == CameraMode.Drag)
            {
                return new Point(-(int)camera.DragTransform.M41, -(int)camera.DragTransform.M42) + offset;
            }
            else
            {
                return Point.Zero;
            }
        }

        public static Point GetMouseWorldPosition(Camera camera)
        {
            if (camera.CurrentMode == CameraMode.Follow)
            {
                return new Point(-(int)camera.FollowTransform.M41, -(int)camera.FollowTransform.M42) + GetMouseWindowPosition();
            }
            else if (camera.CurrentMode == CameraMode.Push)
            {
                return new Point(-(int)camera.PushTransform.M41, -(int)camera.PushTransform.M42) + GetMouseWindowPosition();
            }
            else if (camera.CurrentMode == CameraMode.Drag)
            {
                return new Point(-(int)camera.DragTransform.M41, -(int)camera.DragTransform.M42) + GetMouseWindowPosition();
            }
            else
            {
                return Point.Zero;
            }
        }

        public static bool KeyTapped(Keys key)
        {
            return (currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key));
        }

        public static bool KeyPressed(Keys key)
        {
            return currentPressedKeys.Contains(key);
        }

        public static bool KeysPressed(Keys key1, Keys key2)
        {
            return currentPressedKeys.Contains(key1) && currentPressedKeys.Contains(key2);
        }

        public static bool MouseTapped(ButtonState currentButton, ButtonState previousButton)
        {          
            return (currentButton == ButtonState.Pressed && previousButton == ButtonState.Released);
        }

        public static bool MousePressed(ButtonState currentButton)
        {
            return (currentButton == ButtonState.Pressed);
        }

    }
}
