using Grpc.Core;
using System.Threading.Tasks;
using GrpcGreeter;
using System;

namespace SimpleGrpc_Server.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        //public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        //{
        //    return Task.FromResult(new HelloReply
        //    {
        //        Message = "Hello " + request.Name
        //    });
        //}

        public override async Task SayHello(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                // Log the request received from the client with timestamp and client ID
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Server] Received from {request.ClientId}: {request.Name}");

                // Send a reply back to the client with timestamp including milliseconds
                var replyMessage = "Hello " + request.Name;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Server] Sending to {request.ClientId}: {replyMessage}");
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = replyMessage
                });
            }
        }
    }
}
