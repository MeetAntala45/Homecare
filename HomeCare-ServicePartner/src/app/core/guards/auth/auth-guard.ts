import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { OtpStateService } from '../../services/auth/otp-state-service';
import { SERVICE_PARTNER_ACCESS_TOKEN_KEY } from '../../constants/environment-config';

export const loginauthGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const otpState = inject(OtpStateService);

  if (localStorage.getItem(SERVICE_PARTNER_ACCESS_TOKEN_KEY)) {
    router.navigate(['/service-partner/dashboard']);
    return false;
  }
  return true;
};
