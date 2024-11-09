using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using TCPClientExtensions;

namespace AsyncChat
{
    internal class Program
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        static Random random = new Random();   

        static async Task Main(string[] args)
        {
            await Task3_DoAsyncChatClientWork();
        }

        public async static Task Task3_DoAsyncChatClientWork()
        {

            using (TcpClient client = new TcpClient(SERVER_IP, PORT_NO))
            {
                Task.Run(() => GetServerInput(client));

                while (true)
                {
                    await SendMessageToServer(client);
                }
            }

        }

        private static async Task GetServerInput(TcpClient client)
        {
            while (true) {

                string message = await client.ReadCustomAsync(false);

                if (message != null && message.Length!=0) {
                    Console.WriteLine($"Received message {message}");
                }
            }
        }

        public static async Task SendMessageToServer(TcpClient client)
        {
            string message = await GenerateRandomMessage();
            await ExtendedTCPClient.WriteCustomAsync(client, message, false);
        }

        private static async Task<string> GenerateRandomMessage()
        {
            int messageDelay = random.Next(2000, 8000);
            await Task.Delay(messageDelay);
            return ExtendedTCPClient.GenerateRandomLatinString(20);
        }
        /*
        async static Task SendMessageAsync(TcpClient client)
        {
            // сначала отправляем имя
            //await writer.WriteLineAsync(userName);
            //await writer.FlushAsync();
            Console.WriteLine("Для отправки сообщений введите сообщение и нажмите Enter");

            while (true)
            {
                string? message = Console.ReadLine();
                await client.WriteCustomAsync(message);
                //await writer.FlushAsync();
            }
        }
        // получение сообщений
        async static Task ReceiveMessageAsync(TcpClient client)
        {
            while (true)
            {
                try
                {
                    // считываем ответ в виде строки
                    string? message = await client.ReadCustomAsync();
                    // если пустой ответ, ничего не выводим на консоль
                    if (string.IsNullOrEmpty(message)) continue;
                    Print(message);//вывод сообщения
                }
                catch
                {
                    break;
                }
            }
        }
        // чтобы полученное сообщение не накладывалось на ввод нового сообщения
        static void Print(string message)
        {
            if (OperatingSystem.IsWindows())    // если ОС Windows
            {
                var position = Console.GetCursorPosition(); // получаем текущую позицию курсора
                int left = position.Left;   // смещение в символах относительно левого края
                int top = position.Top;     // смещение в строках относительно верха
                                            // копируем ранее введенные символы в строке на следующую строку
                Console.MoveBufferArea(0, top, left, 1, 0, top + 1);
                // устанавливаем курсор в начало текущей строки
                Console.SetCursorPosition(0, top);
                // в текущей строке выводит полученное сообщение
                Console.WriteLine(message);
                // переносим курсор на следующую строку
                // и пользователь продолжает ввод уже на следующей строке
                Console.SetCursorPosition(left, top + 1);
            }
            else Console.WriteLine(message);
        }

        private static void ConnectAsync(TcpClient client)
        {
            Console.WriteLine(client.ReadCustom());
            string login = Console.ReadLine();
            client.WriteCustom(login, false);
            Console.WriteLine(client.ReadCustom(false));
        }
        */
    }

}
