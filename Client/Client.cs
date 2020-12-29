using Communication;
using Models;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    internal class Client
    {
        private readonly string hostname;
        private readonly int port;
        private TcpClient comm;
        private User user;

        public Client(string h, int p)
        {
            hostname = h;
            port = p;
            comm = null;
            user = null;
        }

        public void Start()
        {
            comm = new TcpClient(hostname, port);
            Console.WriteLine("Connection established");

            while (true)
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
            } while (user != null);
        }

        private SignResponse Login()
        {
            Console.WriteLine("Login:");
            string login = Console.ReadLine();
            Console.WriteLine("Password:");
            string password = Console.ReadLine();

            Net.SendMsg(comm.GetStream(), new LoginRequest(new User(login, password)));
            SignResponse response = (SignResponse)Net.RcvMsg(comm.GetStream());

            user = response.User;
            WriteError(response);

            return response;
        }

        private SignResponse Register()
        {
            Console.WriteLine("Login:");
            string login = Console.ReadLine();
            Console.WriteLine("Password:");
            string password = Console.ReadLine();

            Net.SendMsg(comm.GetStream(), new RegisterRequest(new User(login, password)));
            SignResponse response = (SignResponse)Net.RcvMsg(comm.GetStream());

            user = response.User;
            WriteError(response);

            return response;
        }

        private void CreateTopic()
        {
            Console.WriteLine("What will be the topic's name?");
            string name = Console.ReadLine();

            Net.SendMsg(comm.GetStream(), new NewTopicRequest(new Topic(name)));
            NewTopicResponse reponse = (NewTopicResponse)Net.RcvMsg(comm.GetStream());

            Console.Clear();
            WriteError(reponse);
        }

        private void PublicMessage()
        {
            SendPublicMessage(JoinTopic(GetTopics()));
        }

        private void PrivateMessage()
        {

        }

        private void Logout()
        {
            Net.SendMsg(comm.GetStream(), new LogoutRequest(user));
            user = null;
        }

        private GetTopicsResponse GetTopics()
        {
            Net.SendMsg(comm.GetStream(), new GetTopicsRequest());
            GetTopicsResponse responseGetTopics = (GetTopicsResponse)Net.RcvMsg(comm.GetStream());

            return responseGetTopics;
        }

        private JoinTopicResponse JoinTopic(GetTopicsResponse responseGetTopics)
        {
            if (!responseGetTopics.Error)
            {
                string choice;
                do
                {
                    Console.WriteLine("Which topic do you want to join?");
                    int i = 1;
                    foreach (string topicName in responseGetTopics.Topics)
                    {
                        Console.WriteLine(topicName);
                        i++;
                    }
                    choice = Console.ReadLine();
                } while (!responseGetTopics.Topics.Contains(choice));

                Net.SendMsg(comm.GetStream(), new JoinTopicRequest(new Topic(choice), user));
                return (JoinTopicResponse)Net.RcvMsg(comm.GetStream());
            }
            return new JoinTopicResponse(null, true, responseGetTopics.ErrMsg);
        }

        private void GetPublicMessages(object obj)
        {
            if (obj is Topic topic)
            {
                while (true)
                {
                    Console.Clear();
                    foreach (PublicMessage publicMessage in topic.PublicMessages)
                    {
                        Console.WriteLine(publicMessage);
                    }
                    Console.WriteLine("Write your message or EXIT if you want to exit the topic");

                    NewPublicMessageResponse newPublicMessageResponse = (NewPublicMessageResponse)Net.RcvMsg(comm.GetStream());
                    topic = newPublicMessageResponse.Topic;
                }
            }
        }

        private void SendPublicMessage(JoinTopicResponse joinTopicResponse)
        {
            WriteError(joinTopicResponse);

            if (!joinTopicResponse.Error)
            {
                string message;

                Thread getMessagesThread = new Thread(new ParameterizedThreadStart(GetPublicMessages));
                getMessagesThread.Start(joinTopicResponse.Topic);

                do
                {
                    message = Console.ReadLine();
                    if (message != "EXIT")
                    {
                        Net.SendMsg(comm.GetStream(), new NewPublicMessageRequest(new PublicMessage(message, user, joinTopicResponse.Topic)));
                    }
                } while (message != "EXIT");

                getMessagesThread.Abort();

                ExitTopic(joinTopicResponse.Topic);
            }
        }

        private void ExitTopic(Topic topic)
        {
            Net.SendMsg(comm.GetStream(), new ExitTopicRequest(topic, user));
        }

        private void WriteError(Response response)
        {
            if (response.Error)
            {
                Console.WriteLine(response.ErrMsg + "\n");
            }
        }
    }
}
