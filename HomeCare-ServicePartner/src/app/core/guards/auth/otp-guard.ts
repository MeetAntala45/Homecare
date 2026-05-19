import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { OtpStateService } from '../../services/auth/otp-state-service';

export const otpGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const otpStateService = inject(OtpStateService);

  const navigation = router.getCurrentNavigation();

  if (!otpStateService.isOtpRequested()) {
    router.navigate(['/login']);
    return false;
}

  return true;
};
