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
            // The port number(5001) must match the port of the gRPC server.
            //using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            //var client = new Greeter.GreeterClient(channel);
            //var reply = await client.SayHelloAsync(
            //                  new HelloRequest { Name = "GreeterClient" });
            //Console.WriteLine("Greeting: " + reply.Message);
            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();

            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            // Read ClientId from configuration
            ClientId = configuration["ClientSettings:ClientId"];


            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            using var streamingCall = client.SayHello();

            // Task to read responses from the server
            var responseTask = Task.Run(async () =>
            {
                await foreach (var reply in streamingCall.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{ClientId}] Received from server: " + reply.Message);
                    Console.Write("Enter your name (or press enter to exit): ");
                }
            });

            // Display the initial prompt once
            Console.Write("Enter your name (or press enter to exit): ");

            // Send multiple requests to the server
            while (true)
            {
                var name = Console.ReadLine();
                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{ClientId}] Exiting...");
                    break;
                }

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{ClientId}] Sending to server: " + name);
                await streamingCall.RequestStream.WriteAsync(new HelloRequest { Name = name, ClientId = ClientId });
            }

            await streamingCall.RequestStream.CompleteAsync();
            await responseTask;

            Environment.Exit(0);

        }
        #endregion
    }
}
#endregion