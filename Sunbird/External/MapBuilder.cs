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
using Microsoft.Win32;

namespace Sunbird.External
{
    public enum Authorization
    {
        None,
        Builder
    }

    public class MapBuilder : State, IWorld
    {
        public XDictionary<int, SpriteList<Sprite>> LayerMap { get; set; } = new XDictionary<int, SpriteList<Sprite>>();
        public List<Sprite> Overlay { get; set; } = new List<Sprite>();
        public int Altitude { get; set; } = 0;
        public Player Player { get; set; }
        public GhostMarker GhostMarker { get; set; }
        public Cube CubePreview { get; set; }
        private bool IsLoading { get; set; }
        private bool InFocus { get; set; }
        public Authorization Authorization { get; set; }

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

        private void MainGame_Exiting(object sender, System.EventArgs e)
        {
            Serializer.WriteXML<MapBuilder>(this, "MapBuilderSave.xml", new Type[] { typeof(Player), typeof(Cube), typeof(GhostMarker), typeof(Button) });
        }

        private void CreateContent()
        {
            LayerMap.Add(Altitude, new SpriteList<Sprite>());

            var playerSheet = SpriteSheet.CreateNew(MainGame, "Temp/PirateGirlSheet", 1, 16);
            var playerAnimArgs = new AnimArgs(0, 1, 0.2f, AnimationState.Loop);
            Player = new Player(MainGame, playerSheet, playerAnimArgs) { DrawPriority = 1 };
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

        private void CreateOverlay()
        {
            CreateCubePendant();

            var gridAxisGlyph = SpriteSheet.CreateNew(MainGame, "Temp/GridAxisGlyph");
            Overlay.Add(new Sprite(MainGame, gridAxisGlyph, new Vector2(MainGame.Width - 20, 20), Alignment.TopRight));
        }

        #region Cube Pendant

        private void CreateCubePendant()
        {
            // Pendant background.
            var pendantBg = SpriteSheet.CreateNew(MainGame, "Temp/PendantBackGround");
            var pendantPosition = new Vector2(20, MainGame.Height - 20);
            Overlay.Add(new Sprite(MainGame, pendantBg, pendantPosition, Alignment.BottomLeft));

            // The cube image.
            CubePreview = CubeFactory.CreateCurrentCube(MainGame, Coord.Zero, Coord.Zero, 0);
            CubePreview.Position = pendantPosition + new Vector2(57, -117);
            Overlay.Add(CubePreview);

            // Pendant left.
            var pLBN1s = SpriteSheet.CreateNew(MainGame, "Temp/LeftArrowMinusBN_Silver", 1, 2);
            var pLBN1 = new Button(MainGame, pLBN1s, null, pendantPosition + new Vector2(9, -144));
            pLBN1.Clicked += PLBN1_Clicked;
            Overlay.Add(pLBN1);

            var pLBN2s = SpriteSheet.CreateNew(MainGame, "Temp/LeftArrowMinusBN_Brown", 1, 2);
            var pLBN2 = new Button(MainGame, pLBN2s, null, pendantPosition + new Vector2(9, -108));
            pLBN2.Clicked += PLBN2_Clicked;
            Overlay.Add(pLBN2);

            var pLBN3s = SpriteSheet.CreateNew(MainGame, "Temp/LeftArrowMinusBN_Brown", 1, 2);
            var pLBN3 = new Button(MainGame, pLBN3s, null, pendantPosition + new Vector2(9, -78));
            pLBN3.Clicked += PLBN3_Clicked;
            Overlay.Add(pLBN3);

            var pLBN4s = SpriteSheet.CreateNew(MainGame, "Temp/LeftArrowMinusBN_Silver", 1, 2);
            var pLBN4 = new Button(MainGame, pLBN4s, null, pendantPosition + new Vector2(9, -42));
            pLBN4.Clicked += PLBN4_Clicked;
            Overlay.Add(pLBN4);

            // Pendant right.
            var pRBN1s = SpriteSheet.CreateNew(MainGame, "Temp/RightArrowPlusBN_Silver", 1, 2);
            var pRBN1 = new Button(MainGame, pRBN1s, null, pendantPosition + new Vector2(138, -144));
            pRBN1.Clicked += PRBN1_Clicked;
            Overlay.Add(pRBN1);

            var pRBN2s = SpriteSheet.CreateNew(MainGame, "Temp/RightArrowPlusBN_Brown", 1, 2);
            var pRBN2 = new Button(MainGame, pRBN2s, null, pendantPosition + new Vector2(138, -108));
            pRBN2.Clicked += PRBN2_Clicked;
            Overlay.Add(pRBN2);

            var pRBN3s = SpriteSheet.CreateNew(MainGame, "Temp/RightArrowPlusBN_Brown", 1, 2);
            var pRBN3 = new Button(MainGame, pRBN3s, null, pendantPosition + new Vector2(138, -78));
            pRBN3.Clicked += PRBN3_Clicked;
            Overlay.Add(pRBN3);

            var pRBN4s = SpriteSheet.CreateNew(MainGame, "Temp/RightArrowPlusBN_Silver", 1, 2);
            var pRBN4 = new Button(MainGame, pRBN4s, null, pendantPosition + new Vector2(138, -42));
            pRBN4.Clicked += PRBN4_Clicked;
            Overlay.Add(pRBN4);

            // Pendant random.
            var pRandTs = SpriteSheet.CreateNew(MainGame, "Temp/RandBN", 1, 2);
            var pRandT = new Button(MainGame, pRandTs, null, pendantPosition + new Vector2(57, -144)) { ButtonType = ButtonType.CheckBox, IsPressed = CubeFactory.IsRandomTop };
            pRandT.Checked += PRandTop_Checked;
            pRandT.Unchecked += PRandTop_Unchecked;
            Overlay.Add(pRandT);

            var pRandBs = SpriteSheet.CreateNew(MainGame, "Temp/RandBN", 1, 2);
            var pRandB = new Button(MainGame, pRandBs, null, pendantPosition + new Vector2(57, -33)) { ButtonType = ButtonType.CheckBox, IsPressed = CubeFactory.IsRandomBottom };
            pRandB.Checked += PRandB_Checked;
            pRandB.Unchecked += PRandB_Unchecked;
            Overlay.Add(pRandB);
        }

        #endregion

        #region Cube Pendant Event Handlers

        private void PRBN4_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeBaseMetaData.NextFrame();
            CubePreview.Animator.CurrentFrame = CubeFactory.CurrentCubeBaseMetaData.CurrentFrame;
            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
        }

