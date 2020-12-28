namespace Chat_App
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Server serv = new Server(8976);
            serv.Start();
        }
    }
}
