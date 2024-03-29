﻿using Models;
using System;
using System.Collections.Generic;

namespace Communication
{
	[Serializable]
	public abstract class Response
	{
		private readonly bool err;
		private readonly string errMsg;

		public Response(bool err, string errMsg)
		{
			this.err = err;
			this.errMsg = errMsg;
		}

		public string ErrMsg => errMsg;

		public bool Error => err;
	}

	[Serializable]
	public class SignResponse : Response
	{
		private readonly string userName;

		public SignResponse(string userName, bool err = false, string errMsg = null) : base(err, errMsg)
		{
			this.userName = userName;
		}

		public string UserName => userName;
	}

	[Serializable]
	public class NewTopicResponse : Response
	{
		public NewTopicResponse(bool err = false, string errMsg = null) : base(err, errMsg) { }
	}

	[Serializable]
	public class GetTopicsResponse : Response
	{
		private readonly List<string> topics;

		public GetTopicsResponse(List<string> topics, bool err = false, string errMsg = null) : base(err, errMsg)
		{
			this.topics = topics;
		}

		public List<string> Topics => topics;
	}

	[Serializable]
	public class JoinTopicResponse : Response
	{
		private readonly Topic topic;

		public JoinTopicResponse(Topic topic, bool err = false, string errMsg = null) : base(err, errMsg)
		{
			this.topic = topic;
		}

		public Topic Topic => topic;
	}

	[Serializable]
	public class NewPublicMessageResponse : Response
	{
		private readonly List<PublicMessage> publicMessages;

		public NewPublicMessageResponse(List<PublicMessage> publicMessages, bool err = false, string errMsg = null) : base(err, errMsg)
		{
			this.publicMessages = publicMessages;
		}

		public List<PublicMessage> PublicMessages => publicMessages;
	}

	[Serializable]
	public class GetUsersResponse : Response
	{
		private readonly List<string> users;

		public GetUsersResponse(List<string> users, bool err = false, string errMsg = null) : base(err, errMsg)
		{
			this.users = users;
		}

		public List<string> Users => users;
	}

	[Serializable]
	public class OpenPrivateDiscussionResponse : Response
	{
		private readonly KeyValuePair<string, List<PrivateMessage>> privateMessages;

		public OpenPrivateDiscussionResponse(KeyValuePair<string, List<PrivateMessage>> privateMessages, bool err = false, string errMsg = null) : base(err, errMsg)
		{
			this.privateMessages = privateMessages;
		}

		public KeyValuePair<string, List<PrivateMessage>> PrivateMessages => privateMessages;
	}

	[Serializable]
	public class NewPrivateMessageResponse : Response
	{
		private readonly List<PrivateMessage> privateMessages;

		public NewPrivateMessageResponse(List<PrivateMessage> privateMessages, bool err = false, string errMsg = null) : base(err, errMsg)
		{
			this.privateMessages = privateMessages;
		}

		public List<PrivateMessage> PrivateMessages => privateMessages;
	}
}
