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
using System.Globalization;

namespace Sunbird.Core
{
    public struct Coord
    {
        public static Coord Zero { get; }

        private int x;
        private int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public static Coord operator +(Coord c1, Coord c2)
        {
            return new Coord(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static Coord operator -(Coord c1, Coord c2)
        {
            return new Coord(c1.X - c2.X, c1.Y - c2.Y);
        }

        public static Coord operator *(Coord c1, Coord c2)
        {
            return new Coord(c1.X * c2.X, c1.Y * c2.Y);
        }

        public static Coord operator *(Coord c1, int i)
        {
            return new Coord(c1.X * i, c1.Y * i);
        }

        public static Coord operator /(Coord c1, Coord c2)
        {
            return new Coord(c1.X / c2.X, c1.Y / c2.Y);
        }

        public static bool operator ==(Coord left, Coord right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Coord left, Coord right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return unchecked(x ^ y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Coord)) return false;
            Coord comp = (Coord)obj;
            return comp.X == this.X && comp.Y == this.Y;
        }

        public override string ToString()
        {
            return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) + "}";
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }

    }
}
