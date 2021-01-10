using Communication;
using Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Client
{
	internal class Client
	{
		private readonly string hostname;
		private readonly int port;
		private TcpClient comm;
		private string userName;
		private string topicName;

		public Client(string h, int p)
		{
			hostname = h;
			port = p;
			comm = null;
			userName = null;
			topicName = null;
		}

		public void Start()
		{
			handler += new EventHandler(Handler);
			SetConsoleCtrlHandler(handler, true);

			comm = new TcpClient(hostname, port);
			Console.WriteLine("Connection established");

			while (!exitSystem)
			{
				Authentication();

				Menu();
			}
		}

		private void Authentication()
		{
			SignResponse signResponse;
			string choice;

			Console.Clear();
			do
			{
				Console.WriteLine("Do you wish to:\n" +
					"1- Login\n" +
					"2- Register");
				choice = Console.ReadLine();

				signResponse = choice switch
				{
					"1" => Login(),
					"2" => Register(),
					_ => new SignResponse(null, true),
				};
			} while (signResponse.Error);
		}

		private void Menu()
		{
			string choice;
			do
			{
				Console.Clear();
				do
				{
					Console.WriteLine("Do you wish to:\n" +
						"1- Create Topic\n" +
						"2- Join Topic\n" +
						"3- Private message\n" +
						"4- Logout");
					choice = Console.ReadLine();
				} while (choice != "1" && choice != "2" && choice != "3" && choice != "4");

				switch (choice)
				{
					case "1":
						CreateTopic();
						break;
					case "2":
						PublicMessage();
						break;
					case "3":
						PrivateMessage();
						break;
					case "4":
						Logout();
						break;
				}
			} while (userName != null);
		}

		private SignResponse Login()
		{
			Console.WriteLine("Login:");
			string login = Console.ReadLine();

			Console.WriteLine("Password:");
			string password = Console.ReadLine();

			Net.SendRequest(comm.GetStream(), new LoginRequest(login, password));
			SignResponse response = (SignResponse)Net.RcvResponse(comm.GetStream());

			if (!response.Error)
			{
				userName = login;
			}
			WriteError(response);

			return response;
		}

		private SignResponse Register()
		{
			Console.WriteLine("Login:");
			string login = Console.ReadLine();

			Console.WriteLine("Password:");
			string password = Console.ReadLine();

			Net.SendRequest(comm.GetStream(), new RegisterRequest(login, password));
			SignResponse response = (SignResponse)Net.RcvResponse(comm.GetStream());

			if (!response.Error)
			{
				userName = login;
			}
			WriteError(response);

			return response;
		}

		private void CreateTopic()
		{
			Console.WriteLine("What will be the topic's name?");
			string name = Console.ReadLine();

			Net.SendRequest(comm.GetStream(), new NewTopicRequest(name));
			NewTopicResponse reponse = (NewTopicResponse)Net.RcvResponse(comm.GetStream());

			Console.Clear();
			WriteError(reponse);
		}

		private void PublicMessage()
		{
			SendPublicMessage(JoinTopic(GetTopics()));
		}

		private void PrivateMessage()
		{
			SendPrivateMessage(OpenDiscussion(GetUsers()));
		}

		private void Logout()
		{
			Net.SendRequest(comm.GetStream(), new LogoutRequest(userName));
			userName = null;
		}

		private GetTopicsResponse GetTopics()
		{
			Net.SendRequest(comm.GetStream(), new GetTopicsRequest());

			return (GetTopicsResponse)Net.RcvResponse(comm.GetStream());
		}

		private JoinTopicResponse JoinTopic(GetTopicsResponse getTopicsResponse)
		{
			if (!getTopicsResponse.Error)
			{
				string choice;
				do
				{
					Console.WriteLine("Which topic do you want to join?");
					foreach (string topic in getTopicsResponse.Topics)
					{
						Console.WriteLine(topic);
					}
					choice = Console.ReadLine();
				} while (!getTopicsResponse.Topics.Contains(choice));

				Net.SendRequest(comm.GetStream(), new JoinTopicRequest(choice, userName));
				return (JoinTopicResponse)Net.RcvResponse(comm.GetStream());
			}
			return new JoinTopicResponse(null, true, getTopicsResponse.ErrMsg);
		}

		private void SendPublicMessage(JoinTopicResponse joinTopicResponse)
		{
			WriteError(joinTopicResponse);

			if (!joinTopicResponse.Error)
			{
				topicName = joinTopicResponse.Topic.Name;

				Thread getMessagesThread = new Thread(new ParameterizedThreadStart(GetPublicMessages));
				getMessagesThread.Start(joinTopicResponse.Topic.PublicMessages);

				string message;
				do
				{
					message = Console.ReadLine();
					Net.SendRequest(comm.GetStream(), new NewPublicMessageRequest(message, userName, topicName));
				} while (message != "EXIT");

				ExitTopic();
			}
		}

		private void GetPublicMessages(object obj)
		{
			if (obj is List<PublicMessage> publicMessages)
			{
				NewPublicMessageResponse newPublicMessageResponse;
				do
				{
					Console.Clear();
					foreach (PublicMessage publicMessage in publicMessages)
					{
						Console.WriteLine(publicMessage);
					}
					Console.WriteLine("Write your message or EXIT if you want to exit the topic");

					newPublicMessageResponse = (NewPublicMessageResponse)Net.RcvResponse(comm.GetStream());

					publicMessages = newPublicMessageResponse.PublicMessages;
				} while (!newPublicMessageResponse.Error);
			}
		}

		private void ExitTopic()
		{
			Net.SendRequest(comm.GetStream(), new ExitTopicRequest(topicName, userName));
			topicName = null;
		}
		private GetUsersResponse GetUsers()
		{
			Net.SendRequest(comm.GetStream(), new GetUsersRequest(userName));

			return (GetUsersResponse)Net.RcvResponse(comm.GetStream());
		}

		private void GetPrivateMessages(object obj)
		{
			if (obj is List<PrivateMessage> privateMessages)
			{
				NewPrivateMessageResponse newPrivateMessageResponse;
				do
				{
					Console.Clear();
					foreach (PrivateMessage privateMessage in privateMessages)
					{
						Console.WriteLine(privateMessage);
					}
					Console.WriteLine("Write your message or EXIT if you want to exit the topic");

					newPrivateMessageResponse = (NewPrivateMessageResponse)Net.RcvResponse(comm.GetStream());
					privateMessages = newPrivateMessageResponse.PrivateMessages;
				} while (!newPrivateMessageResponse.Error);
			}
		}

		private OpenPrivateDiscussionResponse OpenDiscussion(GetUsersResponse getUsersResponse)
		{
			if (!getUsersResponse.Error)
			{
				string choice;
				do
				{
					Console.WriteLine("Who do you want to chat with?");
					foreach (string user in getUsersResponse.Users)
					{
						Console.WriteLine(user);
					}
					choice = Console.ReadLine();
				} while (!getUsersResponse.Users.Contains(choice));

				Net.SendRequest(comm.GetStream(), new OpenPrivateDiscussionRequest(choice, userName));
				return (OpenPrivateDiscussionResponse)Net.RcvResponse(comm.GetStream());
			}
			return new OpenPrivateDiscussionResponse(default, true, getUsersResponse.ErrMsg);
		}

		private void SendPrivateMessage(OpenPrivateDiscussionResponse openPrivateDiscussionResponse)
		{
			WriteError(openPrivateDiscussionResponse);

			if (!openPrivateDiscussionResponse.Error)
			{
				Thread getMessagesThread = new Thread(new ParameterizedThreadStart(GetPrivateMessages));
				getMessagesThread.Start(openPrivateDiscussionResponse.PrivateMessages.Value);

				string message;
				do
				{
					message = Console.ReadLine();
					Net.SendRequest(comm.GetStream(), new NewPrivateMessageRequest(openPrivateDiscussionResponse.PrivateMessages.Key, userName, message));
				} while (message != "EXIT");

				ExitPrivateDiscussions();
			}
		}

		private void ExitPrivateDiscussions()
		{
			Net.SendRequest(comm.GetStream(), new ExitPrivateDiscussionRequest(userName));
		}

		private void WriteError(Response response)
		{
			if (response.Error)
			{
				Console.WriteLine(response.ErrMsg + "\n");
			}
		}


		private bool exitSystem = false;
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
		private delegate bool EventHandler(CtrlType sig);
		private EventHandler handler;

		private enum CtrlType
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		private bool Handler(CtrlType sig)
		{
			if (userName != null)
			{
				if (topicName != null)
				{
					ExitTopic();
				}
				Logout();
			}

			Net.SendRequest(comm.GetStream(), new ClientCloseRequest());
			comm.Close();

			exitSystem = true;

			Environment.Exit(-1);

			return true;
		}
	}
}
