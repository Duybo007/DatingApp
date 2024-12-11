import { inject, Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root' // Makes this service available throughout the application without needing to add it to a specific module.
})
export class BusyService {
  private spinnerService = inject(NgxSpinnerService); // Injects the spinner service for displaying a loading spinner.
  
  busyRequestCount = 0; // Tracks the number of active HTTP requests.

  busy() {
    this.busyRequestCount++; // Increment the count for an active HTTP request.
    this.spinnerService.show(undefined, { // Shows the spinner when a request is active.
      type: 'line-scale-party', // Spinner animation type.
      bdColor: 'rgba(255,255,255,0)', // Background color of the spinner.
      color: '#333333' // Spinner color.
    });
  }

  idle() {
    this.busyRequestCount--; // Decrement the count when a request finishes.
    if (this.busyRequestCount <= 0) { // If there are no active requests:
      this.busyRequestCount = 0; // Ensure the count doesn't go negative.
      this.spinnerService.hide(); // Hide the spinner.
    }
  }
}
