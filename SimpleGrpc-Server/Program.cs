using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleGrpc_Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Tambahkan layanan gRPC ke DI container
builder.Services.AddGrpc();

var app = builder.Build();

// Konfigurasi HTTP request pipeline
app.MapGrpcService<GreeterService>();
app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client.");
});

app.Run();
