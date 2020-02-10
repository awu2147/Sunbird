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
using Sunbird.Decorations;
using System.ComponentModel;

namespace Sunbird.Core
{
    public class DimensionAttribute : Attribute
    {
        public int[] XYDimension { get; set; }

        public DimensionAttribute(int[] dim)
        {
            XYDimension = dim;
        }
    }

    public enum BuildDimensions
    {
        _Cube,  
        [Dimension(new int[2] { 1, 1 })]
        _1x1,
        [Dimension(new int[2] { 2, 2 })]
        _2x2,
        [Dimension(new int[2] { 3, 3 })]
        _3x3,
    }

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
        [XmlIgnore]
        public Texture2D Texture { get; set; }

        [XmlIgnore]
        public Texture2D AntiShadow { get; set; }

        [XmlIgnore]
        public Texture2D SelfShadow { get; set; }

        public string Path { get; set; }
        public string TypeName { get; set; }
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

        public void LoadContent(MainGame mainGame)
        {
            Texture = mainGame.Content.Load<Texture2D>(Path);
            AntiShadow = GraphicsHelper.GetAntiShadow(mainGame, Texture);
            SelfShadow = GraphicsHelper.GetShadow(mainGame, Texture);
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
        public static DecoMetaData CurrentDecoMetaData1x1 { get; set; }
        public static DecoMetaData CurrentDecoMetaData2x2 { get; set; }
        public static DecoMetaData CurrentDecoMetaData3x3 { get; set; }

        public static bool IsRandom { get; set; }

        public static int CurrentIndex1x1 { get; set; } = 0;
        public static int CurrentIndex2x2 { get; set; } = 0;
        public static int CurrentIndex3x3 { get; set; } = 0;

        public static XDictionary<int, DecoMetaData> DecoMetaDataLibrary { get; set; }

        public static Deco CreateDeco(MainGame mainGame, DecoMetaData decoMD, Coord coords, Coord relativeCoords, int altitude)
        {
            Type type = Type.GetType(decoMD.TypeName);
            var deco = Activator.CreateInstance(type) as Deco;
            deco.Position = World.TopFace_CoordToLocalOrigin(coords);
            deco.PositionOffset = decoMD.PositionOffset;
            deco.Coords = relativeCoords;
            deco.Altitude = altitude;
            var rand = new Random();            

            // Create deco animator.
            var spriteSheet = SpriteSheet.CreateNew(decoMD.Texture, decoMD.Path, decoMD.SheetRows, decoMD.SheetColumns);
            deco.Animator = new Animator(deco, spriteSheet, decoMD.StartFrame, decoMD.CurrentFrame, decoMD.FrameCount, decoMD.FrameSpeed, decoMD.AnimState);
            deco.AntiShadow = decoMD.AntiShadow;
            deco.SelfShadow = decoMD.SelfShadow;
            //FIXME: memory leak here on the GetMask() method.
            //deco.GenerateShadowTextures(mainGame, deco.Animator);

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

        public static Deco CreateCurrentDeco1x1(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateDeco(mainGame, CurrentDecoMetaData1x1, coords, relativeCoords, altitude);
        }
        public static Deco CreateCurrentDeco2x2(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateDeco(mainGame, CurrentDecoMetaData2x2, coords, relativeCoords, altitude);
        }
        public static Deco CreateCurrentDeco3x3(MainGame mainGame, Coord coords, Coord relativeCoords, int altitude)
        {
            return CreateDeco(mainGame, CurrentDecoMetaData3x3, coords, relativeCoords, altitude);
        }

        public static int[] GetXYDimension(BuildDimensions buildDimension)
        {
            var enumType = typeof(BuildDimensions);
            var memberInfos = enumType.GetMember(buildDimension.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DimensionAttribute), false);
            return ((DimensionAttribute)valueAttributes[0]).XYDimension;
        }
        
        public static void FindNext1x1()
        {
           FindNext(CurrentIndex1x1, BuildDimensions._1x1);
        }
        public static void FindNext2x2()
        {
            FindNext(CurrentIndex2x2, BuildDimensions._2x2);
        }
        public static void FindNext3x3()
        {
            FindNext(CurrentIndex3x3, BuildDimensions._3x3);
        }

        private static void FindNext(int currentIndex, BuildDimensions dim)
        {
            var xyDim = GetXYDimension(dim);

            int startingIndex = currentIndex;

            while (true)
            {
                currentIndex++;
                if (currentIndex >= DecoMetaDataLibrary.Count())
                {
                    currentIndex = 0;
                }
                if (DecoMetaDataLibrary[currentIndex].Dimensions.X == xyDim[0] && DecoMetaDataLibrary[CurrentIndex1x1].Dimensions.Y == xyDim[1])
                {
                    // AFter finding the next valid index, set the corresponding CurrentDecoMetaData.
                    if (dim == BuildDimensions._1x1)
                    {
                        CurrentIndex1x1 = currentIndex;
                        CurrentDecoMetaData1x1 = DecoMetaDataLibrary[CurrentIndex1x1];
                    }
                    else if (dim == BuildDimensions._2x2)
                    {
                        CurrentIndex2x2 = currentIndex;
                        CurrentDecoMetaData2x2 = DecoMetaDataLibrary[CurrentIndex2x2];
                    }
                    else if (dim == BuildDimensions._3x3)
                    {
                        CurrentIndex3x3 = currentIndex;
                        CurrentDecoMetaData3x3 = DecoMetaDataLibrary[CurrentIndex3x3];
                    }
                    break;
                }
                // Break if gone through entire library and no matches (including the item we started with). Thus this shouldn't happen.
                if (currentIndex == startingIndex)
                {
                    throw new Exception("DecoFactory initialized incorrectly.");
                }
            }
        }

        private static void FindPrevious(BuildDimensions dim)
        {
            if (dim == BuildDimensions._1x1)
            {
                FindPrevious(CurrentIndex1x1, dim);
                CurrentDecoMetaData1x1 = DecoMetaDataLibrary[CurrentIndex1x1];
            }
            else if (dim == BuildDimensions._2x2)
            {
                FindPrevious(CurrentIndex2x2, dim);
                CurrentDecoMetaData2x2 = DecoMetaDataLibrary[CurrentIndex2x2];
            }
            else if (dim == BuildDimensions._3x3)
            {
                FindPrevious(CurrentIndex3x3, dim);
                CurrentDecoMetaData3x3 = DecoMetaDataLibrary[CurrentIndex3x3];
            }
        }

        public static void FindPrevious(int currentIndex, BuildDimensions dim)
        {
            var xyDim = GetXYDimension(dim);

            int startingIndex = currentIndex;

            while (true)
            {
                currentIndex--;
                if (currentIndex < 0)
                {
                    currentIndex = DecoMetaDataLibrary.Count() - 1;
                }
                if (DecoMetaDataLibrary[currentIndex].Dimensions.X == xyDim[0] && DecoMetaDataLibrary[CurrentIndex1x1].Dimensions.Y == xyDim[1])
                {
                    break;
                }
                // Break if gone through entire library and no matches (including the item we started with). Thus this shouldn't happen.
                if (currentIndex == startingIndex)
                {
                    throw new Exception("DecoFactory initialized incorrectly.");
                }
            }
        }

    }

