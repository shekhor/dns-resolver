using System;
using System.Text;

// Refer to the RFC 1035 section 4.1.1 and 4.1.2
namespace DNSResolver
{
	public class DnsMessageBuilder
	{
		public struct DnsHeader
		{
			public ushort ID;
			public ushort Flags;
			public ushort QDCOUNT;
			public ushort ANCOUNT;
			public ushort NSCOUNT;
			public ushort ARCOUNT;
        }

		public struct DnsQuestion
		{
			public string QNAME;
			public ushort QTYPE;
			public ushort QCLASS;

		}

		//We converted the DNS header to array of bytes.
		public static byte[] DnsHeaderToByte(DnsHeader dnsHeader)
		{
			byte[] bytes = new byte[12];
			bytes[0] = (byte) (dnsHeader.ID >> 8);
			bytes[1] = (byte)(dnsHeader.ID);
			bytes[2] = (byte)(dnsHeader.Flags >> 8);
			bytes[3] = (byte)(dnsHeader.Flags);
			bytes[4] = (byte)(dnsHeader.QDCOUNT >> 8);
			bytes[5] = (byte)(dnsHeader.QDCOUNT);
			bytes[6] = (byte)(dnsHeader.ANCOUNT >> 8);
			bytes[7] = (byte)(dnsHeader.ANCOUNT);
			bytes[8] = (byte)(dnsHeader.NSCOUNT >> 8);
			bytes[9] = (byte)(dnsHeader.NSCOUNT);
			bytes[10] = (byte)(dnsHeader.ARCOUNT >> 8);
			bytes[11] = (byte)(dnsHeader.ARCOUNT);

			return bytes;
        }

		//Converted the question to array of bytes.
		public static byte[] DnsQuestionToByte(DnsQuestion dnsQuestion)
		{
			byte[] bytes = new byte[dnsQuestion.QNAME.Length + 6];
			string[] labels = dnsQuestion.QNAME.Split('.');
			int offset = 0;

			foreach(string label in labels)
			{
				bytes[offset++] = (byte)(label.Length);
				foreach(char ch in label)
				{
					bytes[offset++] = (byte) ch;
				}
			}
			bytes[offset++] = 0;
			bytes[offset++] = (byte)(dnsQuestion.QTYPE >> 8);
			bytes[offset++] = (byte)(dnsQuestion.QTYPE);
			bytes[offset++] = (byte)(dnsQuestion.QCLASS >> 8);
			bytes[offset++] = (byte)(dnsQuestion.QCLASS);

			return bytes;
        }

		// Built DNS message combining DNS header and question.
		public static byte[] buildDnsMessage(string domainName)
		{
			DnsHeader dnsHeader = new DnsHeader
			{
				ID = 22,
				Flags = 0 * 0100,
				QDCOUNT = 1,
				ANCOUNT = 0,
				NSCOUNT = 0,
				ARCOUNT = 0
			};

			DnsQuestion dnsQuestion = new DnsQuestion
			{
				QNAME = "www.google.com",
				QTYPE = 1,
				QCLASS = 1
			};

			byte[] dnsHeaderBytes = DnsHeaderToByte(dnsHeader);
			byte[] dnsQuestionBytes = DnsQuestionToByte(dnsQuestion);

			byte[] message = new byte[dnsHeaderBytes.Length + dnsQuestionBytes.Length];

			Buffer.BlockCopy(dnsHeaderBytes, 0, message, 0, dnsHeaderBytes.Length);
			Buffer.BlockCopy(dnsQuestionBytes, 0, message, dnsHeaderBytes.Length, dnsQuestionBytes.Length);

			return message;

        }

		public static void Main(string[] args)
		{
			string domainName = "www.google.com";
			byte[] dnsMessage = buildDnsMessage(domainName);
			Console.Write("DNS message is: ");

			foreach(byte b in dnsMessage)
			{
				Console.Write($"{b:x2}");
			}
			Console.WriteLine();

			byte[] responseBuffer = DnsClient.sendMessage(dnsMessage);
			DnsClient.ParseDnsResponse(responseBuffer);
		}


	}
}