        private void PLBN4_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeBaseMetaData.PreviousFrame();
            CubePreview.Animator.CurrentFrame = CubeFactory.CurrentCubeBaseMetaData.CurrentFrame;
            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
        }

        private void PRBN3_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindNextBase();
            var CCBMD = CubeFactory.CurrentCubeBaseMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCBMD.Path, CCBMD.SheetRows, CCBMD.SheetColumns), CubePreview.AnimatorBase);
            CubePreview.ReconfigureAnimator(CCBMD.StartFrame, CCBMD.CurrentFrame, CCBMD.FrameCount, CCBMD.FrameSpeed, CCBMD.AnimState, CubePreview.AnimatorBase);

            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
        }

        private void PLBN3_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindPreviousBase();
            var CCBMD = CubeFactory.CurrentCubeBaseMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCBMD.Path, CCBMD.SheetRows, CCBMD.SheetColumns), CubePreview.AnimatorBase);
            CubePreview.ReconfigureAnimator(CCBMD.StartFrame, CCBMD.CurrentFrame, CCBMD.FrameCount, CCBMD.FrameSpeed, CCBMD.AnimState, CubePreview.AnimatorBase);

            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
        }

        private void PRBN2_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindNext();
            var CCMD = CubeFactory.CurrentCubeMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCMD.Path, CCMD.SheetRows, CCMD.SheetColumns));
            CubePreview.ReconfigureAnimator(CCMD.StartFrame, CCMD.CurrentFrame, CCMD.FrameCount, CCMD.FrameSpeed, CCMD.AnimState);

            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
        }

        private void PLBN2_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.FindPrevious();
            var CCMD = CubeFactory.CurrentCubeMetaData;

            CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCMD.Path, CCMD.SheetRows, CCMD.SheetColumns));
            CubePreview.ReconfigureAnimator(CCMD.StartFrame, CCMD.CurrentFrame, CCMD.FrameCount, CCMD.FrameSpeed, CCMD.AnimState);

            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
        }

        private void PRBN1_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeMetaData.NextFrame();
            CubePreview.Animator.CurrentFrame = CubeFactory.CurrentCubeMetaData.CurrentFrame;
            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
        }

        private void PLBN1_Clicked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.CurrentCubeMetaData.PreviousFrame();
            CubePreview.Animator.CurrentFrame = CubeFactory.CurrentCubeMetaData.CurrentFrame;
            GhostMarker.MorphImage(CubePreview, MainGame, GraphicsDevice, Content);
        }

        private void PRandB_Unchecked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.IsRandomBottom = false;
        }

        private void PRandB_Checked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.IsRandomBottom = true;
        }

        private void PRandTop_Unchecked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.IsRandomTop = false;
        }

        private void PRandTop_Checked(object sender, ButtonClickedEventArgs e)
        {
            CubeFactory.IsRandomTop = true;
        }

        #endregion

        private void LoadContentFromFile()
        {
            IsLoading = true;

            MainGame.CurrentState = Templates.LoadingScreenTemplates[0].CreateLoadingScreen(MainGame, GraphicsDevice, Content) as State;
            var currentState = MainGame.CurrentState as ILoadingScreen;

            for (int i = 0; i < 25; i++)
            {
                Thread.Sleep(20);
                currentState.LoadingBar.Progress += 2;
            }

            var XmlData = Serializer.ReadXML<MapBuilder>("MapBuilderSave.xml", new Type[] { typeof(Player), typeof(Cube), typeof(GhostMarker), typeof(Button) });

            Altitude = XmlData.Altitude;
            Authorization = XmlData.Authorization;

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

            CreateOverlay();      

            for (int i = 0; i < 25; i++)
            {
                Thread.Sleep(20);
                currentState.LoadingBar.Progress += 2;
            }

            IsLoading = false;
            MainGame.CurrentState = this;

            Peripherals.ScrollWheelUp += Peripherals_ScrollWheelUp;
            Peripherals.ScrollWheelDown += Peripherals_ScrollWheelDown;
            MainGame.Exiting += MainGame_Exiting;
        }

        // Should these events be state specific?
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

                if (Authorization == Authorization.Builder)
                {
                    if (Peripherals.LeftButtonPressed() && MainGame.IsActive && InFocus)
                    {
                        var cube = CubeFactory.CreateCurrentCube(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                        LayerMap[Altitude].AddCheck(cube);
                    }

                    if (Peripherals.MouseButtonPressed(Peripherals.currentMouseState.RightButton) && MainGame.IsActive && InFocus)
                    {
                        for (int i = 0; i < LayerMap[Altitude].Count(); i++)
                        {
                            var sprite = LayerMap[Altitude][i];
                            if (sprite is Cube && sprite.Coords == relativeTopFaceCoords)
                            {
                                LayerMap[Altitude].RemoveCheck(sprite); i--;
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
                                if (!(sprite is Cube))
                                {
                                    LayerMap[sprite.Altitude].Add(sprite);
                                }
                                else
                                {
                                    throw new NotImplementedException("Cube trying to move between layers, is this correct? Use AddCheck if so.");
                                }
                            }
                        }
                        sprite.Update(gameTime);
                    }
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
                    }
                    Debug.Assert(l.SetEquals(layer.Value.OccupiedCoords), "Occupied coords set != coords of cubes in sprite list, is this correct?");
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
                var dLayerMap = new Dictionary<int, List<Sprite>>();

                foreach (var layer in LayerMap)
                {
                    foreach (var sprite in layer.Value)
                    {
                        if (!dLayerMap.ContainsKey(sprite.DrawAltitude))
                        {
                            dLayerMap.Add(sprite.DrawAltitude, new List<Sprite>() { sprite });
                        }
                        else
                        {
                            dLayerMap[sprite.DrawAltitude].Add(sprite);
                        }
                    }
                }

                var dAltitudeList = dLayerMap.Keys.ToList();
                dAltitudeList.Sort();

                foreach (var dAltitude in dAltitudeList)
                {
                    dLayerMap[dAltitude].Sort((x, y) =>
                    {
                        int result = decimal.Compare(x.Coords.X - x.Coords.Y, y.Coords.X - y.Coords.Y);
                        if (result == 0)
                        {
                            result = decimal.Compare(x.DrawPriority, y.DrawPriority);
                        }
                        return result;
                    });

                    foreach (var sprite in dLayerMap[dAltitude])
                    {
                        if (Altitude != dAltitude && sprite is Cube && Authorization == Authorization.Builder)
                        {
                            sprite.Alpha = 0.1f;
                            sprite.Draw(gameTime, spriteBatch);
                        }
                        else if ((Altitude == dAltitude || Authorization == Authorization.None) && sprite is Cube)
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
        }

        public override void DrawShadow(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsLoading)
            {
                Dictionary<Coord, List<Sprite>> ShadowDict = new Dictionary<Coord, List<Sprite>>();

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

                var dLayerMap = new Dictionary<int, List<Sprite>>();

                foreach (var layer in LayerMap)
                {
                    foreach (var sprite in layer.Value)
                    {
                        if (!dLayerMap.ContainsKey(sprite.DrawAltitude))
                        {
                            dLayerMap.Add(sprite.DrawAltitude, new List<Sprite>() { sprite });
                        }
                        else
                        {
                            dLayerMap[sprite.DrawAltitude].Add(sprite);
                        }
                    }
                }

                var dAltitudeList = dLayerMap.Keys.ToList();
                dAltitudeList.Sort();

                foreach (var daltitude in dAltitudeList)
                {
                    dLayerMap[daltitude].Sort((x, y) =>
                    {
                        int result = decimal.Compare(x.Coords.X - x.Coords.Y, y.Coords.X - y.Coords.Y);
                        if (result == 0)
                        {
                            result = decimal.Compare(x.DrawPriority, y.DrawPriority);
                        }
                        return result;
                    });

                    foreach (var sprite in dLayerMap[daltitude])
                    {
                        if (sprite.AntiShadow != null && !(sprite is GhostMarker))
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
                        foreach (var higherSprite in ShadowDict[sprite.Coords])
                        {
                            if (higherSprite.Altitude > sprite.Altitude && higherSprite.Shadow != null && !(sprite is GhostMarker))
                            {                        
                                if (!(sprite is Cube) && sprite.SelfShadow != null)
                                {                              
                                    spriteBatch.Draw(sprite.SelfShadow, sprite.Animator.Position, sprite.Animator.SheetViewArea(), Color.White);
                                }
                                else
                                {
                                    spriteBatch.Draw(higherSprite.Shadow, sprite.Position, Color.White);
                                }
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
                        spriteBatch.Draw(sprite.Light, sprite.Animator.Position + new Vector2(-180, -90), Color.White);
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
                    if (!(sprite is Cube))
                    {
                        spriteBatch.Draw(sprite.AntiShadow, sprite.Animator.Position, sprite.Animator.SheetViewArea(), Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(sprite.AntiShadow, sprite.Animator.Position, Color.White);
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

            spriteBatch.DrawString(MainGame.DefaultFont, $"Mouse World Position {Peripherals.GetMouseWorldPosition(MainGame.Camera).ToString() }", new Vector2(10, 10), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Mouse Coords {World.TopFace_PointToRelativeCoord(Peripherals.GetMouseWorldPosition(MainGame.Camera), Altitude) }", new Vector2(10, 30), Color.Black);            
            spriteBatch.DrawString(MainGame.DefaultFont, $"Altitude: { Altitude.ToString() }", new Vector2(10, 50), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Player Position: { Player.Position.ToString() }", new Vector2(10, 70), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Player Coords: { Player.Coords.ToString() }", new Vector2(10, 90), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Authorization: { Authorization }", new Vector2(10, 110), Color.Black);

        }
    }
}
