using Communication;
using Models;
using System;
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
        private User user;
        private Topic topic;

        public Client(string h, int p)
        {
            hostname = h;
            port = p;
            comm = null;
            user = null;
            topic = null;
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
            } while (user != null);
        }

        private SignResponse Login()
        {
            Console.WriteLine("Login:");
            var login = Console.ReadLine();
            Console.WriteLine("Password:");
            var password = Console.ReadLine();

            Net.SendMsg(comm.GetStream(), new LoginRequest(new User(login, password)));
            var response = (SignResponse)Net.RcvMsg(comm.GetStream());

            user = response.User;
            WriteError(response);

            return response;
        }

        private SignResponse Register()
        {
            Console.WriteLine("Login:");
            var login = Console.ReadLine();
            Console.WriteLine("Password:");
            var password = Console.ReadLine();

            Net.SendMsg(comm.GetStream(), new RegisterRequest(new User(login, password)));
            var response = (SignResponse)Net.RcvMsg(comm.GetStream());

            user = response.User;
            WriteError(response);

            return response;
        }

        private void CreateTopic()
        {
            Console.WriteLine("What will be the topic's name?");
            var name = Console.ReadLine();

            Net.SendMsg(comm.GetStream(), new NewTopicRequest(new Topic(name)));
            var reponse = (NewTopicResponse)Net.RcvMsg(comm.GetStream());

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

            return (GetTopicsResponse)Net.RcvMsg(comm.GetStream());
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
                    foreach (var topicName in responseGetTopics.Topics)
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

        private void SendPublicMessage(JoinTopicResponse joinTopicResponse)
        {
            WriteError(joinTopicResponse);

            if (!joinTopicResponse.Error)
            {
                topic = joinTopicResponse.Topic;
                string message;

                var getMessagesThread = new Thread(new ThreadStart(GetPublicMessages));
                getMessagesThread.Start();

                do
                {
                    message = Console.ReadLine();
                    if (message != "EXIT")
                    {
                        Net.SendMsg(comm.GetStream(), new NewPublicMessageRequest(new PublicMessage(message, user, topic)));
                    }
                } while (message != "EXIT");

                ExitTopic();

                getMessagesThread.Abort();
            }
        }

        private void GetPublicMessages()
        {
            while (topic != null)
            {
                Console.Clear();
                foreach (var publicMessage in topic.PublicMessages)
                {
                    Console.WriteLine(publicMessage);
                }
                Console.WriteLine("Write your message or EXIT if you want to exit the topic");

                var newPublicMessageResponse = (NewPublicMessageResponse)Net.RcvMsg(comm.GetStream());
                topic = newPublicMessageResponse.Topic;
            }
        }

        private void ExitTopic()
        {
            Net.SendMsg(comm.GetStream(), new ExitTopicRequest(topic, user));
            topic = null;
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
            if (user != null)
            {
                if (topic != null)
                {
                    ExitTopic();
                }
                Logout();
            }

            Net.SendMsg(comm.GetStream(), new ClientCloseRequest());
            comm.Close();

            exitSystem = true;

            Environment.Exit(-1);

            return true;
        }
    }
}
