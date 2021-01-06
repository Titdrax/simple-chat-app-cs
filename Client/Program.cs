namespace Client
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var c1 = new Client("127.0.0.1", 8976);
			c1.Start();
		}
	}
}
