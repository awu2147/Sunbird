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
    public class Deco : Sprite, IWorldObject
    {
        public XDictionary<int, HashSet<Coord>> OccupiedCoords { get; set; } = new XDictionary<int, HashSet<Coord>>();
        public Dimension Dimensions { get; set; }

        public Deco() { }

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
    public class DecoMetaData
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

        public DecoMetaData()
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

    public static class DecoFactory
    {
        public static DecoMetaData CurrentDecoMetaData { get; set; }

        public static bool IsRandom { get; set; }

        public static int CurrentIndex { get; set; } = 0;

        public static XDictionary<int, DecoMetaData> DecoMetaDataLibrary { get; set; }

        public static Deco CreateDeco(MainGame mainGame, DecoMetaData decoMD, Coord coords, Coord relativeCoords, int altitude)
        {              
            var deco = new Deco() { Position = World.TopFace_CoordToLocalOrigin(coords), PositionOffset = decoMD.PositionOffset, Coords = relativeCoords, Altitude = altitude};
            var rand = new Random();

            // Create multicube animator.
            var spriteSheet = SpriteSheet.CreateNew(mainGame, decoMD.Path, decoMD.SheetRows, decoMD.SheetColumns);
            deco.Animator = new Animator(deco, spriteSheet, decoMD.StartFrame, decoMD.CurrentFrame, decoMD.FrameCount, decoMD.FrameSpeed, decoMD.AnimState);
            deco.GenerateShadowTextures(mainGame, deco.Animator);

            for (int k = 0; k < decoMD.Dimensions.Z; k++)
            {
                deco.OccupiedCoords.Add(altitude + k, new HashSet<Coord>() { });
                for (int i = 0; i < decoMD.Dimensions.X; i++)
                {
                    for (int j = 0; j < decoMD.Dimensions.Y; j++)
                    {
#if DEBUG
                        Debug.Assert(decoMD.Dimensions.X == decoMD.Dimensions.Y);
#endif
                        int offset = decoMD.Dimensions.Y / 2;
                        deco.OccupiedCoords[altitude + k].Add(deco.Coords + new Coord(i - offset, -j + offset));
                    }
                }
            }

            if (IsRandom == true && decoMD.AnimState == AnimationState.None)
            {
                deco.Animator.CurrentFrame = rand.Next(0, deco.Animator.FramesInLoop);
            }
            else
            {
                deco.Animator.CurrentFrame = decoMD.CurrentFrame;
            }

            return deco;
        }

        public static Deco CreateCurrentDeco(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateDeco(mainGame, CurrentDecoMetaData, coords, relativeCoords, altitude);
        }

        public static void FindNext()
        {
            CurrentIndex++;
            if (CurrentIndex >= DecoMetaDataLibrary.Count())
            {
                CurrentIndex = 0;
            }
            CurrentDecoMetaData = DecoMetaDataLibrary[CurrentIndex];
        }

        public static void FindPrevious()
        {
            CurrentIndex--;
            if (CurrentIndex < 0)
            {
                CurrentIndex = DecoMetaDataLibrary.Count() - 1;
            }
            CurrentDecoMetaData = DecoMetaDataLibrary[CurrentIndex];
        }

    }

    /// <summary>
    /// Acts as a data store for the static class CubeFactory during serialization.
    /// </summary>
    [Serializable]
    public class DecoFactoryData
    {
        public DecoMetaData CurrentDecoMetaData { get; set; }

        public bool IsRandom { get; set; }

        public int CurrentIndex { get; set; }

        public XDictionary<int, DecoMetaData> DecoMetaDataLibrary { get; set;}

        public DecoFactoryData()
        {
            SyncIn();
        }

        public void Serialize()
        {
            Serializer.WriteXML<DecoFactoryData>(this, "DecoFactoryData.xml", new Type[] { typeof(DecoMetaData) });
        }

        public void SyncIn()
        {
            CurrentDecoMetaData = DecoFactory.CurrentDecoMetaData;

            IsRandom = DecoFactory.IsRandom;

            CurrentIndex = DecoFactory.CurrentIndex;

            DecoMetaDataLibrary = DecoFactory.DecoMetaDataLibrary;
        }

        public void SyncOut()
        {
            DecoFactory.CurrentDecoMetaData = CurrentDecoMetaData;

            DecoFactory.IsRandom = IsRandom;

            DecoFactory.CurrentIndex = CurrentIndex;

            DecoFactory.DecoMetaDataLibrary = DecoMetaDataLibrary;
        }  

    }

}
