﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.Controllers;
using Sunbird.Serialization;
using Sunbird.GUI;
using Microsoft.Win32;
using System.Windows.Threading;

namespace Sunbird.External
{
    public enum Authorization
    {
        None,
        Builder
    }

    public class MapBuilder : State
    {
        public static readonly XmlSerializer MapBuilderSerializer = Serializer.CreateNew(typeof(MapBuilder));

        public XDictionary<int, SpriteList<Sprite>> LayerMap { get; set; } = new XDictionary<int, SpriteList<Sprite>>();
        public List<Sprite> Overlay { get; set; } = new List<Sprite>();
        public int Altitude { get; set; } = 0;
        public Player Player { get; set; }
        public GhostMarker GhostMarker { get; set; }
        public Cube CubePreview { get; set; }
        public Deco DecoPreview { get; set; }

        private bool IsLoading { get; set; }
        private bool InFocus { get; set; }
        public Authorization Authorization { get; set; }
        public BuildDimensions BuildDimensions { get; set; }
        private Sprite MessageLogBG { get; set; }

        private MapBuilder()
        {

        }

        public MapBuilder(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content) : base(mainGame, graphicsDevice, content)
        {
            if (mainGame.CleanLoad == true)
            {
                CreateContent();
            }
            else
            {
                new Thread(() => LoadContentFromFile()).Start();
            }
        }

        private void CreateOverlay()
        {
            CreateRibbon();
            CreateCubePendant();

            // The deco image. FIXME: should make this static property on DecoFactory?
            if (BuildDimensions == BuildDimensions._1x1)
            {
                DecoPreview = DecoFactory.CreateCurrentDeco1x1(MainGame, Coord.Zero, Coord.Zero, 0);
            }
            else if (BuildDimensions == BuildDimensions._2x2)
            {
                DecoPreview = DecoFactory.CreateCurrentDeco2x2(MainGame, Coord.Zero, Coord.Zero, 0);
            }
            else if (BuildDimensions == BuildDimensions._3x3)
            {
                DecoPreview = DecoFactory.CreateCurrentDeco3x3(MainGame, Coord.Zero, Coord.Zero, 0);
            }

            var mlBGs = SpriteSheet.CreateNew(MainGame, "Temp/MessageLogBackground");
            MessageLogBG = new Sprite(MainGame, mlBGs, new Vector2(5, MainGame.Height - 5), Alignment.BottomLeft);

            var gridAxisGlyph = SpriteSheet.CreateNew(MainGame, "Temp/GridAxisGlyph");
            Overlay.Add(new Sprite(MainGame, gridAxisGlyph, new Vector2(MainGame.Width - 5, 5), Alignment.TopRight));
        }

        #region Ribbon

