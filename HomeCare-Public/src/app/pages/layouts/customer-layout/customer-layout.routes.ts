import { Routes } from '@angular/router';

export const CustomerRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./customer-layout').then(m => m.CustomerLayout),
    children: [
      {
        path: '', redirectTo: 'home', pathMatch: 'full'
      },
      {
        path: 'home',
        loadComponent: () =>
          import('../../features/home-page/homepage')
            .then(m => m.Homepage)
      }, {
        path: 'services',
        loadComponent: () =>
          import('../../features/services-page/servicespage.js')
            .then(m => m.ServicePage)
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('../../features/profile/profile')
            .then(m => m.Profile)
      },
      {
        path: 'contact-us',
        loadComponent: () =>
          import('../../features/contact-us/contact-us.js')
            .then(m => m.ContactUs)
      },
      {
        path: 'service-list',
        loadComponent: () =>
          import('../../features/service-listing-page/service-listing-page/service-listing-page.js')
            .then(m => m.ServiceListingPage)
      },
      {
        path: 'service-details/:id',
        loadComponent: () =>
          import('../../features/service-detail/service-detail')
            .then(m => m.ServiceDetail)
      },
      {
        path: 'my-bookings',
        loadComponent: () =>
          import('./../../features/my-bookings/my-bookings')
            .then(m => m.MyBookings)
      }
    ]
  }
];