using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;

namespace Sunbird.Core
{
    [Serializable]
    public class SpriteSheet
    {
        [XmlIgnore]
        public Dictionary<int, Point> PositionMap { get; set; }

        [XmlIgnore]
        public Texture2D Texture { get; set; }

        public string TexturePath { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int FrameHeight { get { return Texture.Height / Rows; } }
        public int FrameWidth { get { return Texture.Width / Columns; } }

        private SpriteSheet() { }

        public SpriteSheet(Texture2D texture, int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Texture = texture;
            PositionMap = ConstructPositionMap();
        }

        public void LoadContent(MainGame mainGame, GraphicsDevice graphicsDevice, ContentManager content)
        {
            Texture = content.Load<Texture2D>(TexturePath);
            PositionMap = ConstructPositionMap();
        }

        public static SpriteSheet CreateNew(MainGame mainGame, string path, int row, int columns)
        {
            return new SpriteSheet(mainGame.Content.Load<Texture2D>(path), row, columns) { TexturePath = path };
        }

        public static SpriteSheet CreateNew(MainGame mainGame, string path)
        {
            return CreateNew(mainGame, path, 1, 1);
        }

        public Dictionary<int, Point> ConstructPositionMap()
        {
            var positionMap = new Dictionary<int, Point>();

            var columnlist = new List<int>() { };
            for (int j = 0; j < Rows; j++)
            {
                for (int i = 0; i < Columns; i++)
                    columnlist.Add(i);
            }

            var rowlist = new List<int>() { };
            for (int j = 0; j < Rows; j++)
            {
                for (int i = 0; i < Columns; i++)
                    rowlist.Add(j);
            }

            for (int i = 0; i < columnlist.Count(); i++)
            {
                positionMap.Add(i, new Point(columnlist[i] * FrameWidth, rowlist[i] * FrameHeight));
            }

            return positionMap;
        }
    }
}
