using System;
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
using System.Collections;

namespace Sunbird.External
{
    public class MapBuilder : State, IGui
    {
        public static readonly XmlSerializer MapBuilderSerializer = Serializer.CreateNew(typeof(MapBuilder));

        private Sprite MessageLogBG;

        private readonly string SaveFilePath;

        /// <summary>
        /// For optimization reasons, create and assign value to this field during an update() loop so it is ready to be used in the draw() loop.
        /// </summary>
        private Dictionary<Coord, List<Sprite>> ShadowDict = new Dictionary<Coord, List<Sprite>>();

        public XDictionary<int, SpriteList<Sprite>> LayerMap { get; set; } = new XDictionary<int, SpriteList<Sprite>>();

        [XmlIgnore]
        public List<Sprite> Overlay { get; set; } = new List<Sprite>();

        /// <summary>
        /// When adding or removing sprites from the Overlay during runtime, move them here first. 
        /// This ensures that addition or removal occurs before enumeration over Overlay begins.
        /// </summary>
        [XmlIgnore]
        public List<KeyValuePair<Sprite, DeferAction>> DeferredOverlay { get; set; } = new List<KeyValuePair<Sprite, DeferAction>>();

        public bool InFocus;
        public bool IsLoading { get; set; }
        public int Altitude { get; set; }
        public Player Player { get; set; }
        public GhostMarker GhostMarker { get; set; }
        public Cube CubePreview { get; set; }
        public Deco DecoPreview { get; set; }
        public Authorization Authorization { get; set; }
        public BuildMode BuildMode { get; set; } = BuildMode._Cube;

        public static string ClickedSpriteName = string.Empty;

        private BuilderRibbon BuilderRibbon;

        private WaterShader WaterShader;

        private MapBuilder()
        {

        }

        public MapBuilder(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content, string path) : base(mainGame, graphicsDevice, content, path)
        {
            SaveFilePath = path;
            StateChanged += MapBuilder_StateChanged;
            if (mainGame.CleanLoad == true)
            {
                Task.Run(() => CreateContent());
            }
            else
            {      
                Task.Run(() => LoadContentFromFile());
            }
        }

        private void MapBuilder_StateChanged(object sender, EventArgs e)
        {
            // Clean up any event handlers subscribed to static events.
            Peripherals.ScrollWheelUp -= Peripherals_ScrollWheelUp;
            Peripherals.ScrollWheelDown -= Peripherals_ScrollWheelDown;
        }

        private void CreateContent()
        {
            IsLoading = true;

            MainGame.CurrentState = Templates.LoadingScreenTemplates[0].CreateLoadingScreen(MainGame, GraphicsDevice, Content) as State;
            var currentState = MainGame.CurrentState as ILoadingScreen;

            currentState.LoadingBar.Progress += 20;

            WaterShader = new WaterShader(MainGame);

            currentState.LoadingBar.Progress += 30;

            // Create first layer at 0 Altitude.
            LayerMap.Add(Altitude, new SpriteList<Sprite>());

            // Create the player and add to this layer.
            var playerSheet = SpriteSheet.CreateNew(MainGame, "Temp/PirateGirlSheet", 1, 16);
            var playerAnimArgs = new AnimArgs(0, 1, 0.2f, AnimationState.Loop);
            Player = new Player(MainGame, playerSheet, playerAnimArgs)
            {
                Light = Content.Load<Texture2D>("Temp/PlayerLight"),
                LightPath = "Temp/PlayerLight"
            };
            // Should this be Add or AddCheck?
            LayerMap[Altitude].Add(Player);

            currentState.LoadingBar.Progress += 20;

            CreateOverlay();

            // Instantiated BuildMode is _Cube. GhostMarker needs CubePreview to exist to morph so we must create it after the latter (which belongs to the overlay).
            GhostMarker = new GhostMarker(MainGame, SpriteSheet.CreateNew(MainGame, "Temp/TopFaceSelectionMarker")) { DrawPriority = 1 };
            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
            LayerMap[Altitude].Add(GhostMarker);

            currentState.LoadingBar.Progress += 30;

            IsLoading = false;
            MainGame.CurrentState = this;

            Peripherals.ScrollWheelUp += Peripherals_ScrollWheelUp;
            Peripherals.ScrollWheelDown += Peripherals_ScrollWheelDown;
            MainGame.Exiting += MainGame_Exiting;
        }