        private void CreateRibbon()
        {
            // Ribbon background.
            var ribbonBg = SpriteSheet.CreateNew(MainGame, "Temp/RibbonBackGround");
            var ribbonPosition = new Vector2(5, 5);
            var _ribbonBg = new Sprite(MainGame, ribbonBg, ribbonPosition, Alignment.TopLeft);
            ribbonPosition = _ribbonBg.Position;
            Overlay.Add(_ribbonBg);

            var worldBNs = SpriteSheet.CreateNew(MainGame, "Buttons/WorldButtonSheet", 4, 3);
            var worldBN = new Button(MainGame, worldBNs, null, ribbonPosition + new Vector2(9, 9), Alignment.TopLeft) { PressedArgs = new AnimArgs(1, 10, 0.1f, AnimationState.Loop) };
            worldBN.Clicked += WorldBN_Clicked;

            if (Authorization == Authorization.None) { worldBN.IsPressed = true; }
            worldBN.OnUpdated = () =>
            {
                if (Peripherals.KeyTapped(Keys.Q))
                {
                    if (Authorization == Authorization.None) { worldBN.IsPressed = true; }
                    else if (Authorization == Authorization.Builder) { worldBN.IsPressed = false; }
                }
            };

            var buildBNs = SpriteSheet.CreateNew(MainGame, "Buttons/BuildButtonSheet", 4, 3);
            var buildBN = new Button(MainGame, buildBNs, null, ribbonPosition + new Vector2(75, 9), Alignment.TopLeft) { PressedArgs = new AnimArgs(1, 10, 0.1f, AnimationState.Loop) };
            buildBN.Clicked += BuildBN_Clicked;

            if (Authorization == Authorization.Builder) { buildBN.IsPressed = true; }
            buildBN.OnUpdated = () =>
            {
                if (Peripherals.KeyTapped(Keys.Q))
                {
                    if (Authorization == Authorization.Builder) { buildBN.IsPressed = true; }
                    else if (Authorization == Authorization.None) { buildBN.IsPressed = false; }
                }
            };

            Button.BindGroup(new List<Button>() { buildBN, worldBN });
            foreach (var button in new List<Button>() { buildBN, worldBN }) { Overlay.Add(button); }

            var buildCubeBNs = SpriteSheet.CreateNew(MainGame, "Buttons/BuildCubeBN", 1, 2);
            var buildCubeBN = new Button(MainGame, buildCubeBNs, null, ribbonPosition + new Vector2(141, 9), Alignment.TopLeft);
            buildCubeBN.Clicked += BuildCubeBN_Clicked;
            if (BuildDimensions == BuildDimensions._Cube) { buildCubeBN.IsPressed = true; }

            var build1x1BNs = SpriteSheet.CreateNew(MainGame, "Buttons/Build1x1BN", 1, 2);
            var build1x1BN = new Button(MainGame, build1x1BNs, null, ribbonPosition + new Vector2(183, 9), Alignment.TopLeft);
            build1x1BN.Clicked += Build1x1BN_Clicked;
            if (BuildDimensions == BuildDimensions._1x1) { build1x1BN.IsPressed = true; }

            var build2x2BNs = SpriteSheet.CreateNew(MainGame, "Buttons/Build2x2BN", 1, 2);
            var build2x2BN = new Button(MainGame, build2x2BNs, null, ribbonPosition + new Vector2(225, 9), Alignment.TopLeft);
            build2x2BN.Clicked += Build2x2BN_Clicked;
            if (BuildDimensions == BuildDimensions._2x2) { build2x2BN.IsPressed = true; }

            var build3x3BNs = SpriteSheet.CreateNew(MainGame, "Buttons/Build3x3BN", 1, 2);
            var build3x3BN = new Button(MainGame, build3x3BNs, null, ribbonPosition + new Vector2(267, 9), Alignment.TopLeft);
            build3x3BN.Clicked += Build3x3BN_Clicked;
            if (BuildDimensions == BuildDimensions._3x3) { build3x3BN.IsPressed = true; }

            Button.BindGroup(new List<Button>() { buildCubeBN, build1x1BN, build2x2BN, build3x3BN });
            foreach (var button in new List<Button>() { buildCubeBN, build1x1BN, build2x2BN, build3x3BN }) { Overlay.Add(button); }

            var openCatalogBNs = SpriteSheet.CreateNew(MainGame, "Buttons/OpenCatalogBN", 1, 2);

            var openCatalogCubeBN = new Button(MainGame, openCatalogBNs, null, ribbonPosition + new Vector2(141, 54), Alignment.TopLeft);
            var openCatalog1x1BN = new Button(MainGame, openCatalogBNs, null, ribbonPosition + new Vector2(183, 54), Alignment.TopLeft);
            var openCatalog2x2BN = new Button(MainGame, openCatalogBNs, null, ribbonPosition + new Vector2(225, 54), Alignment.TopLeft);
            var openCatalog3x3BN = new Button(MainGame, openCatalogBNs, null, ribbonPosition + new Vector2(267, 54), Alignment.TopLeft);

            Button.BindGroup(new List<Button>() { openCatalogCubeBN, openCatalog1x1BN, openCatalog2x2BN, openCatalog3x3BN });
            foreach (var button in new List<Button>() { openCatalogCubeBN, openCatalog1x1BN, openCatalog2x2BN, openCatalog3x3BN }) { Overlay.Add(button); }



        }

        #endregion

        #region Ribbon Event Handlers

        private void Build3x3BN_Clicked(object sender, ButtonClickedEventArgs e) 
        { 
            BuildDimensions = BuildDimensions._3x3;
            DecoPreview = DecoFactory.CreateCurrentDeco3x3(MainGame, Coord.Zero, Coord.Zero, 0);
            GhostMarker.MorphImage(DecoPreview, MainGame, GraphicsDevice, Content);
        }
        private void Build2x2BN_Clicked(object sender, ButtonClickedEventArgs e) 
        {
            BuildDimensions = BuildDimensions._2x2;
            DecoPreview = DecoFactory.CreateCurrentDeco2x2(MainGame, Coord.Zero, Coord.Zero, 0);
            GhostMarker.MorphImage(DecoPreview, MainGame, GraphicsDevice, Content);
        }
        private void Build1x1BN_Clicked(object sender, ButtonClickedEventArgs e) 
        { 
            BuildDimensions = BuildDimensions._1x1;
            DecoPreview = DecoFactory.CreateCurrentDeco1x1(MainGame, Coord.Zero, Coord.Zero, 0);
            GhostMarker.MorphImage(DecoPreview, MainGame, GraphicsDevice, Content);
        }
        private void BuildCubeBN_Clicked(object sender, ButtonClickedEventArgs e) 
        { 
            BuildDimensions = BuildDimensions._Cube;
            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
        }
        private void BuildBN_Clicked(object sender, ButtonClickedEventArgs e) { Authorization = Authorization.Builder; }
        private void WorldBN_Clicked(object sender, ButtonClickedEventArgs e) { Authorization = Authorization.None; }

