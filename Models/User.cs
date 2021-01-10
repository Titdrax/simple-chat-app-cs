using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Models
{
	[Serializable]
	public class User
	{
		private string login;
		private string password;
		private Dictionary<string, List<PrivateMessage>> privateMessages;

		[NonSerialized]
		private TcpClient tcpClient;
		[NonSerialized]
		private string userInPrivateWith;

		public string Login { get => login; set => login = value; }

		public string Password { get => password; set => password = value; }

		public TcpClient TcpClient { get => tcpClient; set => tcpClient = value; }

		public Dictionary<string, List<PrivateMessage>> PrivateMessages { get => privateMessages; set => privateMessages = value; }

		public string UserInprivateWith { get => userInPrivateWith; set => userInPrivateWith = value; }

		public User(string login, string password, Dictionary<string, List<PrivateMessage>> privateMessages = null, TcpClient tcpClient = null)
		{
			this.login = login;
			this.password = password;
			this.tcpClient = tcpClient;
			if (privateMessages == null)
			{
				privateMessages = new Dictionary<string, List<PrivateMessage>>();
			}
			this.privateMessages = privateMessages;
			userInPrivateWith = null;
		}
	}
}
