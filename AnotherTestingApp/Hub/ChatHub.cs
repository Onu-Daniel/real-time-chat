using Microsoft.AspNetCore.SignalR;

namespace AnotherTestingApp.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IDictionary<string, UserGroupConnection> _connection;

        public ChatHub(IDictionary<string, UserGroupConnection> connection)
        {
            _connection = connection;
        }

        public async Task JoinGroup(UserGroupConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.ChatGroup!);
            // Adds the current user's connection (identified by Context.ConnectionId) to a specified group indicated by userConnection.Group. 

            _connection[Context.ConnectionId] = userConnection;
            // Updates a dictionary _connection with the user connection information. 

            await Clients.Group(userConnection.ChatGroup!)
                .SendAsync("ReceiveMessage", "OpenReplay", $"{userConnection.User} has joined the group", DateTime.Now);
            // Notifies all members of the group that a new member has joined.

            await NotifyConnectedUsersInGroup(userConnection.ChatGroup!);
        }

        public async Task SendChatMessage(string message)
        {
            if (_connection.TryGetValue(Context.ConnectionId, out UserGroupConnection userGroupConnection))
            {
                // Checks if the current user's connection ID exists in the _connection dictionary.

                await Clients.Group(userGroupConnection.ChatGroup!)
                    .SendAsync("ReceiveMessage", userGroupConnection.User, message, DateTime.Now);
                // Sends a message to all clients in the specified chat group. 
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Check if the user's connection ID exists in the _connection dictionary
            if (_connection.TryGetValue(Context.ConnectionId, out UserGroupConnection groupConnection))
            {
                // If the user's connection is found, execute the following code
                Clients.Group(groupConnection.ChatGroup!)
                    .SendAsync("ReceiveMessage", "OpenReplay", $"{groupConnection.User} has left the group", DateTime.Now);
                // Notify all clients in the specified chat group that the user has left

                NotifyConnectedUsersInGroup(groupConnection.ChatGroup!);
            }

            // Call the base implementation of OnDisconnectedAsync
            return base.OnDisconnectedAsync(exception);
        }

        public Task NotifyConnectedUsersInGroup(string group)
        {
            // Retrieve a list of connected users in the specified group from the _connection dictionary
            var connectedUsers = _connection.Values
                .Where(connection => connection.ChatGroup == group)
                .Select(connection => connection.User);

            // Send an update message to all clients in the specified chat group with the list of connected users
            return Clients.Group(group).SendAsync("ConnectedUser", connectedUsers);
        }
    }
}
