import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/message';
import { setPaginatedResponse, setPaginationHeaders } from './PaginationHelper';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/user';
import { Group } from '../_models/group';

@Injectable({
  providedIn: 'root' // Makes this service available globally in the Angular app.
})
export class MessageService {
  // Inject HttpClient for making HTTP requests.
  private http = inject(HttpClient);

  // SignalR hub connection instance for real-time communication.
  hubConnection?: HubConnection;

  // Reactive state to store paginated results of messages.
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);

  // Reactive state to store the message thread (list of messages).
  messageThread = signal<Message[]>([]);

  // Base URL for API endpoints.
  baseUrl = environment.apiUrl;

  // URL for SignalR hubs.
  hubsUrl = environment.hubsUrl;

  // **Starts the SignalR hub connection and subscribes to events.**
  createHubConnection(user: User, otherUsername: string) {
    // Initialize the hub connection with the hub URL and access token for authentication.
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubsUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token, // Provides the token for the connection.
      })
      .withAutomaticReconnect() // Automatically reconnect on disconnect.
      .build();

    // Start the hub connection.
    this.hubConnection.start().catch(err => console.log(err)); // Log errors if the connection fails.

    // Listen for the 'ReceiveMessageThread' event to get the message thread.
    this.hubConnection.on('ReceiveMessageThread', messages => {
      this.messageThread.set(messages); // Reverse the messages and update the state.
    });

    // Listen for the 'NewMessage' event to add a new message to the thread.
    this.hubConnection.on('NewMessage', message => {
      this.messageThread.update(messages => [...messages, message]); // Append the new message.
    });

    this.hubConnection.on("UpdatedGroup", (group: Group) => {
      if(group.connections.some(x => x.username === otherUsername)){
        this.messageThread.update(messages => {
          messages.forEach(message => {
            if(!message.dateRead){
              message.dateRead = new Date(Date.now());
            }
          })
          return messages;
        })
      }
    })
  }

  // **Stops the SignalR hub connection.**
  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch(err => console.log(err)); // Stop and log errors if any.
    }
  }

  // **Sends a message through the SignalR hub.**
  async sendMessage(username: string, content: string) {
    return this.hubConnection?.invoke('SendMessage', { recipientUsername: username, content }); 
    // Invokes the 'SendMessage' method on the hub with the recipient and content.
  }

  // **Fetches paginated messages from the API.**
  getMessages(pageNumber: number, pageSize: number, container: string) {
    // Set up query parameters for pagination.
    let params = setPaginationHeaders(pageNumber, pageSize);
    params = params.append("Container", container); // Add the container filter.

    // Make an HTTP GET request to fetch messages.
    return this.http.get<Message[]>(this.baseUrl + 'messages', { observe: 'response', params }).subscribe({
      next: response => setPaginatedResponse(response, this.paginatedResult), 
      // Update the paginatedResult signal with the response.
    });
  }

  // **Fetches a message thread with a specific user from the API.**
  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username); 
    // Fetches all messages in the thread with the given username.
  }

  // **Deletes a specific message by ID.**
  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id); 
    // Sends a DELETE request to remove the message with the given ID.
  }
}
