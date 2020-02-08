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
    public class MultiCube : Sprite, ICube
    {
        public XDictionary<int, HashSet<Coord>> OccupiedCoords { get; set; } = new XDictionary<int, HashSet<Coord>>();

        public List<Cube> Children { get; set; }

        public Dimension Dimensions { get; set; }

        public MultiCube() { }

        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            base.LoadContent(mainGame, graphicsDevice, content);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }

    }

    [Serializable]
    public class MultiCubeMetaData
    {
        public string Path { get; set; }
        public Vector2 PositionOffset { get; set; }
        public int SheetRows { get; set; } = 1;
        public int SheetColumns { get; set; } = 1;
        public int StartFrame { get; set; } = 0;
        public int CurrentFrame { get; set; } = 0;
        public int FrameCount { get; set; } = 1;
        public float FrameSpeed { get; set; } = 0.133f;
        public AnimationState AnimState { get; set; } = AnimationState.None;
        public Dimension Dimensions { get; set; }

        public MultiCubeMetaData()
        {

        }

        public void NextFrame()
        {
            if (AnimState == AnimationState.None)
            {
                CurrentFrame++;
                if (CurrentFrame >= FrameCount)
                {
                    CurrentFrame = 0;
                }
            }
        }

        public void PreviousFrame()
        {
            if (AnimState == AnimationState.None)
            {
                CurrentFrame--;
                if (CurrentFrame < 0)
                {
                    CurrentFrame = FrameCount - 1;
                }
            }
        }
    } 

    public static class MultiCubeFactory
    {
        public static MultiCubeMetaData CurrentMultiCubeMetaData { get; set; }

        public static bool IsRandom { get; set; }

        public static int CurrentIndex { get; set; } = 0;

        public static XDictionary<int, MultiCubeMetaData> MultiCubeMetaDataLibrary { get; set; }

        public static MultiCube CreateMultiCube(MainGame mainGame, MultiCubeMetaData McubeMD, Coord coords, Coord relativeCoords, int altitude)
        {              
            var multiCube = new MultiCube() { Position = World.TopFace_CoordToLocalOrigin(coords), PositionOffset = McubeMD.PositionOffset, Coords = relativeCoords, Altitude = altitude};
            var rand = new Random();

            // Create multicube animator.
            var spriteSheet = SpriteSheet.CreateNew(mainGame, McubeMD.Path, McubeMD.SheetRows, McubeMD.SheetColumns);
            multiCube.Animator = new Animator(multiCube, spriteSheet, McubeMD.StartFrame, McubeMD.CurrentFrame, McubeMD.FrameCount, McubeMD.FrameSpeed, McubeMD.AnimState);
            multiCube.GenerateShadowTextures(mainGame, multiCube.Animator);

            for (int k = 0; k < McubeMD.Dimensions.Z; k++)
            {
                multiCube.OccupiedCoords.Add(altitude + k, new HashSet<Coord>() { });
                for (int i = 0; i < McubeMD.Dimensions.X; i++)
                {
                    for (int j = 0; j < McubeMD.Dimensions.Y; j++)
                    {
                        multiCube.OccupiedCoords[altitude + k].Add(multiCube.Coords + new Coord(i - 1, -j + 1));
                    }
                }
            }

            if (IsRandom == true && McubeMD.AnimState == AnimationState.None)
            {
                multiCube.Animator.CurrentFrame = rand.Next(0, multiCube.Animator.FramesInLoop);
            }
            else
            {
                multiCube.Animator.CurrentFrame = McubeMD.CurrentFrame;
            }

            // Should all cubes have the same shadow and antishadow?
            //cube.Shadow = mainGame.Content.Load<Texture2D>("Temp/CubeShadow");
            //cube.ShadowPath = "Temp/CubeShadow";
            //cube.AntiShadow = mainGame.Content.Load<Texture2D>("Temp/CubeAntiShadow");
            //cube.AntiShadowPath = "Temp/CubeAntiShadow";

            return multiCube;
        }

        public static MultiCube CreateCurrentMultiCube(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateMultiCube(mainGame, CurrentMultiCubeMetaData, coords, relativeCoords, altitude);
        }

        public static void FindNext()
        {
            CurrentIndex++;
            if (CurrentIndex >= MultiCubeMetaDataLibrary.Count())
            {
                CurrentIndex = 0;
            }
            CurrentMultiCubeMetaData = MultiCubeMetaDataLibrary[CurrentIndex];
        }

        public static void FindPrevious()
        {
            CurrentIndex--;
            if (CurrentIndex < 0)
            {
                CurrentIndex = MultiCubeMetaDataLibrary.Count() - 1;
            }
            CurrentMultiCubeMetaData = MultiCubeMetaDataLibrary[CurrentIndex];
        }

    }

    /// <summary>
    /// Acts as a data store for the static class CubeFactory during serialization.
    /// </summary>
    [Serializable]
    public class MultiCubeFactoryData
    {
        public MultiCubeMetaData CurrentMultiCubeMetaData { get; set; }

        public bool IsRandom { get; set; }

        public int CurrentIndex { get; set; }

        public XDictionary<int, MultiCubeMetaData> MultiCubeMetaDataLibrary { get; set;}

        public MultiCubeFactoryData()
        {
            SyncIn();
        }

        public void Serialize()
        {
            Serializer.WriteXML<CubeFactoryData>(this, "CubeFactoryData.xml", new Type[] { typeof(CubeMetaData), typeof(CubeBaseMetaData) });
        }

        public void SyncIn()
        {
            CurrentMultiCubeMetaData = MultiCubeFactory.CurrentMultiCubeMetaData;

            IsRandom = MultiCubeFactory.IsRandom;

            CurrentIndex = MultiCubeFactory.CurrentIndex;

            MultiCubeMetaDataLibrary = MultiCubeFactory.MultiCubeMetaDataLibrary;
        }

        public void SyncOut()
        {
            MultiCubeFactory.CurrentMultiCubeMetaData = CurrentMultiCubeMetaData;

            MultiCubeFactory.IsRandom = IsRandom;

            MultiCubeFactory.CurrentIndex = CurrentIndex;

            MultiCubeFactory.MultiCubeMetaDataLibrary = MultiCubeMetaDataLibrary;
        }  

    }

}