        #endregion

        #region Cube Pendant

        private void CreateCubePendant()
        {
            // Pendant background.
            var pendantBg = SpriteSheet.CreateNew(MainGame, "Temp/PendantBackGround");
            var pendantPosition = new Vector2(MainGame.Width - 5, MainGame.Height - 5);
            var _pendantBg = new Sprite(MainGame, pendantBg, pendantPosition, Alignment.BottomRight);
            pendantPosition = _pendantBg.Position;
            Overlay.Add(_pendantBg);

            // The cube image. FIXME: should make this static property on CubeFactory?
            CubePreview = CubeFactory.CreateCurrentCube(MainGame, Coord.Zero, Coord.Zero, 0);
            CubePreview.Position = pendantPosition + new Vector2(57, 42);
            Overlay.Add(CubePreview);

            // Pendant left.
            var pLBN1s = SpriteSheet.CreateNew(MainGame, "Temp/LeftArrowMinusBN_Silver", 1, 2);
            var pLBN1 = new Button(MainGame, pLBN1s, null, pendantPosition + new Vector2(9, 15));
            pLBN1.Clicked += PLBN1_Clicked;
            Overlay.Add(pLBN1);

            var pLBN2s = SpriteSheet.CreateNew(MainGame, "Temp/LeftArrowMinusBN_Brown", 1, 2);
            var pLBN2 = new Button(MainGame, pLBN2s, null, pendantPosition + new Vector2(9, 51));
            pLBN2.Clicked += PLBN2_Clicked;
            Overlay.Add(pLBN2);

            var pLBN3s = SpriteSheet.CreateNew(MainGame, "Temp/LeftArrowMinusBN_Brown", 1, 2);
            var pLBN3 = new Button(MainGame, pLBN3s, null, pendantPosition + new Vector2(9, 81));
            pLBN3.Clicked += PLBN3_Clicked;
            Overlay.Add(pLBN3);

            var pLBN4s = SpriteSheet.CreateNew(MainGame, "Temp/LeftArrowMinusBN_Silver", 1, 2);
            var pLBN4 = new Button(MainGame, pLBN4s, null, pendantPosition + new Vector2(9, 117));
            pLBN4.Clicked += PLBN4_Clicked;
            Overlay.Add(pLBN4);

            // Pendant right.
            var pRBN1s = SpriteSheet.CreateNew(MainGame, "Temp/RightArrowPlusBN_Silver", 1, 2);
            var pRBN1 = new Button(MainGame, pRBN1s, null, pendantPosition + new Vector2(138, 15));
            pRBN1.Clicked += PRBN1_Clicked;
            Overlay.Add(pRBN1);

            var pRBN2s = SpriteSheet.CreateNew(MainGame, "Temp/RightArrowPlusBN_Brown", 1, 2);
            var pRBN2 = new Button(MainGame, pRBN2s, null, pendantPosition + new Vector2(138, 51));
            pRBN2.Clicked += PRBN2_Clicked;
            Overlay.Add(pRBN2);

            var pRBN3s = SpriteSheet.CreateNew(MainGame, "Temp/RightArrowPlusBN_Brown", 1, 2);
            var pRBN3 = new Button(MainGame, pRBN3s, null, pendantPosition + new Vector2(138, 81));
            pRBN3.Clicked += PRBN3_Clicked;
            Overlay.Add(pRBN3);

            var pRBN4s = SpriteSheet.CreateNew(MainGame, "Temp/RightArrowPlusBN_Silver", 1, 2);
            var pRBN4 = new Button(MainGame, pRBN4s, null, pendantPosition + new Vector2(138, 117));
            pRBN4.Clicked += PRBN4_Clicked;
            Overlay.Add(pRBN4);

            // Pendant random.
            var pRandTs = SpriteSheet.CreateNew(MainGame, "Temp/RandBN", 1, 2);
            var pRandT = new Button(MainGame, pRandTs, null, pendantPosition + new Vector2(57, 15)) { ButtonType = ButtonType.CheckBox };
            if (CubeFactory.IsRandomTop)
            {
                pRandT.IsPressed = true;
                pRandT.ReconfigureAnimator(pRandT.PressedArgs);
            }
            pRandT.Checked += PRandTop_Checked;
            pRandT.Unchecked += PRandTop_Unchecked;
            Overlay.Add(pRandT);

            var pRandBs = SpriteSheet.CreateNew(MainGame, "Temp/RandBN", 1, 2);
            var pRandB = new Button(MainGame, pRandBs, null, pendantPosition + new Vector2(57, 126)) { ButtonType = ButtonType.CheckBox };
            if (CubeFactory.IsRandomBottom)
            {
                pRandB.IsPressed = true;
                pRandB.ReconfigureAnimator(pRandB.PressedArgs);
            }
            pRandB.Checked += PRandB_Checked;
            pRandB.Unchecked += PRandB_Unchecked;
            Overlay.Add(pRandB);
        }

