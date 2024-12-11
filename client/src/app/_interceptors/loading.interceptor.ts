import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BusyService } from '../_services/busy.service';
import { delay, finalize } from 'rxjs';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService); // Injects the BusyService to control the spinner.

  busyService.busy(); // Notify the BusyService that a new HTTP request has started.

  return next(req).pipe(
    delay(1000), // Artificially delays the response by 1 second (useful for demo/testing purposes).
    finalize(() => {
      busyService.idle(); // Notify the BusyService when the HTTP request completes to stop loading.
    })
  );
};
