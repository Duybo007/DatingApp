import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment'; // Environment-specific configurations, such as API URL.
import { Member } from '../_models/member'; // Member model to define the shape of member data.
import { of, tap } from 'rxjs'; // RxJS utilities for working with streams.
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { setPaginatedResponse, setPaginationHeaders } from './PaginationHelper';

@Injectable({
  providedIn: 'root' // This service will be available globally without needing to import it in individual modules.
})
export class MembersService {
  private http = inject(HttpClient); // Injecting HttpClient to perform HTTP requests.
  private accountService = inject(AccountService); // Injecting AccountService to access user authentication data.

  // members = signal<Member[]>([]); // Signal to store and manage the list of members in a reactive way.
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null); // Signal to store and manage pagination results in a reactive way.
  baseUrl = environment.apiUrl; // Base URL for API endpoints, fetched from environment configuration.
  memberCache = new Map();
  userParams = signal<UserParams>(new UserParams(this.accountService.currentUser()))

  // Fetch all members from the API and store them in the `memberCache` signal.
  getMembers() {
    const response = this.memberCache.get(Object.values(this.userParams()).join('-'))

    if(response) return setPaginatedResponse(response, this.paginatedResult);

    let params = setPaginationHeaders(this.userParams().pageNumber, this.userParams().pageSize)

    params = params.append('minAge', this.userParams().minAge);
    params = params.append('maxAge', this.userParams().maxAge);
    params = params.append('gender', this.userParams().gender);
    params = params.append('orderBy', this.userParams().orderBy);

    return this.http.get<Member[]>(`${this.baseUrl}users`, {observe: 'response', params}).subscribe({
      next: response =>{
        setPaginatedResponse(response, this.paginatedResult);
        this.memberCache.set(Object.values(this.userParams()).join('-'), response)
      }
    });
  }

  resetUserParams(){
    this.userParams.set(new UserParams(this.accountService.currentUser()))
  }


  // Fetch a specific member by username.
  getMember(username: string) {
    const member: Member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.body), [])
      .find( (m: Member) => m.username === username);

    if(member) return of(member);
    
    return this.http.get<Member>(`${this.baseUrl}users/${username}`);
  }

  // Update member details in the backend and also update the local `members` signal.
  updateMember(member: Member) {
    return this.http.put(`${this.baseUrl}users`, member).pipe(
      // tap(() => {
      //   // After a successful API update, update the corresponding member in the `members` signal.
      //   this.members.update(members =>
      //     members.map(m => m.username === member.username ? member : m) // Replace the matching member with the updated one.
      //   );
      // })
    );
  }

  // Set a photo to be main photo and also update the local `members` signal
  setMainPhoto(photo: Photo) {
    return this.http.put(`${this.baseUrl}users/set-main-photo/${photo.id}`, {}).pipe(
      // tap(() => {
      //   // After a successful API update, update the corresponding member in the `members` signal.
      //   this.members.update(members => members.map(m=>{
      //     if(m.photos.includes(photo)){ // member that has `photo` in `photos`
      //       m.photoUrl = photo.url  // update current photoUrl with photo.url
      //     }
      //     return m
      //   }))
      // })
    )
  }

  deletePhoto(photo: Photo){
    return this.http.delete(`${this.baseUrl}users/delete-photo/${photo.id}`, {}).pipe(
      // tap(() => {
      //   this.members.update(members => members.map(m => {
      //     if(m.photos.includes(photo)){
      //       m.photos = m.photos.filter(p => p.id !== photo.id) // remove photo from photos
      //     }
      //     return m
      //   }))
      // })
    )
  }
}
