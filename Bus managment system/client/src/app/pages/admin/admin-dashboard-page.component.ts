import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { AdminSummary } from '../../core/models';

import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-admin-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section>
      <h1 class="text-2xl font-bold text-white">Admin dashboard</h1>
      <p class="mt-1 text-sm text-slate-400">Monitor pending actions and platform fee.</p>

      @if (error()) {
        <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p>
      }

      <div class="mt-6 grid gap-4 sm:grid-cols-3">
        @for (card of cards(); track card.label) {
          <article class="rounded-xl border border-slate-800 bg-slate-900 p-4">
            <p class="text-sm text-slate-400">{{ card.label }}</p>
            <p class="mt-1 text-3xl font-bold text-cyan-300">{{ card.value }}</p>
          </article>
        }
      </div>

      <div class="mt-12 flex items-center justify-between mb-4">
        <h2 class="text-xl font-bold text-white">All Buses</h2>
        <a routerLink="/admin/bookings" class="text-sm font-semibold text-cyan-400 hover:text-cyan-300 flex items-center gap-1">
          View All Bookings
          <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M10.293 3.293a1 1 0 011.414 0l6 6a1 1 0 010 1.414l-6 6a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-4.293-4.293a1 1 0 010-1.414z" clip-rule="evenodd" /></svg>
        </a>
      </div>
      @if (busesLoading()) {
        <p class="text-cyan-300">Loading buses...</p>
      } @else {
        <div class="overflow-x-auto rounded-xl border border-slate-800 bg-slate-900">
          <table class="w-full text-left text-sm text-slate-300">
            <thead class="border-b border-slate-800 bg-slate-950 text-xs uppercase text-slate-400">
              <tr>
                <th class="px-4 py-3">Operator</th>
                <th class="px-4 py-3">Registration</th>
                <th class="px-4 py-3">Route</th>
                <th class="px-4 py-3">Timings</th>
                <th class="px-4 py-3">Price</th>
                <th class="px-4 py-3">Status</th>
              </tr>
            </thead>
            <tbody>
              @for (bus of buses(); track bus.busId) {
                <tr class="border-b border-slate-800 last:border-0 hover:bg-slate-800/50">
                  <td class="px-4 py-3">{{ bus.operatorName }}</td>
                  <td class="px-4 py-3 font-medium text-cyan-300">{{ bus.registrationNumber }}</td>
                  <td class="px-4 py-3">{{ bus.source }} → {{ bus.destination }}</td>
                  <td class="px-4 py-3">{{ bus.departureTime }} - {{ bus.arrivalTime }}</td>
                  <td class="px-4 py-3">₹{{ bus.seatPrice }}</td>
                  <td class="px-4 py-3">
                    <span class="rounded px-2 py-0.5 text-xs"
                      [ngClass]="{
                        'bg-emerald-900/50 text-emerald-300': bus.status === 'ACTIVE',
                        'bg-amber-900/50 text-amber-300': bus.status === 'OUT_OF_SERVICE',
                        'bg-rose-900/50 text-rose-300': bus.status === 'DELETED'
                      }">
                      {{ bus.status }}
                    </span>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </section>
  `
})
export class AdminDashboardPageComponent implements OnInit {
  readonly error = signal('');
  readonly cards = signal<{ label: string; value: string | number }[]>([]);
  readonly buses = signal<any[]>([]);
  readonly busesLoading = signal(true);

  constructor(private readonly api: ApiService) {}

  ngOnInit() {
    this.api.getAdminSummary().subscribe({
      next: (res) => this.cards.set([
        { label: 'Pending Routes', value: res.data.pendingRoutes },
        { label: 'Pending Operator Requests', value: res.data.pendingOperatorRequests },
        { label: 'Total Bookings', value: res.data.totalBookings },
        { label: 'Cancelled Bookings', value: res.data.cancelledBookings },
        { label: 'Platform Fee', value: `₹${res.data.currentPlatformFee}` }
      ]),
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to load admin summary.')
    });

    this.api.getAllBuses().subscribe({
      next: (res) => {
        this.buses.set(res.data);
        this.busesLoading.set(false);
      },
      error: () => {
        this.busesLoading.set(false);
      }
    });
  }
}
