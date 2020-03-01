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

namespace Sunbird.Core
{
    public class WaterShader
    {
        public static int Tick;
        public static Timer Timer = new Timer();

        public WaterShader(MainGame mainGame)
        {
            LoadContent(mainGame);
        }

        public List<Sprite> WaterNoisePanelPairs = new List<Sprite>() { };

        public void LoadContent(MainGame mainGame)
        {
            Timer.OnCompleted = () =>
            {
                Tick++;               
                if (Tick > 900)
                {
                    Tick = 0;
                }
            };
         
            var upSheet = SpriteSheet.CreateNew(mainGame, "Effects/WaterUp");
            var downSheet = SpriteSheet.CreateNew(mainGame, "Effects/WaterDown");

            WaterNoisePanelPairs.Add(new WaterNoisePanelPair(mainGame, Vector2.Zero, upSheet, downSheet));
            WaterNoisePanelPairs.Add(new WaterNoisePanelPair(mainGame, new Vector2(-1800, -900), upSheet, downSheet));
            WaterNoisePanelPairs.Add(new WaterNoisePanelPair(mainGame, new Vector2(-1800, 0), upSheet, downSheet));
            WaterNoisePanelPairs.Add(new WaterNoisePanelPair(mainGame, new Vector2(0, -900), upSheet, downSheet));
            WaterNoisePanelPairs.Add(new WaterNoisePanelPair(mainGame, new Vector2(1800, 0), upSheet, downSheet));
            WaterNoisePanelPairs.Add(new WaterNoisePanelPair(mainGame, new Vector2(1800, -900), upSheet, downSheet));
        }

        public void Update(GameTime gameTime)
        {
            Timer.WaitForMilliseconds(gameTime, 20);
            foreach (var pair in WaterNoisePanelPairs)
            {
                pair.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var pair in WaterNoisePanelPairs)
            {
                pair.Draw(gameTime, spriteBatch);
            }
        }

    }

    public class WaterNoisePanelPair : Sprite
    {
        WaterNoisePanelUp WaterNoisePanelUp;
        WaterNoisePanelDown WaterNoisePanelDown;

        public WaterNoisePanelPair(MainGame mainGame, Vector2 position, SpriteSheet upSheet, SpriteSheet downSheet)
        {
            Position = position;
            WaterNoisePanelUp = new WaterNoisePanelUp(mainGame, upSheet, this);
            WaterNoisePanelDown = new WaterNoisePanelDown(mainGame, downSheet, this) { Alpha = 0.7f };
        }

        public override void Update(GameTime gameTime)
        {       
            WaterNoisePanelUp.Update(gameTime);
            WaterNoisePanelDown.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            WaterNoisePanelUp.Draw(gameTime, spriteBatch);
            WaterNoisePanelDown.Draw(gameTime, spriteBatch);
        }
    }

    public class WaterNoisePanelUp : Sprite
    {
        public Point PanelDimensions = new Point(1800, 900);
        public Point ViewPosition = new Point(0, 0);
        public Rectangle View { get { return new Rectangle(ViewPosition, PanelDimensions); } }

        public WaterNoisePanelUp(MainGame mainGame, SpriteSheet spriteSheet, WaterNoisePanelPair parent) : base(mainGame, spriteSheet)
        {
            Position = parent.Position;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            ViewPosition = new Point(0, WaterShader.Tick);        
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Animator.SpriteSheet.Texture, Animator.Position, View, Color.White * Alpha);
        }
    }

    public class WaterNoisePanelDown : Sprite
    {       
        public Point PanelDimensions = new Point(1800, 900);
        public Point ViewPosition = new Point(0, 900);
        public Rectangle View { get { return new Rectangle(ViewPosition, PanelDimensions); } }

        public WaterNoisePanelDown(MainGame mainGame, SpriteSheet spriteSheet, WaterNoisePanelPair parent) : base(mainGame, spriteSheet)
        {
            Position = parent.Position;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            ViewPosition = new Point(0, 900 - WaterShader.Tick);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Animator.SpriteSheet.Texture, Animator.Position, View, Color.White * Alpha);
        }
    }
}
