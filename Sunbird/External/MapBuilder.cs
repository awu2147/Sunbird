using System;
using System.Collections.Generic;
using System.Linq;
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
        [XmlIgnore]
        public Texture2D Background { get; set; }
        public XDictionary<int, SpriteList<Sprite>> MapList { get; set; } = new XDictionary<int, SpriteList<Sprite>>();
        public Player Player { get; set; }
        private bool IsLoading { get; set; }
        public int Altitude { get; set; } = 0;
        public Authorization Authorization { get; set; }
        public List<Sprite> Overlay { get; set; } = new List<Sprite>();

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
            MapList.Add(Altitude, new SpriteList<Sprite>());

            var cube = CubeFactory.CreateCube(MainGame, "Temp/GrassCube", Coord.Zero, World.GetRelativeCoord(Coord.Zero, Altitude), Altitude);
            MapList[Altitude].AddCheck(cube);

            var playerSheet = SpriteSheet.CreateNew(MainGame, "Temp/PirateGirlSheet", 1, 16);
            var playerAnimator = new Animator(playerSheet, null, 0, 1, 0.2f, AnimationState.Loop);
            Player = new Player(MainGame, playerAnimator) { DrawPriority = 2 };
            MapList[Altitude].Add(Player);

            GhostMarker = GhostMarker.CreateNew(MainGame, "Temp/GrassCube");
            GhostMarker.DrawPriority = 1;
            MapList[Altitude].Add(GhostMarker);

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

            MapList = XmlData.MapList;
            foreach (var layer in MapList)
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
            if (MainGame.IsActive == true && Authorization == Authorization.Builder)
            {
                Altitude--;
                if (MapList.ContainsKey(Altitude) == false)
                {
                    MapList.Add(Altitude, new SpriteList<Sprite>());
                }
            }
        }

        private void Peripherals_ScrollWheelUp(object sender, EventArgs e)
        {
            if (MainGame.IsActive == true && Authorization == Authorization.Builder)
            {
                Altitude++;
                if (MapList.ContainsKey(Altitude) == false)
                {
                    MapList.Add(Altitude, new SpriteList<Sprite>());
                }
            }
        }

        public GhostMarker GhostMarker { get; set; }

        public override void Update(GameTime gameTime)
        {
            if (!IsLoading)
            {

                var relativeTopFaceCoords = World.TopFace_PointToRelativeCoord(MainGame.Camera, Altitude);
                var topFaceCoords = World.TopFace_PointToCoord(MainGame.Camera);

                if (Peripherals.KeyTapped(Keys.E))
                {
                    CubeFactory.FindNext();
                    GhostMarker.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CubeFactory.CurrentPath));
                }

                if (Peripherals.KeyTapped(Keys.Q))
                {
                    if (Authorization == Authorization.None)
                    {
                        Authorization = Authorization.Builder;
                    }
                    else
                    {
                        Authorization = Authorization.None;
                    }
                }

                if (Authorization == Authorization.Builder)
                {
                    if (Peripherals.MousePressed(Peripherals.currentMouseState.LeftButton) && MainGame.IsActive == true)
                    {
                        var cube = CubeFactory.CreateCurrentCube(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                        MapList[Altitude].AddCheck(cube);
                    }

                    if (Peripherals.MousePressed(Peripherals.currentMouseState.RightButton) && MainGame.IsActive == true)
                    {
                        for (int i = 0; i < MapList[Altitude].Count(); i++)
                        {
                            var sprite = MapList[Altitude][i];
                            if (sprite is Cube && sprite.Coords == relativeTopFaceCoords)
                            {
                                MapList[Altitude].RemoveCheck(sprite);
                                i--;
                            }
                        }
                    }

                    GhostMarker.Altitude = Altitude;
                    GhostMarker.Coords = relativeTopFaceCoords;
                }
                else if (Authorization == Authorization.None)
                {
                    var l = MapList.Keys.ToList();
                    l.Sort();
                    l.Reverse();

                    foreach (var key in l)
                    {
                        var targetedCoord = World.TopFace_PointToRelativeCoord(MainGame.Camera, key);
                        if (MapList[key].OccupiedCoords.Contains(targetedCoord))
                        {
                            Altitude = key;
                            GhostMarker.Altitude = Altitude;
                            GhostMarker.Coords = targetedCoord;
                            break;
                        }
                    }
                }

                Player.Coords = relativeTopFaceCoords;
                Player.Altitude = Altitude;

                GhostMarker.Position = World.TopFace_CoordToLocalOrigin(topFaceCoords);

                if (MapList[Altitude].OccupiedCoords.Contains(relativeTopFaceCoords) || Authorization == Authorization.None)
                {
                    GhostMarker.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, "Temp/TopFaceSelectionMarker"));
                }
                else if (Authorization == Authorization.Builder)
                {
                    GhostMarker.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CubeFactory.CurrentPath));
                }

                if (MapList[Altitude].OccupiedCoords.Contains(relativeTopFaceCoords) || Authorization == Authorization.Builder)
                {
                    GhostMarker.DrawPriority = 1;
                }
                else if (Authorization == Authorization.None)
                {
                    GhostMarker.DrawPriority = -1000;
                }

                var keyList = MapList.Keys.ToList();
                keyList.Sort();

                foreach (var key in keyList)
                {
                    for (int i = 0; i < MapList[key].Count(); i++)
                    {
                        var sprite = MapList[key][i];
                        if (sprite.Altitude != key)
                        {
                            MapList[key].Remove(sprite);
                            i--;
                            if (!MapList.ContainsKey(sprite.Altitude))
                            {
                                MapList.Add(sprite.Altitude, new SpriteList<Sprite>() { sprite });
                            }
                            else
                            {
                                if (!(sprite is Cube))
                                {
                                    MapList[sprite.Altitude].Add(sprite);
                                }
                                else
                                {
                                    throw new NotImplementedException("Cube trying to move between layers, is this correct?");
                                }
                            }
                        }
                        sprite.Update(gameTime);
                    }
                }

                foreach (var sprite in Overlay)
                {
                    sprite.Update(gameTime);
                }
            }
        }

        private void Marker_KeyReleased(object sender, KeyReleasedEventArgs e)
        {
            if (e.Key == Keys.F)
            {
                MapList[Altitude].Remove(GhostMarker);
                GhostMarker.IsHidden = true;
                Peripherals.KeyReleased -= Marker_KeyReleased;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsLoading)
            {

                var DrawOrder = new Dictionary<int, List<Sprite>>();

                foreach (var layer in MapList)
                {
                    foreach (var sprite in layer.Value)
                    {
                        if (!DrawOrder.ContainsKey(sprite.DrawAltitude))
                        {
                            DrawOrder.Add(sprite.DrawAltitude, new List<Sprite>() { sprite });
                        }
                        else
                        {
                            DrawOrder[sprite.DrawAltitude].Add(sprite);
                        }
                    }
                }

                var keyList = DrawOrder.Keys.ToList();
                keyList.Sort();

                foreach (var key in keyList)
                {
                    DrawOrder[key].Sort((x, y) =>
                    {
                        int result = decimal.Compare(x.Coords.X - x.Coords.Y, y.Coords.X - y.Coords.Y);
                        if (result == 0)
                        {
                            result = decimal.Compare(x.DrawPriority, y.DrawPriority);
                        }
                        return result;
                    });

                    foreach (var sprite in DrawOrder[key])
                    {
                        if (Altitude != key && sprite is Cube && Authorization == Authorization.Builder)
                        {
                            sprite.Alpha = 0.1f;
                            sprite.Draw(gameTime, spriteBatch);
                        }
                        else if ((Altitude == key || Authorization == Authorization.None) && sprite is Cube)
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
            spriteBatch.DrawString(MainGame.DefaultFont, $"Mouse World Position {Peripherals.GetMouseWorldPosition(MainGame.Camera).ToString()}", new Vector2(10, 10), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Mouse Coords {World.TopFace_PointToRelativeCoord(Peripherals.GetMouseWorldPosition(MainGame.Camera), Altitude)}", new Vector2(10, 30), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Player Coords: { Player.Coords.ToString()}", new Vector2(10, 50), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Altitude: { Altitude.ToString()}", new Vector2(10, 70), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Sprites in List: { MapList[Altitude].Count().ToString()}", new Vector2(10, 90), Color.Black);
            spriteBatch.DrawString(MainGame.DefaultFont, $"Authorization: { Authorization }", new Vector2(10, 110), Color.Black);
            spriteBatch.Draw(Content.Load<Texture2D>(CubeFactory.CurrentPath), new Vector2(30, 150), Color.White);

            foreach (var sprite in Overlay)
            {
                sprite.Draw(gameTime, spriteBatch);
            }
        }
    }
}
