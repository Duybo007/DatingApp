import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { BsDropdownConfig } from 'ngx-bootstrap/dropdown';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.scss'],
  providers: [{ provide: BsDropdownConfig, useValue: { isAnimated: true, autoClose: true } }]
})
export class NavBarComponent implements OnInit {
  public model: any = {}

 constructor(
  public accountService: AccountService
 ) {}

 ngOnInit(): void {
 }

  login(){
    this.accountService.login(this.model).subscribe({
      next: res => {
      },
      error: error => console.log(error)
    })
  }

  logout(){
    this.accountService.logout()
  }
}
