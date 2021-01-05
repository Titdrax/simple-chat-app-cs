using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Chat_App
{
    internal class FileManager
    {
        public static void WriteToBinaryFile<T>(List<T> objectToWrite, string fileName)
        {
            var filePath = fileName + ".dat";

            var stream = File.Open(filePath, FileMode.Create);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, objectToWrite);
            stream.Close();
        }

        public static List<T> ReadFromBinaryFile<T>(string fileName)
        {
            var filePath = fileName + ".dat";
            var stream = File.Open(filePath, FileMode.OpenOrCreate);
            var bf = new BinaryFormatter();
            List<T> obj = default;
            try
            {
                obj = (List<T>)bf.Deserialize(stream);
            }
            catch (SerializationException) { }
            stream.Close();
            return obj;
        }

    }
}
