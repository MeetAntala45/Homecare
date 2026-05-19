import { Routes } from '@angular/router';
import { AuthLayout } from '../app/pages/layouts/auth-layout/auth-layout';
import { Login } from './pages/features/customer-auth/login/login';
import { VerifyOtp } from './pages/features/customer-auth/verify-otp/verify-otp';
import { otpGuard } from './core/guards/auth/otp-guard';
import { authGuard } from './core/guards/auth/auth-guard';


export const routes: Routes = [
  {
    path: 'login',
    component: AuthLayout,
    children: [
      {
        path: '',
        canActivate: [authGuard],
        component: Login
      },
      {
        path: 'verify-otp',
        canActivate: [otpGuard],
        component: VerifyOtp
      },
    ]
  },
  {
    path: 'customer',
    loadChildren: () =>
      import('./pages/layouts/empty-layout/empty-layout.routes')
        .then(m => m.EmptyRoutes)
  },
  {
    path: '**', redirectTo: 'customer', pathMatch: 'full'
  }
];
