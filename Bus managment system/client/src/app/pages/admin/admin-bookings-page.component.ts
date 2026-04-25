import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-admin-bookings-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section>
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-white">System Bookings</h1>
          <p class="mt-1 text-sm text-slate-400">All bookings across the entire platform.</p>
        </div>
      </div>

      @if (error()) {
        <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p>
      }

      <div class="mt-8 overflow-x-auto rounded-xl border border-slate-800 bg-slate-900">
        <table class="w-full text-left text-sm text-slate-300">
          <thead class="border-b border-slate-800 bg-slate-950 text-xs uppercase text-slate-400">
            <tr>
              <th class="px-4 py-3">Booking ID</th>
              <th class="px-4 py-3">User</th>
              <th class="px-4 py-3">Bus/Route</th>
              <th class="px-4 py-3">Travel Date</th>
              <th class="px-4 py-3">Amount</th>
              <th class="px-4 py-3">Status</th>
              <th class="px-4 py-3">Created At</th>
            </tr>
          </thead>
          <tbody>
            @if (loading()) {
              <tr>
                <td colspan="7" class="px-4 py-8 text-center text-cyan-300">Loading bookings...</td>
              </tr>
            } @else if (bookings().length === 0) {
              <tr>
                <td colspan="7" class="px-4 py-8 text-center text-slate-500">No bookings found.</td>
              </tr>
            } @else {
              @for (b of bookings(); track b.bookingId) {
                <tr class="border-b border-slate-800 last:border-0 hover:bg-slate-800/50">
                  <td class="px-4 py-3 font-mono text-[10px] text-slate-400">{{ b.bookingId }}</td>
                  <td class="px-4 py-3">
                    <div class="flex flex-col">
                      <span class="font-medium text-white">{{ b.userName }}</span>
                      <span class="text-[10px] text-slate-500">{{ b.userEmail }}</span>
                    </div>
                  </td>
                  <td class="px-4 py-3">
                    <div class="flex flex-col">
                      <span class="text-cyan-300 font-medium">{{ b.registrationNumber }}</span>
                      <span class="text-[10px]">{{ b.source }} → {{ b.destination }}</span>
                    </div>
                  </td>
                  <td class="px-4 py-3">{{ b.travelDate }}</td>
                  <td class="px-4 py-3 font-medium text-white">₹{{ b.totalAmount }}</td>
                  <td class="px-4 py-3">
                    <span class="rounded px-2 py-0.5 text-xs"
                      [ngClass]="{
                        'bg-emerald-900/50 text-emerald-300': b.status === 'CONFIRMED',
                        'bg-amber-900/50 text-amber-300': b.status === 'PENDING',
                        'bg-rose-900/50 text-rose-300': b.status.includes('CANCELLED')
                      }">
                      {{ b.status }}
                    </span>
                  </td>
                  <td class="px-4 py-3 text-slate-400">{{ b.createdAt | date:'short' }}</td>
                </tr>
              }
            }
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class AdminBookingsPageComponent implements OnInit {
  readonly bookings = signal<any[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');

  constructor(private readonly api: ApiService) {}

  ngOnInit() {
    this.api.getAllBookings().subscribe({
      next: (res) => {
        this.bookings.set(res.data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.errors?.[0] ?? 'Failed to load bookings.');
        this.loading.set(false);
      }
    });
  }
}
