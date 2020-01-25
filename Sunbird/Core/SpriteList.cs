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
using System.Xml.Schema;

namespace Sunbird.Core
{
    public class SpriteList<T> : List<T>
    {

        public void Add(Sprite sprite, IWorldState gameState)
        {
            if (!gameState.OccupiedCoords.Contains(sprite.Coords))
            {
                gameState.SpriteList.Add(sprite);
                gameState.OccupiedCoords.Add(sprite.Coords);
            }
            else
            {
                Debug.Print($"Already a sprite at {sprite.Coords} ");
            }
        }

        public void AddIgnore(Sprite sprite, IWorldState gameState)
        {
            if (!gameState.OccupiedCoords.Contains(sprite.Coords))
            {
                gameState.SpriteList.Add(sprite);
            }
            else
            {
                Debug.Print($"Already a sprite at {sprite.Coords} ");
            }
        }

    }
}
