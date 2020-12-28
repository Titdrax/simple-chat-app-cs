using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Communication
{
    public class Net
    {
        public static void SendMsg(Stream s, IMessage msg)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(s, msg);
        }

        public static IMessage RcvMsg(Stream s)
        {
            BinaryFormatter bf = new BinaryFormatter();
            return (IMessage)bf.Deserialize(s);
        }
    }
}
