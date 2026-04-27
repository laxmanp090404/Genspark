import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-booking-guard-modal',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div class="w-full max-w-md rounded-2xl border border-slate-800 bg-slate-900 p-6 shadow-2xl shadow-cyan-500/10">
        <div class="flex flex-col items-center text-center">
          <div class="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-amber-900/30 text-amber-400">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
          
          <h2 class="text-xl font-bold text-white">{{ title }}</h2>
          <p class="mt-2 text-slate-400">{{ message }}</p>
          
          <div class="mt-8 flex w-full flex-col gap-3">
            @if (mode === 'LOGIN') {
              <a routerLink="/login" class="flex w-full items-center justify-center rounded-lg bg-cyan-600 px-4 py-2.5 font-semibold text-white hover:bg-cyan-500 transition-colors">
                Login Now
              </a>
              <a routerLink="/signup" class="flex w-full items-center justify-center rounded-lg border border-slate-700 bg-slate-800 px-4 py-2.5 font-semibold text-slate-300 hover:bg-slate-700 transition-colors">
                Create Account
              </a>
            } @else {
              <button (click)="close.emit()" class="flex w-full items-center justify-center rounded-lg bg-slate-800 border border-slate-700 px-4 py-2.5 font-semibold text-slate-300 hover:bg-slate-700 transition-colors">
                Back to Search
              </button>
            }
            <button (click)="close.emit()" class="mt-2 text-sm text-slate-500 hover:text-slate-400">Close</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class BookingGuardModalComponent {
  @Input() mode: 'LOGIN' | 'ROLE_DENIED' = 'LOGIN';
  @Output() close = new EventEmitter<void>();

  get title() {
    return this.mode === 'LOGIN' ? 'Login Required' : 'Access Denied';
  }

  get message() {
    return this.mode === 'LOGIN' 
      ? 'Please login to book your bus tickets and manage your journeys.' 
      : 'Administrators and Bus Operators are not allowed to book tickets. Please use a user account.';
  }
}
