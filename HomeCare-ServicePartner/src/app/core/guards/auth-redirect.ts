import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth/auth-service';
import { SERVICE_PARTNER_ACCESS_TOKEN_KEY } from '../constants/environment-config';

export const authRedirectGuard: CanActivateFn = async () => {

    const router = inject(Router);
    const authService = inject(AuthService);

    const token = localStorage.getItem(SERVICE_PARTNER_ACCESS_TOKEN_KEY);

    if (!token) return true;

    if (isTokenValid(token)) {
        return router.createUrlTree(['/admin/dashboard']);
    }

    return router.createUrlTree(['/admin/login']);


    function isTokenValid(token: string): boolean {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return Date.now() < payload.exp * 1000;
        } catch {
            return false;
        }
    }
};