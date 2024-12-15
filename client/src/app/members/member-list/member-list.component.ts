import { Component, inject, OnInit } from '@angular/core';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { MembersService } from '../../_services/members.service';
import { MemberCardComponent } from "../member-card/member-card.component";
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [MemberCardComponent, PaginationModule, FormsModule, ButtonsModule],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css'
})

export class MemberListComponent implements OnInit {
  membersService = inject(MembersService);

  genderList = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}]

  resetFilters(){
    this.membersService.resetUserParams();
    this.getMembers()
  }

  ngOnInit(): void {
    if(!this.membersService.paginatedResult()) this.getMembers();
  }

  getMembers(){
    this.membersService.getMembers()
  }

  pageChanged(event :any){
    if(this.membersService.userParams().pageNumber !== event.page) {
      this.membersService.userParams().pageNumber = event.page;
      this.getMembers();
    }
  }
}
