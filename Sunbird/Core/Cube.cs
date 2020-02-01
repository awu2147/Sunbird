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
    [Serializable]
    public class Cube : Sprite
    {
        public Animator AnimatorBase { get; set; }

        public Cube() { }

        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Animator.LoadContent(mainGame, graphicsDevice, content);
            Animator.Sender = this;
            AnimatorBase.LoadContent(mainGame, graphicsDevice, content);
            AnimatorBase.Sender = this;
        }

        public override void Update(GameTime gameTime)
        {
            Animator.Update(gameTime);
            AnimatorBase.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsHidden == false)
            {             
                AnimatorBase.Draw(gameTime, spriteBatch, Alpha);
                Animator.Draw(gameTime, spriteBatch, Alpha);
            }
        }

    }

    public class CubeMetaData
    {
        public string Path { get; set; }
        public int SheetRows { get; set; } = 1;
        public int SheetColumns { get; set; } = 1;
        public int StartFrame { get; set; } = 0;
        public int CurrentFrame { get; set; } = 0;
        public int FrameCount { get; set; } = 1;
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;
        public CubeMetaData()
        {

        }

        public void NextFrame()
        {
            CurrentFrame++;
            if (CurrentFrame >= FrameCount)
            {
                CurrentFrame = 0;
            }
        }
    }

    public class CubeBaseMetaData
    {
        public string Path { get; set; }
        public int SheetRows { get; set; } = 1;
        public int SheetColumns { get; set; } = 1;
        public int StartFrame { get; set; } = 0;
        public int CurrentFrame { get; set; } = 0;
        public int FrameCount { get; set; } = 1;
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;

        public CubeBaseMetaData()
        {

        }
    }

    public static class CubeFactory
    {
        public static CubeMetaData CurrentCubeMetaData{ get; set; }
        public static CubeBaseMetaData CurrentCubeBaseMetaData { get; set; }

        public static bool IsRandomTop { get; set; }
        public static bool IsRandomBottom { get; set; }

        public static int CurrentIndex { get; set; } = 0;
        public static int CurrentBaseIndex { get; set; } = 0;

        public static XDictionary<int, CubeMetaData> CubeMetaDataLibrary { get; set; }
        public static XDictionary<int, CubeBaseMetaData> CubeBaseMetaDataLibrary { get; set; }

        public static Cube CreateCube(MainGame mainGame, CubeMetaData cubeMD, CubeBaseMetaData cubeBaseMD, Coord coords, Coord relativeCoords, int altitude)
        {
            var spriteSheet = SpriteSheet.CreateNew(mainGame, cubeMD.Path, cubeMD.SheetRows, cubeMD.SheetColumns);
            var spriteSheetBase = SpriteSheet.CreateNew(mainGame, cubeBaseMD.Path, cubeBaseMD.SheetRows, cubeBaseMD.SheetColumns);
            var cube = new Cube() { Position = World.TopFace_CoordToLocalOrigin(coords), Coords = relativeCoords, Altitude = altitude };
            var rand = new Random();
            cube.Animator = new Animator(spriteSheet, cube, cubeMD.StartFrame, cubeMD.FrameCount, cubeMD.FrameSpeed, cubeMD.AnimState);
            if (IsRandomTop == true)
            {
                cube.Animator.CurrentFrame = rand.Next(0, cube.Animator.FrameCount);
            }
            else
            {
                cube.Animator.CurrentFrame = cubeMD.CurrentFrame;
            }
            cube.AnimatorBase = new Animator(spriteSheetBase, cube, cubeBaseMD.StartFrame, cubeBaseMD.FrameCount, cubeBaseMD.FrameSpeed, cubeBaseMD.AnimState);
            if (IsRandomBottom == true)
            {
                cube.AnimatorBase.CurrentFrame = rand.Next(0, cube.AnimatorBase.FrameCount);
            }
            else
            {
                cube.AnimatorBase.CurrentFrame = cubeBaseMD.CurrentFrame;
            }
            return cube;
        }

        public static Cube CreateCurrentCube(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateCube(mainGame, CurrentCubeMetaData, CurrentCubeBaseMetaData, coords, relativeCoords, altitude);
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

        public static void FindNextBase()
        {
            CurrentBaseIndex++;
            if (CurrentBaseIndex >= CubeBaseMetaDataLibrary.Count())
            {
                CurrentBaseIndex = 0;
            }
            CurrentCubeBaseMetaData = CubeBaseMetaDataLibrary[CurrentBaseIndex];
        }

    }

    /// <summary>
    /// Acts as a data store for the static class CubeFactory during serialization.
    /// </summary>
    public class CubeFactoryData
    {
        public CubeMetaData CurrentCubeMetaData { get; set; }
        public CubeBaseMetaData CurrentCubeBaseMetaData { get; set; }

        public bool IsRandomTop { get; set; }
        public bool IsRandomBottom { get; set; }

        public int CurrentIndex { get; set; }
        public int CurrentBaseIndex { get; set; }

        public XDictionary<int, CubeMetaData> CubeMetaDataLibrary { get; set;}
        public XDictionary<int, CubeBaseMetaData> CubeBaseMetaDataLibrary { get; set; }

        public CubeFactoryData()
        {
            SyncIn();
        }

        public void Serialize()
        {
            Serializer.WriteXML<CubeFactoryData>(this, "CubeFactoryData.xml", new Type[] { typeof(CubeMetaData), typeof(CubeBaseMetaData) });
        }

        public void SyncIn()
        {
            CurrentCubeMetaData = CubeFactory.CurrentCubeMetaData;
            CurrentCubeBaseMetaData = CubeFactory.CurrentCubeBaseMetaData;

            IsRandomTop = CubeFactory.IsRandomTop;
            IsRandomBottom = CubeFactory.IsRandomBottom;

            CurrentIndex = CubeFactory.CurrentIndex;
            CurrentBaseIndex = CubeFactory.CurrentBaseIndex;

            CubeMetaDataLibrary = CubeFactory.CubeMetaDataLibrary;
            CubeBaseMetaDataLibrary = CubeFactory.CubeBaseMetaDataLibrary;
        }

        public void SyncOut()
        {
            CubeFactory.CurrentCubeMetaData = CurrentCubeMetaData;
            CubeFactory.CurrentCubeBaseMetaData = CurrentCubeBaseMetaData;

            CubeFactory.IsRandomTop = IsRandomTop;
            CubeFactory.IsRandomBottom = IsRandomBottom;

            CubeFactory.CurrentIndex = CurrentIndex;
            CubeFactory.CurrentBaseIndex = CurrentBaseIndex;

            CubeFactory.CubeMetaDataLibrary = CubeMetaDataLibrary;
            CubeFactory.CubeBaseMetaDataLibrary = CubeBaseMetaDataLibrary;
        }  

    }

}
