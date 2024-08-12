using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleGrpc_Server.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using GrpcGreeter;

var builder = WebApplication.CreateBuilder(args);

// Tambahkan layanan gRPC ke DI container
builder.Services.AddGrpc();

// Konfigurasi logging
builder.Logging.ClearProviders(); // Menghapus semua penyedia logging

var app = builder.Build();

// Konfigurasi HTTP request pipeline
app.MapGrpcService<GreeterService>();
app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client.");
});

var serverTask = Task.Run(() => app.Run());

await ShowMenuAsync();

await serverTask;

async Task ShowMenuAsync()
{
    while (true)
    {
        Console.WriteLine("\nSelect an option:");
        Console.WriteLine("1. Enter message to broadcast all clients");
        Console.WriteLine("2. Enter message to broadcast spesific clients");
        Console.WriteLine("3. View messages from clients");
        Console.WriteLine("4. View connected clients");
        Console.WriteLine("5. Exit");
        Console.Write("Your choice: ");

        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                Console.Write("Enter broadcast message (or press enter to skip): ");
                var message = Console.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    await GreeterService.SendBroadcastAsync(message);
                }
                break;
            case "2":
                Console.Write("Enter recipient client ID: ");
                var recipientClientId = Console.ReadLine();

                Console.Write("Enter your message: ");
                var broadcastMessage = Console.ReadLine();

                if (!string.IsNullOrEmpty(recipientClientId) && !string.IsNullOrEmpty(broadcastMessage))
                {
                    var resultMessage = await GreeterService.SendBroadcastToSpecificClientAsync(recipientClientId, broadcastMessage);
                    Console.WriteLine(resultMessage); 
                }
                break;

            case "3":
                GreeterService.PrintMessages();
                break;
            case "4":
                GreeterService.PrintActiveClients();
                break;
            case "5":
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Invalid choice, please select again.");
                break;
        }
    }
}