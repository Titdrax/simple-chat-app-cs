using System;

namespace Models
{
	[Serializable]
	public abstract class Message
	{
		private string text;
		private string sender;
		private DateTime time;

		public string Text { get => text; set => text = value; }

		public string Sender { get => sender; set => sender = value; }

		public DateTime Time { get => time; set => time = value; }

		public Message(string txt, string usr)
		{
			Text = txt;
			Sender = usr;
			Time = DateTime.Now;
		}

		public override string ToString()
		{
			return Sender + " - " + Time.Day + "/" + Time.Month + "/" + Time.Year + " " + Time.Hour + ":" + Time.Minute + "\n" + Text + "\n";
		}
	}

	[Serializable]
	public class PrivateMessage : Message
	{
		private string receiver;

		public string Receiver { get => receiver; set => receiver = value; }

		public PrivateMessage(string message, string sender, string receiver) : base(message, sender)
		{
			Receiver = receiver;
		}
	}

	[Serializable]
	public class PublicMessage : Message
	{
		private string topic;

		public string Topic { get => topic; set => topic = value; }

		public PublicMessage(string message, string sender, string topic) : base(message, sender)
		{
			Topic = topic;
		}
	}
}
