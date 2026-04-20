import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly AUTH_TOKEN_KEY = 'auth_token';
  private readonly USER_ROLE_KEY = 'user_role';
  private readonly USERNAME_KEY = 'username';
  private readonly VENDOR_NAME_KEY = 'vendor_name';

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: any): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/auth/login`, credentials).pipe(
      tap(response => {
        if (response.success && response.data?.token) {
          localStorage.setItem(this.AUTH_TOKEN_KEY, response.data.token);
          if (response.data.role) localStorage.setItem(this.USER_ROLE_KEY, response.data.role);
          else localStorage.removeItem(this.USER_ROLE_KEY);

          if (response.data.username) localStorage.setItem(this.USERNAME_KEY, response.data.username);
          else localStorage.removeItem(this.USERNAME_KEY);

          if (response.data.vendorName) localStorage.setItem(this.VENDOR_NAME_KEY, response.data.vendorName);
          else localStorage.removeItem(this.VENDOR_NAME_KEY);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.AUTH_TOKEN_KEY);
    localStorage.removeItem(this.USER_ROLE_KEY);
    localStorage.removeItem(this.USERNAME_KEY);
    localStorage.removeItem(this.VENDOR_NAME_KEY);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.AUTH_TOKEN_KEY);
  }

  getUserRole(): string | null {
    return localStorage.getItem(this.USER_ROLE_KEY);
  }

  getUsername(): string | null {
    return localStorage.getItem(this.USERNAME_KEY) || this.getUsernameFromToken();
  }

  getVendorName(): string | null {
    return localStorage.getItem(this.VENDOR_NAME_KEY) || this.getVendorNameFromToken();
  }

  isAdmin(): boolean {
    return this.getUserRole() === 'admin';
  }

  decodeToken(token: string): any {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      
      const base64Url = parts[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const pad = base64.length % 4;
      const padded = pad ? base64 + '='.repeat(4 - pad) : base64;
      
      const jsonPayload = decodeURIComponent(window.atob(padded).split('').map(function(c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));

      return JSON.parse(jsonPayload);
    } catch (e) {
      return null;
    }
  }

  getVendorIdFromToken(): number | null {
    const token = this.getToken();
    if (!token) return null;
    const payload = this.decodeToken(token);
    const vendorId = payload?.VendorId || payload?.vendorId;
    return vendorId ? parseInt(vendorId, 10) : null;
  }

  getUsernameFromToken(): string | null {
    const token = this.getToken();
    if (!token) return null;
    const payload = this.decodeToken(token);
    return payload?.unique_name || payload?.name || payload?.Username ||
      payload?.username || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || null;
  }

  getVendorNameFromToken(): string | null {
    const token = this.getToken();
    if (!token) return null;
    const payload = this.decodeToken(token);
    return payload?.VendorName || payload?.vendorName || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/vendorname'] || null;
  }
}
