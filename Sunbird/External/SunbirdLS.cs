using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Sunbird.GUI;

namespace Sunbird.External
{
    public class SunbirdLS : LoadingScreen
    {
        public SunbirdLS(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content) : base(mainGame, graphicsDevice, content) { }

        public override void LoadContent()
        {
            BackgroundColor = new Color(51, 57, 65);

            var centerIconPosition = new Vector2(MainGame.Width / 2, MainGame.Height / 2 - 60);
            var centerIcon = new Sprite(MainGame, SpriteSheet.CreateNew(MainGame, "Temp/sunbird", 1, 1), centerIconPosition, Alignment.Center);
            spriteList.Add(centerIcon);

            var barEmpty = Content.Load<Texture2D>("GUI/bar1_empty");
            var barFull = Content.Load<Texture2D>("GUI/bar1_full");
            var barPosition = new Vector2(MainGame.Width / 2, MainGame.Height / 2 + 70);
            LoadingBar = new LoadingBar(barEmpty, barFull, barPosition, Alignment.Center);
        }   
    }

    public class SunbirdLS_Factory : ILoadingScreenFactory
    {
        public SunbirdLS_Factory() { }

        public ILoadingScreen CreateLoadingScreen(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            return new SunbirdLS(mainGame, graphicsDevice, content) as ILoadingScreen;
        }
    }
}
