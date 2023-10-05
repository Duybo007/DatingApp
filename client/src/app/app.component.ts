import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { AccountService } from './_services/account.service';
import { User } from './_models/user';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.sass']
})
export class AppComponent implements OnInit {
  title = 'Dating App';
  
  constructor(
    private http : HttpClient,
    private accountServer : AccountService
  ) {}


  ngOnInit(): void {
    
    this.setCurrentUser();
  }



  setCurrentUser(){
    const userString = localStorage.getItem("user");
    if(!userString) return
    const user = JSON.parse(userString)
    this.accountServer.setCurrentUser(user)
  }

}
