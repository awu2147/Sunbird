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
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

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

        public static SpriteSheet CreateNew(Texture2D texture, string path, int row, int columns)
        {
            return new SpriteSheet(texture, row, columns) { TexturePath = path };
        }

        public static SpriteSheet CreateNew(Texture2D texture, string path)
        {
            return CreateNew(texture, path, 1, 1);
        }

        /// <summary>
        /// This overload creates garbage if called more than once. Use with Care.
        /// </summary>
        public static SpriteSheet CreateNew(MainGame mainGame, string path, int row, int columns)
        {
            return new SpriteSheet(mainGame.Content.Load<Texture2D>(path), row, columns) { TexturePath = path };
        }

        /// <summary>
        /// This overload creates garbage if called more than once. Use with Care.
        /// </summary>
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

        //private IntPtr handle;
        //// Other managed resource this class uses.
        //private Component component = new Component();
        //// Track whether Dispose has been called.
        //private bool disposed = false;

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);

        //}

        //protected virtual void Dispose(bool disposing)
        //{
        //    // Check to see if Dispose has already been called.
        //    if (!this.disposed)
        //    {
        //        // If disposing equals true, dispose all managed
        //        // and unmanaged resources.
        //        if (disposing)
        //        {
        //            // Dispose managed resources.
        //            component.Dispose();
        //        }

        //        // Call the appropriate methods to clean up
        //        // unmanaged resources here.
        //        // If disposing is false,
        //        // only the following code is executed.

        //        Texture.Dispose();
        //        CloseHandle(handle);
        //        handle = IntPtr.Zero;

        //        // Note disposing has been done.
        //        disposed = true;
        //    }
        //}

        //[System.Runtime.InteropServices.DllImport("Kernel32")]
        //private extern static Boolean CloseHandle(IntPtr handle);

        //// Use C# destructor syntax for finalization code.
        //// This destructor will run only if the Dispose method
        //// does not get called.
        //// It gives your base class the opportunity to finalize.
        //// Do not provide destructors in types derived from this class.
        //~SpriteSheet()
        //{
        //    // Do not re-create Dispose clean-up code here.
        //    // Calling Dispose(false) is optimal in terms of
        //    // readability and maintainability.
        //    Dispose(false);
        //}

    }
}
