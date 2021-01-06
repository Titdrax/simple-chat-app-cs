using System;
using System.Net.Sockets;

namespace Models
{
	[Serializable]
	public class User
	{
		private string login;
		private string password;

		[NonSerialized]
		private TcpClient tcpClient;

		public string Login { get => login; set => login = value; }

		public string Password { get => password; set => password = value; }

		public TcpClient TcpClient { get => tcpClient; set => tcpClient = value; }

		public User(string login, string password, TcpClient tcpClient = null)
		{
			this.login = login;
			this.password = password;
			this.tcpClient = tcpClient;
		}
	}
}
