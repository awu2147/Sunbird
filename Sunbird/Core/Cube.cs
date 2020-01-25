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
using Sunbird.Serialization;

namespace Sunbird.Core
{

    public class Cube : Sprite
    {

        public Cube()
        {

        }

        public Cube(SpriteSheet spriteSheet) : base(spriteSheet)
        {
            
        }

    }

    public static class CubeFactory
    {
        public static string CurrentPath { get; set; } = "Temp/GrassCube";

        public static int CurrentIndex { get; set; } = 0;

        public static Dictionary<int, string> CubePathLibrary { get; set; } = new Dictionary<int, string>()
        {
            {0, "Temp/GrassCube" },
            {1, "Temp/WaterCube" },
        };

        public static Cube CreateCube(MainGame mainGame, string path, Vector2 position)
        {
            var spriteSheet = new SpriteSheet(mainGame.Content.Load<Texture2D>(path), 1, 1) { TexturePath = path };
            return new Cube(spriteSheet) { Position = position };
        }

        public static Cube CreateCube(MainGame mainGame, string path, Vector2 position, Coord coords)
        {
            var spriteSheet = new SpriteSheet(mainGame.Content.Load<Texture2D>(path), 1, 1) { TexturePath = path };
            return new Cube(spriteSheet) { Position = position, Coords = coords };
        }

        public static Cube CreateCube(MainGame mainGame, string path, int rows, int columns, Vector2 position, Coord coords)
        {
            var spriteSheet = new SpriteSheet(mainGame.Content.Load<Texture2D>(path), rows, columns) { TexturePath = path };
            return new Cube(spriteSheet) { Position = position, Coords = coords };
        }
        public static Cube CreateCurrentCube(MainGame mainGame, int rows, int columns, Vector2 position, Coord coords)
        {
            var spriteSheet = new SpriteSheet(mainGame.Content.Load<Texture2D>(CurrentPath), rows, columns) { TexturePath = CurrentPath };
            return new Cube(spriteSheet) { Position = position, Coords = coords };
        }

        public static void FindNext()
        {
            CurrentIndex++;
            if (CurrentIndex >= CubePathLibrary.Count())
            {
                CurrentIndex = 0;
            }
            CurrentPath = CubePathLibrary[CurrentIndex];
        }

    }

    public class CubeFactoryData
    {

        public string CurrentPath { get; set; }

        public int CurrentIndex { get; set; }

        public CubeFactoryData()
        {
            CurrentPath = CubeFactory.CurrentPath;
            CurrentIndex = CubeFactory.CurrentIndex;
        }

        public void Serialize()
        {
            Serializer.WriteXML<CubeFactoryData>(this, "CubeFactoryData.xml");
        }

        public void SyncIn()
        {
            CurrentPath = CubeFactory.CurrentPath;
            CurrentIndex = CubeFactory.CurrentIndex;
        }

        public void SyncOut()
        {
            CubeFactory.CurrentPath = CurrentPath;
            CubeFactory.CurrentIndex = CurrentIndex;
        }
    
    }
}
