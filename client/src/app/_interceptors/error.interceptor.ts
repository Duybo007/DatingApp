import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs';

// Custom HTTP interceptor to handle API errors globally
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  // Injecting dependencies
  const router = inject(Router); // Router to navigate based on error types
  const toastr = inject(ToastrService); // Toastr for displaying user-friendly error notifications

  return next(req).pipe(
    catchError(error => {
      // Check if the error object exists
      if (error) {
        switch (error.status) {
          case 400: // Bad Request
            if (error.error.errors) {
              // Handle validation errors
              const modalStateErrors = [];
              for (const key in error.error.errors) {
                if (error.error.errors[key]) {
                  modalStateErrors.push(error.error.errors[key]); // Collect error messages
                }
              }
              throw modalStateErrors.flat(); // Flatten the array and throw it for further handling
            } else {
              toastr.error(error.error); // Display other 400 errors
            }
            break;

          case 401: // Unauthorized
            toastr.error('Unauthorised', error.status); // Show unauthorized error message
            break;

          case 404: // Not Found
            router.navigateByUrl('/not-found'); // Navigate to a not-found page
            break;

          case 500: // Internal Server Error
            // Pass error details via state to the server-error page
            const navigationExtras: NavigationExtras = { state: { error: error.error } };
            router.navigateByUrl('/server-error', navigationExtras);
            break;

          default: // Any other unexpected error
            toastr.error("Something unexpected went wrong");
            break;
        }
      }
      // Rethrow the error to ensure the subscriber knows about it
      throw error;
    })
  );
};

// Explanation of Key Sections:
// 1. Interceptor Setup:
// The HttpInterceptorFn is used to define a custom HTTP interceptor, intercepting HTTP requests and responses.

// 2.Dependency Injection:
// The inject function allows access to services like Router and ToastrService.

// 3. Error Handling Logic:
// Based on the error.status code, the interceptor performs specific actions:
// 400 (Bad Request): Handles validation errors or other bad request issues.
// 401 (Unauthorized): Displays a toast notification for authentication errors.
// 404 (Not Found): Redirects the user to a predefined not-found page.
// 500 (Internal Server Error): Passes the error details to a server-error page for display.
// Default Case: Handles any unexpected errors gracefully with a generic toast notification.

// 4. Error Propagation:
// The throw statements ensure the error is passed to the subscriber (e.g., component) for further handling if needed.

// 5. Toastr Notifications:
// Provides immediate visual feedback to the user about errors in a user-friendly manner.

// 6. Navigation Handling:
// Redirects to appropriate pages (e.g., /not-found, /server-error) for specific error scenarios, enhancing the user experience.
// This interceptor centralizes error handling, ensuring consistent behavior across the application. 