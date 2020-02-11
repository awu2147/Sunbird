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
    /// <summary>
    /// A single unit building block.
    /// </summary>
    [Serializable]
    public class Cube : Sprite, IWorldObject
    {
        public Animator AnimatorBase { get; set; }

        public Cube() { }

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This can create garbage if called during runtime.
        /// </summary>
        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Animator.LoadContent(mainGame, graphicsDevice, content);
            Animator.Owner = this;
            AnimatorBase.LoadContent(mainGame, graphicsDevice, content);
            AnimatorBase.Owner = this;
#if DEBUG
            Debug.Assert(ShadowPath != null);
            Debug.Assert(AntiShadowPath != null);
#endif
            Shadow = content.Load<Texture2D>(ShadowPath);
            AntiShadow = content.Load<Texture2D>(AntiShadowPath);
            if (LightPath != null) { Light = content.Load<Texture2D>(LightPath); }
        }

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This is safe to call during runtime.
        /// </summary>
        public override void SafeLoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            LoadContent(mainGame, graphicsDevice, content);
        }

        public override void Update(GameTime gameTime)
        {
            Animator.Update(gameTime);
            AnimatorBase.Update(gameTime);
#if DEBUG
            Debug.Assert(Animator.Position == Position);
            Debug.Assert(AnimatorBase.Position == Position);
#endif
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

    [Serializable]
    public class CubeMetaData
    {
        [XmlIgnore]
        public Texture2D Texture;
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

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This can create garbage if called during runtime.
        /// </summary>
        public void LoadContent(MainGame mainGame)
        {
            Texture = mainGame.Content.Load<Texture2D>(Path);
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

    [Serializable]
    public class CubeBaseMetaData
    {
        [XmlIgnore]
        public Texture2D Texture;
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

        /// <summary>
        /// Core method used to re-instantiate non-serializable properties and delegates. This can create garbage if called during runtime.
        /// </summary>
        public void LoadContent(MainGame mainGame)
        {
            Texture = mainGame.Content.Load<Texture2D>(Path);
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

    public static class CubeFactory
    {
        public static CubeMetaData CurrentCubeMetaData { get; set; }
        public static CubeBaseMetaData CurrentCubeBaseMetaData { get; set; }

        public static bool IsRandomTop { get; set; }
        public static bool IsRandomBottom { get; set; }

        public static int CurrentIndex { get; set; } 
        public static int CurrentBaseIndex { get; set; } 

        public static XDictionary<int, CubeMetaData> CubeMetaDataLibrary { get; set; }
        public static XDictionary<int, CubeBaseMetaData> CubeBaseMetaDataLibrary { get; set; }

        public static Cube CreateCube(MainGame mainGame, CubeMetaData cubeMD, CubeBaseMetaData cubeBaseMD, Coord coords, Coord relativeCoords, int altitude)
        {              
            var cube = new Cube() { Position = World.TopFace_CoordToLocalOrigin(coords), Coords = relativeCoords, Altitude = altitude };
            var rand = new Random();

            // Create cube top animator.
            var spriteSheet = SpriteSheet.CreateNew(cubeMD.Texture, cubeMD.Path, cubeMD.SheetRows, cubeMD.SheetColumns);
            cube.Animator = new Animator(cube, spriteSheet, cubeMD.StartFrame, cubeMD.CurrentFrame, cubeMD.FrameCount, cubeMD.FrameSpeed, cubeMD.AnimState);
            if (IsRandomTop == true && cubeMD.AnimState == AnimationState.None)
            {
                cube.Animator.CurrentFrame = rand.Next(0, cube.Animator.FramesInLoop);
            }
            else
            {
                cube.Animator.CurrentFrame = cubeMD.CurrentFrame;
            }

            // Create cube base animator.
            var spriteSheetBase = SpriteSheet.CreateNew(cubeBaseMD.Texture, cubeBaseMD.Path, cubeBaseMD.SheetRows, cubeBaseMD.SheetColumns);
            cube.AnimatorBase = new Animator(cube, spriteSheetBase, cubeBaseMD.StartFrame, cubeBaseMD.CurrentFrame, cubeBaseMD.FrameCount, cubeBaseMD.FrameSpeed, cubeBaseMD.AnimState);
            if (IsRandomBottom == true && cubeBaseMD.AnimState == AnimationState.None)
            {
                cube.AnimatorBase.CurrentFrame = rand.Next(0, cube.AnimatorBase.FramesInLoop);
            }
            else
            {
                cube.AnimatorBase.CurrentFrame = cubeBaseMD.CurrentFrame;
            }

            // This actually prevents memory leak vs CreateMask() since content manager knows to reuse same texture. Should all cubes have the same shadow and antishadow?
            cube.Shadow = mainGame.Content.Load<Texture2D>("Temp/CubeShadow");
            cube.ShadowPath = "Temp/CubeShadow";
            cube.AntiShadow = mainGame.Content.Load<Texture2D>("Temp/CubeAntiShadow");
            cube.AntiShadowPath = "Temp/CubeAntiShadow";

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

        public static void FindPrevious()
        {
            CurrentIndex--;
            if (CurrentIndex < 0)
            {
                CurrentIndex = CubeMetaDataLibrary.Count() - 1;
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

        public static void FindPreviousBase()
        {
            CurrentBaseIndex--;
            if (CurrentBaseIndex < 0)
            {
                CurrentBaseIndex = CubeBaseMetaDataLibrary.Count() - 1;
            }
            CurrentCubeBaseMetaData = CubeBaseMetaDataLibrary[CurrentBaseIndex];
        }

    }

    /// <summary>
    /// Acts as a data store for the static class CubeFactory during serialization.
    /// </summary>
    [Serializable]
    public class CubeFactoryData
    {
        public static readonly XmlSerializer CubeFactoryDataSerializer = Serializer.CreateNew(typeof(CubeFactoryData), new Type[] { typeof(CubeMetaData), typeof(CubeBaseMetaData) });

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
            Serializer.WriteXML<CubeFactoryData>(CubeFactoryDataSerializer, this, "CubeFactoryData.xml");
        }

        /// <summary>
        /// Create a copy of CubeFactory's static properties;
        /// </summary>
        public void SyncIn()
        {
            // Copy static properties.
            CurrentCubeMetaData = CubeFactory.CurrentCubeMetaData;
            CurrentCubeBaseMetaData = CubeFactory.CurrentCubeBaseMetaData;

            IsRandomTop = CubeFactory.IsRandomTop;
            IsRandomBottom = CubeFactory.IsRandomBottom;

            CurrentIndex = CubeFactory.CurrentIndex;
            CurrentBaseIndex = CubeFactory.CurrentBaseIndex;

            CubeMetaDataLibrary = CubeFactory.CubeMetaDataLibrary;
            CubeBaseMetaDataLibrary = CubeFactory.CubeBaseMetaDataLibrary;
        }

        /// <summary>
        /// Reassign values to CubeFactory's static properties;
        /// </summary>
        public void SyncOut(MainGame mainGame)
        {
            // Generate CurrentCubeMetaData Texture from Path.
            CurrentCubeMetaData.LoadContent(mainGame);
            // Generate Library Textures from Path.
            foreach (var cMD in CubeMetaDataLibrary)
            {
                cMD.Value.LoadContent(mainGame);
            }
            // Generate CurrentCubeBaseMetaData Texture from Path.
            CurrentCubeBaseMetaData.LoadContent(mainGame);
            // Generate Library Textures from Path.
            foreach (var cbMD in CubeBaseMetaDataLibrary)
            {
                cbMD.Value.LoadContent(mainGame);
            }

            // Reassign static properties.
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
