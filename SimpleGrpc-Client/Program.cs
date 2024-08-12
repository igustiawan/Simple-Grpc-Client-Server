#region snippet2
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace GrpcGreeterClient
{
    class Program
    {
        private static string ClientId;

        #region snippet
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            // Read ClientId from configuration
            ClientId = configuration["ClientSettings:ClientId"];


            using var channel = GrpcChannel.ForAddress("https://localhost:7053/");
            var client = new Greeter.GreeterClient(channel);

            //using var streamingCall = client.SayHello();
            var metadata = new Grpc.Core.Metadata { { "client-id", ClientId } };
            using var streamingCall = client.SayHello(headers: metadata);
    
            // Task to read responses from the server
            var responseTask = Task.Run(async () =>
            {
                await foreach (var reply in streamingCall.ResponseStream.ReadAllAsync())
                {
                    //Console.WriteLine();
                    //Console.WriteLine($"From : {reply.Message} [{reply.Timestamp}]");
                    //Console.Write("Enter your message (or press enter to exit): ");
                }
            });

            while (true)
            {
                Console.WriteLine("\nSelect an option:");
                Console.WriteLine("1. Send message to client");
                Console.WriteLine("2. View message inbox");
                Console.WriteLine("3. Exit");
                Console.Write("Your choice: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter recipient client ID: ");
                        var recipientClientId = Console.ReadLine();

                        Console.Write("Enter your message: ");
                        var message = Console.ReadLine();

                        if (!string.IsNullOrEmpty(recipientClientId) && !string.IsNullOrEmpty(message))
                        {
                            var sendMessageRequest = new SendMessageRequest
                            {
                                RecipientClientId = recipientClientId,
                                SenderClientId = ClientId, // Set SenderClientId
                                Message = message
                            };

                            var sendMessageResponse = await client.SendMessageToClientAsync(sendMessageRequest);
                            Console.WriteLine($"Sent to {recipientClientId}: {sendMessageResponse.Message}");
                        }
                        break;

                    case "2":
                        var getMessagesRequest = new GetMessagesRequest { ClientId = ClientId };
                        var call = client.GetMessages(getMessagesRequest);
                        await foreach (var reply in call.ResponseStream.ReadAllAsync())
                        {
                            Console.WriteLine($"[{reply.Timestamp}] [{reply.SenderClientId}]: {reply.Message}"); // Menampilkan ID pengirim
                        }
                        break;

                    case "3":
                        await streamingCall.RequestStream.CompleteAsync();
                        await responseTask;
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("Invalid choice, please select again.");
                        break;
                }
            }
        }
        #endregion
    }
}
#endregion