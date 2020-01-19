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
    public static class Peripherals
    {
        public static MouseState currentMouseState { get; set; } = new MouseState();
        public static MouseState previousMouseState { get; set; } = new MouseState();
        public static KeyboardState currentKeyboardState { get; set; }
        public static KeyboardState previousKeyboardState { get; set; }

        public static void PreUpdate()
        {
            currentMouseState = Mouse.GetState();
            currentKeyboardState = Keyboard.GetState();
        }

        public static void PostUpdate()
        {
            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;
        }

        public static Point MousePositionAsPoint()
        {
            MouseState state = Mouse.GetState();
            return new Point(state.X, state.Y);
        }

        public static bool KeyTapped(Keys key)
        {
            if (currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool KeyHeld(Keys key)
        {
            if (currentKeyboardState.IsKeyDown(key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
