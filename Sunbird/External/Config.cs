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
    public class Config : IConfig
    {
        [XmlIgnore]
        public MainGame MainGame { get; set; }

        public Keys North { get; set; }
        public Keys East { get; set; }
        public Keys South { get; set; }
        public Keys West { get; set; }

        public Config()
        {

        }

        public Config(MainGame mainGame)
        {
            MainGame = mainGame;
            mainGame.Exiting += MainGame_Exiting;
            North = Keys.W;
            East = Keys.D;
            South = Keys.S;
            West = Keys.A;
        }

        public void LoadContent(MainGame mainGame)
        {
            mainGame.Exiting += MainGame_Exiting;
        }

        private void MainGame_Exiting(object sender, EventArgs e)
        {
            Serializer.WriteXML<Config>(this, "Config.xml");
        }
    }

}
