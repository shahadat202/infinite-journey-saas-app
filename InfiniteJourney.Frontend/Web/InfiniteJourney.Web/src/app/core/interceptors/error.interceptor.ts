import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ApiErrorResponse } from '@core/models/api-error.model';
import { ToastService } from '@core/services/toast.service';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const apiError = error.error as ApiErrorResponse | undefined;
      const message = resolveMessage(error, apiError);
      toast.error(message);
      return throwError(() => error);
    })
  );
};

function resolveMessage(error: HttpErrorResponse, apiError?: ApiErrorResponse): string {
  if (apiError?.errors?.length) {
    return apiError.errors.map((e) => e.message).join(' ');
  }

  if (apiError?.message) return apiError.message;

  if (error.status === 0) return 'Network error. Check API and CORS settings.';
  if (error.status === 401) return 'Please sign in to continue.';
  if (error.status === 403) return 'You do not have permission for this action.';
  if (error.status === 404) return 'The requested resource was not found.';

  return error.message || 'An unexpected error occurred.';
}
