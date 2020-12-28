using Models;
using System;
using System.Collections.Generic;

namespace Communication
{
    [Serializable]
    public abstract class Response : IMessage
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
        private readonly User user;

        public SignResponse(User user, bool err = false, string errMsg = null) : base(err, errMsg)
        {
            this.user = user;
        }

        public User User => user;
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
    public class GetPublicMessagesResponse : Response
    {
        private readonly List<PublicMessage> publicMessages;

        public GetPublicMessagesResponse(List<PublicMessage> publicMessages, bool err = false, string errMsg = null) : base(err, errMsg)
        {
            this.publicMessages = publicMessages;
        }

        public List<PublicMessage> PublicMessages => publicMessages;
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
        private readonly Topic topic;

        public NewPublicMessageResponse(Topic topic, bool err = false, string errMsg = null) : base(err, errMsg)
        {
            this.topic = topic;
        }

        public Topic Topic => topic;
    }
}
