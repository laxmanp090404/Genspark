import { CommonModule } from '@angular/common';
import { Component, computed } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="min-h-screen bg-slate-950 text-slate-100">
      <header class="border-b border-slate-800 bg-slate-900/80 backdrop-blur">
        <div class="mx-auto flex w-full max-w-7xl items-center justify-between px-4 py-3">
          <a routerLink="/search" class="text-xl font-semibold text-cyan-300">BusBook</a>
          <nav class="flex flex-wrap items-center gap-2 text-sm">
            <a routerLink="/search" routerLinkActive="bg-slate-700 text-white" class="rounded px-3 py-1.5 text-slate-300 hover:bg-slate-800">Search</a>
            @if (isUser()) {
              <a routerLink="/my-bookings" routerLinkActive="bg-slate-700 text-white" class="rounded px-3 py-1.5 text-slate-300 hover:bg-slate-800">My Bookings</a>
              <a routerLink="/operator-request" routerLinkActive="bg-slate-700 text-white" class="rounded px-3 py-1.5 text-slate-300 hover:bg-slate-800">Become Operator</a>
            }
            @if (isOperator()) {
              <a routerLink="/operator/dashboard" routerLinkActive="bg-slate-700 text-white" class="rounded px-3 py-1.5 text-slate-300 hover:bg-slate-800">Operator</a>
              <a routerLink="/operator/routes" routerLinkActive="bg-slate-700 text-white" class="rounded px-3 py-1.5 text-slate-300 hover:bg-slate-800">Routes</a>
              <a routerLink="/operator/buses" routerLinkActive="bg-slate-700 text-white" class="rounded px-3 py-1.5 text-slate-300 hover:bg-slate-800">Buses</a>
            }
            @if (isAdmin()) {
              <a routerLink="/admin/dashboard" routerLinkActive="bg-slate-700 text-white" class="rounded px-3 py-1.5 text-slate-300 hover:bg-slate-800">Admin</a>
              <a routerLink="/admin/routes" routerLinkActive="bg-slate-700 text-white" class="rounded px-3 py-1.5 text-slate-300 hover:bg-slate-800">Approvals</a>
              <a routerLink="/admin/operator-requests" routerLinkActive="bg-slate-700 text-white" class="rounded px-3 py-1.5 text-slate-300 hover:bg-slate-800">Operators</a>
              <a routerLink="/admin/config" routerLinkActive="bg-slate-700 text-white" class="rounded px-3 py-1.5 text-slate-300 hover:bg-slate-800">Config</a>
            }
          </nav>
          <div class="flex items-center gap-3">
            @if (auth.profile()) {
              <div class="text-right">
                <p class="text-xs text-slate-400">Signed in as</p>
                <p class="text-sm font-medium">{{ auth.profile()?.username }}</p>
              </div>
              <button type="button" (click)="auth.logout()" class="rounded bg-cyan-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-cyan-500">Logout</button>
            } @else {
              <a routerLink="/login" class="rounded bg-slate-800 px-3 py-1.5 text-sm font-medium text-slate-100 hover:bg-slate-700">Login</a>
              <a routerLink="/signup" class="rounded bg-cyan-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-cyan-500">Sign up</a>
            }
          </div>
        </div>
      </header>

      <main class="mx-auto w-full max-w-7xl px-4 py-8">
        <router-outlet />
      </main>
    </div>
  `
})
export class ShellComponent {
  constructor(public readonly auth: AuthService) {}

  readonly isUser = computed(() => this.auth.role() === 'USER');
  readonly isOperator = computed(() => this.auth.role() === 'BUS_OPERATOR');
  readonly isAdmin = computed(() => this.auth.role() === 'ADMIN');
}