        private void LoadContentFromFile()
        {
            IsLoading = true;

            MainGame.CurrentState = Templates.LoadingScreenTemplates[0].CreateLoadingScreen(MainGame, GraphicsDevice, Content) as State;
            var currentState = MainGame.CurrentState as ILoadingScreen;

            currentState.LoadingBar.Progress += 20;

            WaterShader = new WaterShader(MainGame);

            currentState.LoadingBar.Progress += 20;

            // Most time is spent here...
            var XmlData = Serializer.ReadXML<MapBuilder>(MapBuilderSerializer, SaveFilePath);

            Altitude = XmlData.Altitude;
            Authorization = XmlData.Authorization;
            BuildMode = XmlData.BuildMode;

            currentState.LoadingBar.Progress += 20;

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

            currentState.LoadingBar.Progress += 20;

            CreateOverlay();

            currentState.LoadingBar.Progress += 20;

            IsLoading = false;
            MainGame.CurrentState = this;

            Peripherals.ScrollWheelUp += Peripherals_ScrollWheelUp;
            Peripherals.ScrollWheelDown += Peripherals_ScrollWheelDown;
            MainGame.Exiting += MainGame_Exiting;
        }

        private void CreateOverlay()
        {
            // Create ribbon.
            BuilderRibbon = new BuilderRibbon(MainGame, this, new Vector2(3, 3));
            BuilderRibbon.LoadContent();
            Overlay.Add(BuilderRibbon);

            // FIXME: give this the ribbon treatment >:)
            CreateCubePendant();

            var mlBGs = SpriteSheet.CreateNew(MainGame, "GUI/MessageLogBackground");
            MessageLogBG = new Sprite(MainGame, mlBGs, new Vector2(3, MainGame.Height - 3), Alignment.BottomLeft);

            var gridAxisGlyph = SpriteSheet.CreateNew(MainGame, "Temp/GridAxisGlyph");
            Overlay.Add(new Sprite(MainGame, gridAxisGlyph, new Vector2(MainGame.Width - 3, 3), Alignment.TopRight));
        }

        #region Cube Pendant

