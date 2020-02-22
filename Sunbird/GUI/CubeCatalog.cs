using System;
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

namespace Sunbird.GUI
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
                        Debug.Print(item.Animator.SpriteSheet.TexturePath);
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
            // Offset
            var itemTopOffset = new Vector2(21, 39);
            var itemBaseOffset = new Vector2(21, 234);
            int countCMD = 0;
            int countCBMD = 0;
            int currentSegmentTop = ScrollBarTop.CurrentSegment - 1;
            int currentSegmentBase = ScrollBarBase.CurrentSegment - 1;

            // TODO: Abstract this.
            for (int i = 0; i < Items.Count(); i++)
            {
                var item = Items[i];
                if (item.CubeMetaData != null && item.CubeBaseMetaData == null)
                {
                    item.Position = Position + itemTopOffset + new Vector2(78 * (countCMD % 5), 81 * (countCMD / 5) - currentSegmentTop*81);
                    if (countCMD < currentSegmentTop * 5 || countCMD >= (currentSegmentTop + 2) * 5)
                    {
                        item.IsHidden = true;
                    }
                    else
                    {
                        item.IsHidden = false;
                    }
                    item.Draw(gameTime, spriteBatch);
                    countCMD++;
                }
                else if (item.CubeBaseMetaData != null && item.CubeMetaData == null)
                {
                    item.Position = Position + itemBaseOffset + new Vector2(78 * (countCBMD % 5), 81 * (countCBMD / 5) - currentSegmentBase * 81);
                    if (countCBMD < currentSegmentBase * 5 || countCBMD >= (currentSegmentBase + 2) * 5)
                    {
                        item.IsHidden = true;
                    }
                    else
                    {
                        item.IsHidden = false;
                    }
                    item.Draw(gameTime, spriteBatch);
                    countCBMD++;
                }
            }

            if (countCMD <= 10)
            {
                ScrollBarTop.TotalSegments = 1;
            }
            else
            {
                ScrollBarTop.TotalSegments = (countCMD - 1) / 5;
            }

            if (countCBMD <= 10)
            {
                ScrollBarBase.TotalSegments = 1;
            }
            else
            {
                ScrollBarBase.TotalSegments = (countCBMD - 1) / 5;
            }

            ScrollBarTop.Draw(gameTime, spriteBatch);
            ScrollBarBase.Draw(gameTime, spriteBatch);
            ExitButton.Draw(gameTime, spriteBatch);
        }
    }

    public class CubeCatalogItem : Sprite
    {
        public CubeMetaData CubeMetaData { get; set;}
        public CubeBaseMetaData CubeBaseMetaData { get; set; }

        public CubeCatalogItem(MainGame mainGame, CubeMetaData cubeMD, CubeBaseMetaData cubeBaseMD)
        {
            CubeMetaData = cubeMD;
            CubeBaseMetaData = cubeBaseMD;
#if DEBUG
            if (cubeMD != null) { Debug.Assert(cubeMD.Texture != null); }
            if (cubeBaseMD != null) { Debug.Assert(cubeBaseMD.Texture != null); }
#endif
            if (cubeMD != null && cubeBaseMD == null)
            {
                var spriteSheet = SpriteSheet.CreateNew(cubeMD.Texture, cubeMD.Path, cubeMD.SheetRows, cubeMD.SheetColumns);
                Animator = new Animator(this, spriteSheet, cubeMD.StartFrame, cubeMD.CurrentFrame, cubeMD.FrameCount, cubeMD.FrameSpeed, cubeMD.AnimState);
            }
            else if (cubeBaseMD != null && cubeMD == null)
            {
                var spriteSheet = SpriteSheet.CreateNew(cubeBaseMD.Texture, cubeBaseMD.Path, cubeBaseMD.SheetRows, cubeBaseMD.SheetColumns);
                Animator = new Animator(this, spriteSheet, cubeBaseMD.StartFrame, cubeBaseMD.CurrentFrame, cubeBaseMD.FrameCount, cubeBaseMD.FrameSpeed, cubeBaseMD.AnimState);
            }
            else
            {
                throw new NotImplementedException("allow stitching of cubeMD Texture and cubeBaseMD Texture?");
            }
        }

        public override void OnClicked()
        {
            base.OnClicked();
        }

        public override void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            base.LoadContent(mainGame, graphicsDevice, content);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
        }
    }
}
