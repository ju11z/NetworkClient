using System.Net.Sockets;
using System.Net;
using System.Text;
using TCPClientExtensions;
using System.Text.Json;

namespace SimpleClient
{

    internal class Program
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";

        public static class CustomQueryfactory
        {
            private static int counter;
            private static Random random = new Random();

            private static CustomQuery Create(int queryNumber)
            {
                counter++;

                return new CustomQuery()
                {
                    QueryNumber = queryNumber,
                    ThreadNumber = System.Threading.Thread.CurrentThread.ManagedThreadId,
                    RandomInt = random.Next(0, 1000)
                };
            }

            public static string GenerateRandomQuery(int number)
            {
                CustomQuery customQuery = Create(number);
                string resultQuery = customQuery.ToJson();

                bool damageQuery = random.Next(0, 20) % 13 == 0;
                if (damageQuery)
                {
                    resultQuery = resultQuery + "damaged";
                }

                return resultQuery;
            }
        }

        static void Main(string[] args)
        {
            Task2_DoAdvancedClientWork();
            //DoSimpleClientWork();
        }

        public static void Task2_DoAdvancedClientWork()
        {
            object locker = new object();

            using (TcpClient client = new TcpClient(SERVER_IP, PORT_NO))
            {
                int messagesAmount = 10000;

                Parallel.For(0, messagesAmount, (i, state) =>
                {
                    string randomQuery = CustomQueryfactory.GenerateRandomQuery(i);

                    client.WriteCustom(randomQuery);

                });

                while (true)
                {
                    lock (locker)
                    {
                        string responce = client.ReadCustom();

                        try
                        {
                            CustomResponce cr = JsonSerializer.Deserialize<CustomResponce>(responce);

                            if (cr.ResponseCode == ResponceCode.Success)
                            {
                                Console.WriteLine($"Server responce success");
                            }
                            else if (cr.ResponseCode == ResponceCode.ClientError)
                            {
                                Console.WriteLine($"Server responce 400");
                            }
                            else if (cr.ResponseCode == ResponceCode.ServerError)
                            {
                                Console.WriteLine($"Server responce 500");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Server send invalid data that cant be parsed to json: {ex.Message}");
                        }

                        Console.WriteLine($"receiving {responce}");
                    }
                }
            }

            Console.ReadLine();

        }

        public static void DoSimpleClientWork()
        {
            string messageToSend = string.Concat(Enumerable.Repeat("Hello", 2));

            TcpClient client = new TcpClient(SERVER_IP, PORT_NO);

            Console.WriteLine($"Sending to server: {messageToSend}");

            client.WriteCustom(messageToSend);

            string receivedMessage = client.ReadCustom();
            Console.WriteLine($"Received from server: {receivedMessage}");

            client.Close();
        }
    }
}
