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
			var l = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), port);
			l.Start();

			while (true)
			{
				var comm = l.AcceptTcpClient();
				Console.WriteLine("Connection established @" + comm);
				new Thread(new Receiver(comm).DoOperation).Start();
			}
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
				Initialize();

				while (clientRunning)
				{
					var request = (IRequest)Net.RcvMsg(comm.GetStream());

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

						case GetPublicMessagesRequest getPublicMessagesRequest:
							GetPublicMessages(getPublicMessagesRequest);
							break;

						case NewPublicMessageRequest newPublicMessageRequest:
							SendPublicMessage(newPublicMessageRequest);
							break;

						case ExitTopicRequest exitTopicRequest:
							ExitTopic(exitTopicRequest);
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

			private void Register(RegisterRequest registerRequest)
			{
				foreach (var user in users)
				{
					if (registerRequest.User.Login == user.Login)
					{
						Net.SendMsg(comm.GetStream(), new SignResponse(null, true, "Username not available"));
						return;
					}
				}
				var newUser = registerRequest.User;
				newUser.TcpClient = comm;
				users.Add(newUser);

				Net.SendMsg(comm.GetStream(), new SignResponse(newUser));
				Console.WriteLine(newUser.Login + " registered and logged in");

				FileManager.WriteToBinaryFile(users, "users");
			}

			private void Login(LoginRequest loginRequest)
			{
				foreach (var user in users)
				{
					if (loginRequest.User.Login == user.Login && loginRequest.User.Password == user.Password)
					{
						if (user.TcpClient == null)
						{
							user.TcpClient = comm;

							Net.SendMsg(comm.GetStream(), new SignResponse(user));
							Console.WriteLine(user.Login + " logged in");
							return;
						}
						else
						{
							Net.SendMsg(comm.GetStream(), new SignResponse(null, true, "This account is already being used"));
							return;
						}
					}
				}
				Net.SendMsg(comm.GetStream(), new SignResponse(null, true, "This combination of login / password does not exist"));
			}

			private void NewTopic(NewTopicRequest newTopicRequest)
			{
				foreach (var topic in topics)
				{
					if (topic.Name == newTopicRequest.Topic.Name)
					{
						Net.SendMsg(comm.GetStream(), new NewTopicResponse(true, "This topic already exists"));
						return;
					}
				}
				var newTopic = newTopicRequest.Topic;
				topics.Add(newTopic);

				Net.SendMsg(comm.GetStream(), new NewTopicResponse());
				Console.WriteLine("New topic " + newTopic.Name + " created");

				FileManager.WriteToBinaryFile(topics, "topics");
			}

			private void GetTopics()
			{
				if (topics.Count == 0)
				{
					Net.SendMsg(comm.GetStream(), new GetTopicsResponse(null, true, "There is no topic"));
				}
				else
				{
					var topicNames = new List<string>();
					topics.ForEach(delegate (Topic topic)
					{
						topicNames.Add(topic.Name);
					});
					Net.SendMsg(comm.GetStream(), new GetTopicsResponse(topicNames));
				}
			}

			private void JoinTopic(JoinTopicRequest joinTopicRequest)
			{
				foreach (var topic in topics)
				{
					if (topic.Name == joinTopicRequest.Topic.Name)
					{
						joinTopicRequest.User.TcpClient = comm;
						topic.Users.Add(joinTopicRequest.User);
						Console.WriteLine(joinTopicRequest.User.Login + " joined topic " + joinTopicRequest.Topic.Name);
						Net.SendMsg(comm.GetStream(), new JoinTopicResponse(topic));
						return;
					}
				}
				Net.SendMsg(comm.GetStream(), new JoinTopicResponse(null, true, "No such topic"));
			}

			private void GetPublicMessages(GetPublicMessagesRequest getPuublicMessageRequest)
			{
				foreach (var topic in topics)
				{
					if (topic.Name == getPuublicMessageRequest.Topic.Name)
					{
						Console.WriteLine("Joined topic " + topic.Name);
						Net.SendMsg(comm.GetStream(), new GetPublicMessagesResponse(topic.PublicMessages));
						return;
					}
				}
			}

			private void SendPublicMessage(NewPublicMessageRequest newPublicMessageRequest)
			{
				foreach (var topic in topics)
				{
					if (topic.Name == newPublicMessageRequest.PublicMessage.Topic.Name)
					{
						topic.PublicMessages.Add(newPublicMessageRequest.PublicMessage);
						Console.WriteLine(newPublicMessageRequest.PublicMessage.Sender.Login + " sent a message in " + topic.Name);

						FileManager.WriteToBinaryFile(topics, "topics");

						foreach (var user in topic.Users)
						{
							Net.SendMsg(user.TcpClient.GetStream(), new NewPublicMessageResponse(topic));
						}
						return;
					}
				}
			}

			private void ExitTopic(ExitTopicRequest exitTopicRequest)
			{
				foreach (var topic in topics)
				{
					if (topic.Name == exitTopicRequest.Topic.Name)
					{
						foreach (var user in topic.Users)
						{
							if (user.Login == exitTopicRequest.User.Login)
							{
								topic.Users.Remove(user);
								Console.WriteLine(user.Login + " exited topic " + topic.Name);
								return;
							}
						}
					}
				}
			}

			private void Logout(LogoutRequest logoutRequest)
			{
				foreach (var user in users)
				{
					if (user.Login == logoutRequest.User.Login && user.TcpClient != null)
					{
						user.TcpClient = null;
						Console.WriteLine(user.Login + " logged out");

						FileManager.WriteToBinaryFile(users, "users");
						return;
					}
				}
			}

			private void CloseConnection()
			{
				clientRunning = false;
				Console.WriteLine("Connection terminated @" + comm);
				comm.Close();
			}
					
		}
	}
}