        private void CreateCubePendant()
        {
            // Pendant background.
            var pendantBg = SpriteSheet.CreateNew(MainGame, "GUI/PendantBackGround");
            var pendantPosition = new Vector2(MainGame.Width - 3, MainGame.Height - 3);
            var _pendantBg = new Sprite(MainGame, pendantBg, pendantPosition, Alignment.BottomRight);
            pendantPosition = _pendantBg.Position;
            Overlay.Add(_pendantBg);

            // The cube image. FIXME: should make this static property on CubeFactory?
            CubePreview = CubeFactory.CreateCurrentCube(MainGame, Coord.Zero, Coord.Zero, 0);
            CubePreview.Position = pendantPosition + new Vector2(57, 42);
            Overlay.Add(CubePreview);

            // Pendant left.
            var pLBN1s = SpriteSheet.CreateNew(MainGame, "Buttons/LeftArrowMinusBN_Silver", 1, 2);
            var pLBN1 = new Button(MainGame, pLBN1s, null, pendantPosition + new Vector2(9, 15));
            pLBN1.Clicked += PLBN1_Clicked;
            Overlay.Add(pLBN1);

            var pLBN2s = SpriteSheet.CreateNew(MainGame, "Buttons/LeftArrowMinusBN_Brown", 1, 2);
            var pLBN2 = new Button(MainGame, pLBN2s, null, pendantPosition + new Vector2(9, 51));
            pLBN2.Clicked += PLBN2_Clicked;
            Overlay.Add(pLBN2);

            var pLBN3s = SpriteSheet.CreateNew(MainGame, "Buttons/LeftArrowMinusBN_Brown", 1, 2);
            var pLBN3 = new Button(MainGame, pLBN3s, null, pendantPosition + new Vector2(9, 81));
            pLBN3.Clicked += PLBN3_Clicked;
            Overlay.Add(pLBN3);

            var pLBN4s = SpriteSheet.CreateNew(MainGame, "Buttons/LeftArrowMinusBN_Silver", 1, 2);
            var pLBN4 = new Button(MainGame, pLBN4s, null, pendantPosition + new Vector2(9, 117));
            pLBN4.Clicked += PLBN4_Clicked;
            Overlay.Add(pLBN4);

            // Pendant right.
            var pRBN1s = SpriteSheet.CreateNew(MainGame, "Buttons/RightArrowPlusBN_Silver", 1, 2);
            var pRBN1 = new Button(MainGame, pRBN1s, null, pendantPosition + new Vector2(138, 15));
            pRBN1.Clicked += PRBN1_Clicked;
            Overlay.Add(pRBN1);

            var pRBN2s = SpriteSheet.CreateNew(MainGame, "Buttons/RightArrowPlusBN_Brown", 1, 2);
            var pRBN2 = new Button(MainGame, pRBN2s, null, pendantPosition + new Vector2(138, 51));
            pRBN2.Clicked += PRBN2_Clicked;
            Overlay.Add(pRBN2);

            var pRBN3s = SpriteSheet.CreateNew(MainGame, "Buttons/RightArrowPlusBN_Brown", 1, 2);
            var pRBN3 = new Button(MainGame, pRBN3s, null, pendantPosition + new Vector2(138, 81));
            pRBN3.Clicked += PRBN3_Clicked;
            Overlay.Add(pRBN3);

            var pRBN4s = SpriteSheet.CreateNew(MainGame, "Buttons/RightArrowPlusBN_Silver", 1, 2);
            var pRBN4 = new Button(MainGame, pRBN4s, null, pendantPosition + new Vector2(138, 117));
            pRBN4.Clicked += PRBN4_Clicked;
            Overlay.Add(pRBN4);

            // Pendant random.
            var pRandTs = SpriteSheet.CreateNew(MainGame, "Buttons/RandBN", 1, 2);
            var pRandT = new Button(MainGame, pRandTs, null, pendantPosition + new Vector2(57, 15)) { ButtonType = ButtonType.CheckBox };
            if (CubeFactory.IsRandomTop)
            {
                pRandT.IsPressed = true;
                pRandT.ReconfigureAnimator(pRandT.PressedArgs);
            }
            pRandT.Checked += PRandTop_Checked;
            pRandT.Unchecked += PRandTop_Unchecked;
            Overlay.Add(pRandT);

            var pRandBs = SpriteSheet.CreateNew(MainGame, "Buttons/RandBN", 1, 2);
            var pRandB = new Button(MainGame, pRandBs, null, pendantPosition + new Vector2(57, 126)) { ButtonType = ButtonType.CheckBox };
            if (CubeFactory.IsRandomBase)
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
            if (BuildMode == BuildMode._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PLBN4_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeBaseMetaData.PreviousFrame();
            CubePreview.AnimatorBase.CurrentFrame = CubeFactory.CurrentCubeBaseMetaData.CurrentFrame;
            if (BuildMode == BuildMode._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PRBN3_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindNextBase();
            var CCBMD = CubeFactory.CurrentCubeBaseMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCBMD.Path, CCBMD.SheetRows, CCBMD.SheetColumns), CubePreview.AnimatorBase);
            CubePreview.ReconfigureAnimator(CCBMD.StartFrame, CCBMD.CurrentFrame, CCBMD.FrameCount, CCBMD.FrameSpeed, CCBMD.AnimState, CubePreview.AnimatorBase);

            if (BuildMode == BuildMode._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PLBN3_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindPreviousBase();
            var CCBMD = CubeFactory.CurrentCubeBaseMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCBMD.Path, CCBMD.SheetRows, CCBMD.SheetColumns), CubePreview.AnimatorBase);
            CubePreview.ReconfigureAnimator(CCBMD.StartFrame, CCBMD.CurrentFrame, CCBMD.FrameCount, CCBMD.FrameSpeed, CCBMD.AnimState, CubePreview.AnimatorBase);

