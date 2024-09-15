using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Text;

namespace Chat_App.Code
{
    public class WebSocketHandler
    {
        private static readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        // Method to handle new WebSocket connections and receive messages
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
        // Add WebSocket connection for the user
        public void AddConnection(string userId, WebSocket webSocket)
        {
            _connections[userId] = webSocket;
        }
        // Remove WebSocket connection for the user
        public void RemoveConnection(string userId)
        {
            _connections.TryRemove(userId, out _);
        }
        // Send a message to the specified user if they are connected via WebSocket
        public async Task SendMessageToUserAsync(string receiverId, string message)
        {
            if (_connections.TryGetValue(receiverId, out var webSocket) && webSocket.State == WebSocketState.Open)
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);
                var messageSegment = new ArraySegment<byte>(messageBuffer);

                await webSocket.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
