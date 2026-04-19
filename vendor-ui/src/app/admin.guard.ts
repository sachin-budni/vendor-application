import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';

export const adminGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const role = localStorage.getItem('user_role');

  if (role && role.toLowerCase() === 'admin') {
    return true;
  }

  // If not admin, redirect back to a safe route or generic dashboard
  return router.createUrlTree(['/resources']);
};
