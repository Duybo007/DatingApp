import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from "./nav/nav.component";
import { AccountService } from './_services/account.service';
import { HomeComponent } from "./home/home.component";
import { NgxSpinnerComponent } from 'ngx-spinner';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavComponent, HomeComponent, NgxSpinnerComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  // Injecting the AccountService dependency to handle user authentication
  private accountService = inject(AccountService);
  
  // Angular lifecycle hook that runs after the component initializes
  ngOnInit(): void {
    this.setCurrentUser(); // Check and set the current user if they are already logged in
  }

  /**
   * Sets the current user in the AccountService based on data from localStorage.
   * This ensures that the app recognizes a previously logged-in user when it initializes.
   */
  setCurrentUser() {
    const userString = localStorage.getItem('user'); // Retrieve the user data from localStorage
    if (!userString) return; // If no user data exists, exit the method
    const user = JSON.parse(userString); // Parse the JSON string into an object
    this.accountService.setCurrentUser(user); // Update the current user in the AccountService
  }
}