        #endregion

        #region Cube Pendant Event Handlers

        private void PRBN4_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeBaseMetaData.NextFrame();
            CubePreview.AnimatorBase.CurrentFrame = CubeFactory.CurrentCubeBaseMetaData.CurrentFrame;
            if (BuildDimensions == BuildDimensions._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PLBN4_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeBaseMetaData.PreviousFrame();
            CubePreview.AnimatorBase.CurrentFrame = CubeFactory.CurrentCubeBaseMetaData.CurrentFrame;
            if (BuildDimensions == BuildDimensions._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PRBN3_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindNextBase();
            var CCBMD = CubeFactory.CurrentCubeBaseMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCBMD.Path, CCBMD.SheetRows, CCBMD.SheetColumns), CubePreview.AnimatorBase);
            CubePreview.ReconfigureAnimator(CCBMD.StartFrame, CCBMD.CurrentFrame, CCBMD.FrameCount, CCBMD.FrameSpeed, CCBMD.AnimState, CubePreview.AnimatorBase);

            if (BuildDimensions == BuildDimensions._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PLBN3_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindPreviousBase();
            var CCBMD = CubeFactory.CurrentCubeBaseMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCBMD.Path, CCBMD.SheetRows, CCBMD.SheetColumns), CubePreview.AnimatorBase);
            CubePreview.ReconfigureAnimator(CCBMD.StartFrame, CCBMD.CurrentFrame, CCBMD.FrameCount, CCBMD.FrameSpeed, CCBMD.AnimState, CubePreview.AnimatorBase);

