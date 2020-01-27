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
using Sunbird.Serialization;

//TODO: LocalOriginToCoord().

namespace Sunbird.Core
{
    public static class World
    {
        private static int Scale { get; set; } = 3;
        private static int TopFaceGridWidth { get; set; } = 24;
        private static int TopFaceGridHeight { get; set; } = 12;

        private static List<Rectangle> TopFaceAreaC { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPoint(1, 5), ScaledPoint(2, 2)),
            new Rectangle(ScaledPoint(3, 4), ScaledPoint(2, 4)),
            new Rectangle(ScaledPoint(5, 3), ScaledPoint(2, 6)),
            new Rectangle(ScaledPoint(7, 2), ScaledPoint(2, 8)),
            new Rectangle(ScaledPoint(9, 1), ScaledPoint(2, 10)),
            new Rectangle(ScaledPoint(11, 0), ScaledPoint(2, 12)),
            new Rectangle(ScaledPoint(13, 1), ScaledPoint(2, 10)),
            new Rectangle(ScaledPoint(15, 2), ScaledPoint(2, 8)),
            new Rectangle(ScaledPoint(17, 3), ScaledPoint(2, 6)),
            new Rectangle(ScaledPoint(19, 4), ScaledPoint(2, 4)),
            new Rectangle(ScaledPoint(21, 5), ScaledPoint(2, 2))
        };
        private static List<Rectangle> TopFaceAreaTL { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPoint(0, 0), ScaledPoint(1, 6)),
            new Rectangle(ScaledPoint(1, 0), ScaledPoint(2, 5)),
            new Rectangle(ScaledPoint(3, 0), ScaledPoint(2, 4)),
            new Rectangle(ScaledPoint(5, 0), ScaledPoint(2, 3)),
            new Rectangle(ScaledPoint(7, 0), ScaledPoint(2, 2)),
            new Rectangle(ScaledPoint(9, 0), ScaledPoint(2, 1))
        };                                                      
        private static List<Rectangle> TopFaceAreaTR { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPoint(13, 0), ScaledPoint(2, 1)),
            new Rectangle(ScaledPoint(15, 0), ScaledPoint(2, 2)),
            new Rectangle(ScaledPoint(17, 0), ScaledPoint(2, 3)),
            new Rectangle(ScaledPoint(19, 0), ScaledPoint(2, 4)),
            new Rectangle(ScaledPoint(21, 0), ScaledPoint(2, 5)),
            new Rectangle(ScaledPoint(23, 0), ScaledPoint(1, 6)),
        };
        private static List<Rectangle> TopFaceAreaBL { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPoint(0, 6), ScaledPoint(1, 6)),
            new Rectangle(ScaledPoint(1, 7), ScaledPoint(2, 5)),
            new Rectangle(ScaledPoint(3, 8), ScaledPoint(2, 4)),
            new Rectangle(ScaledPoint(5, 9), ScaledPoint(2, 3)),
            new Rectangle(ScaledPoint(7, 10), ScaledPoint(2, 2)),
            new Rectangle(ScaledPoint(9, 11), ScaledPoint(2, 1))
        };
        private static List<Rectangle> TopFaceAreaBR { get; set; } = new List<Rectangle>()
        {
            new Rectangle(ScaledPoint(13, 11), ScaledPoint(2, 1)),
            new Rectangle(ScaledPoint(15, 10), ScaledPoint(2, 2)),
            new Rectangle(ScaledPoint(17, 9), ScaledPoint(2, 3)),
            new Rectangle(ScaledPoint(19, 8), ScaledPoint(2, 4)),
            new Rectangle(ScaledPoint(21, 7), ScaledPoint(2, 5)),
            new Rectangle(ScaledPoint(23, 6), ScaledPoint(1, 6)),
        };


        public static Coord GetRelativeCoord(Coord coord, int altitude)
        {
            return coord + (new Coord(1, -1) * altitude);
        }

        public static Coord TopFace_PointToRelativeCoord(Point point, int altitude)
        {
            var normalizedPoint = TopFace_NormalizedPoint(point);
            var offset = TopFace_CoordOffset(normalizedPoint, point);

            var gridCoord = TopFace_PointToGridCoord(point);
            var coord = TopFace_GridCoordToCoord(gridCoord);

            var altitudeOffset = new Coord(1, -1) * altitude;

            return coord + offset + altitudeOffset;
        }

        public static Coord TopFace_PointToRelativeCoord(Camera camera, int altitude)
        {
            return TopFace_PointToRelativeCoord(Peripherals.GetMouseWorldPosition(camera), altitude);
        }

        public static Coord TopFace_PositionToRelativeCoord(Vector2 position, int altitude)
        {
            return TopFace_PointToRelativeCoord(position.ToPoint(), altitude);
        }

        public static Coord TopFace_PointToCoord(Point point)
        {
            return TopFace_PointToRelativeCoord(point, 0);
        }

        public static Coord TopFace_PointToCoord(Camera camera)
        {
            return TopFace_PointToRelativeCoord(Peripherals.GetMouseWorldPosition(camera), 0);
        }

        public static Coord TopFace_PositionToCoord(Vector2 position)
        {
            return TopFace_PointToCoord(position.ToPoint());
        }

        /// <summary>
        /// Maps a point in the world to a position in rectangle of size (Scaled(TopFaceGridWidth), Scaled(TopFaceGridWidth)), with the top left corner at the origin.
        /// </summary>
        /// <param name="gridCoord">Top Face grid coord.</param>
        /// <returns></returns>
        public static Point TopFace_NormalizedPoint(Point point)
        {
            var x = point.X % Scaled(TopFaceGridWidth);
            if (x < 0)
            {
                x = Scaled(TopFaceGridWidth) + x;
            }
            var y = point.Y % Scaled(TopFaceGridHeight);
            if (y < 0)
            {
                y = Scaled(TopFaceGridHeight) + y;
            }
            return new Point(x, y);
        }

        private static Coord TopFace_CoordOffset(Point point, Point worldPoint)
        {
            if (TopFaceAreaC.Any(rect => rect.Contains(point)))
            {
                return new Coord(0, 0);
            }
            else if (TopFaceAreaTL.Any(rect => rect.Contains(point)))
            {
                return new Coord(-1, 0);
            }
            else if (TopFaceAreaTR.Any(rect => rect.Contains(point)))
            {
                return new Coord(0, 1);
            }
            else if (TopFaceAreaBL.Any(rect => rect.Contains(point)))
            {
                return new Coord(0, -1);
            }
            else if (TopFaceAreaBR.Any(rect => rect.Contains(point)))
            {
                return new Coord(1, 0);
            }
            else
            {
                throw new Exception("TopFaceRectangle does not contain point. Check point is normalized and/or TopFaceRectangle components are correct.");
            }
        }

        /// <summary>
        /// Takes a point in the world and returns the corresponding Top Face grid coord.
        /// </summary>
        /// <param name="point">World position as a point.</param>
        /// <returns></returns>
        public static Coord TopFace_PointToGridCoord(Point point)
        {
            var x = point.X / Scaled(TopFaceGridWidth);
            var xRem = point.X % Scaled(TopFaceGridWidth); // We need remainder check to preserve grid edge schema after translation.
            if (point.X < 0 && xRem != 0)
            {
                x -= 1;
            }
            var y = point.Y / Scaled(TopFaceGridHeight);
            var yRem = point.Y % Scaled(TopFaceGridHeight);
            if (point.Y < 0 && yRem != 0)
            {
                y -= 1;
            }
            return new Coord(x, y);
        }

        /// <summary>
        /// Performs a 2x2 matrix multiplication on <paramref name="gridCoord"/>, where M = ([1 , 1], [1, -1]).
        /// </summary>
        /// <param name="gridCoord">Top Face grid coord.</param>
        /// <returns></returns>
        private static Coord TopFace_GridCoordToCoord(Coord gridCoord)
        {
            return new Coord(gridCoord.Y + gridCoord.X, gridCoord.Y * -1 + gridCoord.X);
        }

        /// <summary>
        /// Maps coord to local origin with respect to a Top Face grid.
        /// </summary>
        /// <param name="gridCoord">Top Face grid coord.</param>
        /// <returns></returns>
        public static Vector2 TopFace_CoordToLocalOrigin(Coord coord)
        {
            return ScaledVector(12, 6) * coord.X + ScaledVector(12, -6) * coord.Y;
        }

        public static Point ScaledPoint(int x, int y)
        {
            return new Point(x, y) * new Point(Scale, Scale);
        }

        public static Vector2 ScaledVector(int x, int y)
        {
            return new Vector2(x, y) * Scale;
        }

        public static int Scaled(int value)
        {
            return value * Scale;
        }
    }
}
