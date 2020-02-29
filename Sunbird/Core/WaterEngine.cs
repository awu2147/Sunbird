using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.Controllers;

namespace Sunbird.Core
{
    public static class WaterEngine
    {
        public static List<Sprite> WaterNoise1 = new List<Sprite>() { };
        public static List<Sprite> WaterNoise2 = new List<Sprite>() { };

        private static SpriteSheet WaterNoiseSheet1;
        private static SpriteSheet WaterNoiseSheet2;
        private static int SheetWidth;
        private static int SheetHeight;
        private static int WindowWidth;
        private static int WindowHeight;

        public static void LoadContent(MainGame mainGame)
        {
            WaterNoiseSheet1 = SpriteSheet.CreateNew(mainGame, "Effects/Water1");
            WaterNoiseSheet2 = SpriteSheet.CreateNew(mainGame, "Effects/Water2");
#if DEBUG
            Debug.Assert(WaterNoiseSheet1.Texture.Width == WaterNoiseSheet2.Texture.Width);
            Debug.Assert(WaterNoiseSheet1.Texture.Height == WaterNoiseSheet2.Texture.Height);
#endif
            SheetWidth = WaterNoiseSheet1.Texture.Width;
            SheetHeight = WaterNoiseSheet1.Texture.Height;

            WindowWidth = mainGame.Width;
            WindowHeight = mainGame.Height;

            for (int i = -2; i <= (WindowWidth / SheetWidth) + 1; i++)
            {
                for (int j = -2; j <= (WindowHeight / SheetHeight) + 1; j++)
                {
                    var position = new Vector2(SheetWidth * i, SheetHeight * j);
                    var waterNoiseSprite = new Sprite(mainGame, WaterNoiseSheet1, position) { };
                    WaterNoise1.Add(waterNoiseSprite);
                }
            }

            for (int i = 1; i >= -(WindowWidth / SheetWidth) - 2; i--)
            {
                for (int j = 1; j >= -(WindowHeight / SheetHeight) - 2; j--)
                {
                    var offset = new Vector2(WindowWidth, WindowHeight);
                    var position = new Vector2(WindowWidth, WindowHeight) + new Vector2(SheetWidth * i, SheetHeight * j);
                    var waterNoiseSprite = new Sprite(mainGame, WaterNoiseSheet2, position) { Alpha = 0.7f };
                    WaterNoise2.Add(waterNoiseSprite);
                }
            }
        }


    }
}
