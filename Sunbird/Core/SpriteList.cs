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
        /// <summary>
        /// Add to list if coord unoccupied.
        /// </summary>
        /// <param name="sprite">The sprite to be added.</param>
        /// <param name="gameState">The state instance that this list belongs to.</param>
        public void Add(Sprite sprite, IWorldState gameState)
        {
            if (!gameState.OccupiedCoords.Contains(sprite.Coords))
            {
                gameState.SpriteList.Add(sprite);
                gameState.OccupiedCoords.Add(sprite.Coords);
            }
            else
            {
                //Debug.Print($"Already a sprite at {sprite.Coords}");
            }
        }

        public void Remove(Sprite sprite, IWorldState gameState)
        {
            if (gameState.OccupiedCoords.Contains(sprite.Coords))
            {
                gameState.SpriteList.Remove(sprite);
                gameState.OccupiedCoords.Remove(sprite.Coords);
            }
            else
            {
                //Debug.Print($"Already a sprite at {sprite.Coords}");
            }
        }

    }
}
