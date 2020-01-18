using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sunbird.States;
using Sunbird.Core;
using Sunbird.Controllers;

namespace Sunbird.Core
{
    public class Fish : Sprite
    {
        public string fishType;

        private Fish()
        {

        }

        public Fish(SpriteSheet spriteSheet) : base (spriteSheet)
        {
            fishType = "goldfish";
        }
    }
}
