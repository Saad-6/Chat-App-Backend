using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Text;

namespace Chat_App.Code;

public class WebSocketHandler
{
    private static readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    public async Task HandleWebSocketAsync(WebSocket webSocket, string userId)
    {
        var buffer = new byte[1024 * 4];
        // Add the user's WebSocket connection
        
        AddConnection(userId, webSocket);
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!result.CloseStatus.HasValue)
        {
            // Echo the received message back to the client (optional)
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        // Remove the WebSocket connection when it's closed
        RemoveConnection(userId);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
    public async Task<bool> UserIsOnline (string userId)
    {
        if (_connections.TryGetValue(userId, out WebSocket webSocket))
        {
            return true;
        }
        return false;
    }
    public void AddConnection(string userId, WebSocket webSocket)
    {
        if (_connections.ContainsKey(userId))
        {
            Console.WriteLine($"User ID {userId} already exists. Connection not added.");
            return; // Exit the method if the user already has a connection
        }

        _connections[userId] = webSocket;
        Console.WriteLine($"Added connection for User ID {userId}");
    }
    public void RemoveConnection(string userId)
    {
        _connections.TryRemove(userId, out _);
    }

    public async Task SendMessageToUserAsync(string receiverId, string message)
    {
        if (_connections.TryGetValue(receiverId, out var webSocket) && webSocket.State == WebSocketState.Open)
        {
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var messageSegment = new ArraySegment<byte>(messageBuffer);
            await webSocket.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async Task RingCall()
    {

    }
}
