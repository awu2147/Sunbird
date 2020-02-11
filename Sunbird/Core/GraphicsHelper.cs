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

    }
}
