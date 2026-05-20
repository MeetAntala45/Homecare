import { Routes } from '@angular/router';
import { authGuard } from '../../../core/guards/auth-guard';

export const SECURE_ROUTES: Routes = [
    {
        path: '',
        canActivate: [authGuard],
        loadComponent: () =>
            import('./secure-layout').then(m => m.SecureLayout),
        children: [
            {
                path: 'dashboard',
                loadComponent: () =>
                    import('../../features/dashboard/dashboard').then(m => m.Dashboard)
            },
            {
                path: 'master-data',
                loadComponent: () =>
                    import('../../features/master-data/master-data')
                        .then(m => m.MasterData)
            },
            {
                path: 'offers',
                loadComponent: () =>
                    import('../../features/offers/offers/offers.js')
                        .then(m => m.OffersComponent)
            },
            {
                path: 'service-management',
                loadComponent: () =>
                    import('../../features//service-management/service-management')
                        .then(m => m.ServiceManagement)
            },
            {
                path: 'service-management/services/:id',
                loadComponent: () =>
                    import('../../features/service-management/service-detail/service-detail')
                        .then(m => m.ServiceDetail)
            },
            {
                path: 'profile',
                loadComponent: () =>
                    import('../../features/profile/profile')
                        .then(p => p.Profile)
            },
            {
                path: 'admin-users',
                loadComponent: () =>
                    import('../../features/admin-user-management/admin-users/admin-users')
                        .then(m => m.AdminUsers)
            },
            {
                path: 'service-partners',
                loadComponent: () =>
                    import('../../features/service-partner/service-partner')
                        .then(m => m.ServicePartner)
            },
            {
                path: 'service-partners/:id',
                loadComponent: () =>
                    import('../../features/service-partner/service-partner-detail/service-partner-detail')
                        .then(m => m.ServicePartnerDetail)
            },
            {
                path: 'customer-users',
                loadComponent: () =>
                    import('../../features/customer-user-management/customer-user/customer-user')
                        .then(m => m.CustomerUser)
            },
            {
                path: 'customer-users/:id',
                loadComponent: () =>
                    import('../../features/customer-user-management/customer-user-detail/customer-user-detail')
                        .then(m => m.CustomerUserDetail)
            },
            {
                path: 'payments',
                loadComponent: () =>
                    import('../../features/payments-and-transactions/payments/payments')
                        .then(m => m.Payments)
            },
            {
                path: 'payments/:id',
                loadComponent: () =>
                    import('../../features/payments-and-transactions/payment-detail/payment-detail')
                        .then(m => m.PaymentDetail)
            },
            {
                path: 'support',
                loadComponent: () =>
                    import('./../../features/support/support')
                        .then(m => m.Support)
            },
            {
                path: 'booking-management',
                loadComponent: () =>
                    import('../../features/booking-management/booking-management')
                        .then(m => m.BookingManagement)
            },
            {
                path: 'reviews',
                loadComponent: () =>
                  import('../../features/reviews/reviews')
                    .then(m => m.Reviews)
              },
            {
                path:'leave-requests',
                loadComponent :()=>
                    import('../../features//admin-leave-request/admin-leave-request')
                .then(m=>m.AdminLeaveRequest)
            },
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
        ]
    }
]