            if (BuildDimensions == BuildDimensions._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PRBN2_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindNext();
            var CCMD = CubeFactory.CurrentCubeMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCMD.Path, CCMD.SheetRows, CCMD.SheetColumns));
            CubePreview.ReconfigureAnimator(CCMD.StartFrame, CCMD.CurrentFrame, CCMD.FrameCount, CCMD.FrameSpeed, CCMD.AnimState);

            if (BuildDimensions == BuildDimensions._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PLBN2_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindPrevious();
            var CCMD = CubeFactory.CurrentCubeMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCMD.Path, CCMD.SheetRows, CCMD.SheetColumns));
            CubePreview.ReconfigureAnimator(CCMD.StartFrame, CCMD.CurrentFrame, CCMD.FrameCount, CCMD.FrameSpeed, CCMD.AnimState);

            if (BuildDimensions == BuildDimensions._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PRBN1_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeMetaData.NextFrame();
            CubePreview.Animator.CurrentFrame = CubeFactory.CurrentCubeMetaData.CurrentFrame;
            if (BuildDimensions == BuildDimensions._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PLBN1_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeMetaData.PreviousFrame();
            CubePreview.Animator.CurrentFrame = CubeFactory.CurrentCubeMetaData.CurrentFrame;
            if (BuildDimensions == BuildDimensions._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PRandB_Unchecked(object sender, ButtonClickedEventArgs e) { CubeFactory.IsRandomBottom = false; }
        private void PRandB_Checked(object sender, ButtonClickedEventArgs e) { CubeFactory.IsRandomBottom = true; }
        private void PRandTop_Unchecked(object sender, ButtonClickedEventArgs e) { CubeFactory.IsRandomTop = false; }
        private void PRandTop_Checked(object sender, ButtonClickedEventArgs e) { CubeFactory.IsRandomTop = true; }

        #endregion

        private void CreateContent()
        {
            LayerMap.Add(Altitude, new SpriteList<Sprite>());

            var playerSheet = SpriteSheet.CreateNew(MainGame, "Temp/PirateGirlSheet", 1, 16);
            var playerAnimArgs = new AnimArgs(0, 1, 0.2f, AnimationState.Loop);
            Player = new Player(MainGame, playerSheet, playerAnimArgs) { DrawPriority = 0 };
            Player.Light = Content.Load<Texture2D>("Temp/PlayerLight");
            Player.LightPath = "Temp/PlayerLight";
            LayerMap[Altitude].Add(Player);

            CreateOverlay();

            // GhostMarker relies on CubePreview so we must create it after the latter (which belongs to the overlay).
            GhostMarker = new GhostMarker(MainGame, SpriteSheet.CreateNew(MainGame, "Temp/TopFaceSelectionMarker")) { DrawPriority = 1 };
            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
            LayerMap[Altitude].Add(GhostMarker);

            Peripherals.ScrollWheelUp += Peripherals_ScrollWheelUp;
            Peripherals.ScrollWheelDown += Peripherals_ScrollWheelDown;
            MainGame.Exiting += MainGame_Exiting;
        }

        private void LoadContentFromFile()
        {
            IsLoading = true;

            MainGame.CurrentState = Templates.LoadingScreenTemplates[0].CreateLoadingScreen(MainGame, GraphicsDevice, Content) as State;
            var currentState = MainGame.CurrentState as ILoadingScreen;

            for (int i = 0; i < 25; i++)
            {
                Thread.Sleep(15);
                currentState.LoadingBar.Progress += 2;
            }

            // Most time is spent here...
            var XmlData = Serializer.ReadXML<MapBuilder>(MapBuilderSerializer, "MapBuilderSave.xml");

            Altitude = XmlData.Altitude;
            Authorization = XmlData.Authorization;
            BuildDimensions = XmlData.BuildDimensions;

            LayerMap = XmlData.LayerMap;
            foreach (var layer in LayerMap)
            {
                foreach (var sprite in layer.Value)
                {                
                    if (sprite is Player)
                    {
                        Player = sprite as Player;
                    }
                    else if (sprite is GhostMarker)
                    {
                        GhostMarker = sprite as GhostMarker;
                    }                  
                    sprite.LoadContent(MainGame, GraphicsDevice, Content);
                }
            }

            for (int i = 0; i < 25; i++)
            {
                Thread.Sleep(15);
                currentState.LoadingBar.Progress += 2;
            }

            CreateOverlay();  

            IsLoading = false;
            MainGame.CurrentState = this;

            Peripherals.ScrollWheelUp += Peripherals_ScrollWheelUp;
            Peripherals.ScrollWheelDown += Peripherals_ScrollWheelDown;
            MainGame.Exiting += MainGame_Exiting;
        }

        private void MainGame_Exiting(object sender, System.EventArgs e)
        {
            Serializer.WriteXML<MapBuilder>(MapBuilderSerializer, this, "MapBuilderSave.xml");
        }

        // Should these events be state specific? FIXME: handler must be detached manually when new currentstate assigned.
        private void Peripherals_ScrollWheelDown(object sender, EventArgs e)
        {
            if (MainGame.IsActive == true)
            {
                if (Peripherals.KeyPressed(Keys.LeftControl) && World.Zoom > 1)
                {
                    World.Zoom--;
                    World.ReconstructTopFaceArea();
                    if (MainGame.Camera.CurrentMode == CameraMode.Drag)
                    {
                        MainGame.Camera.DragTransform = MainGame.Camera.CreateDragTransform();
                    }
                }
                else if (!Peripherals.KeyPressed(Keys.LeftControl) && Authorization == Authorization.Builder)
                {
                    Altitude--;
                    if (LayerMap.ContainsKey(Altitude) == false)
                    {
                        LayerMap.Add(Altitude, new SpriteList<Sprite>());
                    }
                }
            }
            
        }

        private void Peripherals_ScrollWheelUp(object sender, EventArgs e)
        {
            if (MainGame.IsActive == true)
            {
                if (Peripherals.KeyPressed(Keys.LeftControl) && World.Zoom < 5)
                {
                    World.Zoom++;
                    World.ReconstructTopFaceArea();
                    if (MainGame.Camera.CurrentMode == CameraMode.Drag)
                    {
                        MainGame.Camera.DragTransform = MainGame.Camera.CreateDragTransform();
                    }                 
                }
                else if (!Peripherals.KeyPressed(Keys.LeftControl) && Authorization == Authorization.Builder)
                {
                    Altitude++;
                    if (LayerMap.ContainsKey(Altitude) == false)
                    {
                        LayerMap.Add(Altitude, new SpriteList<Sprite>());
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsLoading)
            {
                // Defined with respect to current mouse position.
                var relativeTopFaceCoords = World.TopFace_PointToRelativeCoord(MainGame.Camera, Altitude);
                var topFaceCoords = World.TopFace_PointToCoord(MainGame.Camera);

                // Check if cursor on Overlay sprite.
                foreach (var sprite in Overlay)
                {
                    if (sprite.Animator.WorldArea().Contains(Peripherals.GetMouseWindowPosition()))
                    {
                        InFocus = false;
                        break;
                    }
                    else
                    {
                        InFocus = true;
                    }
                }

                // User input actions.
                if (Peripherals.KeyTapped(Keys.Q))
                {
                    var i = (int)Authorization + 1;
                    if (i >= Enum.GetNames(typeof(Authorization)).Length) { i = 0; }
                    Authorization = (Authorization)(i);
                }

                if (Peripherals.KeyTapped(Keys.E))
                {
                    CurrentLightingColor = CurrentLightingColor == Color.Black ? Color.LightGray : Color.Black;
                }

                if (Peripherals.KeyTapped(Keys.R))
                {
                    if (BuildDimensions == BuildDimensions._1x1)
                    {
                        DecoFactory.FindNext1x1();
                        DecoPreview = DecoFactory.CreateCurrentDeco1x1(MainGame, Coord.Zero, Coord.Zero, 0);
                        GhostMarker.MorphImage(DecoPreview, MainGame, GraphicsDevice, Content);
                    }
                }

                if (Authorization == Authorization.Builder)
                {
                    if (Peripherals.LeftButtonPressed() && MainGame.IsActive && InFocus)
                    {
                        if (BuildDimensions == BuildDimensions._Cube)
                        {
                            var cube = CubeFactory.CreateCurrentCube(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                            LayerMap[Altitude].AddCheck(cube, Altitude);
                        }
                        else if (BuildDimensions == BuildDimensions._1x1)
                        {
                            var multiCube = DecoFactory.CreateCurrentDeco1x1(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                            LayerMap[Altitude].AddCheck(multiCube, Altitude);
                        }
                        else if (BuildDimensions == BuildDimensions._2x2)
                        {
                            var multiCube = DecoFactory.CreateCurrentDeco2x2(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                            LayerMap[Altitude].AddCheck(multiCube, Altitude);
                        }
                        else if (BuildDimensions == BuildDimensions._3x3)
                        {
                            var multiCube = DecoFactory.CreateCurrentDeco3x3(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                            LayerMap[Altitude].AddCheck(multiCube, Altitude);
                        }
                    }

                    if (Peripherals.RightButtonPressed() && MainGame.IsActive && InFocus)
                    {
                        if (BuildDimensions == BuildDimensions._Cube)
                        {
                            for (int i = 0; i < LayerMap[Altitude].Count(); i++)
                            {
                                var sprite = LayerMap[Altitude][i];
                                if (sprite is Cube && sprite.Coords == relativeTopFaceCoords)
                                {
                                    LayerMap[Altitude].RemoveCheck(sprite, Altitude); i--;
                                }
                            }
                        }
                        else if (BuildDimensions == BuildDimensions._1x1 || BuildDimensions == BuildDimensions._2x2 || BuildDimensions == BuildDimensions._3x3)
                        {
                            for (int i = 0; i < LayerMap[Altitude].Count(); i++)
                            {
                                var sprite = LayerMap[Altitude][i];
                                if (sprite is Deco)
                                {
                                    var mc = sprite as Deco;
                                    if (mc.OccupiedCoords[Altitude].Contains(relativeTopFaceCoords))
                                    {
                                        LayerMap[Altitude].RemoveCheck(mc, Altitude); i--;
                                    }
                                }
                            }
                        }
                    }
                }

                // Ghost marker management.
                if (Authorization == Authorization.Builder)
                {
                    GhostMarker.Altitude = Altitude;
                    GhostMarker.Coords = relativeTopFaceCoords;
                }
                else if (Authorization == Authorization.None)
                {
                    var l = LayerMap.Keys.ToList();
                    l.Sort();
                    l.Reverse();

                    foreach (var key in l)
                    {
                        var targetedCoord = World.GetRelativeCoord(topFaceCoords, key);
                        if (LayerMap[key].OccupiedCoords.Contains(targetedCoord))
                        {
                            Altitude = key;
                            GhostMarker.Altitude = Altitude;
                            break;
                        }
                    }
                    GhostMarker.Coords = World.TopFace_PointToRelativeCoord(MainGame.Camera, Altitude);
                }

                GhostMarker.Position = World.TopFace_CoordToLocalOrigin(topFaceCoords);

                if (LayerMap[Altitude].OccupiedCoords.Contains(relativeTopFaceCoords) || Authorization == Authorization.None)
                {
                    GhostMarker.DrawDefaultMarker = true;
                }
                else if (Authorization == Authorization.Builder)
                {
                    GhostMarker.DrawDefaultMarker = false;
                }

                if (LayerMap[Altitude].OccupiedCoords.Contains(relativeTopFaceCoords))
                {
                    GhostMarker.DrawPriority = 1;
                }
                else if (Authorization == Authorization.Builder)
                {
                    GhostMarker.DrawPriority = 0;
                }
                else if (Authorization == Authorization.None)
                {
                    GhostMarker.DrawPriority = -1000;
                }

                GhostMarker.IsHidden = !InFocus;

                // Player management.
                Player.Altitude = 1;

                // Rearrange sprites into their correct altitude layer.
                var altitudeList = LayerMap.Keys.ToList();
                altitudeList.Sort();

                foreach (var altitude in altitudeList)
                {
                    for (int i = 0; i < LayerMap[altitude].Count(); i++)
                    {
                        var sprite = LayerMap[altitude][i];
                        if (sprite.Altitude != altitude)
                        {
                            LayerMap[altitude].Remove(sprite); i--;
                            if (!LayerMap.ContainsKey(sprite.Altitude))
                            {
                                LayerMap.Add(sprite.Altitude, new SpriteList<Sprite>() { sprite });
                            }
                            else
                            {
                                if (!(sprite is IWorldObject))
                                {
                                    LayerMap[sprite.Altitude].Add(sprite);
                                }
                                else
                                {
                                    throw new NotImplementedException("Cube/Deco trying to move between layers, is this correct? Use AddCheck if so.");
                                }
                            }
                        }
                    }
                }

                // Update sorted sprites and report OnClicked event.
                Sprite clickedSprite = null;
                foreach (var sprite in World.Sort(LayerMap))
                {
                    sprite.Update(gameTime);
                    if (sprite.Animator.ScaledWorldArea().Contains(Peripherals.GetMouseWorldPosition(MainGame.Camera)) && Peripherals.LeftButtonTapped() && MainGame.IsActive && InFocus && !(sprite is GhostMarker))
                    {
                        clickedSprite = sprite;
                    }
                }
                if (clickedSprite != null)
                {
                    clickedSprite.OnClicked();
                }

                // Overlay update.
                foreach (var sprite in Overlay)
                {
                    sprite.Update(gameTime);
                }
#if DEBUG  
                foreach (var layer in LayerMap)
                {
                    var l = new HashSet<Coord>();
                    foreach (var sprite in layer.Value)
                    {
                        if (sprite is Cube)
                        {
                            l.Add(sprite.Coords);
                        }
                        else if (sprite is Deco)
                        {
                            var d = sprite as Deco;
                            foreach (var coord in d.OccupiedCoords[layer.Key])
                            {
                                l.Add(coord);
                            }
                        }
                    }
                    Debug.Assert(l.SetEquals(layer.Value.OccupiedCoords), "Occupied coords set != (occupied) coords of cubes and decos in sprite list, is this correct?");
                }
                Debug.Assert(GhostMarker.Altitude == Altitude);
#endif
            }
        }

        [Obsolete]
        private void Marker_KeyReleased(object sender, KeyReleasedEventArgs e)
        {
            if (e.Key == Keys.F)
            {
                LayerMap[Altitude].Remove(GhostMarker);
                GhostMarker.IsHidden = true;
                Peripherals.KeyReleased -= Marker_KeyReleased;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsLoading)
            {
                // Draw sorted sprites;
                foreach (var sprite in World.Sort(LayerMap))
                {
                    if (Altitude != sprite.Altitude && sprite is IWorldObject && Authorization == Authorization.Builder)
                    {
                        sprite.Alpha = 0.1f;
                        sprite.Draw(gameTime, spriteBatch);
                    }
                    else if ((Altitude == sprite.Altitude || Authorization == Authorization.None) && sprite is IWorldObject)
                    {
                        sprite.Alpha = 1f;
                        sprite.Draw(gameTime, spriteBatch);
                    }
                    else
                    {
                        sprite.Draw(gameTime, spriteBatch);
                    }
                }
            }
        }

        public override void DrawShadow(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsLoading)
            {
                // Create ascending column view of sprites keyed by coord.
                var ShadowDict = new Dictionary<Coord, List<Sprite>>();

                var AltitudeList = LayerMap.Keys.ToList();
                AltitudeList.Sort();

                foreach (var altitude in AltitudeList)
                {
                    foreach (var sprite in LayerMap[altitude])
                    {
                        if (ShadowDict.ContainsKey(sprite.Coords) == false)
                        {
                            ShadowDict.Add(sprite.Coords, new List<Sprite>() { });
                        }
                        else
                        {
                            ShadowDict[sprite.Coords].Add(sprite);
                        }
                    }
                }

                // Draw AntiShadows and Shadows of sorted sprites;
                foreach (var sprite in World.Sort(LayerMap))
                {
                    if (sprite.AntiShadow != null && !(sprite is GhostMarker))
                    {
                        if (sprite is Cube)
                        {
                            // Special case because number of frames can vary but AntiShadow remains the same.
                            spriteBatch.Draw(sprite.AntiShadow, sprite.Animator.Position, Color.White);
                        }
                        else
                        {
                            // Sprites here have AntiShadow generated automatically for entire sheet so use SheetViewArea() to retrieve view rectangle.
                            spriteBatch.Draw(sprite.AntiShadow, sprite.Animator.Position, sprite.Animator.SheetViewArea(), Color.White);
                        }
                    }
                    foreach (var higherSprite in ShadowDict[sprite.Coords])
                    {
                        if (higherSprite.Altitude > sprite.Altitude && higherSprite.Shadow != null && !(sprite is GhostMarker))
                        {
                            if (sprite is Cube)
                            {
                                // Special case, same reasoning as above.
                                spriteBatch.Draw(higherSprite.Shadow, sprite.Animator.Position, Color.White);

                            }
                            else
                            {
#if DEBUG
                                Debug.Assert(sprite.SelfShadow != null, "Is there a valid reason why SelfShadow can be null while the sprite belongs to the LayerMap?");
#endif
                                spriteBatch.Draw(sprite.SelfShadow, sprite.Animator.Position, sprite.Animator.SheetViewArea(), Color.White);
                            }
                        }
                    }
                }
            }
        }

        public override void DrawLighting(GameTime gameTime, SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(CurrentLightingColor);
            var AltitudeList = LayerMap.Keys.ToList();
            AltitudeList.Sort();

            foreach (var altitude in AltitudeList)
            {
                foreach (var sprite in LayerMap[altitude])
                {
                    if (sprite.Light != null)
                    {
                        spriteBatch.Draw(sprite.Light, sprite.Animator.Position + new Vector2(-180, -90), Color.White); // FIXME
                    }
                }
            }
        }

        public override void DrawLightingStencil(GameTime gameTime, SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(CurrentLightingColor);

            var AltitudeList = LayerMap.Keys.ToList();
            AltitudeList.Sort();

            foreach (var altitude in AltitudeList)
            {
                foreach (var sprite in LayerMap[altitude])
                {
                    if (sprite is Cube)
                    {
                        spriteBatch.Draw(sprite.AntiShadow, sprite.Animator.Position, Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(sprite.AntiShadow, sprite.Animator.Position, sprite.Animator.SheetViewArea(), Color.White);
                    }
                }
            }
        }

        public override void DrawOverlay(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var sprite in Overlay)
            {
                sprite.Draw(gameTime, spriteBatch);
            }

            MessageLogBG.Draw(gameTime, spriteBatch);

            spriteBatch.DrawString(MainGame.DefaultFont, $"Mouse World Position {Peripherals.GetMouseWorldPosition(MainGame.Camera).ToString() }", MessageLogBG.Position + new Vector2(15, 15), Color.White);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Mouse Coords {World.TopFace_PointToRelativeCoord(Peripherals.GetMouseWorldPosition(MainGame.Camera), Altitude) }", MessageLogBG.Position + new Vector2(15,35), Color.White);            
            spriteBatch.DrawString(MainGame.DefaultFont, $"Altitude: { Altitude.ToString() }", MessageLogBG.Position + new Vector2(15, 55), Color.White);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Player Position: { Player.Position.ToString() }", MessageLogBG.Position + new Vector2(15, 75), Color.White);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Player Coords: { Player.Coords.ToString() }", MessageLogBG.Position + new Vector2(15, 95), Color.White);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Authorization: { Authorization }", MessageLogBG.Position + new Vector2(15, 115), Color.White);

        }
    }
}
