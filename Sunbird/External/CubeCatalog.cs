﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
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
    public interface ICatalogObject
    {
        Texture2D Glyph { get; set; }
    }

    public class CubeCatalog : Sprite
    {
        public List<CubeCatalogItem> Items = new List<CubeCatalogItem>();

        public Button ExitButton;

        public IGui Sender;
        public Button SenderBN;

        public ScrollBarContainer ScrollBarTop;
        public ScrollBarContainer ScrollBarBase;

        private CubeCatalog()
        {

        }

        public CubeCatalog(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position, IGui sender, Button senderBN) : base(mainGame, spriteSheet, position)
        {
            Sender = sender;
            SenderBN = senderBN;
            ScrollBarTop = new ScrollBarContainer(mainGame, "GUI/ScrollBarG", Orientation.Vertical, 126);
            ScrollBarTop.Position = Position + new Vector2(420, 54);
            ScrollBarBase = new ScrollBarContainer(mainGame, "GUI/ScrollBarG", Orientation.Vertical, 126);
            ScrollBarBase.Position = Position + new Vector2(420, 249);
            var exitButtonS = SpriteSheet.CreateNew(mainGame, "Buttons/ExitBN", 1, 2);
            ExitButton = new Button(mainGame, exitButtonS, null, Position + new Vector2(417, 6)) { ButtonType = ButtonType.SafeRelease };
            ExitButton.Clicked += ExitButton_Clicked;
        }

        private void ExitButton_Clicked(object sender, ButtonClickedEventArgs e)
        {
            Sender.DeferredOverlay.Add(new KeyValuePair<Sprite, DeferAction>(this, DeferAction.Remove));
            SenderBN.IsPressed = false;
        }

        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            base.LoadContent(mainGame, graphicsDevice, content);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            foreach (var item in Items)
            {
                // If a cube top or bottom is animated, this call advances the frames.
                item.Update(gameTime);
                // Lead with basic rectangle contains check.
                if (Peripherals.LeftButtonTapped() && item.Animator.WorldArea().Contains(Peripherals.GetMouseWindowPosition()) && MainGame.IsActive && !item.IsHidden)
                {
                    if (GraphicsHelper.SolidPixels(item.Animator).Contains(Peripherals.GetMouseWindowPosition() - item.Animator.Position.ToPoint()) && Peripherals.LeftButtonTapped())
                    {
                        MapBuilder.ClickedSpriteName = item.Animator.SpriteSheet.TexturePath;
                        item.OnClicked();
                    }
                }
            }
            ScrollBarTop.Update(gameTime);
            ScrollBarBase.Update(gameTime);
            ExitButton.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            var topPartition = new PartitionArgs(new Vector2(21, 39), ScrollBarTop, new Point(5, 2), new Point(78, 81));
            var basePartition = new PartitionArgs(new Vector2(21, 234), ScrollBarBase, new Point(5, 2), new Point(78, 81));

            for (int i = 0; i < Items.Count(); i++)
            {
                var item = Items[i];
                if (item.CubeTopMetaData != null && item.CubeBaseMetaData == null)
                {
                    topPartition.NextItemPosition(item, Position);
                    item.Draw(gameTime, spriteBatch);
                }
                else if (item.CubeBaseMetaData != null && item.CubeTopMetaData == null)
                {
                    basePartition.NextItemPosition(item, Position);
                    item.Draw(gameTime, spriteBatch);
                }
            }

            topPartition.RescaleScrollBar();
            basePartition.RescaleScrollBar();

            ScrollBarTop.Draw(gameTime, spriteBatch);
            ScrollBarBase.Draw(gameTime, spriteBatch);
            ExitButton.Draw(gameTime, spriteBatch);
        }
    }

    public class CubeCatalogItem : Sprite
    {
        public CubeMetaData CubeTopMetaData { get; set;}
        public CubeMetaData CubeBaseMetaData { get; set; }

        public CubeCatalogItem(MainGame mainGame, CubeMetaData cubeTopMD, CubeMetaData cubeBaseMD)
        {
            CubeTopMetaData = cubeTopMD;
            CubeBaseMetaData = cubeBaseMD;
#if DEBUG
            if (cubeTopMD != null) { Debug.Assert(cubeTopMD.Texture != null); }
            if (cubeBaseMD != null) { Debug.Assert(cubeBaseMD.Texture != null); }
#endif
            if (cubeTopMD != null && cubeBaseMD == null)
            {
                var spriteSheet = SpriteSheet.CreateNew(cubeTopMD.Texture, cubeTopMD.Path, cubeTopMD.SheetRows, cubeTopMD.SheetColumns);
                Animator = new Animator(this, spriteSheet, cubeTopMD.StartFrame, cubeTopMD.CurrentFrame, cubeTopMD.FrameCount, cubeTopMD.FrameSpeed, cubeTopMD.AnimState);
            }
            else if (cubeBaseMD != null && cubeTopMD == null)
            {
                var spriteSheet = SpriteSheet.CreateNew(cubeBaseMD.Texture, cubeBaseMD.Path, cubeBaseMD.SheetRows, cubeBaseMD.SheetColumns);
                Animator = new Animator(this, spriteSheet, cubeBaseMD.StartFrame, cubeBaseMD.CurrentFrame, cubeBaseMD.FrameCount, cubeBaseMD.FrameSpeed, cubeBaseMD.AnimState);
            }
            else
            {
                throw new NotImplementedException("allow stitching of cubeMD Texture and cubeBaseMD Texture?");
            }
        }

    }
}
