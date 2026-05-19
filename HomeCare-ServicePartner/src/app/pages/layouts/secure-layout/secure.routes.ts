import { Routes } from '@angular/router';
// import { authGuard } from '../../../core/guards/auth-guard';

export const SECURE_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('./secure-layout').then(m => m.SecureLayout),
        children: [
            {
                path: 'dashboard',
                loadComponent: () =>
                    import('../../features/dashboard/dashboard').then(m => m.PartnerDashboard)
            },
            {
                path: 'my-jobs',
                loadComponent: () =>
                    import('../../features/my-jobs/my-jobs').then(m => m.MyJobs)
            },
            {
                path: 'my-services',
                loadComponent: () =>
                    import('../../features/my-service/my-service').then(m => m.MyService)
            },
            {
                path: 'ratings-reviews',
                loadComponent: () =>
                    import('../../features/ratings-reviews/ratings-reviews').then(m => m.RatingsReviews)
            },
            {
                path: 'service-detail/:id',
                loadComponent: () =>
                    import('../../features/my-service/myservice-detail/myservice-detail')
                        .then(m => m.ServiceDetailComponent)
            },
            {
                path: 'profile',
                loadComponent: () =>
                    import('../../features/partner-profile/partner-profile')
                        .then(m => m.PartnerProfile)
            },
            {
            path:'leaves',
            loadComponent:()=>
                import('../../features/partner-leave/partner-leave').
            then(m=>m.PartnerLeave)
}   ,
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
        ]
    }
]