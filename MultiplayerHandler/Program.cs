using System.Net;
using System.Net.Sockets;
using System.Text;

var tcpListener = new TcpListener(IPAddress.Parse("26.181.15.68"), 8888);

try
{
    tcpListener.Start();
    Console.WriteLine("Сервер запущен. Ожидание подключений... ");

    while (true)
    {
        // получаем подключение в виде TcpClient
        using var tcpClient = await tcpListener.AcceptTcpClientAsync();
        // получаем объект NetworkStream для взаимодействия с клиентом
        var stream = tcpClient.GetStream();
        // буфер для входящих данных
        var buffer = new List<byte>();
        int bytesRead = 10;
        while (true)
        {
            // считываем данные до конечного символа
            while ((bytesRead = stream.ReadByte()) != '\n')
            {
                // добавляем в буфер
                buffer.Add((byte)bytesRead);
            }
            var message = Encoding.UTF8.GetString(buffer.ToArray());
            // если прислан маркер окончания взаимодействия,
            // выходим из цикла и завершаем взаимодействие с клиентом
            if (message == "END") break;
            Console.WriteLine($"Получено сообщение: {message}");
            buffer.Clear();

            byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString());
            await stream.WriteAsync(data);
            Console.WriteLine($"Клиенту {tcpClient.Client.RemoteEndPoint} отправлены данные");
        }
    }
}
finally
{
    tcpListener.Stop();
}