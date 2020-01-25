using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunbird.Core
{
    public interface IWorldState
    {
        SpriteList<Sprite> SpriteList { get; set; }
        HashSet<Coord> OccupiedCoords { get; set; }
    }
}
