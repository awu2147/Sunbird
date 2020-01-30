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
            Serializer.WriteXML<MapBuilder>(this, "MapBuilderSave.xml", new Type[] { typeof(Player), typeof(Cube), typeof(GhostMarker) });
        }

        private void CreateContent()
        {
            LayerMap.Add(Altitude, new SpriteList<Sprite>());

            //var cube = CubeFactory.CreateCube(MainGame, "Temp/GrassCube", Coord.Zero, World.GetRelativeCoord(Coord.Zero, Altitude), Altitude);
            //LayerMap[Altitude].AddCheck(cube);

            var playerSheet = SpriteSheet.CreateNew(MainGame, "Temp/PirateGirlSheet", 1, 16);
            var playerAnimator = new Animator(playerSheet, null, 0, 1, 0.2f, AnimationState.Loop);
            Player = new Player(MainGame, playerAnimator) { DrawPriority = 2 };
            LayerMap[Altitude].Add(Player);

            GhostMarker = GhostMarker.CreateNew(MainGame, "Temp/GrassCube");
            GhostMarker.DrawPriority = 1;
            LayerMap[Altitude].Add(GhostMarker);

            CubePreview = CubeFactory.CreateCurrentCube(MainGame, Coord.Zero, Coord.Zero, 0);
            CubePreview.Position = new Vector2(30, MainGame.Height - 210);
            Overlay.Add(CubePreview);

            var gridAxisGlyph = SpriteSheet.CreateNew(MainGame, "Temp/GridAxisGlyph");
            Overlay.Add(new Sprite(gridAxisGlyph, new Vector2(20, MainGame.Height - 20), Alignment.BottomLeft));

            Peripherals.ScrollWheelUp += Peripherals_ScrollWheelUp;
            Peripherals.ScrollWheelDown += Peripherals_ScrollWheelDown;
            MainGame.Exiting += MainGame_Exiting;
        }

        public void LoadContentFromFile()
        {
            IsLoading = true;

            MainGame.CurrentState = Templates.LoadingScreenTemplates[0].CreateLoadingScreen(MainGame, GraphicsDevice, Content) as State;
            var currentState = MainGame.CurrentState as ILoadingScreen;

            for (int i = 0; i < 25; i++)
            {
                Thread.Sleep(20);
                currentState.LoadingBar.Progress += 2;
            }

            var XmlData = Serializer.ReadXML<MapBuilder>("MapBuilderSave.xml", new Type[] { typeof(Player), typeof(Cube), typeof(GhostMarker) });

            Altitude = XmlData.Altitude;
            Authorization = XmlData.Authorization;

            LayerMap = XmlData.LayerMap;
            foreach (var layer in LayerMap)
            {
                foreach (var sprite in layer.Value)
                {
                    sprite.LoadContent(MainGame, GraphicsDevice, Content);
                    if (sprite is Player)
                    {
                        Player = sprite as Player;
                    }
                    else if (sprite is GhostMarker)
                    {
                        GhostMarker = sprite as GhostMarker;
                    }
                }
            }

            Overlay = XmlData.Overlay;
            foreach (var sprite in Overlay)
            {
                sprite.LoadContent(MainGame, GraphicsDevice, Content);
                if (sprite is Cube)
                {
                    CubePreview = sprite as Cube;
                }
            }

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

        private void Peripherals_ScrollWheelDown(object sender, EventArgs e)
        {
            if (MainGame.IsActive == true)
            {
                if (Authorization == Authorization.Builder)
                {
                    Altitude--;
                    if (LayerMap.ContainsKey(Altitude) == false)
                    {
                        LayerMap.Add(Altitude, new SpriteList<Sprite>());
                    }
                }
                else if (Authorization == Authorization.None && World.Zoom > 1)
                {
                    World.Zoom--;
                    World.ReconstructTopFaceArea();
                }
            }
            
        }

        private void Peripherals_ScrollWheelUp(object sender, EventArgs e)
        {
            if (MainGame.IsActive == true)
            {
                if (Authorization == Authorization.Builder)
                {
                    Altitude++;
                    if (LayerMap.ContainsKey(Altitude) == false)
                    {
                        LayerMap.Add(Altitude, new SpriteList<Sprite>());
                    }
                }
                else if (Authorization == Authorization.None && World.Zoom < 5)
                {
                    World.Zoom++;
                    World.ReconstructTopFaceArea();
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

                // Peripheral actions.
                if (Peripherals.KeyTapped(Keys.E))
                {
                    CubeFactory.FindNext();
                    var CCMD = CubeFactory.CurrentCubeMetaData;
                    GhostMarker.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCMD.Path, CCMD.SheetRows, CCMD.SheetColumns));
                    GhostMarker.ReconfigureAnimator(CCMD.StartFrame, CCMD.FrameCount, CCMD.FrameSpeed, CCMD.AnimState);

                    CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCMD.Path, CCMD.SheetRows, CCMD.SheetColumns));
                    CubePreview.ReconfigureAnimator(CCMD.StartFrame, CCMD.FrameCount, CCMD.FrameSpeed, CCMD.AnimState);
                }

                if (Peripherals.KeyTapped(Keys.Q))
                {
                    var i = (int)Authorization + 1;
                    if (i >= Enum.GetNames(typeof(Authorization)).Length) { i = 0; }
                    Authorization = (Authorization)(i);
                }

                if (Authorization == Authorization.Builder)
                {
                    if (Peripherals.MousePressed(Peripherals.currentMouseState.LeftButton) && MainGame.IsActive == true)
                    {
                        Cube cube = null;
                        if (CubeFactory.CurrentCubeMetaData.AnimState != AnimationState.None)
                        {
                            cube = CubeFactory.CreateCurrentCube(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                        }
                        else
                        {
                            cube = CubeFactory.CreateRandomCurrentCube(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                        }
                        LayerMap[Altitude].AddCheck(cube);
                    }

                    if (Peripherals.MousePressed(Peripherals.currentMouseState.RightButton) && MainGame.IsActive == true)
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
                            GhostMarker.Coords = targetedCoord;
                            break;
                        }
                    }
                }

                GhostMarker.Position = World.TopFace_CoordToLocalOrigin(topFaceCoords);

                if (LayerMap[Altitude].OccupiedCoords.Contains(relativeTopFaceCoords) || Authorization == Authorization.None)
                {
                    GhostMarker.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, "Temp/TopFaceSelectionMarker"));
                    GhostMarker.ReconfigureAnimator();
                }
                else if (Authorization == Authorization.Builder)
                {
                    var CCMD = CubeFactory.CurrentCubeMetaData;
                    GhostMarker.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCMD.Path, CCMD.SheetRows, CCMD.SheetColumns));
                    GhostMarker.ReconfigureAnimator(CCMD.StartFrame, CCMD.FrameCount, CCMD.FrameSpeed, CCMD.AnimState);
                }

                if (LayerMap[Altitude].OccupiedCoords.Contains(relativeTopFaceCoords) || Authorization == Authorization.Builder)
                {
                    GhostMarker.DrawPriority = 1;
                }
                else if (Authorization == Authorization.None)
                {
                    GhostMarker.DrawPriority = -1000;
                }

                // Player management.
                Player.Coords = relativeTopFaceCoords;
                Player.Altitude = Altitude + 30;

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

        public override void DrawOverlay(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var sprite in Overlay)
            {
                sprite.Draw(gameTime, spriteBatch);
            }

            spriteBatch.DrawString(MainGame.DefaultFont, $"Mouse World Position {Peripherals.GetMouseWorldPosition(MainGame.Camera).ToString()}", new Vector2(10, 10), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Mouse Coords {World.TopFace_PointToRelativeCoord(Peripherals.GetMouseWorldPosition(MainGame.Camera), Altitude)}", new Vector2(10, 30), Color.Black);            
            spriteBatch.DrawString(MainGame.DefaultFont, $"Altitude: { Altitude.ToString()}", new Vector2(10, 50), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Sprites in List: { LayerMap[Altitude].Count().ToString()}", new Vector2(10, 70), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Zoom: { Convert.ToInt32(Math.Floor(World.ZoomRatio * 100))} %", new Vector2(10, 90), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Authorization: { Authorization }", new Vector2(10, 110), Color.Black);

        }
    }
}
