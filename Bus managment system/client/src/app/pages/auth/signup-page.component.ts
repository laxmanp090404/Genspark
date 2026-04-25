import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { Role } from '../../core/models';
@Component({
  selector: 'app-signup-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-slate-950 px-4 py-8">
      <div class="w-full max-w-lg rounded-2xl border border-slate-800 bg-slate-900 p-8 shadow-2xl shadow-cyan-900/20">
        <h1 class="text-2xl font-bold text-white">Create account</h1>
        <p class="mt-1 text-sm text-slate-400">Start searching and booking buses.</p>

        @if (error()) {
          <p class="mt-4 rounded bg-rose-900/50 px-3 py-2 text-sm text-rose-200">{{ error() }}</p>
        }

        <form [formGroup]="form" (ngSubmit)="submit()" class="mt-6 grid grid-cols-1 gap-4 md:grid-cols-2">
          <label class="block md:col-span-2">
            <span class="mb-1 block text-sm text-slate-300">Username</span>
            <input formControlName="username" class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none ring-cyan-500 focus:ring-2" />
          </label>
          <label class="block md:col-span-2">
            <span class="mb-1 block text-sm text-slate-300">Email</span>
            <input type="email" formControlName="email" class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none ring-cyan-500 focus:ring-2" />
          </label>
          <label class="block">
            <span class="mb-1 block text-sm text-slate-300">Gender</span>
            <select formControlName="gender" class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none ring-cyan-500 focus:ring-2">
              <option value="MALE">MALE</option>
              <option value="FEMALE">FEMALE</option>
              <option value="OTHER">OTHER</option>
            </select>
          </label>
          <label class="block">
            <span class="mb-1 block text-sm text-slate-300">Age</span>
            <input type="number" formControlName="age" class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none ring-cyan-500 focus:ring-2" />
          </label>
          <label class="block md:col-span-2">
            <span class="mb-1 block text-sm text-slate-300">Role</span>
            <select formControlName="role" class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none ring-cyan-500 focus:ring-2">
              <option value="USER">User (Can book buses)</option>
              <option value="BUS_OPERATOR">Bus Operator (Can manage buses)</option>
            </select>
          </label>
          <label class="block md:col-span-2">
            <span class="mb-1 block text-sm text-slate-300">Password</span>
            <input type="password" formControlName="password" class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none ring-cyan-500 focus:ring-2" />
          </label>
          <label class="block md:col-span-2">
            <span class="mb-1 block text-sm text-slate-300">Confirm Password</span>
            <input type="password" formControlName="confirmPassword" class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none ring-cyan-500 focus:ring-2" />
          </label>

          <button [disabled]="loading() || form.invalid" class="md:col-span-2 rounded-lg bg-cyan-600 px-4 py-2 font-semibold text-white transition hover:bg-cyan-500 disabled:cursor-not-allowed disabled:opacity-60">
            {{ loading() ? 'Creating account...' : 'Create account' }}
          </button>
        </form>

        <p class="mt-4 text-sm text-slate-400">
          Already registered?
          <a routerLink="/login" class="text-cyan-300 hover:text-cyan-200">Sign in</a>
        </p>
      </div>
    </div>
  `
})
export class SignupPageComponent {
  readonly loading = signal(false);
  readonly error = signal('');
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    gender: ['MALE'],
    age: [18, [Validators.required, Validators.min(1), Validators.max(120)]],
    role: ['USER', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]]
  });

  constructor(
    private readonly auth: AuthService
  ) {}

//   submit() {
//     if (this.form.invalid || this.loading()) return;
//     if (this.form.controls.password.value !== this.form.controls.confirmPassword.value) {
//       this.error.set('Password and confirm password must match.');
//       return;
//     }

//     this.loading.set(true);
//     this.error.set('');

//     const { confirmPassword, ...payload } = this.form.getRawValue();
//     const raw = this.form.getRawValue();
//     const payload = {
//   ...raw,
//   role: raw.role as Role
// };

// delete (payload as any).confirmPassword;
//     this.auth.signup(payload).subscribe({
//       next: () => {
//         this.loading.set(false);
//         void this.auth.redirectAfterLogin();
//       },
//       error: (err) => {
//         this.loading.set(false);
//         this.error.set(err?.error?.errors?.[0] ?? 'Unable to create account.');
//       }
//     });
//   }
submit() {
  if (this.form.invalid || this.loading()) return;

  if (this.form.controls.password.value !== this.form.controls.confirmPassword.value) {
    this.error.set('Password and confirm password must match.');
    return;
  }

  this.loading.set(true);
  this.error.set('');

  const raw = this.form.getRawValue();

  const payload = {
    ...raw,
    role: raw.role as Role
  };

  delete (payload as any).confirmPassword;

  this.auth.signup(payload).subscribe({
    next: () => {
      this.loading.set(false);
      void this.auth.redirectAfterLogin();
    },
    error: (err) => {
      this.loading.set(false);
      this.error.set(err?.error?.errors?.[0] ?? 'Unable to create account.');
    }
  });
}
}
