export interface ApiFieldError {
  field: string;
  message: string;
}

export interface ApiErrorResponse {
  statusCode: number;
  message: string;
  errorCode: string;
  traceId?: string;
  errors?: ApiFieldError[];
}
