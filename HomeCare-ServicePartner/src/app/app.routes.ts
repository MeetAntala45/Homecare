import { Routes } from '@angular/router';
import { LoginComponent } from './pages/features/service-partner-auth/login/login';
import { AuthLayout } from './pages/layouts/auth-layout/auth-layout';
import { VerifyOtpComponent } from './pages/features/service-partner-auth/verify-otp/verify-otp';
import { otpGuard } from './core/guards/auth/otp-guard';
import { loginauthGuard } from './core/guards/auth/auth-guard';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
    {
        path: 'login',
        component: AuthLayout,
        children: [
            {
                path: '',
                canActivate: [loginauthGuard],
                component: LoginComponent
            },
            {
                path: 'verify-otp',
                canActivate: [otpGuard],
                component: VerifyOtpComponent
            }
        ]
    },

    {
        path: 'service-partner',
        canActivate: [authGuard],
        loadChildren: () =>
            import('./pages/layouts/secure-layout/secure.routes')
                .then(m => m.SECURE_ROUTES)
    },

    {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
    },

    {
        path: '**',
        redirectTo: 'login'
    }
];