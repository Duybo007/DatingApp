import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment'; // Environment-specific configurations, such as API URL.
import { Member } from '../_models/member'; // Member model to define the shape of member data.
import { of, tap } from 'rxjs'; // RxJS utilities for working with streams.
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root' // This service will be available globally without needing to import it in individual modules.
})
export class MembersService {
  private http = inject(HttpClient); // Injecting HttpClient to perform HTTP requests.

  members = signal<Member[]>([]); // Signal to store and manage the list of members in a reactive way.
  baseUrl = environment.apiUrl; // Base URL for API endpoints, fetched from environment configuration.

  // Fetch all members from the API and store them in the `members` signal.
  getMembers() {
    return this.http.get<Member[]>(`${this.baseUrl}users`).subscribe({
      next: (members) => this.members.set(members) // Update the `members` signal with the fetched data.
    });
  }

  // Fetch a specific member by username.
  getMember(username: string) {
    // Check if the member is already in the `members` signal.
    const member = this.members().find(x => x.username === username);
    if (member !== undefined) return of(member); // If found, return it as an Observable using `of()`.

    // If not found in the local `members`, make an API call to fetch the member.
    return this.http.get<Member>(`${this.baseUrl}users/${username}`);
  }

  // Update member details in the backend and also update the local `members` signal.
  updateMember(member: Member) {
    return this.http.put(`${this.baseUrl}users`, member).pipe(
      tap(() => {
        // After a successful API update, update the corresponding member in the `members` signal.
        this.members.update(members =>
          members.map(m => m.username === member.username ? member : m) // Replace the matching member with the updated one.
        );
      })
    );
  }

  // Set a photo to be main photo and also update the local `members` signal
  setMainPhoto(photo: Photo) {
    return this.http.put(`${this.baseUrl}users/set-main-photo/${photo.id}`, {}).pipe(
      tap(() => {
        // After a successful API update, update the corresponding member in the `members` signal.
        this.members.update(members => members.map(m=>{
          if(m.photos.includes(photo)){ // member that has `photo` in `photos`
            m.photoUrl = photo.url  // update current photoUrl with photo.url
          }
          return m
        }))
      })
    )
  }

  deletePhoto(photo: Photo){
    return this.http.delete(`${this.baseUrl}users/delete-photo/${photo.id}`, {}).pipe(
      tap(() => {
        this.members.update(members => members.map(m => {
          if(m.photos.includes(photo)){
            m.photos = m.photos.filter(p => p.id !== photo.id) // remove photo from photos
          }
          return m
        }))
      })
    )
  }
}
