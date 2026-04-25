import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { catchError, map, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse, AuthPayload, Profile, Role } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly profileKey = 'busbook.profile';
  private readonly api = environment.apiBaseUrl;
  private readonly _profile = signal<Profile | null>(this.readProfile());

  readonly profile = computed(() => this._profile());
  readonly isLoggedIn = computed(() => !!this._profile());
  readonly role = computed(() => this._profile()?.role ?? null);

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {}

  signup(payload: {
    username: string;
    email: string;
    gender: string;
    age: number;
    password: string;
    role: Role;
  }) {
    return this.http.post<ApiResponse<AuthPayload>>(`${this.api}/auth/register`, payload, { withCredentials: true }).pipe(
      tap((res) => this.setProfileFromAuth(res.data))
    );
  }

  login(payload: { email: string; password: string; rememberMe: boolean }) {
    return this.http.post<ApiResponse<AuthPayload>>(`${this.api}/auth/login`, payload, { withCredentials: true }).pipe(
      tap((res) => this.setProfileFromAuth(res.data))
    );
  }

  bootstrap() {
    return this.http.get<ApiResponse<AuthPayload>>(`${this.api}/auth/me`, { withCredentials: true }).pipe(
      tap((res) => this.setProfileFromAuth(res.data)),
      map(() => true),
      catchError(() =>
        this.http.post<ApiResponse<AuthPayload>>(`${this.api}/auth/refresh`, {}, { withCredentials: true }).pipe(
          tap((res) => this.setProfileFromAuth(res.data)),
          map(() => true),
          catchError(() => {
            this.clearProfile();
            return of(false);
          })
        )
      )
    );
  }

  logout() {
    this.http.post(`${this.api}/auth/logout`, {}, { withCredentials: true }).subscribe({
      next: () => undefined,
      error: () => undefined
    });
    this.clearProfile();
    void this.router.navigate(['/login']);
  }

  canAccess(roles?: Role[]) {
    if (!roles || roles.length === 0) return true;
    const role = this._profile()?.role;
    return !!role && roles.includes(role);
  }

  redirectAfterLogin() {
    const role = this._profile()?.role;
    if (role === 'ADMIN') return this.router.navigate(['/admin/dashboard']);
    if (role === 'BUS_OPERATOR') return this.router.navigate(['/operator/dashboard']);
    return this.router.navigate(['/search']);
  }

  private readProfile(): Profile | null {
    const raw = localStorage.getItem(this.profileKey);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as Profile;
    } catch {
      localStorage.removeItem(this.profileKey);
      return null;
    }
  }

  private clearProfile() {
    localStorage.removeItem(this.profileKey);
    this._profile.set(null);
  }

  private setProfileFromAuth(auth: AuthPayload) {
    const profile: Profile = {
      userId: auth.userId,
      username: auth.username,
      email: auth.email,
      role: auth.role
    };
    localStorage.setItem(this.profileKey, JSON.stringify(profile));
    this._profile.set(profile);
  }
}
