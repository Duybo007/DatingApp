import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { BsDropdownConfig } from 'ngx-bootstrap/dropdown';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.scss'],
  providers: [{ provide: BsDropdownConfig, useValue: { isAnimated: true, autoClose: true } }]
})
export class NavBarComponent implements OnInit {
  public model: any = {}

 constructor(
  public accountService: AccountService,
  private router: Router,
  private toastr: ToastrService
 ) {}

 ngOnInit(): void {
 }

  login(){
    this.accountService.login(this.model).subscribe({
      next: () => this.router.navigateByUrl("/members")
    })
  }

  logout(){
    this.accountService.logout()
    this.router.navigateByUrl("/")
  }
}
