# gRPC Client-Server Example

This project demonstrates a basic gRPC setup with a server and multiple clients. 
It supports two-way communication, where clients and the server can send and receive messages. 
Clients read their configuration, including their unique `ClientId`, from an `appsettings.json` file.

# Communication

## Server
The server is set up to handle bidirectional streaming. It can read messages from multiple clients and respond to each client with a greeting message.

## Client
The client establishes a bidirectional streaming connection with the server. 
It can send messages to the server and receive responses in real-time. 
Each client sends its ClientId along with the message to identify itself to the server.

# Code Structure

## SimpleGrpc-Server: 
Contains the gRPC server implementation. The server handles incoming messages from clients, processes them, and responds with greetings. It supports two-way communication where it can handle messages from multiple clients.

## SimpleGrpc-Client: 
Contains the gRPC client implementation. Each client reads its ClientId from appsettings.json, sends messages to the server, and processes responses from the server. It supports two-way communication where it can send and receive messages in real-time.

## Configure appsettings.json (SimpleGrpc-Client)
```
{
  "ClientSettings": {
    "ClientId": "Client 1"
  }
}
```

## Example Output from Client
```
Enter your name (or press enter to exit): Alice
[15:20:52.778] [Client 1] Sending to server: Alice
[15:20:52.948] [Client 1] Received from server: Hello Alice
Enter your name (or press enter to exit):
```
## Example Output from Server
```
[15:20:52.851] [Server] Received from Client 1: Alice
[15:20:52.860] [Server] Sending to Client 1: Hello Alice
```
