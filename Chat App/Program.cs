namespace Chat_App
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var serv = new Server(8976);
            serv.Start();
        }
    }
}
