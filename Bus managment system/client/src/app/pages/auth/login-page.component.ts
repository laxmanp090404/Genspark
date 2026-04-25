import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-slate-950 px-4">
      <div class="w-full max-w-md rounded-2xl border border-slate-800 bg-slate-900 p-8 shadow-2xl shadow-cyan-900/20">
        <h1 class="text-2xl font-bold text-white">Welcome back</h1>
        <p class="mt-1 text-sm text-slate-400">Sign in to continue booking.</p>

        @if (error()) {
          <p class="mt-4 rounded bg-rose-900/50 px-3 py-2 text-sm text-rose-200">{{ error() }}</p>
        }

        <form [formGroup]="form" (ngSubmit)="submit()" class="mt-6 space-y-4">
          <label class="block">
            <span class="mb-1 block text-sm text-slate-300">Email</span>
            <input type="email" formControlName="email" class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none ring-cyan-500 focus:ring-2" />
          </label>
          <label class="block">
            <span class="mb-1 block text-sm text-slate-300">Password</span>
            <input type="password" formControlName="password" class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none ring-cyan-500 focus:ring-2" />
          </label>
          <label class="flex items-center gap-2 text-sm text-slate-300">
            <input type="checkbox" formControlName="rememberMe" class="accent-cyan-500" />
            Remember me for 30 days
          </label>

          <button [disabled]="loading() || form.invalid" class="w-full rounded-lg bg-cyan-600 px-4 py-2 font-semibold text-white transition hover:bg-cyan-500 disabled:cursor-not-allowed disabled:opacity-60">
            {{ loading() ? 'Signing in...' : 'Sign in' }}
          </button>
        </form>

        <p class="mt-4 text-sm text-slate-400">
          New user?
          <a routerLink="/signup" class="text-cyan-300 hover:text-cyan-200">Create account</a>
        </p>
      </div>
    </div>
  `
})
export class LoginPageComponent {
  readonly loading = signal(false);
  readonly error = signal('');
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    rememberMe: [false]
  });

  constructor(
    private readonly auth: AuthService
  ) {}

  submit() {
    if (this.form.invalid || this.loading()) return;
    this.loading.set(true);
    this.error.set('');

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.loading.set(false);
        void this.auth.redirectAfterLogin();
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.errors?.[0] ?? 'Invalid credentials.');
      }
    });
  }
}
