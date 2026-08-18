import { Routes } from '@angular/router';
import { authGuard, organizerGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'events' },
  {
    path: 'events',
    loadComponent: () => import('./pages/event-list/event-list.component').then((m) => m.EventListComponent)
  },
  {
    path: 'events/new',
    canActivate: [organizerGuard],
    loadComponent: () => import('./pages/event-form/event-form.component').then((m) => m.EventFormComponent)
  },
  {
    path: 'events/:id',
    loadComponent: () => import('./pages/event-detail/event-detail.component').then((m) => m.EventDetailComponent)
  },
  {
    path: 'events/:id/edit',
    canActivate: [organizerGuard],
    loadComponent: () => import('./pages/event-form/event-form.component').then((m) => m.EventFormComponent)
  },
  {
    path: 'my-registrations',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/my-registrations/my-registrations.component').then((m) => m.MyRegistrationsComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/register/register.component').then((m) => m.RegisterComponent)
  },
  { path: '**', redirectTo: 'events' }
];
