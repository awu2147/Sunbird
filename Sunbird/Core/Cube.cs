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
        private Cube() { }

        public Cube(SpriteSheet spriteSheet) : base(spriteSheet) { }

    }

    public static class CubeFactory
    {
        public static string CurrentPath { get; set; }

        public static int CurrentIndex { get; set; } = 0;

        public static XDictionary<int, string> CubePathLibrary { get; set; }

        public static Cube CreateCube(MainGame mainGame, string path, Coord coords, Coord relativeCoords, int altitude)
        {
            var spriteSheet = SpriteSheet.CreateNew(mainGame, path, 1, 1);
            return new Cube(spriteSheet) { Position = World.TopFace_CoordToLocalOrigin(coords), Coords = relativeCoords, Altitude = altitude };
        }

        public static Cube CreateCube(MainGame mainGame, string path, int rows, int columns, Coord coords, Coord relativeCoords, int altitude)
        {
            var spriteSheet = SpriteSheet.CreateNew(mainGame, path, rows, columns);
            return new Cube(spriteSheet) { Position = World.TopFace_CoordToLocalOrigin(coords), Coords = relativeCoords, Altitude = altitude };
        }

        public static Cube CreateCurrentCube(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateCube(mainGame, CurrentPath, coords, relativeCoords, altitude);
        }

        public static Cube CreateCurrentCube(MainGame mainGame, int rows, int columns, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateCube(mainGame, CurrentPath, rows, columns, coords, relativeCoords, altitude);
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

    /// <summary>
    /// Acts as a data store for the static class CubeFactory during serialization.
    /// </summary>
    public class CubeFactoryData
    {
        public string CurrentPath { get; set; }

        public int CurrentIndex { get; set; }

        public XDictionary<int, string> CubePathLibrary { get; set;}

        public CubeFactoryData()
        {
            CurrentPath = CubeFactory.CurrentPath;
            CurrentIndex = CubeFactory.CurrentIndex;
            CubePathLibrary = CubeFactory.CubePathLibrary;
        }

        public void Serialize()
        {
            Serializer.WriteXML<CubeFactoryData>(this, "CubeFactoryData.xml");
        }

        public void SyncIn()
        {
            CurrentPath = CubeFactory.CurrentPath;
            CurrentIndex = CubeFactory.CurrentIndex;
            CubePathLibrary = CubeFactory.CubePathLibrary;
        }

        public void SyncOut()
        {
            CubeFactory.CurrentPath = CurrentPath;
            CubeFactory.CurrentIndex = CurrentIndex;
            CubeFactory.CubePathLibrary = CubePathLibrary;
        }  

    }

}
