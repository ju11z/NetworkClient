using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using TCPClientExtensions;

namespace AsyncChat
{
    internal class Program
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        static Random random = new Random();

        private static bool isLoggedIn = false;

        static async Task Main(string[] args)
        {
            await Task3_DoAsyncChatClientWork();
        }

        public async static Task Task3_DoAsyncChatClientWork()
        {

            using (TcpClient client = new TcpClient(SERVER_IP, PORT_NO))
            {
                Task.Run(() => HandleServerResponces(client));

                while (true)
                {
                    //await SendMessageToServer(client);
                }
            }
        }

        private static async Task HandleServerResponces(TcpClient client)
        {
            while (true)
            {
                string message = await client.ReadCustomAsync(false);

                if (message != null && message.Length != 0)
                {
                    bool isServerLoginRequest = await HandleServerLoginResponce(message, client);
                    if (isServerLoginRequest)
                        continue;

                    //...еще методы, которые обрабатывают разные json запросы от сервера 

                    //если в методах выше не смогли сериализовать ответ в json объект, то считаем, что пришла просто строка
                    await HandleServerDefaultResponce(message, client);

                }
            }
        }


        private static async Task HandleServerDefaultResponce(string message, TcpClient client)
        {
            Console.WriteLine($"Server default responce: {message}");
        }

        private static async Task<bool> HandleServerLoginResponce(string message, TcpClient client)
        {
            bool responceRead = false;

            try
            {
                ServerLoginResponce serverLoginResponce = JsonSerializer.Deserialize<ServerLoginResponce>(message);

                responceRead = true;

                if (serverLoginResponce.IsClientLoggedIn)
                {
                    isLoggedIn = true;
                    Console.WriteLine(serverLoginResponce.Message);
                    return responceRead;
                }

                Console.WriteLine(serverLoginResponce.Message);

                string login = Console.ReadLine();

                ClientLoginResponce clientLoginResponce = new ClientLoginResponce(login);

                await client.WriteCustomAsync(JsonSerializer.Serialize(clientLoginResponce), false);
            }
            catch
            {

            }

            return responceRead;
        }

        private static async Task GetServerInput(TcpClient client)
        {
            while (true)
            {

                string message = await client.ReadCustomAsync(false);

                if (message != null && message.Length != 0)
                {
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
    }

}
