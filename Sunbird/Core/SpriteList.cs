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
using Sunbird.External;

namespace Sunbird.Core
{
    public class SpriteList<T> : List<T>, IXmlSerializable
    {
        /// <summary>
        /// Current set of occupied coords.
        /// </summary>
        public HashSet<Coord> OccupiedCoords { get; set; } = new HashSet<Coord>();

        /// <summary>
        /// Add to list if coord unoccupied.
        /// </summary>
        /// <param name="sprite">The sprite to be added.</param>
        public void AddCheck(T sprite)
        {
            var s = sprite as Sprite;
            if (!OccupiedCoords.Contains(s.Coords))
            {
                Add(sprite);
                OccupiedCoords.Add(s.Coords);
            }
        }

        public void AddCheckMulti(T sprite, int altitude)
        {
            var multiCube = sprite as MultiCube;
            if (multiCube.OccupiedCoords[altitude].Any((x) => OccupiedCoords.Contains(x)) == false)
            {
                Add(sprite);
                foreach (var coord in multiCube.OccupiedCoords[altitude])
                {
                    OccupiedCoords.Add(coord);
                }
            }
        }

        /// <summary>
        /// Remove from list if coord occupied. Call this instead of Remove() where appropriate for extra safety.
        /// </summary>
        /// <param name="sprite">The sprite to be removed.</param>
        public void RemoveCheck(T sprite)
        {
            var s = sprite as Sprite;
            if (OccupiedCoords.Contains(s.Coords))
            {
                Remove(sprite);
                OccupiedCoords.Remove(s.Coords);
            }
        }

        public void RemoveCheckMulti(T sprite, int altitude)
        {
            var s = sprite as MultiCube;
            if (OccupiedCoords.Contains(s.Coords))
            {
                Remove(sprite);
                foreach (var coord in s.OccupiedCoords[altitude])
                {
                    OccupiedCoords.Remove(coord);
                }
            }
        }

        public XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer spriteSerializer = new XmlSerializer(typeof(T), new Type[] { typeof(GhostMarker), typeof(Player), typeof(Cube), typeof(MultiCube) });
            XmlSerializer occupiedCoordsSerializer = new XmlSerializer(typeof(HashSet<Coord>));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.Name == "Sprite")
                {
                    reader.ReadStartElement("Sprite");
                    T sprite = (T)spriteSerializer.Deserialize(reader);
                    reader.ReadEndElement();
                    this.Add(sprite);
                }

                else if (reader.Name == "OccupiedCoords")
                {
                    reader.ReadStartElement("OccupiedCoords");
                    OccupiedCoords = (HashSet<Coord>)occupiedCoordsSerializer.Deserialize(reader);
                    reader.ReadEndElement();
                }

                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer spriteSerializer = new XmlSerializer(typeof(T), new Type[] { typeof(GhostMarker), typeof(Player), typeof(Cube), typeof(MultiCube) });
            XmlSerializer occupiedCoordsSerializer = new XmlSerializer(typeof(HashSet<Coord>));

            foreach (var sprite in this)
            {
                writer.WriteStartElement("Sprite");
                spriteSerializer.Serialize(writer, sprite);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("OccupiedCoords");
            occupiedCoordsSerializer.Serialize(writer, OccupiedCoords);
            writer.WriteEndElement();
        }

    }
}
