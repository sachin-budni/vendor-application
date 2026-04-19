import { Routes } from '@angular/router';
import { authGuard } from './auth.guard';
import { adminGuard } from './admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/login').then(m => m.Login)
  },
  {
    path: 'resources',
    canActivate: [authGuard],
    loadComponent: () => import('./resources/resources').then(m => m.Resources)
  },
  {
    path: 'head-count',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./head-count/head-count').then(m => m.HeadCount)
  },

  {
    path: '',
    redirectTo: 'resources',
    pathMatch: 'full'
  }
];
