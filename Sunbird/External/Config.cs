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
        public static readonly XmlSerializer ConfigSerializer = Serializer.CreateNew(typeof(Config));

        public Keys North { get; set; }
        public Keys East { get; set; }
        public Keys South { get; set; }
        public Keys West { get; set; }

        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }

        public int WorldZoom { get; set; } = 3;

        public Config() { }

        public Config(MainGame mainGame)
        {
            mainGame.Exiting += MainGame_Exiting;

            // Default values here;
            North = Keys.W;
            East = Keys.D;
            South = Keys.S;
            West = Keys.A;
            WindowWidth = 800;
            WindowHeight = 800;
        }

        public void LoadContent(MainGame mainGame)
        {
            mainGame.Exiting += MainGame_Exiting;
            SyncOut();
        }

        private void MainGame_Exiting(object sender, EventArgs e)
        {
            SyncIn();
            Serializer.WriteXML<Config>(ConfigSerializer, this, "Config.xml");
        }

        // Static World property piggybacks on the Config file.
        public void SyncIn()
        {
            WorldZoom = World.Zoom;
        }

        public void SyncOut()
        {
            World.Zoom = WorldZoom;
            World.ReconstructTopFaceArea();
        }
    }

}
