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

        private CubeCatalog()
        {

        }

        public CubeCatalog(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position, IGui sender, Button senderBN) : base(mainGame, spriteSheet, position)
        {
            Sender = sender;
            SenderBN = senderBN;
            var exitButtonS = SpriteSheet.CreateNew(mainGame, "Buttons/ExitBN", 1, 2);
            ExitButton = new Button(mainGame, exitButtonS, null, Position + new Vector2(399, 6)) { ButtonType = ButtonType.SafeRelease };
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
                if (item.CubeMetaData != null && item.CubeBaseMetaData == null)
                {
                    item.Update(gameTime);
                }
                else if (item.CubeBaseMetaData != null && item.CubeMetaData == null)
                {
                    item.Update(gameTime);
                }
                // Lead with basic rectangle contains check.
                if (Peripherals.LeftButtonTapped() && item.Animator.WorldArea().Contains(Peripherals.GetMouseWindowPosition()) && MainGame.IsActive)
                {
                    if (GraphicsHelper.SolidPixels(item.Animator).Contains(Peripherals.GetMouseWindowPosition() - item.Animator.Position.ToPoint()) && Peripherals.LeftButtonTapped())
                    {
                        Debug.Print(item.Animator.SpriteSheet.TexturePath);
                        item.OnClicked();
                    }
                }
            }
            ExitButton.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            var itemTopOffset = new Vector2(12, 39);
            var itemBaseOffset = new Vector2(12, 234);
            int countCMD = 0;
            int countCBMD = 0;

            for (int i = 0; i < Items.Count(); i++)
            {
                var item = Items[i];
                if (item.CubeMetaData != null && item.CubeBaseMetaData == null)
                {
                    item.Position = Position + itemTopOffset + new Vector2(78 * (countCMD % 5), 81 * (countCMD / 5));
                    item.Draw(gameTime, spriteBatch);
                    countCMD++;
                }
                else if (item.CubeBaseMetaData != null && item.CubeMetaData == null)
                {
                    item.Position = Position + itemBaseOffset + new Vector2(78 * (countCBMD % 5), 81 * (countCBMD / 5));
                    item.Draw(gameTime, spriteBatch);
                    countCBMD++;
                }
            }
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
