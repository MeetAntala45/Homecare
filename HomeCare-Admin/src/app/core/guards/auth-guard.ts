  import { inject } from '@angular/core';
  import { CanActivateFn, Router } from '@angular/router';
  import { AuthService } from '../services/auth/auth-service';
import { ADMIN_ACCESS_TOKEN_KEY } from '../constants/environment-config';

  export const authGuard: CanActivateFn = async () => {

    const router = inject(Router);
    const authService = inject(AuthService);

    const token = localStorage.getItem(ADMIN_ACCESS_TOKEN_KEY);

    if (!token) {
      return router.createUrlTree(['/login']);
    }

    return true;
  };

