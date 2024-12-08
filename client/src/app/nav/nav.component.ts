import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [FormsModule, BsDropdownModule],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  accountService = inject(AccountService)

  model: any = {} // login form

  login(){
    this.accountService.login(this.model).subscribe({
      next: res => {
        console.log(res)
      },
      error: err => console.log(err)
    })
  }

  logout(){
    this.accountService.logout()
  }
}