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
    public class MapBuilder : State, IWorldState
    {
        [XmlIgnore]
        public Texture2D Background { get; set; }
        public SpriteList<Sprite> SpriteList { get; set; } = new SpriteList<Sprite>();
        public Player Player { get; set; }
        public HashSet<Coord> OccupiedCoords { get; set; } = new HashSet<Coord>();
        private bool IsLoading { get; set; }

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

            var cube = CubeFactory.CreateCube(MainGame, "Temp/GrassCube", Vector2.Zero);
            SpriteList.Add(cube, this);

            var playerSheet = new SpriteSheet(Content.Load<Texture2D>("Temp/testplayer"), 2, 6) { TexturePath = "Temp/testplayer" };
            var playerAnimator = new Animator(playerSheet, null, 0, 2, 0.2f, AnimationState.Loop);
            Player = new Player(MainGame, playerAnimator);
            SpriteList.Add(Player);

            GhostMarker = GhostMarker.CreateGhostMarker(MainGame, "Temp/GrassCube");
            SpriteList.Add(GhostMarker);

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
            OccupiedCoords = XmlData.OccupiedCoords;
            SpriteList = XmlData.SpriteList;
            foreach (var sprite in SpriteList)
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
                var topFaceCoords = World.TopFace_PointToCoord(Peripherals.GetMouseWorldPosition(MainGame.Camera));

                if (Peripherals.KeyTapped(Keys.E))
                {
                    CubeFactory.FindNext();
                    GhostMarker.ReplaceSpriteSheet(new SpriteSheet(Content.Load<Texture2D>(CubeFactory.CurrentPath), 1, 1) { TexturePath = CubeFactory.CurrentPath });
                }

                if (Peripherals.MousePressed(Peripherals.currentMouseState.LeftButton) && MainGame.IsActive == true)
                {
                    var cube = CubeFactory.CreateCube(MainGame, CubeFactory.CurrentPath, World.TopFace_CoordToLocalOrigin(topFaceCoords), topFaceCoords);
                    SpriteList.Add(cube, this);
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

                GhostMarker.Coords = topFaceCoords;
                GhostMarker.Position = World.TopFace_CoordToLocalOrigin(topFaceCoords);
                GhostMarker.Update(gameTime);
                if (OccupiedCoords.Contains(topFaceCoords))
                {
                    GhostMarker.IsHidden = true;
                }
                else
                {
                    GhostMarker.IsHidden = false;
                }            

                foreach (var sprite in SpriteList)
                {
                    sprite.Update(gameTime);
                }
            }
        }

        private void Marker_KeyReleased(object sender, KeyReleasedEventArgs e)
        {
            if (e.key == Keys.F)
            {            
                SpriteList.Remove(GhostMarker);
                GhostMarker.IsHidden = true;
                Peripherals.KeyReleased -= Marker_KeyReleased;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsLoading)
            {

                SpriteList.Sort((x, y) =>
                {
                    int result = decimal.Compare(x.Coords.X - x.Coords.Y, y.Coords.X - y.Coords.Y);
                    //if (result == 0)
                    //{
                    //    result = decimal.Compare((decimal)x.positionBase.Y, (decimal)y.positionBase.Y);
                    //}
                    return result;
                });

                foreach (var sprite in SpriteList)
                {
                    sprite.Draw(gameTime, spriteBatch);
                }

            }
        }

    }
}
