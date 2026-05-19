import { Routes } from '@angular/router';
import { AuthLayout } from '../app/pages/layouts/auth-layout/auth-layout';
import { Login } from './pages/features/auth/login/login';
import { ForgotPassword } from './pages/features/auth/forgot-password/forgot-password';
import { ResetPassword } from './pages/features/auth/reset-password/reset-password';
import { authGuard } from './core/guards/auth-guard';
import { authRedirectGuard } from './core/guards/auth-redirect';

export const routes: Routes = [
  {
    path: '',
    component: AuthLayout,
    canActivate: [authRedirectGuard],
    children: [
      { path: 'login', component: Login },
      { path: 'forgot-password', component: ForgotPassword },
      { path: 'reset-password', component: ResetPassword },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },
  {
    path: 'admin',
    loadChildren: () =>
      import('./pages/layouts/secure-layout/secure.routes').then(
        m => m.SECURE_ROUTES
      ),
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];