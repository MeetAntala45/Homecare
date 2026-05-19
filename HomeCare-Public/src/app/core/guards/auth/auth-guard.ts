import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { OtpStateService } from '../../services/auth/otp-state-service';
import { CUSTOMER_ACCESS_TOKEN_KEY } from '../../constants/environment-config';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const otpState = inject(OtpStateService);

  if(localStorage.getItem(CUSTOMER_ACCESS_TOKEN_KEY)){
    router.navigate(['/homepage']);
    return false;
  }
  return true;
};
