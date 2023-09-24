using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;


using UdpClient udpclient = new UdpClient(8001);
IPEndPoint ep = IPEndPoint.Parse("26.189.235.213:8001");
int c = 0;
while (true)
{
    byte[] result = udpclient.Receive(ref ep);

    foreach (byte b in result)
    {
        var a = b + 1;
    }
    c += 1;
    Console.WriteLine(c);
}