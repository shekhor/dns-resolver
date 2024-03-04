using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DNSResolver
{
	public class DnsClient
	{
		public static byte[] sendMessage(byte[] dnsMessage)
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			IPAddress dnsServerIp = IPAddress.Parse("8.8.8.8"); // We are manually using the domain name server 8.8.8.8 and port 53.
			IPEndPoint dnsEndPoint = new IPEndPoint(dnsServerIp, 53);

			try
			{
				socket.Connect(dnsEndPoint);
				socket.Send(dnsMessage);

				byte[] responseBuffer = new byte[1024];
				int bytesLength = socket.Receive(responseBuffer);

                ushort receivedId = (ushort)((responseBuffer[0] << 8) | responseBuffer[1]); // First two bytes are the header ID and we are checking if the sent message and received message contain same ID.
                ushort sentId = (ushort)((dnsMessage[0] << 8) | dnsMessage[1]);

                if (receivedId == sentId)
                {
                    Console.WriteLine("Received response with matching ID. ");
                }
                else
                {
                    Console.WriteLine("Received response with non-matching ID. ");
                }
                return responseBuffer;

            }
			catch(SocketException ex)
			{
				Console.WriteLine($"SocketException: {ex.Message}");
                return new byte[] { };
			}
			finally
			{
				socket.Close();
			}
        }

        public static string ExtractDomainName(byte[] responseBuffer, ref int currentPosition)
        {
            StringBuilder domainName = new StringBuilder();

            while (true)
            {
                byte labelLength = responseBuffer[currentPosition++]; // Domain name consists of two octets and first octet is the length and 2nd octet is the part of domain name i.g. 3www.6google.3com.0

                if (labelLength == 0)
                {
                    break;
                }

                if ((labelLength & 0xC0) == 0xC0) // Domain system utilizes some compression method so that it can eliminate the repetition of domain names in a message, if the first two bits of the first octet is 11 then it's a pointer and combining the two octets returns the position of the dns name, refer to RFC 1035 section4.1.4
                {
                    int pointerOffset = ((labelLength & 0x3F) << 8) | responseBuffer[currentPosition++]; //We eliminated the first 2 bits as it represents only if it is pointer or the dns name.
                    int pointerPosition = pointerOffset;
                    return ExtractDomainName(responseBuffer, ref pointerPosition); // It will recursively move to the pointer position.
                }

                if (domainName.Length > 0)
                {
                    domainName.Append('.');
                }

                domainName.Append(Encoding.ASCII.GetString(responseBuffer, currentPosition, labelLength));
                currentPosition += labelLength;
            }

            return domainName.ToString();
        }


        public static void ParseDnsResponse(byte[] responseBuffer)
		{

            bool isResponse = (responseBuffer[2] & 0x80) != 0;

            if (isResponse)
            {
				int currentPosition = 12; // First 12 positions(0-11) are for DNSheader and question.
                ushort answerCount = (ushort)((responseBuffer[6] << 8) | responseBuffer[7]); // 6th and 7th bytes are number of answer records, refer to RFC 1035 section 4.1.1
				Console.WriteLine($"Number of answer: {answerCount}");

                for (int i = 0; i < answerCount; i++)
				{
                    string domainName = ExtractDomainName(responseBuffer, ref currentPosition); //We are extracting the domain name which should be same as the domain name we sent in the DNSmessage.
                    Console.WriteLine(domainName);

                }

                ushort recordType = (ushort)((responseBuffer[currentPosition++] << 8) | responseBuffer[currentPosition++]); //13th & 14th bytes hold the type of RDATA, which is the IP resources refer RFC 1035 section 4.1.3, if the record type is an A record (IPv4 address)

                Console.WriteLine(recordType);
                if(recordType == 1)
                {
                    currentPosition += 6; //We are skipping Class, TTL refer to RFC 1035 section 4.1.3

                    ushort rdLength = (ushort)((responseBuffer[currentPosition++] << 8) | responseBuffer[currentPosition++]); 
                    Console.WriteLine($"Number of ip address is: {rdLength}");


                    for (int i = 0; i < rdLength; i++)
                    {
                        StringBuilder ip = new StringBuilder();

                        string ipAddress = $"{responseBuffer[currentPosition++]}.{responseBuffer[currentPosition++]}.{responseBuffer[currentPosition++]}.{responseBuffer[currentPosition++]}";

                        Console.WriteLine($"Ip address is {ipAddress}");
                    }
                }
                
            }
        }
	}
}

