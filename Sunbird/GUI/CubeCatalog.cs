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
        public List<CubeCatalogItem> Items;

        public CubeCatalog(MainGame mainGame, SpriteSheet spriteSheet, Vector2 position) : base(mainGame, spriteSheet, position)
        {

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
            foreach (var item in Items)
            {
                if (item.CubeMetaData != null && item.CubeBaseMetaData == null)
                {
                    item.Position = new Vector2(0, 4);
                    // position.next()
                    item.Draw(gameTime, spriteBatch);
                }
                else if (item.CubeBaseMetaData != null && item.CubeMetaData == null)
                {

                }
            }
        }
    }

    public class CubeCatalogItem : Sprite
    {
        public CubeMetaData CubeMetaData { get; set;}
        public CubeBaseMetaData CubeBaseMetaData { get; set; }

        public CubeCatalogItem(MainGame mainGame, CubeMetaData cubeMD, CubeBaseMetaData cubeBaseMD, Vector2 position)
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
