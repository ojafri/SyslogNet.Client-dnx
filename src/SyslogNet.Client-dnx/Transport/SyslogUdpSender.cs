using System;
using System.Collections.Generic;
using System.Text;
using SyslogNet.Client.Serialization;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace SyslogNet.Client.Transport
{
	public class SyslogUdpSender : ISyslogMessageSender, IDisposable
	{
		private readonly UdpClient udpClient;
        private readonly IPAddress _ipAddress;
        private readonly int _port;

		public SyslogUdpSender(string hostname, int port)
		{
            var ipAddreses = Dns.GetHostAddressesAsync(hostname).Result;
            _ipAddress = ipAddreses[0];
            _port = port;
            udpClient = new UdpClient();
		}

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			byte[] datagramBytes = serializer.Serialize(message);
            var result = udpClient.SendAsync(datagramBytes, datagramBytes.Length, new IPEndPoint(_ipAddress, _port)).Result;
		}

		public void Send(IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer)
		{
			foreach(SyslogMessage message in messages)
			{
				Send(message, serializer);
			}
		}

		public void Reconnect() { /* UDP is connectionless */ }

		public void Dispose()
		{
			udpClient.Dispose();
		}
	}
}