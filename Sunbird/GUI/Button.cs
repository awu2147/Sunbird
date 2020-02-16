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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.Controllers;
using Sunbird.Serialization;
using Sunbird.GUI;

namespace Sunbird.GUI
{
    public class ButtonClickedEventArgs : EventArgs
    {
        public ButtonClickedEventArgs()
        {

        }
    }

    public enum ButtonType
    { 
        Default,
        SafeRelease,
        CheckBox,
        Group,
    }

    public class Button : Sprite
    {
        new public event EventHandler<ButtonClickedEventArgs> Clicked;
        public event EventHandler<ButtonClickedEventArgs> Checked;
        public event EventHandler<ButtonClickedEventArgs> Unchecked;

        public string Label { get; set; }
        public ButtonType ButtonType { get; set; }

        private bool _IsPressed;
        public bool IsPressed 
        {
            get
            {
                return _IsPressed;
            }
            set
            {
                _IsPressed = value;
                if (value == true)
                {
                    ReconfigureAnimator(PressedArgs);
                }
                else
                {
                    ReconfigureAnimator(ReleasedArgs);
                }
            }
        }
        public AnimArgs ReleasedArgs { get; set; } = new AnimArgs(0);
        public AnimArgs PressedArgs { get; set; } = new AnimArgs(1);
        public Timer Timer { get; set; } = new Timer();

        [XmlIgnore]
        public List<Button> Siblings { get; set; }

        [XmlIgnore]
        public Action OnUpdated { get; set; } = () => { };

        private Button() { }

        public Button(MainGame mainGame, SpriteSheet spriteSheet, string label) : this(mainGame, spriteSheet, label, Vector2.Zero) { }

        public Button(MainGame mainGame, SpriteSheet spriteSheet, string label, Vector2 position) : this(mainGame, spriteSheet, label, position, Alignment.TopLeft) { }

        public Button(MainGame mainGame, SpriteSheet spriteSheet, string label, Vector2 position, Alignment alignment) : base (mainGame, spriteSheet, position, alignment)
        {
            Label = label;
        }

        public override void OnClicked()
        {
            if (Siblings != null)
            {
                foreach (var button in Siblings)
                {
                    button.IsPressed = false;
                }
            }
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

        public static void BindGroup(List<Button> group)
        {
            foreach (var button in group)
            {
                button.ButtonType = ButtonType.Group;
                button.Siblings = new List<Button>() { };
                foreach (var sibling in group)
                {
                    if (sibling == button)
                    {
                        continue;
                    }
                    button.Siblings.Add(sibling);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Animator.WorldArea().Contains(Peripherals.GetMouseWindowPosition()) && Peripherals.LeftButtonTapped() && MainGame.IsActive)
            {
                if (ButtonType == ButtonType.Default)
                {
                    OnClicked();
                    IsPressed = true;
                    Timer.OnCompleted = () => 
                    { 
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
                    if (IsPressed)
                    {
                        OnChecked();
                    }
                    else
                    {
                        OnUnchecked();
                    }
                }
                else if (ButtonType == ButtonType.Group)
                {
                    OnClicked();
                    IsPressed = true;
                }
            }

            if (IsPressed == true && ButtonType == ButtonType.Default)
            {   
                Timer.WaitForMilliseconds(gameTime, 100);                                
            }
            OnUpdated();
        }

        private void Peripherals_LeftButtonReleased(object sender, EventArgs e)
        {
            if (!Peripherals.LeftButtonPressed())
            {
                if (Animator.WorldArea().Contains(Peripherals.GetMouseWindowPosition()))
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
