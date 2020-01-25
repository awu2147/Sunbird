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
using System.Xml.Schema;
namespace Sunbird.Core
{
    public class GhostMarker : Sprite
    {
        private GhostMarker()
        {

        }

        public GhostMarker(SpriteSheet spriteSheet) : base(spriteSheet)
        {

        }

        public static GhostMarker CreateGhostMarker(MainGame mainGame, string path)
        {
            var spriteSheet = new SpriteSheet(mainGame.Content.Load<Texture2D>(path), 1, 1) { TexturePath = path };
            return new GhostMarker(spriteSheet) { Alpha = 0.25f };
        }

    }
}
