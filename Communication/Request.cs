using Models;
using System;

namespace Communication
{
    public interface IRequest : IMessage { }

    [Serializable]
    public abstract class SignRequest : IRequest
    {
        private readonly User user;

        public SignRequest(User user)
        {
            this.user = user;
        }

        public User User => user;
    }

    [Serializable]
    public class LoginRequest : SignRequest
    {
        public LoginRequest(User user) : base(user) { }
    }

    [Serializable]
    public class RegisterRequest : SignRequest
    {
        public RegisterRequest(User user) : base(user) { }
    }

    [Serializable]
    public class LogoutRequest : SignRequest
    {
        public LogoutRequest(User user) : base(user) { }
    }

    [Serializable]
    public class NewTopicRequest : IRequest
    {
        private readonly Topic topic;

        public NewTopicRequest(Topic topic)
        {
            this.topic = topic;
        }

        public Topic Topic => topic;
    }

    [Serializable]
    public class GetTopicsRequest : IRequest
    {
        public GetTopicsRequest() { }
    }

    [Serializable]
    public class GetPublicMessagesRequest : IRequest
    {
        private readonly Topic topic;

        public GetPublicMessagesRequest(Topic topic)
        {
            this.topic = topic;
        }

        public Topic Topic => topic;
    }

    [Serializable]
    public class NewPublicMessageRequest : IRequest
    {
        private readonly PublicMessage publicMessage;

        public NewPublicMessageRequest(PublicMessage publicMessage)
        {
            this.publicMessage = publicMessage;
        }

        public PublicMessage PublicMessage => publicMessage;
    }

    [Serializable]
    public class JoinTopicRequest : IRequest
    {
        private readonly Topic topic;
        private readonly User user;

        public JoinTopicRequest(Topic topic, User user)
        {
            this.topic = topic;
            this.user = user;
        }

        public Topic Topic => topic;

        public User User => user;
    }

    [Serializable]
    public class ExitTopicRequest : IRequest
    {
        private readonly Topic topic;
        private readonly User user;

        public ExitTopicRequest(Topic topic, User user)
        {
            this.topic = topic;
            this.user = user;
        }

        public Topic Topic => topic;

        public User User => user;
    }
}
