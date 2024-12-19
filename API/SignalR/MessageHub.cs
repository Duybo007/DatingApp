using API.DTOs; // DTOs for transferring message data.
using API.Entities; // Entity models like Group and Message.
using API.Extensions; // Extension methods, e.g., for extracting the username from claims.
using API.Interfaces; // Interfaces for repositories like IunitOfWork.MessageRepository and IunitOfWork.UserRepository.
using AutoMapper; // Used for mapping objects between entities and DTOs.
using Microsoft.AspNetCore.SignalR; // SignalR base class for creating real-time communication hubs.

namespace API.SignalR
{
    // SignalR hub for managing real-time messaging functionality.
    public class MessageHub(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
    {
        // Triggered when a client connects to the SignalR hub.
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext(); // Retrieve the HTTP context for the current SignalR connection.
            var otherUser = httpContext?.Request.Query["user"]; // Extract the "user" query parameter.
            
            // Ensure the user is authenticated and "user" parameter is provided.
            if (Context.User == null || string.IsNullOrEmpty(otherUser))
                throw new Exception("Cannot join group");

            // Generate a unique group name for the two participants.
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);

            // Add the current connection to the SignalR group.
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Add the connection to the group in the database and get updated group details.
            var group = await AddToGroup(groupName);

            // Notify group members about the updated group details.
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            // Retrieve the message thread between the two users.
            var messages = await unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);

            if(unitOfWork.HasChanges()) await unitOfWork.Complete();

            // Send the retrieved messages to the caller (current client).
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        // Triggered when a client disconnects from the SignalR hub.
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove the connection from the group in the database.
            var group = await RemoveFromMessageGroup();

            // Notify group members about the updated group details.
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

            await base.OnDisconnectedAsync(exception); // Call the base class implementation.
        }

        // Adds the current connection to the group in the database.
        private async Task<Group> AddToGroup(string groupName)
        {
            var username = Context.User?.GetUsername() ?? throw new Exception("Cannot get username");

            // Retrieve the group from the database or create a new one if it doesn't exist.
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection { ConnectionId = Context.ConnectionId, Username = username };

            if (group == null)
            {
                group = new Group { Name = groupName }; // Create a new group if none exists.
                unitOfWork.MessageRepository.AddGroup(group); // Add the group to the repository.
            }

            group.Connections.Add(connection); // Add the connection to the group.

            // Save the changes and return the updated group.
            if (await unitOfWork.Complete()) return group;

            throw new HubException("Failed to join group"); // Throw an error if saving fails.
        }

        // Removes the current connection from its group in the database.
        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId); // Get the group for the connection.
            var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId); // Find the connection.

            if (connection != null && group != null)
            {
                unitOfWork.MessageRepository.RemoveConnection(connection); // Remove the connection from the repository.

                // Save the changes and return the updated group.
                if (await unitOfWork.Complete()) return group;
            }

            throw new Exception("Failed to remove from group"); // Throw an error if removal fails.
        }

        // Sends a message from one user to another.
        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User?.GetUsername() ?? throw new Exception("Could not get user");

            // Prevent users from sending messages to themselves.
            if (username == createMessageDto.RecipientUsername)
                throw new HubException("Cannot send message to yourself");

            // Retrieve the sender and recipient from the database.
            var sender = await unitOfWork.UserRepository.GetUserByNameAsync(username);
            var recipient = await unitOfWork.UserRepository.GetUserByNameAsync(createMessageDto.RecipientUsername);

            // Validate that both users exist and their usernames are valid.
            if (sender == null || recipient == null || sender.UserName == null || recipient.UserName == null)
                throw new HubException("Cannot send message at this time");

            // Create a new message entity.
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content,
            };

            // Generate the group name and retrieve the group from the database.
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);

            if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow; // Mark the message as read if the recipient is in the group.
            }
            else
            {
                // Notify the recipient of the new message if they are online.
                var connections = await PresenseTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null && connections?.Count != null) // Use the exact condition as provided.
                {
                    await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            unitOfWork.MessageRepository.AddMessage(message); // Add the message to the repository.

            // Save the changes and notify the group about the new message.
            if (await unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            };
        }

        // Generates a consistent group name based on the usernames of the two participants.
        private string GetGroupName(string caller, string? other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0; // Compare the usernames alphabetically.
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}"; // Return a consistently formatted group name.
        }
    }
}
