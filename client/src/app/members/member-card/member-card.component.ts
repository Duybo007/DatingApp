import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../_models/member';
import { RouterLink } from '@angular/router';
import { LikesService } from '../../_services/likes.service';

@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css'
})
export class MemberCardComponent {
  private likesService = inject(LikesService)

  member = input.required<Member>()
  hasLiked = computed(() => this.likesService.likeIds().includes(this.member().id)) // true/false
  // check if the current user has like a user by checking if the target userId is in the list of userIds that the current user has liked

  toggleLike(){
    this.likesService.toggleId(this.member().id).subscribe({
      next: () => {
        if(this.hasLiked()){  // if this user has been like, unlike by removing his/her id from likeIds signal in likesService
          this.likesService.likeIds.update(ids => ids.filter( x => x !== this.member().id))
        } else {  // else add his/her id to likeIds signal in likesService
          this.likesService.likeIds.update(ids => [...ids, this.member().id])
        }
      }
    })
  }
}
