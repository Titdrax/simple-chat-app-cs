using System;

namespace Communication
{
	public interface IRequest { }

	[Serializable]
	public abstract class SignRequest : IRequest
	{
		private readonly string userName;
		private readonly string password;

		public SignRequest(string userName, string password)
		{
			this.userName = userName;
			this.password = password;
		}

		public string UserName => userName;

		public string Password => password;
	}

	[Serializable]
	public class LoginRequest : SignRequest
	{
		public LoginRequest(string userName, string password) : base(userName, password) { }
	}

	[Serializable]
	public class RegisterRequest : SignRequest
	{
		public RegisterRequest(string userName, string password) : base(userName, password) { }
	}

	[Serializable]
	public class LogoutRequest : SignRequest
	{
		public LogoutRequest(string userName) : base(userName, null) { }
	}

	[Serializable]
	public class NewTopicRequest : IRequest
	{
		private readonly string topicName;

		public NewTopicRequest(string topicName)
		{
			this.topicName = topicName;
		}

		public string TopicName => topicName;
	}

	[Serializable]
	public class GetTopicsRequest : IRequest { }

	[Serializable]
	public class NewPublicMessageRequest : IRequest
	{
		private readonly string message;
		private readonly string userName;
		private readonly string topicName;

		public NewPublicMessageRequest(string message, string userName, string topicName)
		{
			this.message = message;
			this.userName = userName;
			this.topicName = topicName;
		}

		public string Message => message;

		public string UserName => userName;

		public string TopicName => topicName;
	}

	[Serializable]
	public class JoinTopicRequest : IRequest
	{
		private readonly string topicName;
		private readonly string userName;

		public JoinTopicRequest(string topicName, string userName)
		{
			this.topicName = topicName;
			this.userName = userName;
		}

		public string TopicName => topicName;

		public string UserName => userName;
	}

	[Serializable]
	public class ExitTopicRequest : IRequest
	{
		private readonly string userName;

		public ExitTopicRequest(string userName)
		{
			this.userName = userName;
		}

		public string UserName => userName;
	}

	[Serializable]
	public class ClientCloseRequest : IRequest { }

	[Serializable]
	public class GetUsersRequest : IRequest
	{
		private readonly string userName;

		public GetUsersRequest(string userName)
		{
			this.userName = userName;
		}

		public string UserName => userName;
	}

	[Serializable]
	public class OpenPrivateDiscussionRequest : IRequest
	{
		private readonly string chosenUserName;
		private readonly string userName;

		public OpenPrivateDiscussionRequest(string chosenUserName, string userName)
		{
			this.chosenUserName = chosenUserName;
			this.userName = userName;
		}

		public string ChosenUserName => chosenUserName;
		public string UserName => userName;
	}

	[Serializable]
	public class NewPrivateMessageRequest : IRequest
	{
		private readonly string receiverName;
		private readonly string senderName;
		private readonly string message;

		public NewPrivateMessageRequest(string receiverName, string senderName, string message)
		{
			this.receiverName = receiverName;
			this.senderName = senderName;
			this.message = message;
		}

		public string ReceiverName => receiverName;
		public string SenderName => senderName;
		public string Message => message;
	}

	[Serializable]
	public class ExitPrivateDiscussionRequest : IRequest
	{
		private readonly string userName;

		public ExitPrivateDiscussionRequest(string userName)
		{
			this.userName = userName;
		}

		public string UserName => userName;
	}
}
