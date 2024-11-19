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
        private const int askMessagesIntervalMs = 10000;

        private static bool isLoggedIn = false;
        private static string currentLogin;


        static async Task Main(string[] args)
        {
            await Task3_DoAsyncChatClientWork();
        }

        public async static Task Task3_DoAsyncChatClientWork()
        {

            using (TcpClient client = new TcpClient(SERVER_IP, PORT_NO))
            {
                Task.Run(() => HandleServerResponces(client));
                Task.Run(() => HandleUserInput(client));
                Task.Run(() => AskServerForNewMessages(client));
            }
        }

        private static async Task AskServerForNewMessages(TcpClient client)
        {
            while (true)
            {
                if (isLoggedIn)
                {
                    Console.WriteLine("Asking server for new messages");

                    AskForNewMessagesRequest askForNewMessagesRequest = new AskForNewMessagesRequest(currentLogin);
                    await client.WriteCustomAsync(JsonSerializer.Serialize(askForNewMessagesRequest), false);
                    await Task.Delay(askMessagesIntervalMs);
                }

            }
        }

        private static async Task HandleUserInput(TcpClient client)
        {
            while (true)
            {
                if (isLoggedIn)
                {
                    Console.WriteLine("Type 1 to send message.\n");
                    string input = Console.ReadLine();

                    switch (input.Trim())
                    {
                        case "1":
                            {
                                Console.WriteLine("Input receiver login:");
                                string receiverLogin = Console.ReadLine();

                                Console.WriteLine("Input mesage:");
                                string message = Console.ReadLine();

                                await SendMessageAsync(client, receiverLogin, message);

                                break;
                            }
                    }
                }
            }
        }

        public static async Task SendMessageAsync(TcpClient client, string receiverLogin, string message)
        {
            ClientMessageData clientMessageData = new ClientMessageData(currentLogin, receiverLogin, message);

            await ExtendedTCPClient.WriteCustomAsync(client, JsonSerializer.Serialize(clientMessageData), false);
        }


        private static async Task HandleServerResponces(TcpClient client)
        {
            while (true)
            {
                string message = await client.ReadCustomAsync();

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
                    currentLogin = serverLoginResponce.AgreedLogin;
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


    }

}
