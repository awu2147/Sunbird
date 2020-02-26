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

namespace Sunbird.External
{
    public class Deco2x2Catalog : Sprite
    {
        public List<DecoCatalogItem> Items = new List<DecoCatalogItem>();

        public Button ExitButton;

        public IGui Sender;
        public Button SenderBN;

        public ScrollBarContainer ScrollBarZ1;
        public ScrollBarContainer ScrollBarZ2;
        public ScrollBarContainer ScrollBarZ3;

        private Deco2x2Catalog()
        {

        }

        public Deco2x2Catalog(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position, IGui sender, Button senderBN) : base(mainGame, spriteSheet, position)
        {
            Sender = sender;
            SenderBN = senderBN;
            ScrollBarZ1 = new ScrollBarContainer(mainGame, "GUI/ScrollBarG", Orientation.Vertical, 81);
            ScrollBarZ1.Position = Position + new Vector2(498, 54);
            ScrollBarZ2 = new ScrollBarContainer(mainGame, "GUI/ScrollBarG", Orientation.Vertical, 117);
            ScrollBarZ2.Position = Position + new Vector2(498, 204);
            ScrollBarZ3 = new ScrollBarContainer(mainGame, "GUI/ScrollBarG", Orientation.Vertical, 150);
            ScrollBarZ3.Position = Position + new Vector2(498, 390);
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
                        MapBuilder.ClickedSpriteName = item.Animator.SpriteSheet.TexturePath;
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

            var z1Partition = new PartitionArgs(new Vector2(21, 39), ScrollBarZ1, new Point(3, 1), new Point(159, 81));
            var z2Partition = new PartitionArgs(new Vector2(21, 189), ScrollBarZ2, new Point(3, 1), new Point(159, 81));
            var z3Partition = new PartitionArgs(new Vector2(21, 375), ScrollBarZ3, new Point(3, 1), new Point(159, 81));

            for (int i = 0; i < Items.Count(); i++)
            {
                var item = Items[i];
                if (item.DecoMetaData.Dimensions.Z == 1)
                {
                    z1Partition.NextItemPosition(item, Position);
                    item.Draw(gameTime, spriteBatch);
                }
                else if (item.DecoMetaData.Dimensions.Z == 2)
                {
                    z2Partition.NextItemPosition(item, Position);
                    item.Draw(gameTime, spriteBatch);
                }
                else if (item.DecoMetaData.Dimensions.Z == 3)
                {
                    z3Partition.NextItemPosition(item, Position);
                    item.Draw(gameTime, spriteBatch);
                }
            }

            z1Partition.RescaleScrollBar();
            z2Partition.RescaleScrollBar();
            z3Partition.RescaleScrollBar();

            ScrollBarZ1.Draw(gameTime, spriteBatch);
            ScrollBarZ2.Draw(gameTime, spriteBatch);
            ScrollBarZ3.Draw(gameTime, spriteBatch);
            ExitButton.Draw(gameTime, spriteBatch);
        }
    }
}