    /// <summary>
    /// Acts as a data store for the static class CubeFactory during serialization.
    /// </summary>
    [Serializable]
    public class DecoFactoryData
    {
        public static readonly XmlSerializer DecoFactoryDataSerializer = Serializer.CreateNew(typeof(DecoFactoryData), new Type[] { typeof(DecoMetaData) });

        public DecoMetaData CurrentDecoMetaData1x1 { get; set; }
        public DecoMetaData CurrentDecoMetaData2x2 { get; set; }
        public DecoMetaData CurrentDecoMetaData3x3 { get; set; }

        public bool IsRandom { get; set; }

        public int CurrentIndex1x1 { get; set; }
        public int CurrentIndex2x2 { get; set; }
        public int CurrentIndex3x3 { get; set; }

        public XDictionary<int, DecoMetaData> DecoMetaDataLibrary { get; set;}

        public DecoFactoryData()
        {
            SyncIn();
        }

        public void Serialize()
        {
            Serializer.WriteXML<DecoFactoryData>(DecoFactoryDataSerializer, this, "DecoFactoryData.xml");
        }

        public void SyncIn()
        {
            CurrentDecoMetaData1x1 = DecoFactory.CurrentDecoMetaData1x1;
            CurrentDecoMetaData2x2 = DecoFactory.CurrentDecoMetaData2x2;
            CurrentDecoMetaData3x3 = DecoFactory.CurrentDecoMetaData3x3;

            IsRandom = DecoFactory.IsRandom;

            CurrentIndex1x1 = DecoFactory.CurrentIndex1x1;
            CurrentIndex2x2 = DecoFactory.CurrentIndex2x2;
            CurrentIndex3x3 = DecoFactory.CurrentIndex3x3;

            DecoMetaDataLibrary = DecoFactory.DecoMetaDataLibrary;
        }

        public void SyncOut(MainGame mainGame)
        {
            CurrentDecoMetaData1x1.LoadContent(mainGame);
            CurrentDecoMetaData2x2.LoadContent(mainGame);
            CurrentDecoMetaData3x3.LoadContent(mainGame);
            foreach (var dMD in DecoMetaDataLibrary)
            {
                dMD.Value.LoadContent(mainGame);
            }

            DecoFactory.CurrentDecoMetaData1x1 = CurrentDecoMetaData1x1;
            DecoFactory.CurrentDecoMetaData2x2 = CurrentDecoMetaData2x2;
            DecoFactory.CurrentDecoMetaData3x3 = CurrentDecoMetaData3x3;

            DecoFactory.IsRandom = IsRandom;

            DecoFactory.CurrentIndex1x1 = CurrentIndex1x1;
            DecoFactory.CurrentIndex2x2 = CurrentIndex2x2;
            DecoFactory.CurrentIndex3x3 = CurrentIndex3x3;

            DecoFactory.DecoMetaDataLibrary = DecoMetaDataLibrary;
        }  

    }

}
