using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunbird.Core
{
    class EnumStore { }

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

    public enum BuildMode
    {
        _Cube,
        _1x1,
        _2x2,
        _3x3,
    }

    public enum Orientation
    {
        Vertical,
        Horizontal
    }
}
