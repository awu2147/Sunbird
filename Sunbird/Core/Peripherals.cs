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
        public Keys key { get; set; }

        public KeyReleasedEventArgs(Keys key)
        {
            this.key = key;
        }
    }

    public class Peripherals
    {
        public MouseState currentMouseState { get; set; } = new MouseState();
        public MouseState previousMouseState { get; set; } = new MouseState();
        public KeyboardState currentKeyboardState { get; set; }
        public KeyboardState previousKeyboardState { get; set; }
        public Keys[] currentPressedKeys { get; set; }
        public Keys[] previousPressedKeys { get; set; }

        public event EventHandler<KeyReleasedEventArgs> KeyReleased;

        public Peripherals()
        {

        }

        public void PreUpdate()
        {
            currentMouseState = Mouse.GetState();
            currentKeyboardState = Keyboard.GetState();
            currentPressedKeys = currentKeyboardState.GetPressedKeys();
        }

        public void PostUpdate()
        {
            CheckForRelease();
            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;
            previousPressedKeys = currentPressedKeys;
        }

        public void CheckForRelease()
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
        }

        public void OnKeyReleased(KeyReleasedEventArgs e)
        {
            EventHandler<KeyReleasedEventArgs> handler = KeyReleased;
            handler?.Invoke(null, e);
        }

        public Point MousePositionAsPoint()
        {
            MouseState state = Mouse.GetState();
            return new Point(state.X, state.Y);
        }

        public bool KeyTapped(Keys key)
        {
            return (currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key)) ? true : false;
        }

    }
}
