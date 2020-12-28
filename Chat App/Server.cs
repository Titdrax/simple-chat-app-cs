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
            TcpListener l = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), port);
            l.Start();

            while (true)
            {
                TcpClient comm = l.AcceptTcpClient();
                Console.WriteLine("Connection established @" + comm);
                new Thread(new Receiver(comm).DoOperation).Start();
            }
        }

        private class Receiver
        {
            private readonly TcpClient comm;

            public Receiver(TcpClient s)
            {
                comm = s;
            }

            public void DoOperation()
            {
                Initialize();

                while (true)
                {
                    IRequest request = (IRequest)Net.RcvMsg(comm.GetStream());

                    if (request is RegisterRequest registerRequest)
                    {
                        Register(registerRequest);
                    }
                    else if (request is LoginRequest loginRequest)
                    {
                        Login(loginRequest);
                    }
                    else if (request is NewTopicRequest newTopicRequest)
                    {
                        NewTopic(newTopicRequest);
                    }
                    else if (request is GetTopicsRequest)
                    {
                        GetTopics();
                    }
                    else if (request is JoinTopicRequest joinTopicRequest)
                    {
                        JoinTopic(joinTopicRequest);
                    }
                    else if (request is GetPublicMessagesRequest publicMessagesRequest)
                    {
                        GetPublicMessages(publicMessagesRequest);
                    }
                    else if (request is NewPublicMessageRequest sendPublicMessageRequest)
                    {
                        SendPublicMessage(sendPublicMessageRequest);
                    }
                    else if (request is ExitTopicRequest exitTopicRequest)
                    {
                        ExitTopic(exitTopicRequest);
                    }
                    else if (request is LogoutRequest logoutRequest)
                    {
                        Logout(logoutRequest);
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
                foreach (User user in users)
                {
                    if (registerRequest.User.Login == user.Login)
                    {
                        Net.SendMsg(comm.GetStream(), new SignResponse(null, true, "Username not available"));
                        return;
                    }
                }
                User newUser = registerRequest.User;
                newUser.TcpClient = comm;
                users.Add(newUser);

                Net.SendMsg(comm.GetStream(), new SignResponse(newUser));
                Console.WriteLine(newUser.Login + " registered and logged in");

                FileManager.WriteToBinaryFile(users, "users");
            }

            private void Login(LoginRequest loginRequest)
            {
                foreach (User user in users)
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
                foreach (Topic t in topics)
                {
                    if (t.Name == newTopicRequest.Topic.Name)
                    {
                        Net.SendMsg(comm.GetStream(), new NewTopicResponse(true, "This topic already exists"));
                        return;
                    }
                }
                Topic newTopic = newTopicRequest.Topic;
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
                    List<string> topicNames = new List<string>();
                    topics.ForEach(delegate (Topic topic)
                    {
                        topicNames.Add(topic.Name);
                    });
                    Net.SendMsg(comm.GetStream(), new GetTopicsResponse(topicNames));
                }
            }

            private void JoinTopic(JoinTopicRequest joinTopicRequest)
            {
                foreach (Topic topic in topics)
                {
                    if (topic.Name == joinTopicRequest.Topic.Name)
                    {
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
                foreach (Topic topic in topics)
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
                foreach (Topic topic in topics)
                {
                    if (topic.Name == newPublicMessageRequest.PublicMessage.Topic.Name)
                    {
                        topic.PublicMessages.Add(newPublicMessageRequest.PublicMessage);
                        Console.WriteLine(newPublicMessageRequest.PublicMessage.Sender.Login + " sent a message in " + topic.Name +
                            "\nNotification sent to every user in this topic");

                        FileManager.WriteToBinaryFile(topics, "topics");

                        foreach (User user in topic.Users)
                        {
                            Net.SendMsg(user.TcpClient.GetStream(), new NewPublicMessageResponse(topic));
                        }
                        return;
                    }
                }
            }

            private void ExitTopic(ExitTopicRequest exitTopicRequest)
            {
                foreach (Topic topic in topics)
                {
                    if (topic.Name == exitTopicRequest.Topic.Name)
                    {
                        foreach (User user in topic.Users)
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
                foreach (User user in users)
                {
                    if (user.Login == logoutRequest.User.Login)
                    {
                        user.TcpClient = null;
                        Console.WriteLine(user.Login + " logged out");

                        FileManager.WriteToBinaryFile(users, "users");
                        return;
                    }
                }
            }
        }
    }
}
