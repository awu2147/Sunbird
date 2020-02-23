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

namespace Sunbird.Core
{
    public static class GraphicsHelper
    {
        public static RenderTarget2D NewRenderTarget2D(GraphicsDevice graphicsDevice)
        {
            return new RenderTarget2D(
            graphicsDevice,
            graphicsDevice.PresentationParameters.BackBufferWidth,
            graphicsDevice.PresentationParameters.BackBufferHeight,
            false,
            graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.None);
        }

        /// <summary>
        /// <para>Takes an original color array and returns a new array which only includes data points contained inside the destination rectangle.</para>
        /// <para>The returned array will be ordered from top-left to bottom-right, like the original. The given the rectangle must lie within the original area.</para>
        /// </summary>
        /// <param name="colorData"> The original color data. </param>
        /// <param name="width"> The original width. </param>
        /// <param name="rectangle"> The rectangular section of data to extract. </param>
        /// <returns></returns>
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

        public static HashSet<Point> SolidPixels(Animator animator)
        {
            // Get the animator spritesheet pixel data.
            var texture = animator.SpriteSheet.Texture;
            var textureTP = texture.Width * texture.Height;
            Color[] textureColorArray = new Color[textureTP];
            texture.GetData(textureColorArray);
            // Reduce to only the visible frame's pixel data.
            Color[] viewAreaColorArray = GetImageData(textureColorArray, texture.Width, animator.SheetViewArea());

            // Resulting set of pixels on which Contains() will be called on.
            var solidPixels = new HashSet<Point>();

            // Analyse all original pixels.
            for (int i = 0; i < viewAreaColorArray.Length; i++)
            {
                if (viewAreaColorArray[i].A != 0)
                {
                    // Add valid pixels.
                    solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth));
                }
            }
            return solidPixels;
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

        #region Obsolete

        [Obsolete("Use the LightingStencil shader effect instead.")]
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
        /// Given an animator and zoom factor, returns a list of Alpha != 0 points for the current frame, normalized as if animator.Position was at (0, 0).
        /// </summary>
        [Obsolete("Use ScaledMousePosition instead when interacting with world objects")]
        private static List<Point> SolidPixels(Animator animator, int zoom)
        {
            // Get the animator spritesheet pixel data.
            var texture = animator.SpriteSheet.Texture;
            var textureTP = texture.Width * texture.Height;
            Color[] textureColorArray = new Color[textureTP];
            texture.GetData(textureColorArray);
            // Reduce to only the visible frame's pixel data.
            Color[] viewAreaColorArray = GetImageData(textureColorArray, texture.Width, animator.SheetViewArea());

            // Resulting set of pixels on which Contains() will be called on.
            var solidPixels = new List<Point>();
            // Take +/- difference between scale and zoom. This decides the translation scale factor.
            int szDifference = zoom - World.Scale;
            // ValidRemainders used to decide which pixels to ignore during culling.
            List<int> validRemainders = new List<int>();
            for (int i = 0; i < Math.Min(zoom, World.Scale); i++) { validRemainders.Add(i); }

            // Analyse all original pixels.
            for (int i = 0; i < viewAreaColorArray.Length; i++)
            {
                // Depending on which Scale x Scale quadrant original pixel lies in, apply a coordinates translation to any added pixels.
                var translation = new Point((i % animator.SpriteSheet.FrameWidth) / World.Scale, (i / animator.SpriteSheet.FrameWidth) / World.Scale) * new Point(szDifference, szDifference);
                if (zoom <= World.Scale)
                {
                    // Ignore pixels not in validRemainders and translate.
                    if (viewAreaColorArray[i].A != 0 && validRemainders.Contains(i % World.Scale) && validRemainders.Contains((i / animator.SpriteSheet.FrameWidth) % World.Scale))
                    {
                        solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth) + translation);
                    }
                }
                else if (zoom > World.Scale)
                {
                    if (viewAreaColorArray[i].A != 0)
                    {
                        // Add all existing and valid pixels.
                        solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth) + translation);
                        // If corner pixel of Scale x Scale quadrant, create a square formation of new pixels extending diagonally outwards.  Number of pixels created = szDifference*szDifference.
                        if (i % World.Scale == (World.Scale - 1) && (i / animator.SpriteSheet.FrameWidth) % World.Scale == (World.Scale - 1))
                        {
                            for (int x = 1; x <= szDifference; x++)
                            {
                                for (int y = 1; y < szDifference; y++)
                                {
                                    solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth + x, i / animator.SpriteSheet.FrameWidth + y) + translation);
                                }
                            }
                        }
                        // Else if bottom edge pixel of Scale x Scale quadrant, create column of new pixels below. Number of pixels created = szDifference.
                        else if (i % World.Scale == (World.Scale - 1))
                        {
                            for (int x = 0; x < szDifference; x++)
                            {
                                solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth + x, i / animator.SpriteSheet.FrameWidth) + translation);
                            }
                        }
                        // Else if right edge pixel of Scale x Scale quadrant, create row of new pixels to the right.  Number of pixels created = szDifference.
                        else if ((i / animator.SpriteSheet.FrameWidth) % World.Scale == (World.Scale - 1))
                        {
                            for (int y = 0; y < szDifference; y++)
                            {
                                solidPixels.Add(new Point(i % animator.SpriteSheet.FrameWidth, i / animator.SpriteSheet.FrameWidth + y) + translation);
                            }
                        }
                    }
                }
            }
            return solidPixels;
        }

        #endregion

    }
}
