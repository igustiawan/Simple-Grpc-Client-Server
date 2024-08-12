using Grpc.Core;
using System.Threading.Tasks;
using GrpcGreeter;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Google.Protobuf;
using System.Collections.Generic;

namespace SimpleGrpc_Server.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        // Simpan stream client yang terhubung
        private static readonly ConcurrentDictionary<string, IServerStreamWriter<HelloReply>> _clients = new();
        private static readonly ConcurrentDictionary<string, List<HelloRequest>> _inbox = new();

        public override async Task SayHello(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            // Ambil client ID dari header
            string clientId = context.RequestHeaders.FirstOrDefault(h => h.Key == "client-id").Value ?? "UnknownClient";

            // Tambahkan client ke dalam daftar
            _clients.TryAdd(clientId, responseStream);

            try
            {
                await foreach (var request in requestStream.ReadAllAsync())
                {
                    // Simpan pesan dalam inbox client pengirim
                    if (!_inbox.ContainsKey(clientId))
                    {
                        _inbox[clientId] = new List<HelloRequest>();
                    }
                    _inbox[clientId].Add(request);

                    // Broadcast pesan ke semua client
                    foreach (var client in _clients.Values)
                    {
                        await client.WriteAsync(new HelloReply
                        {
                            SenderClientId = clientId, // Menyertakan ID client pengirim
                            Message = $"[{clientId}] {request.Message}",
                            Timestamp = DateTime.Now.ToString("[HH:mm:ss.fff]")
                        });
                    }
                }
            }
            finally
            {
                // Hapus client dari daftar saat koneksi berakhir
                _clients.TryRemove(clientId, out _);
            }
        }

        // Metode untuk mengirim broadcast dari server
        public static async Task SendBroadcastAsync(string message)
        {
            var timestamp = DateTime.UtcNow.ToString("[HH:mm:ss.fff]");

            // Simpan pesan ke inbox untuk semua client
            foreach (var clientId in _clients.Keys)
            {
                if (!_inbox.ContainsKey(clientId))
                {
                    _inbox[clientId] = new List<HelloRequest>();
                }
                _inbox[clientId].Add(new HelloRequest
                {
                    ClientId = "Server", // Set ID pengirim ke "Server"
                    Message = message,
                    Timestamp = timestamp
                });
            }

            // Kirim pesan ke semua klien
            foreach (var client in _clients.Values)
            {
                await client.WriteAsync(new HelloReply
                {
                    SenderClientId = "Server", // Set ID pengirim ke "Server"
                    Message = $"[Server]: {message}",
                    Timestamp = timestamp
                });
            }
        }

        // Method untuk cek client aktif
        public static void PrintActiveClients()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Active Clients:");
            foreach (var clientId in _clients.Keys)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {clientId}");
            }
        }

        // Method untuk check pesan dari semua client
        public static void PrintMessages()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Messages from Clients:");

            // Tampilkan pesan dari _inbox
            foreach (var kvp in _inbox)
            {
                var recipientClientId = kvp.Key;
                var messages = kvp.Value;
                foreach (var message in messages)
                {
                    Console.WriteLine($"[{message.Timestamp}] [From {message.ClientId} to {recipientClientId}]: {message.Message}");
                }
            }
        }

        // Mengirim pesan ke client tertentu
        public override async Task<HelloReply> SendMessageToClient(SendMessageRequest request, ServerCallContext context)
        {
            if (_clients.TryGetValue(request.RecipientClientId, out var recipientClient))
            {
                await recipientClient.WriteAsync(new HelloReply
                {
                    SenderClientId = request.SenderClientId, 
                    Message = request.Message,
                    Timestamp = DateTime.Now.ToString("[HH:mm:ss.fff]")
                });

                // Save message to recipient's inbox
                if (!_inbox.ContainsKey(request.RecipientClientId))
                {
                    _inbox[request.RecipientClientId] = new List<HelloRequest>();
                }
                _inbox[request.RecipientClientId].Add(new HelloRequest
                {
                    ClientId = request.SenderClientId,
                    Message = request.Message,
                    Timestamp = DateTime.Now.ToString("[HH:mm:ss.fff]")
                });

                return new HelloReply { Message = "Message sent successfully." };
            }

            return new HelloReply { Message = "Recipient not found." };
        }

        // Mengambil pesan yang diterima oleh klien tertentu
        public override async Task GetMessages(GetMessagesRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            var clientId = request.ClientId;

            // Ambil pesan yang relevan untuk client yang diminta
            var messages = _inbox.ContainsKey(clientId) ? _inbox[clientId] : new List<HelloRequest>();

            foreach (var message in messages)
            {
                await responseStream.WriteAsync(new HelloReply
                {
                    SenderClientId = message.ClientId, // Sertakan ID pengirim
                    Message = message.Message,
                    Timestamp = message.Timestamp
                });
            }
        }

        // Send broadcast to a specific client
        public static async Task<string> SendBroadcastToSpecificClientAsync(string clientId, string message)
        {
            var timestamp = DateTime.UtcNow.ToString("[HH:mm:ss.fff]");

            // Cek apakah klien dengan ID yang diberikan ada
            if (!_clients.ContainsKey(clientId))
            {
                return "Recipient not found.";
            }

            // Simpan pesan ke inbox untuk klien tertentu
            if (!_inbox.ContainsKey(clientId))
            {
                _inbox[clientId] = new List<HelloRequest>();
            }
            _inbox[clientId].Add(new HelloRequest
            {
                ClientId = "Server", // Set ID pengirim ke "Server"
                Message = message,
                Timestamp = timestamp
            });

            // Kirim pesan ke klien tertentu
            if (_clients.TryGetValue(clientId, out var client))
            {
                await client.WriteAsync(new HelloReply
                {
                    SenderClientId = "Server", // Set ID pengirim ke "Server"
                    Message = $"[Server]: {message}",
                    Timestamp = timestamp
                });
                return "Message sent successfully.";
            }

            return "Recipient not found.";
        }
    }
}