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

    public enum BuildMode
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
        public Texture2D Texture;
        public string Path { get; set; }

        [XmlIgnore]
        public Texture2D AntiShadow;
        [XmlIgnore]
        public Texture2D SelfShadow;

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

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This can create garbage if called during runtime.
        /// </summary>
        public void LoadContent(MainGame mainGame)
        {
            Texture = mainGame.Content.Load<Texture2D>(Path);
            AntiShadow = GraphicsHelper.GetAntiShadow(mainGame, Texture);
            SelfShadow = GraphicsHelper.GetSelfShadow(mainGame, Texture);
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
        public static int CurrentIndex2x2 { get; set; } = 1;
        public static int CurrentIndex3x3 { get; set; } = 2;
        
        public static List<DecoMetaData> DecoMetaDataLibrary1x1 { get; set; }
        public static List<DecoMetaData> DecoMetaDataLibrary2x2 { get; set; }
        public static List<DecoMetaData> DecoMetaDataLibrary3x3 { get; set; }

        public static XDictionary<string, DecoMetaData> DecoMetaDataLibrary { get; set; }

        public static Deco CreateDeco(MainGame mainGame, DecoMetaData decoMD, Coord coords, Coord relativeCoords, int altitude)
        {
            var rand = new Random();
            Type type = Type.GetType(decoMD.TypeName);
            var deco = Activator.CreateInstance(type) as Deco;
            deco.Position = World.TopFace_CoordToLocalOrigin(coords);
            deco.PositionOffset = decoMD.PositionOffset;
            deco.Coords = relativeCoords;
            deco.Altitude = altitude;
            deco.AntiShadow = decoMD.AntiShadow;
            deco.SelfShadow = decoMD.SelfShadow;
            // Odd NxN takes priority over even NxN when coords along same horizontal line.
            deco.Dimensions = decoMD.Dimensions;
            if (deco.Dimensions.X % 2 == 0)
            {
                deco.DrawPriority = -1;
            }         

            // Create deco animator.
            var spriteSheet = SpriteSheet.CreateNew(decoMD.Texture, decoMD.Path, decoMD.SheetRows, decoMD.SheetColumns);
            deco.Animator = new Animator(deco, spriteSheet, decoMD.StartFrame, decoMD.CurrentFrame, decoMD.FrameCount, decoMD.FrameSpeed, decoMD.AnimState);

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

        public static int[] GetXYDimension(BuildMode buildDimension)
        {
            var enumType = typeof(BuildMode);
            var memberInfos = enumType.GetMember(buildDimension.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DimensionAttribute), false);
            return ((DimensionAttribute)valueAttributes[0]).XYDimension;
        }
        
        public static void FindNext(BuildMode buildMode)
        {
            if (buildMode == BuildMode._1x1)
            {
                FindNext1x1();
            }
            else if (buildMode == BuildMode._2x2)
            {
                FindNext2x2();
            }
            else if (buildMode == BuildMode._3x3)
            {
                FindNext3x3();
            }
        }

        public static void FindNext1x1()
        {
            CurrentIndex1x1++;
            if (CurrentIndex1x1 >= DecoMetaDataLibrary1x1.Count())
            {
                CurrentIndex1x1 = 0;
            }
            CurrentDecoMetaData1x1 = DecoMetaDataLibrary1x1[CurrentIndex1x1];
        }
        public static void FindNext2x2()
        {
            CurrentIndex2x2++;
            if (CurrentIndex2x2 >= DecoMetaDataLibrary2x2.Count())
            {
                CurrentIndex2x2 = 0;
            }
            CurrentDecoMetaData2x2 = DecoMetaDataLibrary2x2[CurrentIndex2x2];
        }
        public static void FindNext3x3()
        {
            CurrentIndex3x3++;
            if (CurrentIndex3x3 >= DecoMetaDataLibrary3x3.Count())
            {
                CurrentIndex3x3 = 0;
            }
            CurrentDecoMetaData3x3 = DecoMetaDataLibrary3x3[CurrentIndex3x3];
        }

        private static void FindPrevious(BuildMode buildMode)
        {
            if (buildMode == BuildMode._1x1)
            {
                FindPrevious1x1();
            }
            else if (buildMode == BuildMode._2x2)
            {
                FindPrevious2x2();
            }
            else if (buildMode == BuildMode._3x3)
            {
                FindPrevious3x3();
            }
        }

        public static void FindPrevious1x1()
        {
            CurrentIndex1x1--;
            if (CurrentIndex1x1 < 0)
            {
                CurrentIndex1x1 = DecoMetaDataLibrary1x1.Count() - 1;
            }
            CurrentDecoMetaData1x1 = DecoMetaDataLibrary1x1[CurrentIndex1x1];
        }
        public static void FindPrevious2x2()
        {
            CurrentIndex2x2--;
            if (CurrentIndex2x2 < 0)
            {
                CurrentIndex2x2 = DecoMetaDataLibrary2x2.Count() - 1;
            }
            CurrentDecoMetaData2x2 = DecoMetaDataLibrary2x2[CurrentIndex2x2];
        }
        public static void FindPrevious3x3()
        {
            CurrentIndex3x3--;
            if (CurrentIndex3x3 < 0)
            {
                CurrentIndex3x3 = DecoMetaDataLibrary3x3.Count() - 1;
            }
            CurrentDecoMetaData3x3 = DecoMetaDataLibrary3x3[CurrentIndex3x3];
        }    

    }

    /// <summary>
    /// Acts as a data store for the static class CubeFactory during serialization.
    /// </summary>
    [Serializable]
    public class DecoFactoryData
    {
        public static readonly XmlSerializer DecoFactoryDataSerializer = Serializer.CreateNew(typeof(DecoFactoryData), new Type[] { typeof(DecoMetaData) });

        public bool IsRandom { get; set; }

        public DecoMetaData CurrentDecoMetaData1x1 { get; set; }
        public DecoMetaData CurrentDecoMetaData2x2 { get; set; }
        public DecoMetaData CurrentDecoMetaData3x3 { get; set; }

        public int CurrentIndex1x1 { get; set; }
        public int CurrentIndex2x2 { get; set; }
        public int CurrentIndex3x3 { get; set; }

        public List<DecoMetaData> DecoMetaDataLibrary1x1 { get; set; }
        public List<DecoMetaData> DecoMetaDataLibrary2x2 { get; set; }
        public List<DecoMetaData> DecoMetaDataLibrary3x3 { get; set; }

        public DecoFactoryData()
        {
            SyncIn();
        }

        public void Serialize()
        {
            Serializer.WriteXML<DecoFactoryData>(DecoFactoryDataSerializer, this, "DecoFactoryData.xml");
        }

        /// <summary>
        /// Create a copy of DecoFactory's static properties;
        /// </summary>
        public void SyncIn()
        {
            // Copy static properties.
            IsRandom = DecoFactory.IsRandom;

            CurrentDecoMetaData1x1 = DecoFactory.CurrentDecoMetaData1x1;
            CurrentDecoMetaData2x2 = DecoFactory.CurrentDecoMetaData2x2;
            CurrentDecoMetaData3x3 = DecoFactory.CurrentDecoMetaData3x3;

            CurrentIndex1x1 = DecoFactory.CurrentIndex1x1;
            CurrentIndex2x2 = DecoFactory.CurrentIndex2x2;
            CurrentIndex3x3 = DecoFactory.CurrentIndex3x3;

            DecoMetaDataLibrary1x1 = DecoFactory.DecoMetaDataLibrary1x1;
            DecoMetaDataLibrary2x2 = DecoFactory.DecoMetaDataLibrary2x2;
            DecoMetaDataLibrary3x3 = DecoFactory.DecoMetaDataLibrary3x3;
        }

        /// <summary>
        /// Reassign values to DecoFactory's static properties;
        /// </summary>
        public void SyncOut(MainGame mainGame)
        {
            DecoFactory.IsRandom = IsRandom;

            DecoFactory.CurrentDecoMetaData1x1 = CurrentDecoMetaData1x1;
            DecoFactory.CurrentDecoMetaData2x2 = CurrentDecoMetaData2x2;
            DecoFactory.CurrentDecoMetaData3x3 = CurrentDecoMetaData3x3;

            DecoFactory.CurrentIndex1x1 = CurrentIndex1x1;
            DecoFactory.CurrentIndex2x2 = CurrentIndex2x2;
            DecoFactory.CurrentIndex3x3 = CurrentIndex3x3;

            DecoFactory.DecoMetaDataLibrary1x1 = DecoMetaDataLibrary1x1;
            DecoFactory.DecoMetaDataLibrary2x2 = DecoMetaDataLibrary2x2;
            DecoFactory.DecoMetaDataLibrary3x3 = DecoMetaDataLibrary3x3;

            // Generate Library Textures, AntiShadows, and SelfShadows from Path.
            // Populate master DecoMetaDataLibrary.
            DecoFactory.DecoMetaDataLibrary = new XDictionary<string, DecoMetaData>();
            foreach (var dmd in DecoFactory.DecoMetaDataLibrary1x1)
            {
                dmd.LoadContent(mainGame);
                if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                {
                    DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                }
            }
            foreach (var dmd in DecoFactory.DecoMetaDataLibrary2x2)
            {
                dmd.LoadContent(mainGame);
                if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                {
                    DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                }
            }
            foreach (var dmd in DecoFactory.DecoMetaDataLibrary3x3)
            {
                dmd.LoadContent(mainGame);
                if (!DecoFactory.DecoMetaDataLibrary.ContainsKey(dmd.Path))
                {
                    DecoFactory.DecoMetaDataLibrary.Add(dmd.Path, dmd);
                }
            }

            // Generate CurrentDecoMetaDataNxN Texture from Path.
            DecoFactory.CurrentDecoMetaData1x1.LoadContent(mainGame);
            DecoFactory.CurrentDecoMetaData2x2.LoadContent(mainGame);
            DecoFactory.CurrentDecoMetaData3x3.LoadContent(mainGame);
        }  

    }

}
