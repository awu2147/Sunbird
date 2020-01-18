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
using Sunbird.Controllers;

namespace Sunbird.States
{
    public class GameState : State
    {
        [XmlIgnore]
        public Texture2D Background { get; set; }

        public List<Sprite> spriteList;

        private GameState()
        {

        }

        public GameState(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content) : base(mainGame, graphicsDevice, content)
        {
            //LoadContent();
            var XmlData = ReadXML<GameState>("GameStateSave.xml");
            spriteList = XmlData.spriteList;
            foreach (var sprite in spriteList)
            {
                sprite.LoadContent(mainGame, graphicsDevice, content);
            }

            mainGame.Exiting += MainGame_Exiting;
        }

        private void MainGame_Exiting(object sender, System.EventArgs e)
        {
            WriteXML("GameStateSave.xml");
        }

        private void LoadContent()
        {
            spriteList = new List<Sprite>();

            var spriteSheet = new SpriteSheet(Content.Load<Texture2D>("Temp/testsolid"), 1, 1) { texturePath = "Temp/testsolid" };
            spriteList.Add(new Sprite(spriteSheet) {Position = new Vector2(1, 2), });

            var spriteSheet2 = new SpriteSheet(Content.Load<Texture2D>("Temp/testtile2"), 1, 1) { texturePath = "Temp/testtile2" };
            spriteList.Add(new Sprite(spriteSheet2) { Position = new Vector2(40, 50) });
        }

        public T ReadXML<T>(string path)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T), new Type[] {typeof(Fish)}); // <- Inherited classes of Sprite go here.
            TextReader reader = new StreamReader("GameStateSave.xml");
            object obj = deserializer.Deserialize(reader);
            reader.Close();
            return (T)obj;
        }

        public void WriteXML(string path)
        {
            XmlSerializer mySerializer = new XmlSerializer(GetType(), new Type[] {typeof(Fish)});
            StreamWriter myWriter = new StreamWriter(path);
            mySerializer.Serialize(myWriter, this);
            myWriter.Close();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var sprite in spriteList)
            {
                sprite.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var sprite in spriteList)
            {
                sprite.Draw(gameTime, spriteBatch);
            }
        }

    }
}
