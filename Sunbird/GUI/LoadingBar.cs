using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.Controllers;

namespace Sunbird.GUI
{
    public class LoadingBar : Sprite
    {
        Texture2D BarEmpty { get; set; }
        Texture2D BarFull { get; set; }
        public int Progress { get; set; } = 0;

        public LoadingBar(Texture2D barEmpty, Texture2D barFull, Vector2 position) 
        {
            BarEmpty = barEmpty;
            BarFull = barFull;
            Position = position;
        }

        public LoadingBar(Texture2D barEmpty, Texture2D barFull, Vector2 position, Alignment alignment)
        {
            BarEmpty = barEmpty;
            BarFull = barFull;
            if (alignment == Alignment.TopLeft)
            {
                Position = position;
            }
            else if (alignment == Alignment.TopRight)
            {
                Position = position + new Vector2(-barEmpty.Width, 0);
            }
            else if (alignment == Alignment.Center)
            {
                Position = position + new Vector2(-barEmpty.Width / 2, -barEmpty.Height / 2);
            }
            else if (alignment == Alignment.BottomLeft)
            {
                Position = position + new Vector2(0, -barEmpty.Height);
            }
            else if (alignment == Alignment.BottomRight)
            {
                Position = position + new Vector2(-barEmpty.Width, -barEmpty.Height);
            }
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BarEmpty, Position, Color.White);
            spriteBatch.Draw(BarFull, Position, new Rectangle(new Point(0, 0), new Point(3 + Progress * BarEmpty.Width / 100, BarEmpty.Height)), Color.White);
        }

    }
}
