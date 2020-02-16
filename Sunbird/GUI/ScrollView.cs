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
    public enum Orientation
    {
        Vertical,
        Horizontal
    }

    public class ScrollView : Sprite
    {
        ScrollBar VerticalScrollBar;
        ScrollBar HorizontalScrollBar;

        public ScrollView()
        {

        }

    }

    public class ScrollBarContainer : Sprite
    {
        public ScrollBar ScrollBar { get; set; }

        public int CurrentLength
        {
            get { return ScrollBar.CurrentLength; }
            set { ScrollBar.CurrentLength = value; }
        }


        public ScrollBarContainer(MainGame mainGame, Texture2D scrollBar, Orientation orientation, int maxLength)
        {
            MainGame = mainGame;
            ScrollBar = new ScrollBar(this, scrollBar, orientation, maxLength);
        }

        private void ScrollBar_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Peripherals.LeftButtonTapped() && ScrollBar.WorldArea().Contains(Peripherals.GetMouseWindowPosition()) && MainGame.IsActive)
            {
                OnClicked();
            }
        }

        public override void OnClicked()
        {
            base.OnClicked();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }

    }



    public class ScrollBar : Sprite
    {
        private ScrollBarContainer sender;
        private Orientation orientation;
        private Texture2D scrollBar;
        private Rectangle viewRectangle;
        new private Vector2 Position { get { return sender.Position + PositionOffset; } }
        public int MaxLength { get; set; }
        public int CurrentLength { get; set; }

        public ScrollBar(ScrollBarContainer sender, Texture2D scrollBar, Orientation orientation, int maxLength)
        {
            this.sender = sender;
            this.orientation = orientation;
            MaxLength = maxLength;
            CurrentLength = maxLength;
            if (orientation == Orientation.Vertical)
            {
                viewRectangle = new Rectangle(0, 0, scrollBar.Width, MaxLength);
            }
            else if (orientation == Orientation.Horizontal)
            {
                viewRectangle = new Rectangle(0, 0, MaxLength, scrollBar.Height);
            }
        }

        public Rectangle WorldArea()
        {
            return new Rectangle(Position.ToPoint(), new Point(viewRectangle.Width, viewRectangle.Height));
        }

        public override void Update(GameTime gameTime)
        {
            if (orientation == Orientation.Vertical)
            {
                viewRectangle.Height = CurrentLength;
            }
            else if (orientation == Orientation.Horizontal)
            {
                viewRectangle.Width = CurrentLength;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(scrollBar, Position, viewRectangle, Color.White);
        }
    }
}