            if (BuildMode == BuildMode._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PRBN2_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindNextTop();
            var CCTMD = CubeFactory.CurrentCubeTopMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCTMD.Path, CCTMD.SheetRows, CCTMD.SheetColumns));
            CubePreview.ReconfigureAnimator(CCTMD.StartFrame, CCTMD.CurrentFrame, CCTMD.FrameCount, CCTMD.FrameSpeed, CCTMD.AnimState);

            if (BuildMode == BuildMode._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PLBN2_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindPreviousTop();
            var CCTMD = CubeFactory.CurrentCubeTopMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCTMD.Path, CCTMD.SheetRows, CCTMD.SheetColumns));
            CubePreview.ReconfigureAnimator(CCTMD.StartFrame, CCTMD.CurrentFrame, CCTMD.FrameCount, CCTMD.FrameSpeed, CCTMD.AnimState);

            if (BuildMode == BuildMode._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PRBN1_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeTopMetaData.NextFrame();
            CubePreview.Animator.CurrentFrame = CubeFactory.CurrentCubeTopMetaData.CurrentFrame;
            if (BuildMode == BuildMode._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PLBN1_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeTopMetaData.PreviousFrame();
            CubePreview.Animator.CurrentFrame = CubeFactory.CurrentCubeTopMetaData.CurrentFrame;
            if (BuildMode == BuildMode._Cube) { GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content); }
        }

        private void PRandB_Unchecked(object sender, ButtonClickedEventArgs e) { CubeFactory.IsRandomBase = false; }
        private void PRandB_Checked(object sender, ButtonClickedEventArgs e) { CubeFactory.IsRandomBase = true; }
        private void PRandTop_Unchecked(object sender, ButtonClickedEventArgs e) { CubeFactory.IsRandomTop = false; }
        private void PRandTop_Checked(object sender, ButtonClickedEventArgs e) { CubeFactory.IsRandomTop = true; }

        #endregion

        private void MainGame_Exiting(object sender, System.EventArgs e)
        {
            Serializer.WriteXML<MapBuilder>(MapBuilderSerializer, this, SaveFilePath);
        }

        // Should this have both generic and state specific components?
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
                GuiHelper.CheckFocus(ref InFocus, this);

                // User input actions.
                if (Peripherals.KeyTapped(Keys.Q) && MainGame.IsActive)
                {
                    var i = (int)Authorization + 1;
                    if (i >= Enum.GetNames(typeof(Authorization)).Length) { i = 0; }
                    Authorization = (Authorization)(i);
                }

                if (Peripherals.KeyTapped(Keys.E) && MainGame.IsActive)
                {
                    CurrentLightingColor = CurrentLightingColor == new Color(0, 1, 0) ? new Color(0, 220, 0) : new Color(0, 1, 0);
                }

                if (Peripherals.KeyTapped(Keys.M) && MainGame.IsActive)
                {
                    Peripherals.ScrollWheelUp -= Peripherals_ScrollWheelUp;
                    Peripherals.ScrollWheelDown -= Peripherals_ScrollWheelDown;
                    MainGame.Exiting -= MainGame_Exiting;
                    Serializer.WriteXML<MapBuilder>(MapBuilderSerializer, this, SaveFilePath);
                    MainGame.CurrentState = new MapBuilder(MainGame, GraphicsDevice, Content, "housetest.xml");
                    IsLoading = true;
                }

                if (Peripherals.KeyTapped(Keys.N) && MainGame.IsActive)
                {
                    Peripherals.ScrollWheelUp -= Peripherals_ScrollWheelUp;
                    Peripherals.ScrollWheelDown -= Peripherals_ScrollWheelDown;
                    MainGame.Exiting -= MainGame_Exiting;
                    Serializer.WriteXML<MapBuilder>(MapBuilderSerializer, this, SaveFilePath);
                    MainGame.CurrentState = new MapBuilder(MainGame, GraphicsDevice, Content, "MapBuilderSave.xml");
                    IsLoading = true;
                }

                if (Peripherals.KeyTapped(Keys.Left) && MainGame.IsActive)
                {
                    MainGame.Brightness += 0.1f;
                }
                if (Peripherals.KeyTapped(Keys.Right) && MainGame.IsActive)
                {
                    MainGame.Brightness -= 0.1f;
                }

                if (Peripherals.KeyTapped(Keys.R) && MainGame.IsActive)
                {
                    if (BuildMode == BuildMode._1x1)
                    {
                        DecoFactory.FindNext1x1();
                        DecoPreview = DecoFactory.CreateCurrentDeco1x1(MainGame, Coord.Zero, Coord.Zero, 0);
                        GhostMarker.MorphImage(DecoPreview, MainGame, GraphicsDevice, Content);
                    }
                    if (BuildMode == BuildMode._2x2)
                    {
                        DecoFactory.FindNext2x2();
                        DecoPreview = DecoFactory.CreateCurrentDeco2x2(MainGame, Coord.Zero, Coord.Zero, 0);
                        GhostMarker.MorphImage(DecoPreview, MainGame, GraphicsDevice, Content);
                    }
                    else if (BuildMode == BuildMode._3x3)
                    {
                        DecoFactory.FindNext3x3();
                        DecoPreview = DecoFactory.CreateCurrentDeco3x3(MainGame, Coord.Zero, Coord.Zero, 0);
                        GhostMarker.MorphImage(DecoPreview, MainGame, GraphicsDevice, Content);
                    }
                }

                if (Peripherals.KeyTapped(Keys.T) && MainGame.IsActive)
                {
                    if (BuildMode == BuildMode._1x1)
                    {
                        DecoFactory.CurrentDecoMetaData1x1.NextFrame();
                        DecoPreview.Animator.CurrentFrame = DecoFactory.CurrentDecoMetaData1x1.CurrentFrame;
                        GhostMarker.MorphImage(DecoPreview, MainGame, GraphicsDevice, Content);
                    }
                    else if (BuildMode == BuildMode._2x2)
                    {
                        DecoFactory.CurrentDecoMetaData2x2.NextFrame();
                        DecoPreview.Animator.CurrentFrame = DecoFactory.CurrentDecoMetaData2x2.CurrentFrame;
                        GhostMarker.MorphImage(DecoPreview, MainGame, GraphicsDevice, Content);
                    }
                    else if (BuildMode == BuildMode._3x3)
                    {
                        DecoFactory.CurrentDecoMetaData3x3.NextFrame();
                        DecoPreview.Animator.CurrentFrame = DecoFactory.CurrentDecoMetaData3x3.CurrentFrame;
                        GhostMarker.MorphImage(DecoPreview, MainGame, GraphicsDevice, Content);
                    }
                }

                if (Authorization == Authorization.Builder)
                {
                    if (Peripherals.LeftButtonPressed() && MainGame.IsActive && InFocus)
                    {
                        if (BuildMode == BuildMode._Cube)
                        {
                            var cube = CubeFactory.CreateCurrentCube(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                            LayerMap[Altitude].AddCheck(cube, Altitude);
                        }
                        else if (BuildMode == BuildMode._1x1)
                        {
                            var deco = DecoFactory.CreateCurrentDeco1x1(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                            LayerMap[Altitude].AddCheck(deco, Altitude);
                        }
                        else if (BuildMode == BuildMode._2x2)
                        {
                            var deco = DecoFactory.CreateCurrentDeco2x2(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                            LayerMap[Altitude].AddCheck(deco, Altitude);
                        }
                        else if (BuildMode == BuildMode._3x3)
                        {
                            var deco = DecoFactory.CreateCurrentDeco3x3(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                            LayerMap[Altitude].AddCheck(deco, Altitude);
                        }
                    }

                    if (Peripherals.RightButtonPressed() && MainGame.IsActive && InFocus)
                    {
                        //var rect = new Rectangle(Peripherals.GetScaledMouseWorldPosition(MainGame.Camera), new Point(200,200));
                        if (BuildMode == BuildMode._Cube)
                        {
                            for (int i = 0; i < LayerMap[Altitude].Count(); i++)
                            {
                                var sprite = LayerMap[Altitude][i];
                                if (sprite is Cube && sprite.Coords == relativeTopFaceCoords)
                                {
                                    LayerMap[Altitude].RemoveCheck(sprite, Altitude); i--;
                                }
                                //if (sprite is Cube && rect.Contains(sprite.Position.ToPoint()))
                                //{
                                //    LayerMap[Altitude].RemoveCheck(sprite, Altitude); i--;
                                //}
                            }
                        }
                        else if (BuildMode == BuildMode._1x1 || BuildMode == BuildMode._2x2 || BuildMode == BuildMode._3x3)
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

                #region Pre Loop

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

                #endregion

                #region Main Loop

                ShadowDict = new Dictionary<Coord, List<Sprite>>();
                Sprite clickedSprite = null;

                foreach (var sprite in World.Sort(LayerMap))
                {
                    sprite.Update(gameTime);
                    // Lead with basic rectangle contains check.
                    if (Peripherals.LeftButtonTapped() && sprite.Animator.WorldArea().Contains(Peripherals.GetScaledMouseWorldPosition(MainGame.Camera)) && MainGame.IsActive && InFocus && !(sprite is GhostMarker))
                    {
                        // Generate solid pixel hashset and do a more thorough contains check. 'Offset' sprite animator.Position back to zero via mouse position, such that it maps onto solid pixel collection coords.
                        if (GraphicsHelper.SolidPixels(sprite.Animator).Contains(Peripherals.GetScaledMouseWorldPosition(MainGame.Camera) - sprite.Animator.Position.ToPoint()))
                        {
                            clickedSprite = sprite;
                        }
                    }
                    // Create dict: Key = coords, Value = column (list) of sprites with same coord different altitude.
                    if (ShadowDict.ContainsKey(sprite.Coords) == false)
                    {
                        ShadowDict.Add(sprite.Coords, new List<Sprite>() { });
                    }
                    else
                    {
                        ShadowDict[sprite.Coords].Add(sprite);
                    }
                }
                if (clickedSprite != null && Authorization == Authorization.None)
                {
                    ClickedSpriteName = clickedSprite.Animator.SpriteSheet.TexturePath;
                    clickedSprite.OnClicked();
                }

                #endregion

                WaterShader.Update(gameTime);

                // Move from DeferredOverlay to Overlay.
                for (int i = 0; i < DeferredOverlay.Count(); i++)
                {
                    var keyPair = DeferredOverlay[i];
                    if (keyPair.Value == DeferAction.Add)
                    {
                        Overlay.Add(keyPair.Key);
                    }
                    else if (keyPair.Value == DeferAction.Remove)
                    {
                        Overlay.Remove(keyPair.Key);
                    }
                    DeferredOverlay.RemoveAt(i);
                    i--;
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


        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteBatch spriteBatchShadow, SpriteBatch spriteBatchLighting, SpriteBatch spriteBatchLightingStencil)
        {
            if (!IsLoading)
            {
                GraphicsHelper.CalculateFPS(gameTime);
                // FIXME: make this circular and/or coord based?
                var rect = new Rectangle((int)Player.Position.X - 4000, (int)Player.Position.Y - 4000, 8000, 8000);

                WaterShader.Draw(gameTime, MainGame.SpriteBatchWater);

                #region Main Loop

                // Draw sorted sprites;
                foreach (var sprite in World.Sort(LayerMap))
                {
                    if (rect.Contains(sprite.Position))
                    {
                        // Game
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

                        // Shadows
                        if (sprite.AntiShadow != null && !(sprite is GhostMarker))
                        {
                            if (sprite is Cube)
                            {
                                // Special case because number of frames can vary but AntiShadow remains the same.
                                spriteBatchShadow.Draw(sprite.AntiShadow, sprite.Animator.Position, Color.White);
                                spriteBatchLightingStencil.Draw(sprite.AntiShadow, sprite.Animator.Position, Color.White);
                                if (sprite.Light != null)
                                {
                                    spriteBatchLightingStencil.Draw(sprite.Light, sprite.Animator.Position, Color.White);
                                }
                                else 
                                {
                                    //spriteBatchLightingStencil.Draw(sprite.AntiShadow, sprite.Animator.Position, Color.White);
                                }

                                if (sprite.Animator.SpriteSheet.TexturePath != "Cubes/WaterCubeTop")
                                {
                                    MainGame.SpriteBatchWaterStencil.Draw(sprite.AntiShadow, sprite.Animator.Position, Color.White);                      
                                }
                                else
                                {
                                    MainGame.SpriteBatchWaterStencil.Draw(sprite.Shadow, sprite.Animator.Position, Color.White);
                                }
                            }
                            else
                            {
                                // Sprites here have AntiShadow generated automatically for entire sheet so use SheetViewArea() to retrieve view rectangle.
                                spriteBatchShadow.Draw(sprite.AntiShadow, sprite.Animator.Position, sprite.Animator.SheetViewArea(), Color.White);
                                spriteBatchLightingStencil.Draw(sprite.AntiShadow, sprite.Animator.Position, sprite.Animator.SheetViewArea(), Color.White);
                                if (sprite.Light != null)
                                {
                                    spriteBatchLighting.Draw(sprite.Light, sprite.Animator.Position + new Vector2(-180, -90), Color.White);
                                    
                                }
                                else if (sprite is Deco)
                                {
                                    var deco = sprite as Deco;
                                    if (deco.Dimensions.Z > 2 && deco.Dimensions.X > 1)
                                    {
                                        spriteBatchLighting.Draw(sprite.AntiShadow, sprite.Animator.Position, sprite.Animator.SheetViewArea(), Color.White);
                                    }
                                }
                                MainGame.SpriteBatchWaterStencil.Draw(sprite.AntiShadow, sprite.Animator.Position, sprite.Animator.SheetViewArea(), Color.White);
                            }
                        }

                        if (ShadowDict.ContainsKey(sprite.Coords))
                        {
                            foreach (var higherSprite in ShadowDict[sprite.Coords])
                            {
                                if (higherSprite.Altitude > sprite.Altitude && higherSprite.Shadow != null && !(sprite is GhostMarker))
                                {
                                    if (sprite is Cube)
                                    {
                                        // Special case, same reasoning as above.
                                        spriteBatchShadow.Draw(higherSprite.Shadow, sprite.Animator.Position, Color.White);
                                    }
                                    else
                                    {
#if DEBUG
                                        Debug.Assert(sprite.SelfShadow != null, "Is there a valid reason why SelfShadow can be null while the sprite belongs to the LayerMap?");
#endif
                                        spriteBatchShadow.Draw(sprite.SelfShadow, sprite.Animator.Position, sprite.Animator.SheetViewArea(), Color.White);
                                    }
                                }
                            }
                        }

                    }
                }

                #endregion
            }
        }

        public override void DrawOverlay(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsLoading)
            {
                foreach (var sprite in Overlay)
                {
                    sprite.Draw(gameTime, spriteBatch);
                }

                // When loading the following objects can be null due to latency.
                if (MessageLogBG != null)
                {
                    MessageLogBG.Draw(gameTime, spriteBatch);

                    spriteBatch.DrawString(MainGame.DefaultFont, $"Mouse World Position {Peripherals.GetScaledMouseWorldPosition(MainGame.Camera).ToString() }", MessageLogBG.Position + new Vector2(12, 12), Color.White);
                    spriteBatch.DrawString(MainGame.DefaultFont, $"Mouse Coords {World.TopFace_PointToRelativeCoord(Peripherals.GetMouseWorldPosition(MainGame.Camera), Altitude) }", MessageLogBG.Position + new Vector2(12, 32), Color.White);
                    spriteBatch.DrawString(MainGame.DefaultFont, $"FPS: { GraphicsHelper.FPS.ToString() }", MessageLogBG.Position + new Vector2(12, 52), Color.White);
                    spriteBatch.DrawString(MainGame.DefaultFont, $"Player Position: { Player?.Position.ToString() }", MessageLogBG.Position + new Vector2(12, 72), Color.White);
                    spriteBatch.DrawString(MainGame.DefaultFont, $"Player Coords: { Player?.Coords.ToString() }", MessageLogBG.Position + new Vector2(12, 92), Color.White);
                    spriteBatch.DrawString(MainGame.DefaultFont, $"Clicked Sprite Name: \n{ ClickedSpriteName }", MessageLogBG.Position + new Vector2(12, 112), Color.White);
                }
            }
        }
    }

    public enum Authorization
    {
        None,
        Builder
    }
}
