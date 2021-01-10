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
			string filePath = fileName + ".dat";

			FileStream stream = File.Open(filePath, FileMode.Create);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(stream, objectToWrite);
			stream.Close();
		}

		public static List<T> ReadFromBinaryFile<T>(string fileName)
		{
			string filePath = fileName + ".dat";
			FileStream stream = File.Open(filePath, FileMode.OpenOrCreate);
			BinaryFormatter bf = new BinaryFormatter();
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
