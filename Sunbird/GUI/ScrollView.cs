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

        private Orientation orientation;

        public int CurrentLength
        {
            get { return ScrollBar.CurrentLength; }
            set { ScrollBar.CurrentLength = value; }
        }

        public int MaxLength
        {
            get { return ScrollBar.MaxLength; }
            set { ScrollBar.MaxLength = value; }
        }

        public int TotalSegments { get; set; } = 5;
        public int CurrentSegment { get; set; } = 2;

        private int StartingSegment;
        private Point Anchor;
        private Point DragPositionChange;
        private bool Dragged;

        public ScrollBarContainer(MainGame mainGame, string path, Orientation orientation, int maxLength)
        {
            MainGame = mainGame;
            this.orientation = orientation;
            var scrollBar = mainGame.Content.Load<Texture2D>(path);
            ScrollBar = new ScrollBar(this, scrollBar, orientation, maxLength);
        }

        public override void Update(GameTime gameTime)
        {
            var segmentLength = MaxLength / TotalSegments;
            CurrentLength = segmentLength;
            var offset = (CurrentSegment - 1) / (float)TotalSegments * MaxLength;
            if (orientation == Orientation.Vertical)
            {
                ScrollBar.PositionOffset = new Vector2(0, offset);
            }
            else if (orientation == Orientation.Horizontal)
            {
                ScrollBar.PositionOffset = new Vector2(offset, 0);
            }
            if (Peripherals.LeftButtonPressed() && MainGame.IsActive == true)
            {
                if (Peripherals.LeftButtonTapped() && ScrollBar.WorldArea().Contains(Peripherals.GetMouseWindowPosition()))
                {
                    Peripherals.LeftButtonReleased += Peripherals_LeftButtonReleased;
                    Anchor = Peripherals.GetMouseWindowPosition();
                    StartingSegment = CurrentSegment;
                    Dragged = true;
                }
                // After initial click, this bool allows continued dragging even if the cursor leaves the scroll bar area.
                if (Dragged)
                {
                    var currentPosition = Peripherals.GetMouseWindowPosition();
                    DragPositionChange = (currentPosition - Anchor);
                    if (orientation == Orientation.Vertical)
                    {
                        var change = Math.Abs(DragPositionChange.Y);
                        var threshold = change + segmentLength / 2;
                        if (threshold >= segmentLength && DragPositionChange.Y < 0)
                        {
                            CurrentSegment = StartingSegment - threshold / segmentLength;
                        }
                        else if (threshold >= segmentLength && DragPositionChange.Y > 0)
                        {
                            CurrentSegment = StartingSegment + threshold / segmentLength;
                        }
                        else
                        {
                            CurrentSegment = StartingSegment;
                        }
                        
                        if (CurrentSegment < 1)
                        {
                            CurrentSegment = 1;
                        }
                        else if (CurrentSegment > TotalSegments)
                        {
                            CurrentSegment = TotalSegments;
                        }

                    }
                    else if (orientation == Orientation.Horizontal)
                    {
                        throw new NotImplementedException("Add me");
                    }
                }
            }
            ScrollBar.Update(gameTime);
        }

        private void Peripherals_LeftButtonReleased(object sender, EventArgs e)
        {
            DragPositionChange = Point.Zero;
            Dragged = false;
            Peripherals.LeftButtonReleased -= Peripherals_LeftButtonReleased;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            ScrollBar.Draw(gameTime, spriteBatch);
        }

    }

    public class ScrollBar : Sprite
    {
        private ScrollBarContainer container;
        private Orientation orientation;
        private Texture2D scrollBar;
        private Rectangle viewRectangle;
        new public Vector2 Position { get { return container.Position + PositionOffset; } }
        public int MaxLength { get; set; }
        public int CurrentLength { get; set; }

        public ScrollBar(ScrollBarContainer container, Texture2D scrollBar, Orientation orientation, int maxLength)
        {
            this.container = container;
            this.orientation = orientation;
            this.scrollBar = scrollBar;
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
            //Debug.Print($"{container.Position}, {PositionOffset}");
            return new Rectangle((container.Position + PositionOffset).ToPoint(), new Point(viewRectangle.Width, viewRectangle.Height));
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
