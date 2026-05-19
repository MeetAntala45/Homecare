import { Routes } from '@angular/router';


export const EmptyRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./empty-layout').then((m) => m.EmptyLayout),
    children: [
      {
        path: '',
        redirectTo: '',
        pathMatch: 'full',
      },
      {
        path: '',
        loadChildren: () =>
          import('../customer-layout/customer-layout.routes').then((m) => m.CustomerRoutes),
      },
    ],
  },
  {
    path: 'checkout/:id',
    loadComponent: () => import('../../features/checkout/checkout').then((m) => m.Checkout),
  },
  {
    path: 'booking/success',
    loadComponent: () =>
      import('../../features/booking-success/booking-success').then((m) => m.BookingSuccess),
  },
  {
    path: 'service-partner/onboarding',
    loadComponent: () =>
      import('../../features/service-partner/service-partner').then((m) => m.ServicePartner),
  },
  {
    path: 'service-partner/success',
    loadComponent: () =>
      import('../../features/service-partner-success/service-partner-success').then(
        (m) => m.ServicePartnerSuccess
      ),
  },
];
