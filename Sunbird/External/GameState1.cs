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

namespace Sunbird.External
{
    public class GameState1 : State
    {
        [XmlIgnore]
        public Texture2D Background { get; set; }

        public List<Sprite> spriteList;

        private bool IsLoading;

        private GameState1()
        {

        }

        public GameState1(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content) : base(mainGame, graphicsDevice, content)
        {
            (new Thread(() => LoadContentFromFile())).Start();
            //CreateContent();
        }

        private void MainGame_Exiting(object sender, System.EventArgs e)
        {
            Serializer.WriteXML<GameState1>(this, "GameStateSave.xml", new Type[] { typeof(Player) });
        }

        private void CreateContent()
        {
            spriteList = new List<Sprite>();

            var spriteSheet = new SpriteSheet(Content.Load<Texture2D>("Temp/testsolid"), 1, 1) { TexturePath = "Temp/testsolid" };
            spriteList.Add(new Sprite(spriteSheet) { Position = new Vector2(1, 2) });

            var spriteSheet2 = new SpriteSheet(Content.Load<Texture2D>("Temp/testtile2"), 1, 1) { TexturePath = "Temp/testtile2" };
            spriteList.Add(new Sprite(spriteSheet2) { Position = new Vector2(90, 50) });

            spriteList.Add(MainGame.Player);

            MainGame.Exiting += MainGame_Exiting;
        }

        public void LoadContentFromFile()
        {
            IsLoading = true;
            MainGame.CurrentState = Templates.LoadingScreenTemplates[0].CreateLoadingScreen(MainGame, GraphicsDevice, Content) as State;
            var currentState = MainGame.CurrentState as ILoadingScreen;

            var XmlData = Serializer.ReadXML<GameState1>("GameStateSave.xml", new Type[] { typeof(Player) });
            spriteList = XmlData.spriteList;
            foreach (var sprite in spriteList)
            {
                sprite.LoadContent(MainGame, GraphicsDevice, Content);
            }

            for (int i = 0; i < 50; i++)
            {
                Thread.Sleep(30);
                currentState.LoadingBar.Progress += 2;
            }

            IsLoading = false;
            MainGame.CurrentState = this;

            MainGame.Exiting += MainGame_Exiting;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsLoading)
            {
                foreach (var sprite in spriteList)
                {
                    sprite.Update(gameTime);
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsLoading)
            {
                foreach (var sprite in spriteList)
                {
                    sprite.Draw(gameTime, spriteBatch);
                }
            }
        }

    }
}
