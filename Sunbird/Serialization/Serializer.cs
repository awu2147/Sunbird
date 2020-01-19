using Sunbird.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sunbird.Serialization
{
    public static class Serializer
    {
        public static T ReadXML<T>(string path)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T)); // <- Inherited classes of Sprite go here.
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
            XmlSerializer mySerializer = new XmlSerializer(typeof(T));
            StreamWriter myWriter = new StreamWriter(path);
            mySerializer.Serialize(myWriter, self);
            myWriter.Close();
        }

        public static void WriteXML<T>(object self, string path, Type[] extraTypes)
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(T), extraTypes);
            StreamWriter myWriter = new StreamWriter(path);
            mySerializer.Serialize(myWriter, self);
            myWriter.Close();
        }

    }
}
