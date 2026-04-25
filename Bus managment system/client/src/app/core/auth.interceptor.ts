import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const withCredentials = req.clone({ withCredentials: true });
  return next(withCredentials);
};
