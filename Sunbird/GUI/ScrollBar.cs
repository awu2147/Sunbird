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
    public class ScrollBarContainer : Sprite
    {     
        private Point Anchor;
        private Point DragPositionChange;
        private bool Dragged;
        private Orientation Orientation;
        private int StartingSegment;

        public ScrollBar ScrollBar { get; set; }

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
        public int CurrentSegment { get; set; } = 1;

        public ScrollBarContainer(MainGame mainGame, string path, Orientation orientation, int maxLength)
        {
            MainGame = mainGame;
            Orientation = orientation;
            var scrollBarTexture = mainGame.Content.Load<Texture2D>(path);
            ScrollBar = new ScrollBar(this, scrollBarTexture, orientation, maxLength);
        }

        public override void Update(GameTime gameTime)
        {
            var segmentLength = MaxLength / TotalSegments;
            CurrentLength = segmentLength;
            var offset = ((CurrentSegment - 1) / (float)TotalSegments) * MaxLength;
            if (Orientation == Orientation.Vertical)
            {
                ScrollBar.PositionOffset = new Vector2(0, offset);
            }
            else if (Orientation == Orientation.Horizontal)
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
                    if (Orientation == Orientation.Vertical)
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
                    else if (Orientation == Orientation.Horizontal)
                    {
                        throw new NotImplementedException();
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
        private ScrollBarContainer Container;
        private Orientation Orientation;
        private Texture2D ScrollBarTexture;
        private Rectangle ViewRectangle;
        new public Vector2 Position { get { return Container.Position + PositionOffset; } }
        public int MaxLength { get; set; }
        public int CurrentLength { get; set; }

        public ScrollBar(ScrollBarContainer container, Texture2D scrollBarTexture, Orientation orientation, int maxLength)
        {
            Container = container;
            Orientation = orientation;
            ScrollBarTexture = scrollBarTexture;
            MaxLength = maxLength;
            CurrentLength = maxLength;
            if (orientation == Orientation.Vertical)
            {
                ViewRectangle = new Rectangle(0, 0, scrollBarTexture.Width, MaxLength);
            }
            else if (orientation == Orientation.Horizontal)
            {
                ViewRectangle = new Rectangle(0, 0, MaxLength, scrollBarTexture.Height);
            }
        }

        public Rectangle WorldArea()
        {
            return new Rectangle((Container.Position + PositionOffset).ToPoint(), new Point(ViewRectangle.Width, ViewRectangle.Height));
        }

        public override void Update(GameTime gameTime)
        {
            if (Orientation == Orientation.Vertical)
            {
                ViewRectangle.Height = CurrentLength;
            }
            else if (Orientation == Orientation.Horizontal)
            {
                ViewRectangle.Width = CurrentLength;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ScrollBarTexture, Position, ViewRectangle, Color.White);
        }
    }
}
