import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { setPaginatedResponse, setPaginationHeaders } from './PaginationHelper';

@Injectable({
  providedIn: 'root'
})
export class LikesService {
  private http = inject(HttpClient)

  likeIds = signal<number[]>([])  //  list of userIds the current user has liked
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null); // Signal to store and manage pagination results in a reactive way.

  baseUrl = environment.apiUrl
  
  //  like/unlike a user
  toggleId(targetUserId: number) {
    return this.http.post(`${this.baseUrl}likes/${targetUserId}`, {})
  }

  //  retrieve a list of MemberDto objects based on the given predicate and user ID
  getLikes(predicate: string, pageNumber: number, pageSize: number){
    let params = setPaginationHeaders(pageNumber, pageSize);

    params = params.append('predicate', predicate)

    return this.http.get<Member[]>(`${this.baseUrl}likes`, {observe: 'response', params}).subscribe({
      next: response => setPaginatedResponse(response, this.paginatedResult)
    })
  }

  //  retrieve a list of target user IDs that the current user has liked
  getLikeIds(){
    return this.http.get<number[]>(`${this.baseUrl}likes/list`).subscribe({
      next: ids => this.likeIds.set(ids)
    })
  }
}
