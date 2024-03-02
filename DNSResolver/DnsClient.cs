using System;
using System.Net;
using System.Net.Sockets;
namespace DNSResolver
{
	public class DnsClient
	{
		public static void sendMessage(byte[] dnsMessage)
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			IPAddress dnsServerIp = IPAddress.Parse("8.8.8.8");
			IPEndPoint dnsEndPoint = new IPEndPoint(dnsServerIp, 53);

			try
			{
				socket.Connect(dnsEndPoint);
				socket.Send(dnsMessage);

				byte[] buffer = new byte[1024];
				int bytesLength = socket.Receive(buffer);

				ushort receivedId = (ushort)((buffer[0] << 8) | buffer[1]);
				ushort sentId = (ushort)((dnsMessage[0] << 8) | dnsMessage[1]);

				if(receivedId == sentId)
				{
					Console.WriteLine("Received response with matching ID. ");
				}
				else
				{
                    Console.WriteLine("Received response with non-matching ID. ");
                }
			}
			catch(SocketException ex)
			{
				Console.WriteLine($"SocketException: {ex.Message}");
			}
			finally
			{
				socket.Close();
			}
        }
	}
}

