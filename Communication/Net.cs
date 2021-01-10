using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Communication
{
	public class Net
	{
		public static void SendRequest(Stream s, IRequest msg)
		{
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(s, msg);
		}

		public static IRequest RcvRequest(Stream s)
		{
			BinaryFormatter bf = new BinaryFormatter();
			return (IRequest)bf.Deserialize(s);
		}

		public static void SendResponse(Stream s, Response msg)
		{
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(s, msg);
		}

		public static Response RcvResponse(Stream s)
		{
			BinaryFormatter bf = new BinaryFormatter();
			return (Response)bf.Deserialize(s);
		}
	}
}
