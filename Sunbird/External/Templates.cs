using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sunbird.Core;
using Sunbird.GUI;
using Sunbird.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Sunbird.External
{
    public static class Templates
    {
        public static Dictionary<int, ILoadingScreenFactory> LoadingScreenTemplates { get; set; } = new Dictionary<int, ILoadingScreenFactory>();

        public static void InitializeTemplates()
        {
            CreateLoadingScreenTemplates();
        }

        private static void CreateLoadingScreenTemplates()
        {
            LoadingScreenTemplates.Add(0, new LoadingScreen1_Factory());
        }

    }
}
