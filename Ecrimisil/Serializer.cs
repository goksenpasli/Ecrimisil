using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Ecrimisil
{
    internal static class Serializer
    {
        internal static T DeSerialize<T>(this string xmldatapath) where T : class, new()
        {
            try
            {
                XmlSerializer serializer = new(typeof(T));
                using StreamReader stream = new(xmldatapath);
                return serializer.Deserialize(stream) as T;
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static T DeSerialize<T>(this XElement xElement) where T : class, new()
        {
            XmlSerializer serializer = new(typeof(T));
            return serializer.Deserialize(xElement.CreateReader()) as T;
        }

        internal static ObservableCollection<T> DeSerialize<T>(this IEnumerable<XElement> xElement) where T : class, new()
        {
            ObservableCollection<T> list = [];
            foreach (XElement element in xElement)
            {
                list.Add(element.DeSerialize<T>());
            }
            return list;
        }

        internal static void Serialize<T>(this T dataToSerialize) where T : class
        {
            if (!File.Exists(ViewModel.xmldatapath))
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(ViewModel.xmldatapath));
            }
            XmlSerializer serializer = new(typeof(T));
            using TextWriter stream = new StreamWriter(ViewModel.xmldatapath);
            serializer.Serialize(stream, dataToSerialize);
        }
    }
}
