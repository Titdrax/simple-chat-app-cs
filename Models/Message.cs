using System;

namespace Models
{
    [Serializable]
    public abstract class Message
    {
        private string text;
        private User sender;
        private DateTime time;

        public string Text { get => text; set => text = value; }

        public User Sender { get => sender; set => sender = value; }

        public DateTime Time { get => time; set => time = value; }

        public Message(string txt, User usr)
        {
            Text = txt;
            Sender = usr;
            Time = DateTime.Now;
        }
    }

    [Serializable]
    public class PrivateMessage : Message
    {
        private User receiver;

        public User Receiver { get => receiver; set => receiver = value; }

        public PrivateMessage(string message, User sender, User receiver) : base(message, sender)
        {
            Receiver = receiver;
        }
    }

    [Serializable]
    public class PublicMessage : Message
    {
        private Topic topic;

        public Topic Topic { get => topic; set => topic = value; }

        public PublicMessage(string message, User sender, Topic topic) : base(message, sender)
        {
            Topic = topic;
        }

        public override string ToString()
        {
            return Sender.Login + " - " + Time.Day + "/" + Time.Month + "/" + Time.Year + " " + Time.Hour + ":" + Time.Minute + "\n" + Text + "\n";
        }
    }
}
