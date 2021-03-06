using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using SyslogNet.Client.Serialization;
using System.Net.Sockets;

namespace SyslogNet.Client.Transport
{
	public enum MessageTransfer {
		OctetCounting		= 0,
		NonTransparentFraming	= 1
	}

	public class SyslogTcpSender : ISyslogMessageSender, IDisposable
	{
		protected String hostname;
		protected int port;

		protected TcpClient tcpClient = null;
		protected Stream transportStream = null;

		public virtual MessageTransfer messageTransfer { get; set; }
		public byte trailer { get; set; }

		public SyslogTcpSender(string hostname, int port)
		{
			this.hostname = hostname;
			this.port = port;

			Connect();

			messageTransfer = MessageTransfer.OctetCounting;
			trailer = 10; // LF
		}

		protected void Connect()
		{
			try
			{
                tcpClient = new TcpClient();
                tcpClient.ConnectAsync(hostname, port);
				transportStream = tcpClient.GetStream();
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		public virtual void Reconnect()
		{
			Dispose();
			Connect();
		}

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			Send(message, serializer, true);
		}

		protected void Send(SyslogMessage message, ISyslogMessageSerializer serializer, bool flush = true)
		{
			if(transportStream == null)
			{
				throw new IOException("No transport stream exists");
			}

			var datagramBytes = serializer.Serialize(message);

			if (messageTransfer.Equals(MessageTransfer.OctetCounting))
			{
				byte[] messageLength = Encoding.ASCII.GetBytes(datagramBytes.Length.ToString());
				transportStream.Write(messageLength, 0, messageLength.Length);
				transportStream.WriteByte(32); // Space
			}

			transportStream.Write(datagramBytes, 0, datagramBytes.Length);

			if (messageTransfer.Equals(MessageTransfer.NonTransparentFraming))
			{
				transportStream.WriteByte(trailer); // LF
			}

			if (flush && !(transportStream is NetworkStream))
				transportStream.Flush();
		}

		public void Send(IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer)
		{
			foreach (SyslogMessage message in messages)
			{
				Send(message, serializer, false);
			}

			if (!(transportStream is NetworkStream))
				transportStream.Flush();
		}

		public void Dispose()
		{
			if (transportStream != null)
			{
                transportStream.Dispose();
				transportStream = null;
			}

			if (tcpClient != null)
			{
				tcpClient.Dispose();
				tcpClient = null;
			}
		}
	}
}