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

        public Cube(SpriteSheet spriteSheet, int startFrame, int frameCount, float frameSpeed, AnimationState animState)
        {
            Animator = new Animator(spriteSheet, this, startFrame, frameCount, frameSpeed, animState);
        }

    }

    public class CubeMetaData
    {
        public string Path { get; set; }
        public int SheetRows { get; set; } = 1;
        public int SheetColumns { get; set; } = 1;
        public int StartFrame { get; set; } = 0;
        public int FrameCount { get; set; } = 1;
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;
        public CubeMetaData()
        {

        }
    }

    public static class CubeFactory
    {
        public static CubeMetaData CurrentCubeMetaData{ get; set; }

        public static int CurrentIndex { get; set; } = 0;

        public static XDictionary<int, CubeMetaData> CubeMetaDataLibrary { get; set; }

        public static Cube CreateCube(MainGame mainGame, CubeMetaData cubeMetaData, Coord coords, Coord relativeCoords, int altitude)
        {
            var spriteSheet = SpriteSheet.CreateNew(mainGame, cubeMetaData.Path, cubeMetaData.SheetRows, cubeMetaData.SheetColumns);
            return new Cube(spriteSheet, cubeMetaData.StartFrame, cubeMetaData.FrameCount, cubeMetaData.FrameSpeed, cubeMetaData.AnimState) { Position = World.TopFace_CoordToLocalOrigin(coords), Coords = relativeCoords, Altitude = altitude };
        }

        public static Cube CreateRandomCube(MainGame mainGame, CubeMetaData cubeMetaData, Coord coords, Coord relativeCoords, int altitude)
        {
            var r = new Random();
            var spriteSheet = SpriteSheet.CreateNew(mainGame, cubeMetaData.Path, cubeMetaData.SheetRows, cubeMetaData.SheetColumns);
            var cube = new Cube(spriteSheet, cubeMetaData.StartFrame, cubeMetaData.FrameCount, cubeMetaData.FrameSpeed, cubeMetaData.AnimState) { Position = World.TopFace_CoordToLocalOrigin(coords), Coords = relativeCoords, Altitude = altitude };
            cube.Animator.CurrentFrame = r.Next(0, cubeMetaData.FrameCount);
            return cube;
        }

        public static Cube CreateCurrentCube(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateCube(mainGame, CurrentCubeMetaData, coords, relativeCoords, altitude);
        }

        public static Cube CreateRandomCurrentCube(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateRandomCube(mainGame, CurrentCubeMetaData, coords, relativeCoords, altitude);
        }

        public static void FindNext()
        {
            CurrentIndex++;
            if (CurrentIndex >= CubeMetaDataLibrary.Count())
            {
                CurrentIndex = 0;
            }
            CurrentCubeMetaData = CubeMetaDataLibrary[CurrentIndex];
        }

    }

    /// <summary>
    /// Acts as a data store for the static class CubeFactory during serialization.
    /// </summary>
    public class CubeFactoryData
    {
        public CubeMetaData CurrentCubeMetaData { get; set; }

        public int CurrentIndex { get; set; }

        public XDictionary<int, CubeMetaData> CubeMetaDataLibrary { get; set;}

        public CubeFactoryData()
        {
            CurrentCubeMetaData = CubeFactory.CurrentCubeMetaData;
            CurrentIndex = CubeFactory.CurrentIndex;
            CubeMetaDataLibrary = CubeFactory.CubeMetaDataLibrary;
        }

        public void Serialize()
        {
            Serializer.WriteXML<CubeFactoryData>(this, "CubeFactoryData.xml", new Type[] { typeof(CubeMetaData) });
        }

        public void SyncIn()
        {
            CurrentCubeMetaData = CubeFactory.CurrentCubeMetaData;
            CurrentIndex = CubeFactory.CurrentIndex;
            CubeMetaDataLibrary = CubeFactory.CubeMetaDataLibrary;
        }

        public void SyncOut()
        {
            CubeFactory.CurrentCubeMetaData = CurrentCubeMetaData;
            CubeFactory.CurrentIndex = CurrentIndex;
            CubeFactory.CubeMetaDataLibrary = CubeMetaDataLibrary;
        }  

    }

}
