import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { User } from '../_models/user';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes.service';
import { PresenseService } from './presense.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private presenseService = inject(PresenseService)
  private http = inject(HttpClient); // Injecting HttpClient to perform HTTP requests
  private likesService = inject(LikesService)

  baseUrl = environment.apiUrl; // Base URL for the API endpoints
  currentUser = signal<User | null>(null); // Signal to store the current user and enable reactivity across components
  roles = computed(() => {
    const user = this.currentUser()
    if(user && user.token){
      const role =  JSON.parse(atob(user.token.split('.')[1])).role;
      return Array.isArray(role) ? role : [role]
    }
    return null
  })

  /**
   * Logs in a user by sending their credentials to the API.
   * If successful, stores the user in localStorage and updates the currentUser signal.
   * @param model - The user's login credentials (e.g., email and password).
   * @returns An Observable that completes after user data is processed.
   */
  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user)
        }
      })
    );
  }

  /**
   * Logs out the user by removing their data from localStorage
   * and resetting the currentUser signal to null.
   */
  logout() {
    localStorage.removeItem('user'); // Remove user data from localStorage
    this.currentUser.set(null); // Reset the currentUser signal
    this.presenseService.stopHubConnection()
  }

  /**
   * Registers a new user by sending their details to the API.
   * If successful, stores the user in localStorage and updates the currentUser signal.
   * @param model - The user's registration details (e.g., name, email, password).
   * @returns An Observable with the newly registered user data.
   */
  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user)
        }
        return user; // Return the registered user data
      })
    );
  }

  setCurrentUser(user: User){
    localStorage.setItem('user', JSON.stringify(user)); // Save user data in localStorage for session persistence
    this.currentUser.set(user); // Update the currentUser signal
    this.likesService.getLikeIds()
    this.presenseService.createHubConnection(user)
  }
}
