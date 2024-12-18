using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    // The PresenceHub class handles user presence notifications and maintains online user information.
    // It is secured with the [Authorize] attribute, so only authenticated users can connect to this hub.
    [Authorize]
    public class PresenceHub(PresenseTracker tracker) : Hub
    {
        // Overrides the OnConnectedAsync method, which is triggered when a client connects to the hub.
        public override async Task OnConnectedAsync()
        {
            // Ensure that the connected user is valid.
            if (Context.User == null) throw new HubException("Cannot get current user claim");

            // Use the tracker to register the user's connection and check if this is their first connection.
            var isOnline = await tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);

            // If the user just came online, notify other connected clients.
            if (isOnline) await Clients.Others.SendAsync("UserIsOnline", Context.User?.GetUsername());

            // Retrieve the list of currently online users and send it to all connected clients.
            var currentUsers = await tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }

        // Overrides the OnDisconnectedAsync method, which is triggered when a client disconnects from the hub.
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Ensure that the disconnected user is valid.
            if (Context.User == null) throw new HubException("Cannot get current user claim");

            // Use the tracker to deregister the user's connection and check if this was their last active connection.
            var isOffline = await tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

            // If the user has gone offline, notify other connected clients.
            if (isOffline) await Clients.Others.SendAsync("UserIsOffline", Context.User?.GetUsername());

            // Call the base class implementation to handle any additional disconnection logic.
            await base.OnDisconnectedAsync(exception);
        }
    }
}
