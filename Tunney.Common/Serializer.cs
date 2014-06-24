using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tunney
{
    public static class Serializer
    {
        public static byte[] Serialize(object _object)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, _object);
                byte[] retval = new byte[stream.Length];
                stream.Write(retval, 0, (int)stream.Length - 1);
                stream.Close();
                return retval;
            }
        }

        public static void Serialize(object _object, Stream _target)
        {
            IFormatter formatter = new BinaryFormatter();

            {
                formatter.Serialize(_target, _object);
            }
        }

        public static T Deserialize<T>(byte[] _bytes)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream(_bytes))
            {
                T retval = (T)formatter.Deserialize(stream);
                stream.Close();
                return retval;
            }
        }

        public static T Deserialize<T>(Stream _stream)
        {
            IFormatter formatter = new BinaryFormatter();
            T retval = (T)formatter.Deserialize(_stream);
            return retval;
        }

        //public static string SerializeXML(object _object)
        //{
        //    using (StringWriter sw = new StringWriter())
        //    {
        //        XmlSerializer serializer = new XmlSerializer(_object.GetType(), new Type[] {_object.GetType()});
        //        serializer.Serialize(sw, _object);
        //        return sw.ToString();
        //    }
        //}

        //public static T DeserializeXML<T>(string _xml)
        //{
        //    using(StringReader sr = new StringReader(_xml))
        //    {
        //        XmlSerializer serializer = new XmlSerializer(typeof (T));
        //        return (T) serializer.Deserialize(sr);
        //    }
        //}
    }
}