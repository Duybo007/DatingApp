namespace API.SignalR
{
    // This class is responsible for tracking the presence of users connected to the application through SignalR.
    public class PresenseTracker
    {
        // A static dictionary to store online users and their corresponding SignalR connection IDs.
        // Key: Username, Value: List of connection IDs associated with the user.
        private static readonly Dictionary<string, List<string>> OnlineUsers = [];

        // Handles when a user connects to SignalR. Returns true if this is the user's first connection.
        public Task<bool> UserConnected(string username, string connectionId)
        {
            var isOnline = false;
            lock (OnlineUsers) // Lock ensures thread safety when modifying the shared dictionary.
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    // If the user already exists in the dictionary, add the new connection ID.
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    // If the user doesn't exist, add a new entry with the connection ID.
                    OnlineUsers.Add(username, [connectionId]);
                    isOnline = true; // Indicates that the user has come online for the first time.
                }
            }
            return Task.FromResult(isOnline); // Return whether this connection marks the user as online.
        }

        // Handles when a user disconnects from SignalR. Returns true if this is the user's last connection.
        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            var isOffline = false;
            lock (OnlineUsers) // Lock ensures thread safety when modifying the shared dictionary.
            {
                // If the user isn't found, return false as there's nothing to do.
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

                // Remove the specific connection ID from the user's list.
                OnlineUsers[username].Remove(connectionId);

                // If the user has no remaining connection IDs, remove the user from the dictionary.
                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true; // Indicates that the user has gone offline.
                }
            }
            return Task.FromResult(isOffline); // Return whether this disconnection marks the user as offline.
        }

        // Retrieves an array of currently online users (sorted alphabetically by username).
        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock (OnlineUsers) // Lock ensures thread safety while accessing the dictionary.
            {
                // Get the usernames (keys) from the dictionary, sorted alphabetically.
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }
            return Task.FromResult(onlineUsers); // Return the list of online users.
        }

        // Retrieves a list of all connection IDs for a given user.
        public static Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            // Try to get the user's connections from the dictionary.
            if (OnlineUsers.TryGetValue(username, out var connections))
            {
                lock (connections) // Lock ensures thread safety when accessing the user's connection list.
                {
                    connectionIds = connections.ToList(); // Create a copy of the connection list.
                }
            }
            else
            {
                connectionIds = []; // If the user isn't found, return an empty list.
            }

            return Task.FromResult(connectionIds); // Return the list of connection IDs.
        }
    }
}
