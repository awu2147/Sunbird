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
    public class BuilderRibbon : Sprite, IGui
    {
        private MapBuilder MapBuilder;
        private GraphicsDevice GraphicsDevice { get { return MainGame.GraphicsDevice; } }
        private ContentManager Content { get { return MainGame.Content; } }

        private Button BuildBN;
        private Button WorldBN;
        private Button BuildCubeBN;
        private Button Build1x1BN;
        private Button Build2x2BN;
        private Button Build3x3BN;
        private Button OpenCatalogCubeBN;
        private Button OpenCatalog1x1BN;
        private Button OpenCatalog2x2BN;
        private Button OpenCatalog3x3BN;
        private CubeCatalog CubeCatalog;
        private Deco1x1Catalog Deco1x1Catalog;
        private Deco2x2Catalog Deco2x2Catalog;

        public List<Sprite> Overlay { get; set; } = new List<Sprite>();
        public List<KeyValuePair<Sprite, DeferAction>> DeferredOverlay { get; set; } = new List<KeyValuePair<Sprite, DeferAction>>();

        public BuilderRibbon(MainGame mainGame, MapBuilder sender, Vector2 position)
        {
            MainGame = mainGame;
            MapBuilder = sender;
            Position = position;
        }

        public void LoadContent()
        {
            // Set Ribbon background as BuilderRibbon native Animator.
            var ribbonBgS = SpriteSheet.CreateNew(MainGame, "GUI/RibbonBackGround");
            Animator = new Animator(this, ribbonBgS);

            var worldBNs = SpriteSheet.CreateNew(MainGame, "Buttons/WorldButtonSheet", 4, 3);
            WorldBN = new Button(MainGame, worldBNs, null, Position) { PressedArgs = new AnimArgs(1, 10, 0.1f, AnimationState.Loop) };
            WorldBN.PositionOffset = new Vector2(9, 9);
            WorldBN.Clicked += WorldBN_Clicked;

            if (MapBuilder.Authorization == Authorization.None) { WorldBN.IsPressed = true; }
            WorldBN.OnUpdated = () =>
            {
                if (Peripherals.KeyTapped(Keys.Q))
                {
                    if (MapBuilder.Authorization == Authorization.None) { WorldBN.IsPressed = true; }
                    else if (MapBuilder.Authorization == Authorization.Builder) { WorldBN.IsPressed = false; }
                }
            };

            var buildBNs = SpriteSheet.CreateNew(MainGame, "Buttons/BuildButtonSheet", 4, 3);
            BuildBN = new Button(MainGame, buildBNs, null, Position) { PressedArgs = new AnimArgs(1, 10, 0.1f, AnimationState.Loop) };
            BuildBN.PositionOffset = new Vector2(75, 9);
            BuildBN.Clicked += BuildBN_Clicked;

            if (MapBuilder.Authorization == Authorization.Builder) { BuildBN.IsPressed = true; }
            BuildBN.OnUpdated = () =>
            {
                if (Peripherals.KeyTapped(Keys.Q))
                {
                    if (MapBuilder.Authorization == Authorization.Builder) { BuildBN.IsPressed = true; }
                    else if (MapBuilder.Authorization == Authorization.None) { BuildBN.IsPressed = false; }
                }
            };

            Button.BindGroup(new List<Button>() { BuildBN, WorldBN });
            foreach (var button in new List<Button>() { BuildBN, WorldBN }) { Overlay.Add(button); }

            var buildCubeBNs = SpriteSheet.CreateNew(MainGame, "Buttons/BuildCubeBN", 1, 2);
            BuildCubeBN = new Button(MainGame, buildCubeBNs, null, Position);
            BuildCubeBN.PositionOffset = new Vector2(144, 12);
            BuildCubeBN.Clicked += BuildCubeBN_Clicked;
            if (MapBuilder.BuildMode == BuildMode._Cube)
            {
                BuildCubeBN.IsPressed = true;
            }

            var build1x1BNs = SpriteSheet.CreateNew(MainGame, "Buttons/Build1x1BN", 1, 2);
            Build1x1BN = new Button(MainGame, build1x1BNs, null, Position);
            Build1x1BN.PositionOffset = new Vector2(186, 12);
            Build1x1BN.Clicked += Build1x1BN_Clicked;
            if (MapBuilder.BuildMode == BuildMode._1x1)
            {
                Build1x1BN.IsPressed = true;
            }

            var build2x2BNs = SpriteSheet.CreateNew(MainGame, "Buttons/Build2x2BN", 1, 2);
            Build2x2BN = new Button(MainGame, build2x2BNs, null, Position);
            Build2x2BN.PositionOffset = new Vector2(228, 12);
            Build2x2BN.Clicked += Build2x2BN_Clicked;
            if (MapBuilder.BuildMode == BuildMode._2x2)
            {
                Build2x2BN.IsPressed = true;
            }

            var build3x3BNs = SpriteSheet.CreateNew(MainGame, "Buttons/Build3x3BN", 1, 2);
            Build3x3BN = new Button(MainGame, build3x3BNs, null, Position);
            Build3x3BN.PositionOffset = new Vector2(270, 12);
            Build3x3BN.Clicked += Build3x3BN_Clicked;
            if (MapBuilder.BuildMode == BuildMode._3x3)
            {
                Build3x3BN.IsPressed = true;
            }

            Button.BindGroup(new List<Button>() { BuildCubeBN, Build1x1BN, Build2x2BN, Build3x3BN });
            foreach (var button in new List<Button>() { BuildCubeBN, Build1x1BN, Build2x2BN, Build3x3BN }) { Overlay.Add(button); }

            var openCatalogBNs = SpriteSheet.CreateNew(MainGame, "Buttons/OpenCatalogBN", 1, 2);

            OpenCatalogCubeBN = new Button(MainGame, openCatalogBNs, null, Position);
            OpenCatalogCubeBN.PositionOffset = new Vector2(144, 54);
            OpenCatalogCubeBN.Clicked += OpenCatalogCubeBN_Clicked;

            OpenCatalog1x1BN = new Button(MainGame, openCatalogBNs, null, Position);
            OpenCatalog1x1BN.PositionOffset = new Vector2(186, 54);
            OpenCatalog1x1BN.Clicked += OpenCatalog1x1BN_Clicked;

            OpenCatalog2x2BN = new Button(MainGame, openCatalogBNs, null, Position);
            OpenCatalog2x2BN.PositionOffset = new Vector2(228, 54);
            OpenCatalog2x2BN.Clicked += OpenCatalog2x2BN_Clicked;

            OpenCatalog3x3BN = new Button(MainGame, openCatalogBNs, null, Position);
            OpenCatalog3x3BN.PositionOffset = new Vector2(270, 54);
            OpenCatalog3x3BN.Clicked += OpenCatalog3x3BN_Clicked;

            Button.BindGroup(new List<Button>() { OpenCatalogCubeBN, OpenCatalog1x1BN, OpenCatalog2x2BN, OpenCatalog3x3BN });
            foreach (var button in new List<Button>() { OpenCatalogCubeBN, OpenCatalog1x1BN, OpenCatalog2x2BN, OpenCatalog3x3BN }) { Overlay.Add(button); }

        }

        private void Build3x3BN_Clicked(object sender, ButtonClickedEventArgs e)
        {
            MapBuilder.BuildMode = BuildMode._3x3;
            MapBuilder.DecoPreview = DecoFactory.CreateCurrentDeco3x3(MainGame, Coord.Zero, Coord.Zero, 0);
            MapBuilder.GhostMarker.MorphImage(MapBuilder.DecoPreview, MainGame, GraphicsDevice, Content);            
            var button = sender as Button; 
            button.IsPressed = true; // Set this (again) here incase click event invoked manually.
        }

        private void Build2x2BN_Clicked(object sender, ButtonClickedEventArgs e)
        {
            MapBuilder.BuildMode = BuildMode._2x2;
            MapBuilder.DecoPreview = DecoFactory.CreateCurrentDeco2x2(MainGame, Coord.Zero, Coord.Zero, 0);
            MapBuilder.GhostMarker.MorphImage(MapBuilder.DecoPreview, MainGame, GraphicsDevice, Content);
            var button = sender as Button;
            button.IsPressed = true;  // Set this (again) here incase click event invoked manually.
        }

        private void Build1x1BN_Clicked(object sender, ButtonClickedEventArgs e)
        {
            MapBuilder.BuildMode = BuildMode._1x1;
            MapBuilder.DecoPreview = DecoFactory.CreateCurrentDeco1x1(MainGame, Coord.Zero, Coord.Zero, 0);
            MapBuilder.GhostMarker.MorphImage(MapBuilder.DecoPreview, MainGame, GraphicsDevice, Content);
            var button = sender as Button;
            button.IsPressed = true;  // Set this (again) here incase click event invoked manually.
        }

        private void BuildCubeBN_Clicked(object sender, ButtonClickedEventArgs e)
        {
            MapBuilder.BuildMode = BuildMode._Cube;
            MapBuilder.GhostMarker.MorphImage(MapBuilder.CubePreview, MainGame, GraphicsDevice, Content);
            var button = sender as Button;
            button.IsPressed = true;  // Set this (again) here incase click event invoked manually.
        }

        private void BuildBN_Clicked(object sender, ButtonClickedEventArgs e) 
        {
            MapBuilder.Authorization = Authorization.Builder; 
        }

        private void WorldBN_Clicked(object sender, ButtonClickedEventArgs e) 
        { 
            MapBuilder.Authorization = Authorization.None; 
        }

        private void OpenCatalog3x3BN_Clicked(object sender, ButtonClickedEventArgs e)
        {
            if (Overlay.Contains(CubeCatalog))
            {
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(CubeCatalog, DeferAction.Remove));
            }
        }

        private void OpenCatalog2x2BN_Clicked(object sender, ButtonClickedEventArgs e)
        {
            if (Overlay.Contains(CubeCatalog))
            {
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(CubeCatalog, DeferAction.Remove));
            }
            else if (Overlay.Contains(Deco1x1Catalog))
            {
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(Deco1x1Catalog, DeferAction.Remove));
            }
            if (Deco2x2Catalog == null)
            {
                // Create for the first time if null. From then on, load from cache.
                var Deco2x2CatalogS = SpriteSheet.CreateNew(MainGame, "GUI/Deco2x2CatalogBackground", 1, 1);
                Deco2x2Catalog = new Deco2x2Catalog(MainGame, Deco2x2CatalogS, new Vector2(3, 87), this, (Button)sender);
                foreach (var dmd in DecoFactory.DecoMetaDataLibrary2x2)
                {
#if DEBUG
                    Debug.Assert(dmd.Dimensions.X == dmd.Dimensions.Y);
#endif
                    var DCI = new DecoCatalogItem(MainGame, dmd);
                    DCI.Clicked += D2x2CI_Clicked;
                    Deco2x2Catalog.Items.Add(DCI);
                }
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(Deco2x2Catalog, DeferAction.Add));
            }
            else if (!Overlay.Contains(Deco2x2Catalog))
            {
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(Deco2x2Catalog, DeferAction.Add));
            }
            Build2x2BN.OnClicked();
        }

        private void D2x2CI_Clicked(object sender, EventArgs e)
        {
            var item = sender as DecoCatalogItem;
            var CDMD = item.DecoMetaData;
            DecoFactory.CurrentDecoMetaData2x2 = CDMD;
            MapBuilder.DecoPreview = DecoFactory.CreateCurrentDeco2x2(MainGame, Coord.Zero, Coord.Zero, 0);
            if (MapBuilder.BuildMode == BuildMode._2x2) { MapBuilder.GhostMarker.MorphImage(MapBuilder.DecoPreview, MainGame, GraphicsDevice, Content); }
        }

        private void OpenCatalog1x1BN_Clicked(object sender, ButtonClickedEventArgs e)
        {
            if (Overlay.Contains(CubeCatalog))
            {
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(CubeCatalog, DeferAction.Remove));
            }
            else if (Overlay.Contains(Deco2x2Catalog))
            {
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(Deco2x2Catalog, DeferAction.Remove));
            }
            if (Deco1x1Catalog == null)
            {
                // Create for the first time if null. From then on, load from cache.
                var Deco1x1CatalogS = SpriteSheet.CreateNew(MainGame, "GUI/Deco1x1CatalogBackground", 1, 1);
                Deco1x1Catalog = new Deco1x1Catalog(MainGame, Deco1x1CatalogS, new Vector2(3, 87), this, (Button)sender);
                foreach (var dmd in DecoFactory.DecoMetaDataLibrary1x1)
                {
#if DEBUG
                    Debug.Assert(dmd.Dimensions.X == dmd.Dimensions.Y);
#endif
                    var DCI = new DecoCatalogItem(MainGame, dmd);
                    DCI.Clicked += D1x1CI_Clicked;
                    Deco1x1Catalog.Items.Add(DCI);               
                }
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(Deco1x1Catalog, DeferAction.Add));
            }
            else if (!Overlay.Contains(Deco1x1Catalog))
            {
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(Deco1x1Catalog, DeferAction.Add));
            }
            Build1x1BN.OnClicked();
        }

        private void D1x1CI_Clicked(object sender, EventArgs e)
        {
            var item = sender as DecoCatalogItem;
            var CDMD = item.DecoMetaData;
            DecoFactory.CurrentDecoMetaData1x1 = CDMD;
            MapBuilder.DecoPreview = DecoFactory.CreateCurrentDeco1x1(MainGame, Coord.Zero, Coord.Zero, 0);
            if (MapBuilder.BuildMode == BuildMode._1x1) { MapBuilder.GhostMarker.MorphImage(MapBuilder.DecoPreview, MainGame, GraphicsDevice, Content); }
        }

        private void OpenCatalogCubeBN_Clicked(object sender, ButtonClickedEventArgs e)
        {
            if (Overlay.Contains(Deco1x1Catalog))
            {
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(Deco1x1Catalog, DeferAction.Remove));
            }
            else if (Overlay.Contains(Deco2x2Catalog))
            {
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(Deco2x2Catalog, DeferAction.Remove));
            }
            if (CubeCatalog == null)
            {
                // Create for the first time if null. From then on, load from cache.
                var cubeCatalogS = SpriteSheet.CreateNew(MainGame, "GUI/CubeCatalogBackground", 1, 1);
                CubeCatalog = new CubeCatalog(MainGame, cubeCatalogS, new Vector2(3, 87), this, (Button)sender);
                foreach (var cmd in CubeFactory.CubeTopMetaDataLibrary)
                {
                    var CCI = new CubeCatalogItem(MainGame, cmd, null);
                    CCI.Clicked += CCI_Clicked;
                    CubeCatalog.Items.Add(CCI);
                }
                foreach (var cbmd in CubeFactory.CubeBaseMetaDataLibrary)
                {
                    var CCI = new CubeCatalogItem(MainGame, null, cbmd);
                    CCI.Clicked += CCI_Clicked;
                    CubeCatalog.Items.Add(CCI);
                }
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(CubeCatalog, DeferAction.Add));
            }
            else if (!Overlay.Contains(CubeCatalog))
            {
                DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(CubeCatalog, DeferAction.Add));
            }
            BuildCubeBN.OnClicked();
        }

        private void CCI_Clicked(object sender, EventArgs e)
        {
            var item = sender as CubeCatalogItem;
            if (item.CubeTopMetaData != null && item.CubeBaseMetaData == null)
            {
                var CCMD = item.CubeTopMetaData;
                CubeFactory.CurrentCubeTopMetaData = CCMD;
                MapBuilder.CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCMD.Path, CCMD.SheetRows, CCMD.SheetColumns));
                MapBuilder.CubePreview.ReconfigureAnimator(CCMD.StartFrame, CCMD.CurrentFrame, CCMD.FrameCount, CCMD.FrameSpeed, CCMD.AnimState);

                if (MapBuilder.BuildMode == BuildMode._Cube) { MapBuilder.GhostMarker.MorphImage(MapBuilder.CubePreview, MainGame, GraphicsDevice, Content); }
            }
            else if (item.CubeBaseMetaData != null && item.CubeTopMetaData == null)
            {
                var CCBMD = item.CubeBaseMetaData;
                CubeFactory.CurrentCubeBaseMetaData = CCBMD;
                MapBuilder.CubePreview.ReplaceSpriteSheet(SpriteSheet.CreateNew(MainGame, CCBMD.Path, CCBMD.SheetRows, CCBMD.SheetColumns), MapBuilder.CubePreview.AnimatorBase);
                MapBuilder.CubePreview.ReconfigureAnimator(CCBMD.StartFrame, CCBMD.CurrentFrame, CCBMD.FrameCount, CCBMD.FrameSpeed, CCBMD.AnimState, MapBuilder.CubePreview.AnimatorBase);

                if (MapBuilder.BuildMode == BuildMode._Cube) { MapBuilder.GhostMarker.MorphImage(MapBuilder.CubePreview, MainGame, GraphicsDevice, Content); }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            foreach (var sprite in Overlay)
            {
                sprite.Draw(gameTime, spriteBatch);
            }
        }

    }
}
