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

namespace Sunbird.Serialization
{
    public static class Serializer
    {
        public static Type[] ExtraTypes { get; set; } = new Type[] { };

        public static T ReadXML<T>(string path)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T), ExtraTypes); 
            TextReader reader = new StreamReader(path);
            object obj = deserializer.Deserialize(reader);
            reader.Close();
            return (T)obj;
        }

        public static T ReadXML<T>(string path, Type[] extraTypes)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T), extraTypes); // <- Inherited classes of Sprite go here.
            TextReader reader = new StreamReader(path);
            object obj = deserializer.Deserialize(reader);
            reader.Close();
            return (T)obj;
        }

        public static void WriteXML<T>(object self, string path)
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(T), ExtraTypes);
            StreamWriter myWriter = new StreamWriter(path);
            mySerializer.Serialize(myWriter, self);
            myWriter.Close();
        }

        public static void WriteXML<T>(object self, string path, Type[] extraTypes) // <- Inherited classes of Sprite go here.
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(T), extraTypes);
            StreamWriter myWriter = new StreamWriter(path);
            mySerializer.Serialize(myWriter, self);
            myWriter.Close();
        }
    }
}
