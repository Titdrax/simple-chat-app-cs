using System;
using System.Collections.Generic;

namespace Models
{
	[Serializable]
	public class Topic
	{
		private string name;
		private readonly List<PublicMessage> publicMessages;
		private List<User> users;

		public string Name { get => name; set => name = value; }

		public List<PublicMessage> PublicMessages => publicMessages;
		public List<User> Users { get => users; set => users = value; }

		public Topic(string name, List<PublicMessage> publicMessages = null)
		{
			this.name = name;
			if (publicMessages == null)
			{
				publicMessages = new List<PublicMessage>();
			}
			this.publicMessages = publicMessages;
			users = new List<User>();
		}
	}
}
