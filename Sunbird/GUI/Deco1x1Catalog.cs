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
    public class Deco1x1Catalog : Sprite
    {
        public List<DecoCatalogItem> Items = new List<DecoCatalogItem>();

        public Button ExitButton;

        public IGui Sender;
        public Button SenderBN;

        public ScrollBarContainer ScrollBarZ1;
        public ScrollBarContainer ScrollBarZ2;
        public ScrollBarContainer ScrollBarZ3;

        private Deco1x1Catalog()
        {

        }

        public Deco1x1Catalog(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position, IGui sender, Button senderBN) : base(mainGame, spriteSheet, position)
        {
            Sender = sender;
            SenderBN = senderBN;
            ScrollBarZ1 = new ScrollBarContainer(mainGame, "GUI/ScrollBarG", Orientation.Vertical, 126);
            ScrollBarZ1.Position = Position + new Vector2(498, 54);
            ScrollBarZ2 = new ScrollBarContainer(mainGame, "GUI/ScrollBarG", Orientation.Vertical, 81);
            ScrollBarZ2.Position = Position + new Vector2(498, 249);
            ScrollBarZ3 = new ScrollBarContainer(mainGame, "GUI/ScrollBarG", Orientation.Vertical, 117);
            ScrollBarZ3.Position = Position + new Vector2(498, 399);
            var exitButtonS = SpriteSheet.CreateNew(mainGame, "Buttons/ExitBN", 1, 2);
            ExitButton = new Button(mainGame, exitButtonS, null, Position + new Vector2(495, 6)) { ButtonType = ButtonType.SafeRelease };
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
                // If a deco is animated, this call advances the frames.
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
            ScrollBarZ1.Update(gameTime);
            ScrollBarZ2.Update(gameTime);
            ScrollBarZ3.Update(gameTime);
            ExitButton.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            // Offset
            var itemOffsetZ1 = new Vector2(21, 39);
            var itemOffsetZ2 = new Vector2(21, 234);
            var itemOffsetZ3 = new Vector2(21, 384);
            int countZ1 = 0;
            int countZ2 = 0;
            int countZ3 = 0;
            int currentSegmentZ1 = ScrollBarZ1.CurrentSegment - 1;
            int currentSegmentZ2 = ScrollBarZ2.CurrentSegment - 1;
            int currentSegmentZ3 = ScrollBarZ3.CurrentSegment - 1;

            // TODO: Abstract this.
            for (int i = 0; i < Items.Count(); i++)
            {
                var item = Items[i];
                if (item.DecoMetaData.Dimensions.Z == 1)
                {
                    item.Position = Position + itemOffsetZ1 + new Vector2(78 * (countZ1 % 6), 81 * (countZ1 / 6) - currentSegmentZ1 * 81);
                    if (countZ1 < currentSegmentZ1 * 6 || countZ1 >= (currentSegmentZ1 + 2) * 6)
                    {
                        item.IsHidden = true;
                    }
                    else
                    {
                        item.IsHidden = false;
                    }
                    item.Draw(gameTime, spriteBatch);
                    countZ1++;
                }
                else if (item.DecoMetaData.Dimensions.Z == 2)
                {
                    item.Position = Position + itemOffsetZ2 + new Vector2(78 * (countZ2 % 6), 81 * (countZ2 / 6) - currentSegmentZ2 * 81);
                    if (countZ2 < currentSegmentZ2 * 6 || countZ2 >= (currentSegmentZ2 + 2) * 6)
                    {
                        item.IsHidden = true;
                    }
                    else
                    {
                        item.IsHidden = false;
                    }
                    item.Draw(gameTime, spriteBatch);
                    countZ2++;
                }
                else if (item.DecoMetaData.Dimensions.Z == 3)
                {
                    item.Position = Position + itemOffsetZ3 + new Vector2(78 * (countZ3 % 6), 81 * (countZ3 / 6) - currentSegmentZ3 * 81);
                    if (countZ2 < currentSegmentZ3 * 6 || countZ3 >= (currentSegmentZ3 + 2) * 6)
                    {
                        item.IsHidden = true;
                    }
                    else
                    {
                        item.IsHidden = false;
                    }
                    item.Draw(gameTime, spriteBatch);
                    countZ3++;
                }
            }

            if (countZ1 <= 12)
            {
                ScrollBarZ1.TotalSegments = 1;
            }
            else
            {
                ScrollBarZ1.TotalSegments = (countZ1 - 1) / 6;
            }

            if (countZ2 <= 12)
            {
                ScrollBarZ2.TotalSegments = 1;
            }
            else
            {
                ScrollBarZ2.TotalSegments = (countZ2 - 1) / 6;
            }

            if (countZ3 <= 12)
            {
                ScrollBarZ3.TotalSegments = 1;
            }
            else
            {
                ScrollBarZ3.TotalSegments = (countZ3 - 1) / 6;
            }

            ScrollBarZ1.Draw(gameTime, spriteBatch);
            ScrollBarZ2.Draw(gameTime, spriteBatch);
            ScrollBarZ3.Draw(gameTime, spriteBatch);
            ExitButton.Draw(gameTime, spriteBatch);
        }
    }

    public class DecoCatalogItem : Sprite
    {
        public DecoMetaData DecoMetaData { get; set; }

        public DecoCatalogItem(MainGame mainGame, DecoMetaData decoMD)
        {
            DecoMetaData = decoMD;
#if DEBUG
            if (decoMD != null) { Debug.Assert(decoMD.Texture != null); }
#endif
            var spriteSheet = SpriteSheet.CreateNew(decoMD.Texture, decoMD.Path, decoMD.SheetRows, decoMD.SheetColumns);
            Animator = new Animator(this, spriteSheet, decoMD.StartFrame, decoMD.CurrentFrame, decoMD.FrameCount, decoMD.FrameSpeed, decoMD.AnimState);
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
