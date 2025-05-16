import { Injectable } from '@angular/core';
import {
  CanActivate,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  Router
} from '@angular/router';
import {AuthService} from '../../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    const expectedRoles = route.data['roles'] as string[]; // defined in routing
    const userRole = this.authService.getUserRole();

    if (expectedRoles.includes(userRole ?? '')) {
      return true;
    }

    // Optionally redirect
    this.router.navigate(['/unauthorized']);
    return false;
  }
}
