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
using Sunbird.States;
using Sunbird.Core;
using Sunbird.External;
using Sunbird.Controllers;
using Sunbird.Serialization;
using System.Reflection;

namespace Sunbird.Core
{
    public enum Direction
    {
        None,
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    public static class GraphicsHelper
    {
        public static RenderTarget2D NewRenderTarget2D(GraphicsDevice graphicsDevice)
        {
            return new RenderTarget2D(
            graphicsDevice,
            graphicsDevice.PresentationParameters.BackBufferWidth,
            graphicsDevice.PresentationParameters.BackBufferHeight,
            true,
            graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);
        }

        public static void ApplyStencil(Texture2D source, Texture2D stencil, Color color)
        {
            var totalPixels = source.Width * source.Height;
            Color[] stencilPixels = new Color[totalPixels];
            stencil.GetData(stencilPixels);
            Color[] sourcePixels = new Color[totalPixels];
            source.GetData(sourcePixels);

#if DEBUG
            Debug.Assert(stencilPixels.Length == sourcePixels.Length);
#endif

            for (int i = 0; i < stencilPixels.Length; i++)
            {
                if (stencilPixels[i] != Color.Black)
                {
                    sourcePixels[i] = color;
                }
            }

            source.SetData(sourcePixels);
        }

        /// <summary>
        /// Returns a mask texture from a base texture. This creates garbage.
        /// </summary>
        public static Texture2D GetMask(MainGame mainGame, Texture2D texture, Color color)
        {
            var totalPixels = texture.Width * texture.Height;
            Color[] maskPixels = new Color[totalPixels];
            texture.GetData(maskPixels);

            for (int i = 0; i < maskPixels.Length; i++)
            {
                if (maskPixels[i].A != 0)
                {
                    maskPixels[i] = color;
                }
            }

            var mask = new Texture2D(mainGame.GraphicsDevice, texture.Width, texture.Height);
            mask.SetData(maskPixels);
            return mask;
        }

        private static Color[] GetImageData(Color[] colorData, int width, Rectangle rectangle)
        {
            Color[] color = new Color[rectangle.Width * rectangle.Height];
            for (int x = 0; x < rectangle.Width; x++)
            {
                for (int y = 0; y < rectangle.Height; y++)
                {
                    color[x + y * rectangle.Width] = colorData[x + rectangle.X + (y + rectangle.Y) * width];
                }
            }
            return color;
        }

        public static List<Point> SolidPixels(Animator animator, int zoom)
        {
            var texture = animator.SpriteSheet.Texture;
            var textureTP = texture.Width * texture.Height;
            Color[] textureColorArray = new Color[textureTP];
            texture.GetData(textureColorArray);

            Color[] viewAreaColorArray = GetImageData(textureColorArray, texture.Width, animator.SheetViewArea());
            var solidPixels = new List<Point>();
            var colCount = 0;
            var rowCount = 0;
            if (zoom == 1)
            {
                for (int i = 0; i < viewAreaColorArray.Length; i++)
                {
                    if (viewAreaColorArray[i].A != 0 && i % 3 == 0 && (i / animator.SpriteSheet.FrameWidth) % 3 == 0)
                    {
                        solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth) / new Point(3, 3));
                    }
                }


                //var solidPixels1x1 = new List<Point>();
                //for (int i = 0; i < solidPixels.Count(); i++)
                //{
                //    if (i % 3 == 0 && ((i / animator.SpriteSheet.FrameWidth) % 3) == 0)
                //    {
                //        solidPixels1x1.Add(solidPixels[i] / new Point(3, 3));
                //    }
                //}
                //return solidPixels1x1;
            }
            else if (zoom == 2)
            {
                for (int i = 0; i < viewAreaColorArray.Length; i++)
                {
                    if (viewAreaColorArray[i].A != 0 && i % 3 != 2 && (i / animator.SpriteSheet.FrameWidth) % 3 != 2)
                    {                    
                        solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth) - new Point((i % animator.SpriteSheet.FrameWidth) / 3, (i / animator.SpriteSheet.FrameWidth) / 3));
                    }
                }
            }
            else if (zoom == 4)
            {
                for (int i = 0; i < viewAreaColorArray.Length; i++)
                {
                    if (viewAreaColorArray[i].A != 0)
                    {
                        solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth) + new Point((i % animator.SpriteSheet.FrameWidth) / 3, (i / animator.SpriteSheet.FrameWidth) / 3));
                        if (i % 3 == 2 && (i / animator.SpriteSheet.FrameWidth) % 3 == 2)
                        {
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth + 1, i / animator.SpriteSheet.FrameWidth + 1) + new Point((i % animator.SpriteSheet.FrameWidth) / 3, (i / animator.SpriteSheet.FrameWidth) / 3));
                        }
                        else if (i % 3 == 2)
                        {
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth + 1, i / animator.SpriteSheet.FrameWidth) + new Point((i % animator.SpriteSheet.FrameWidth) / 3, (i / animator.SpriteSheet.FrameWidth) / 3));
                        }
                        else if ((i / animator.SpriteSheet.FrameWidth) % 3 == 2)
                        {
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth + 1) + new Point((i % animator.SpriteSheet.FrameWidth) / 3, (i / animator.SpriteSheet.FrameWidth) / 3));
                        }
                    }
                }
            }
            else if (zoom == 5)
            {
                for (int i = 0; i < viewAreaColorArray.Length; i++)
                {
                    if (viewAreaColorArray[i].A != 0)
                    {
                        var pOffset = new Point((i % animator.SpriteSheet.FrameWidth) / 3, (i / animator.SpriteSheet.FrameWidth) / 3) * new Point(2, 2);
                        solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth) + pOffset);
                        if (i % 3 == 2 && (i / animator.SpriteSheet.FrameWidth) % 3 == 2)
                        {
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth + 1, i / animator.SpriteSheet.FrameWidth + 1) + pOffset);
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth + 2, i / animator.SpriteSheet.FrameWidth + 1) + pOffset);
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth + 1, i / animator.SpriteSheet.FrameWidth + 2) + pOffset);
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth + 2, i / animator.SpriteSheet.FrameWidth + 2) + pOffset);
                        }
                        else if (i % 3 == 2)
                        {
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth + 1, i / animator.SpriteSheet.FrameWidth) + pOffset);
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth + 2, i / animator.SpriteSheet.FrameWidth) + pOffset);
                        }
                        else if ((i / animator.SpriteSheet.FrameWidth) % 3 == 2)
                        {
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth + 1) + pOffset);
                            solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth + 2) + pOffset);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < viewAreaColorArray.Length; i++)
                {
                    if (viewAreaColorArray[i].A != 0)
                    {
                        solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth));
                    }
                }
            }
            return solidPixels;
        }

        public static Texture2D SolidPixels2(MainGame mainGame, Animator animator, int zoom)
        {
            var texture = animator.SpriteSheet.Texture;
            var textureTP = texture.Width * texture.Height;
            Color[] textureColorArray = new Color[textureTP];
            texture.GetData(textureColorArray);

            Color[] viewAreaColorArray = GetImageData(textureColorArray, texture.Width, animator.SheetViewArea());
            Color[] zoomColorArray = new Color[(int)(animator.SpriteSheet.FrameWidth * World.ZoomRatio) * (int)(animator.SpriteSheet.FrameWidth * World.ZoomRatio)];
            var colCount = 0;
            var rowCount = 0;
            for (int i = 0; i < viewAreaColorArray.Length; i++)
            {
                if (zoom == 1)
                {
                    if ((colCount == 1) || (colCount == 2) || (rowCount == 1) || (rowCount == 2))
                    {

                    }
                    else
                    {
                        if (viewAreaColorArray[i].A != 0)
                        {
                            viewAreaColorArray[i] = Color.Black;
                        }
                        zoomColorArray[i] = (viewAreaColorArray[i]);
                        colCount = 0;
                        rowCount = (i / animator.SpriteSheet.FrameWidth) % 3;
                    }
                    colCount++;
                }
                //else if (zoom == 2 && (count == 1))
                //{

                //}
                //else
                //{
                //    if (viewAreaColorArray[i].A != 0)
                //    {
                //        solidPixels.Add(new Point((int)((i % animator.SpriteSheet.FrameWidth) * World.ZoomRatio), (int)(i / (animator.SpriteSheet.FrameWidth * World.ZoomRatio))));
                //    }
                //}
            }
            var mask = new Texture2D(mainGame.GraphicsDevice, (int)(animator.SpriteSheet.FrameWidth * World.ZoomRatio), (int)(animator.SpriteSheet.FrameWidth * World.ZoomRatio));
            mask.SetData(viewAreaColorArray);
            return mask;
        }

        public static Texture2D GetAntiShadow(MainGame mainGame, Texture2D texture)
        {
            return GetMask(mainGame, texture, Color.Black);
        }

        public static Texture2D GetSelfShadow(MainGame mainGame, Texture2D texture)
        {
            return GetMask(mainGame, texture, new Color(109, 117, 141));
        }

        /// <summary>
        /// Generate AntiShadow and SelfShadow textures from a base Texture.
        /// </summary>
        /// <param name="mainGame"></param>
        public static void GenerateShadowTextures(MainGame mainGame, Texture2D texture, ref Texture2D antiShadow, ref Texture2D selfShadow)
        {
            antiShadow = GetAntiShadow(mainGame, texture);
            selfShadow = GetSelfShadow(mainGame, texture);
        }

    }
}
