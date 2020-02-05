using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.GUI;

namespace Sunbird.GUI
{
    public interface ILoadingScreen
    {
        Color BackgroundColor { get; set; }
        Texture2D Background { get; set; }
        List<Sprite> spriteList { get; set; }
        LoadingBar LoadingBar { get; set; }
    }

    public interface ILoadingScreenFactory
    {
        ILoadingScreen CreateLoadingScreen(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content);
    }
}