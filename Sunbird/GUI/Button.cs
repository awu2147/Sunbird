using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
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
using Timer = Sunbird.Core.Timer;

namespace Sunbird.GUI
{
    public class ButtonClickedEventArgs : EventArgs
    {
        public bool IsPressed { get; set; }

        public ButtonClickedEventArgs(bool isPressed)
        {
            IsPressed = isPressed;
        }
    }

    public enum ButtonType
    { 
        Default,
        SafeRelease,
        CheckBox
    }

    public class Button : Sprite
    {
        public event EventHandler<ButtonClickedEventArgs> Clicked;
        public event EventHandler<ButtonClickedEventArgs> Checked;
        public event EventHandler<ButtonClickedEventArgs> Unchecked;

        public string Label { get; set; }
        public ButtonType ButtonType { get; set; } 
        public bool IsPressed { get; set; }
        public SwitchAnimArgs PressedArgs { get; set; } = new SwitchAnimArgs(1);
        public SwitchAnimArgs ReleasedArgs { get; set; } = new SwitchAnimArgs(0);
        public Timer Timer { get; set; } = new Timer();

        private Button()
        {

        }

        public Button(MainGame mainGame, SpriteSheet spriteSheet, string label) : base(mainGame, spriteSheet)
        {
            Label = label;
        }

        public Button(MainGame mainGame, SpriteSheet spriteSheet, string label, Vector2 position) : base(mainGame, spriteSheet, position)
        {
            Label = label;
        }

        public Button(MainGame mainGame, SpriteSheet spriteSheet, string label, Vector2 position, Alignment alignment) : base (mainGame, spriteSheet, position, alignment)
        {
            Label = label;
        }

        public void OnClicked()
        {
            EventHandler<ButtonClickedEventArgs> handler = Clicked;
            handler?.Invoke(this, null);
        }

        public void OnChecked()
        {
            EventHandler<ButtonClickedEventArgs> handler = Checked;
            handler?.Invoke(this, null);
        }

        public void OnUnchecked()
        {
            EventHandler<ButtonClickedEventArgs> handler = Unchecked;
            handler?.Invoke(this, null);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Animator.VisibleArea().Contains(Peripherals.GetMouseWindowPosition()) && Peripherals.LeftButtonTapped())
            {
                if (ButtonType == ButtonType.Default)
                {
                    OnClicked();               
                    IsPressed = true;
                    Timer.OnCompleted = () => 
                    { 
                        Animator.SwitchAnimation(ReleasedArgs);
                        IsPressed = false;
                    };
                }
                else if (ButtonType == ButtonType.SafeRelease)
                {
                    Peripherals.LeftButtonReleased += Peripherals_LeftButtonReleased;
                    IsPressed = true;
                }
                else if (ButtonType == ButtonType.CheckBox)
                {
                    IsPressed = !IsPressed;
                    OnClicked();
                    if (IsPressed)
                    {
                        OnChecked();
                    }
                    else
                    {
                        OnUnchecked();
                    }
                }
                
            }

            if (IsPressed == true)
            {        
                if (ButtonType == ButtonType.Default)
                {
                    Animator.SwitchAnimation(PressedArgs);
                    Timer.WaitForMilliseconds(gameTime, 100);                  
                }
                else 
                {
                    Animator.SwitchAnimation(PressedArgs);
                }

            }
            else
            {
                Animator.SwitchAnimation(ReleasedArgs);
            }
        }

        private void Peripherals_LeftButtonReleased(object sender, EventArgs e)
        {
            if (!Peripherals.LeftButtonPressed())
            {
                if (Animator.VisibleArea().Contains(Peripherals.GetMouseWindowPosition()))
                {
                    OnClicked();
                }
                IsPressed = false;
                Peripherals.LeftButtonReleased -= Peripherals_LeftButtonReleased;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            if (Label != null)
            {
                spriteBatch.DrawString(MainGame.DefaultFont, Label, Position, Color.Black);
            }
        }
    }
}
