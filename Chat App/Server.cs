using Communication;
using Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chat_App
{
	public class Server
	{
		private readonly int port;
		private static List<User> users = new List<User>();
		private static List<Topic> topics = new List<Topic>();

		public Server(int port)
		{
			this.port = port;
		}

		public void Start()
		{
			Initialize();

			TcpListener l = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), port);
			l.Start();

			while (true)
			{
				TcpClient comm = l.AcceptTcpClient();
				Console.WriteLine("Connection established @" + comm);
				new Thread(new Receiver(comm).DoOperation).Start();
			}
		}

		private void Initialize()
		{
			users = FileManager.ReadFromBinaryFile<User>("users");
			topics = FileManager.ReadFromBinaryFile<Topic>("topics");
			if (users == null)
			{
				users = new List<User>();
			}
			if (topics == null)
			{
				topics = new List<Topic>();
			}
			topics.ForEach(delegate (Topic topic)
			{
				topic.Users = new List<User>();
			});
		}

		private class Receiver
		{
			private readonly TcpClient comm;
			private bool clientRunning;

			public Receiver(TcpClient s)
			{
				comm = s;
				clientRunning = true;
			}

			public void DoOperation()
			{
				while (clientRunning)
				{
					IRequest request = Net.RcvRequest(comm.GetStream());

					switch (request)
					{
						case RegisterRequest registerRequest:
							Register(registerRequest);
							break;

						case LoginRequest loginRequest:
							Login(loginRequest);
							break;

						case NewTopicRequest newTopicRequest:
							NewTopic(newTopicRequest);
							break;

						case GetTopicsRequest _:
							GetTopics();
							break;

						case JoinTopicRequest joinTopicRequest:
							JoinTopic(joinTopicRequest);
							break;

						case NewPublicMessageRequest newPublicMessageRequest:
							SendPublicMessage(newPublicMessageRequest);
							break;

						case ExitTopicRequest exitTopicRequest:
							ExitTopic(exitTopicRequest);
							break;

						case GetUsersRequest getUsersRequest:
							GetUsers(getUsersRequest);
							break;

						case OpenPrivateDiscussionRequest openPrivateDiscussionRequest:
							OpenPrivateDiscussion(openPrivateDiscussionRequest);
							break;

						case NewPrivateMessageRequest newPrivateMessageRequest:
							SendPrivateMessage(newPrivateMessageRequest);
							break;

						case ExitPrivateDiscussionRequest exitPrivateDiscussionRequest:
							ExitPrivateDiscussion(exitPrivateDiscussionRequest);
							break;

						case LogoutRequest logoutRequest:
							Logout(logoutRequest);
							break;

						case ClientCloseRequest _:
							CloseConnection();
							break;

						default:
							break;
					}
				}
			}

			private void Register(RegisterRequest registerRequest)
			{
				if (users.Find(user => registerRequest.UserName == user.Login) != null)
				{
					Net.SendResponse(comm.GetStream(), new SignResponse(null, true, "Username not available"));
					return;
				}

				User newUser = new User(registerRequest.UserName, registerRequest.Password, null, comm);
				users.Add(newUser);

				Net.SendResponse(comm.GetStream(), new SignResponse(newUser.Login));
				Console.WriteLine(newUser.Login + " registered and logged in");
			}

			private void Login(LoginRequest loginRequest)
			{
				User user = users.Find(u => loginRequest.UserName == u.Login && loginRequest.Password == u.Password);

				if (user != null)
				{
					if (user.TcpClient == null)
					{
						user.TcpClient = comm;

						Net.SendResponse(comm.GetStream(), new SignResponse(user.Login));
						Console.WriteLine(user.Login + " logged in");
					}
					else
					{
						Net.SendResponse(comm.GetStream(), new SignResponse(null, true, "This account is already being used"));
					}
				}
				else
				{
					Net.SendResponse(comm.GetStream(), new SignResponse(null, true, "This combination of login / password does not exist"));
				}
			}

			private void NewTopic(NewTopicRequest newTopicRequest)
			{
				if (topics.Find(topic => topic.Name == newTopicRequest.TopicName) != null)
				{
					Net.SendResponse(comm.GetStream(), new NewTopicResponse(true, "This topic already exists"));
					return;
				}

				Topic newTopic = new Topic(newTopicRequest.TopicName);
				topics.Add(newTopic);

				Net.SendResponse(comm.GetStream(), new NewTopicResponse());
				Console.WriteLine("New topic " + newTopic.Name + " created");
			}

			private void GetTopics()
			{
				if (topics.Count > 0)
				{
					List<string> topicNames = new List<string>();
					topics.ForEach(delegate (Topic topic)
					{
						topicNames.Add(topic.Name);
					});
					Net.SendResponse(comm.GetStream(), new GetTopicsResponse(topicNames));
				}
				else
				{
					Net.SendResponse(comm.GetStream(), new GetTopicsResponse(null, true, "There is no topic"));
				}
			}

			private void JoinTopic(JoinTopicRequest joinTopicRequest)
			{
				Topic topic = topics.Find(t => t.Name == joinTopicRequest.TopicName);
				User user = users.Find(u => u.Login == joinTopicRequest.UserName);

				topic.Users.Add(user);
				Console.WriteLine(joinTopicRequest.UserName + " joined topic " + joinTopicRequest.TopicName);
				Net.SendResponse(comm.GetStream(), new JoinTopicResponse(topic));
			}

			private void SendPublicMessage(NewPublicMessageRequest newPublicMessageRequest)
			{
				if (newPublicMessageRequest.Message == "EXIT")
				{
					Net.SendResponse(comm.GetStream(), new NewPublicMessageResponse(null, true));
					return;
				}

				Topic topic = topics.Find(t => t.Name == newPublicMessageRequest.TopicName);

				topic.PublicMessages.Add(new PublicMessage(newPublicMessageRequest.Message, newPublicMessageRequest.UserName, topic.Name));
				Console.WriteLine(newPublicMessageRequest.UserName + " sent a message in " + topic.Name);

				foreach (User userConnected in topic.Users)
				{
					Net.SendResponse(userConnected.TcpClient.GetStream(), new NewPublicMessageResponse(topic.PublicMessages));
				}
			}

			private void ExitTopic(ExitTopicRequest exitTopicRequest)
			{
				Topic topic = topics.Find(t => t.Name == exitTopicRequest.TopicName);
				User user = users.Find(u => u.Login == exitTopicRequest.UserName);

				topic.Users.Remove(user);
				Console.WriteLine(user.Login + " exited topic " + topic.Name);
			}

			private void GetUsers(GetUsersRequest getUsersRequest)
			{
				if (users.Count > 1)
				{
					List<string> userNames = new List<string>();
					users.ForEach(delegate (User user)
					{
						if (getUsersRequest.UserName != user.Login)
						{
							userNames.Add(user.Login);
						}
					});
					Net.SendResponse(comm.GetStream(), new GetUsersResponse(userNames));
				}
				else
				{
					Net.SendResponse(comm.GetStream(), new GetUsersResponse(null, true, "There is no other user than you"));
				}
			}

			private void OpenPrivateDiscussion(OpenPrivateDiscussionRequest openPrivateDiscussionRequest)
			{
				User user = users.Find(u => u.Login == openPrivateDiscussionRequest.UserName);
				User chosenUser = users.Find(u => u.Login == openPrivateDiscussionRequest.ChosenUserName);

				if (!user.PrivateMessages.ContainsKey(chosenUser.Login))
				{
					user.PrivateMessages.Add(chosenUser.Login, new List<PrivateMessage>());
				}
				if (!chosenUser.PrivateMessages.ContainsKey(user.Login))
				{
					chosenUser.PrivateMessages.Add(user.Login, new List<PrivateMessage>());
				}

				foreach (KeyValuePair<string, List<PrivateMessage>> kvp in user.PrivateMessages)
				{
					if (kvp.Key == chosenUser.Login)
					{
						Net.SendResponse(comm.GetStream(), new OpenPrivateDiscussionResponse(kvp));
						Console.WriteLine(openPrivateDiscussionRequest.UserName +
							" entered private discussion with " +
							openPrivateDiscussionRequest.ChosenUserName);

						user.UserInprivateWith = chosenUser.Login;
						return;
					}
				}
			}

			private void SendPrivateMessage(NewPrivateMessageRequest newPrivateMessageRequest)
			{
				if (newPrivateMessageRequest.Message == "EXIT")
				{
					Net.SendResponse(comm.GetStream(), new NewPrivateMessageResponse(null, true));
					return;
				}

				User sender = users.Find(user => user.Login == newPrivateMessageRequest.SenderName);
				User receiver = users.Find(user => user.Login == newPrivateMessageRequest.ReceiverName);

				foreach (KeyValuePair<string, List<PrivateMessage>> kvp in sender.PrivateMessages)
				{
					if (kvp.Key == receiver.Login && sender.UserInprivateWith == receiver.Login)
					{
						kvp.Value.Add(new PrivateMessage(
							newPrivateMessageRequest.Message, newPrivateMessageRequest.SenderName, newPrivateMessageRequest.ReceiverName));

						Net.SendResponse(comm.GetStream(), new NewPrivateMessageResponse(kvp.Value));
						break;
					}
				}

				foreach (KeyValuePair<string, List<PrivateMessage>> kvp in receiver.PrivateMessages)
				{
					if (kvp.Key == sender.Login)
					{
						kvp.Value.Add(new PrivateMessage(
							newPrivateMessageRequest.Message, newPrivateMessageRequest.SenderName, newPrivateMessageRequest.ReceiverName));

						if (receiver.TcpClient != null && receiver.UserInprivateWith == sender.Login)
						{
							Net.SendResponse(receiver.TcpClient.GetStream(), new NewPrivateMessageResponse(kvp.Value));
						}
						break;
					}
				}

				Console.WriteLine(sender.Login + " has sent a message to " + receiver.Login);
			}

			private void ExitPrivateDiscussion(ExitPrivateDiscussionRequest exitPrivateDiscussionRequest)
			{
				User user = users.Find(u => u.Login == exitPrivateDiscussionRequest.UserName);

				Console.WriteLine(user.Login + " exited private discussion with " + user.UserInprivateWith);
				user.UserInprivateWith = null;
			}

			private void Logout(LogoutRequest logoutRequest)
			{
				User user = users.Find(u => u.Login == logoutRequest.UserName && u.TcpClient != null);

				user.TcpClient = null;
				Console.WriteLine(user.Login + " logged out");
			}

			private void CloseConnection()
			{
				clientRunning = false;
				Console.WriteLine("Connection terminated @" + comm);
				comm.Close();

				FileManager.WriteToBinaryFile(topics, "topics");
				FileManager.WriteToBinaryFile(users, "users");
			}

		}
	}
}
