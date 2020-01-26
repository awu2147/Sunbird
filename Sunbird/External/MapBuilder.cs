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
    public class MapBuilder : State, IWorld
    {
        [XmlIgnore]
        public Texture2D Background { get; set; }
        public XDictionary<int, SpriteList<Sprite>> MapList { get; set; } = new XDictionary<int, SpriteList<Sprite>>();
        public Player Player { get; set; }
        private bool IsLoading { get; set; }
        public int Altitude { get; set; } = 0;

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

            var playerSheet = SpriteSheet.CreateNew(MainGame, "Temp/PirateGirlSheet", 1, 4);
            var playerAnimator = new Animator(playerSheet, null, 0, 1, 0.2f, AnimationState.Loop);
            Player = new Player(MainGame, playerAnimator) { DrawPriority = 2 };
            MapList[Altitude].Add(Player);

            GhostMarker = GhostMarker.CreateNew(MainGame, "Temp/GrassCube");
            GhostMarker.DrawPriority = 1;
            MapList[Altitude].Add(GhostMarker);

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
            //OccupiedCoords = XmlData.OccupiedCoords;
            MapList = XmlData.MapList;
            Altitude = XmlData.Altitude;
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

            for (int i = 0; i < 25; i++)
            {
                Thread.Sleep(20);
                currentState.LoadingBar.Progress += 2;
            }

            IsLoading = false;
            MainGame.CurrentState = this;

            MainGame.Exiting += MainGame_Exiting;
        }

        public GhostMarker GhostMarker { get; set; }

        public override void Update(GameTime gameTime)
        {
            if (!IsLoading)
            {

                var relativeTopFaceCoords = World.TopFace_PointToCoord(Peripherals.GetMouseWorldPosition(MainGame.Camera), Altitude);
                var topFaceCoords = World.TopFace_PointToCoord(Peripherals.GetMouseWorldPosition(MainGame.Camera));

                if (Peripherals.KeyTapped(Keys.E))
                {
                    CubeFactory.FindNext();
                    GhostMarker.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CubeFactory.CurrentPath));
                }

                if (Peripherals.KeyTapped(Keys.Q))
                {
                    if (Altitude == 0)
                    {
                        Altitude++;
                    }
                    else
                    {
                        Altitude--;
                    }
                }

                if (MapList.ContainsKey(Altitude) == false)
                {
                    MapList.Add(Altitude, new SpriteList<Sprite>());
                }

                if (Peripherals.MousePressed(Peripherals.currentMouseState.LeftButton) && MainGame.IsActive == true)
                {
                    var cube = CubeFactory.CreateCurrentCube(MainGame, topFaceCoords, relativeTopFaceCoords, Altitude);
                    MapList[Altitude].AddCheck(cube);
                    Debug.Print(cube.Altitude.ToString());
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

                //if (Peripherals.KeyPressed(Keys.F))
                //{
                //    CubeMarker.Coords = topFaceCoords;
                //    CubeMarker.Position = World.TopFace_CoordToLocalOrigin(topFaceCoords);
                //    CubeMarker.Update(gameTime);
                //    if (Peripherals.KeyTapped(Keys.F))
                //    {
                //        CubeMarker.IsVisible = true;
                //        spriteList.Add(CubeMarker, new OnAddEventArgs(CubeMarker) { AddToOccupied = false });
                //        Peripherals.KeyReleased += Marker_KeyReleased;
                //    }
                //}

                Player.Coords = relativeTopFaceCoords;
                Player.Altitude = Altitude; 

                GhostMarker.Coords = relativeTopFaceCoords;
                GhostMarker.Position = World.TopFace_CoordToLocalOrigin(topFaceCoords);
                GhostMarker.Update(gameTime);
                GhostMarker.Altitude = Altitude;
                if (MapList[Altitude].OccupiedCoords.Contains(relativeTopFaceCoords))
                {
                    //GhostMarker.IsHidden = true;
                    GhostMarker.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, "Temp/TopFaceSelectionMarker"));
                }
                else
                {
                    GhostMarker.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CubeFactory.CurrentPath));
                    //GhostMarker.IsHidden = false;
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
                            MapList[sprite.Altitude].Add(sprite);
                        }
                        sprite.Update(gameTime);
                    }
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
                var keyList = MapList.Keys.ToList();
                keyList.Sort();

                foreach (var key in keyList)
                {
                    MapList[key].Sort((x, y) =>
                    {
                        int result = decimal.Compare(x.Coords.X - x.Coords.Y, y.Coords.X - y.Coords.Y);
                        if (result == 0)
                        {
                            result = decimal.Compare(x.DrawPriority, y.DrawPriority);
                        }
                        return result;
                    });

                    foreach (var sprite in MapList[key])
                    {
                        if (Altitude != key && sprite is Cube)
                        {
                            sprite.Alpha = 0.2f;
                            sprite.Draw(gameTime, spriteBatch);
                        }
                        else if (Altitude == key && sprite is Cube)
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

    }
}
