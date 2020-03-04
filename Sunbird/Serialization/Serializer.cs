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

namespace Sunbird.Serialization
{
    public static class Serializer
    {             

        public static Type[] ExtraTypes { get; set; } = new Type[] { };

        public static T ReadXML<T>(XmlSerializer deserializer, string path)
        {
            TextReader reader = new StreamReader(path);
            object obj = deserializer.Deserialize(reader);
            reader.Close();
            return (T)obj;
        }

        public static void WriteXML<T>(XmlSerializer serializer, object self, string path)
        {
            StreamWriter myWriter = new StreamWriter(path);
            serializer.Serialize(myWriter, self);
            myWriter.Close();
        }

        public static XmlSerializer CreateNew(Type type)
        {
            return new XmlSerializer(type, ExtraTypes);
        }

        public static XmlSerializer CreateNew(Type type, Type[] extraTypes)
        {
            return new XmlSerializer(type, extraTypes);
        }
    }
}
