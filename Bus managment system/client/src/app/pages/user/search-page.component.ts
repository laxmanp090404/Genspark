import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { BusSearchResult } from '../../core/models';
import { RouterLink, Router } from '@angular/router';
import { BookingGuardModalComponent } from '../../shared/components/booking-guard-modal.component';

@Component({
  selector: 'app-search-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BookingGuardModalComponent],
  template: `
    <section>
      <h1 class="text-2xl font-bold text-white">Find your bus</h1>
      <p class="mt-1 text-sm text-slate-400">Search by source, destination and date.</p>

      @if (showGuardModal()) {
        <app-booking-guard-modal 
          [mode]="guardMode()" 
          (close)="showGuardModal.set(false)"
        />
      }

      <form [formGroup]="form" (ngSubmit)="search()" class="mt-6 grid grid-cols-1 gap-3 rounded-xl border border-slate-800 bg-slate-900 p-4 md:grid-cols-4">
        <input list="places" formControlName="source" placeholder="Source" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />
        <input list="places" formControlName="destination" placeholder="Destination" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />
        <input type="date" formControlName="date" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />
        <button [disabled]="loading() || form.invalid" class="rounded-lg bg-cyan-600 px-4 py-2 font-semibold text-white hover:bg-cyan-500 disabled:opacity-60">
          {{ loading() ? 'Searching...' : 'Search' }}
        </button>
      </form>

      <datalist id="places">
        @for (place of places(); track place) {
          <option [value]="place"></option>
        }
      </datalist>

      @if (error()) {
        <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p>
      }

      @if (!loading() && searched() && buses().length === 0) {
        <p class="mt-4 rounded border border-slate-800 bg-slate-900 px-3 py-2 text-sm text-slate-300">No bus found.</p>
      }

      <div class="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        @for (bus of buses(); track bus.busId + bus.scheduleId) {
          <article class="rounded-xl border border-slate-800 bg-slate-900 p-4">
            <div class="flex items-start justify-between">
              <h2 class="font-semibold text-cyan-300">{{ bus.source }} → {{ bus.destination }}</h2>
              <span class="rounded px-2 py-0.5 text-xs" [class]="bus.availableSeats > 0 ? 'bg-emerald-900/50 text-emerald-300' : 'bg-rose-900/50 text-rose-300'">
                {{ bus.availableSeats }} seats
              </span>
            </div>
            <p class="mt-2 text-sm text-slate-300">{{ bus.operatorName }} · {{ bus.registrationNumber }}</p>
            <p class="mt-1 text-sm text-slate-400">{{ bus.travelDate }} · {{ bus.departureTime }} - {{ bus.arrivalTime }}</p>
            <p class="mt-3 text-lg font-semibold text-white">₹{{ bus.seatPrice }}</p>
            @if (bus.scheduleId && bus.availableSeats > 0) {
              <button (click)="handleBookClick(bus.scheduleId)" class="mt-4 block w-full text-center rounded-lg bg-cyan-600 px-4 py-2 font-semibold text-white hover:bg-cyan-500">
                {{ (auth.role() === 'ADMIN' || auth.role() === 'BUS_OPERATOR') ? 'View Seats' : 'Book' }}
              </button>
            } @else if (bus.scheduleId) {
              <button disabled class="mt-4 w-full cursor-not-allowed rounded-lg bg-slate-700 px-4 py-2 font-semibold text-slate-400">
                Sold Out
              </button>
            }
          </article>
        }
      </div>
    </section>
  `
})
export class SearchPageComponent implements OnInit {
  readonly loading = signal(false);
  readonly error = signal('');
  readonly searched = signal(false);
  readonly buses = signal<BusSearchResult[]>([]);
  readonly places = signal<string[]>([]);
  readonly showGuardModal = signal(false);
  readonly guardMode = signal<'LOGIN' | 'ROLE_DENIED'>('LOGIN');

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    source: ['', [Validators.required]],
    destination: ['', [Validators.required]],
    date: [new Date().toISOString().split('T')[0], [Validators.required]]
  });

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
    public readonly auth: AuthService
  ) {}

  ngOnInit() {
    this.api.getPlaces(undefined, 500).subscribe({
      next: (res) => this.places.set(res.data),
      error: () => this.places.set([])
    });
  }

  handleBookClick(scheduleId: string) {
    if (!this.auth.isLoggedIn()) {
      this.guardMode.set('LOGIN');
      this.showGuardModal.set(true);
      return;
    }

    if (this.auth.role() === 'ADMIN' || this.auth.role() === 'BUS_OPERATOR') {
      // If the user wants to strictly block even viewing, show ROLE_DENIED.
      // But usually Admins/Operators can View Seats.
      // The requirement says "if admin or operator try to book bus give a modal popup".
      // I'll allow them to enter the page but block the actual booking button there.
      // OR I can block them here too. 
      // Let's block them here for "Book" but if it says "View Seats" maybe they can.
      // Wait, the button label changes.
      this.router.navigate(['/book', scheduleId]);
      return;
    }

    this.router.navigate(['/book', scheduleId]);
  }

  search() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.searched.set(true);
    this.error.set('');
    const { source, destination, date } = this.form.getRawValue();
    this.api.searchBuses(source, destination, date).subscribe({
      next: (res) => {
        this.buses.set(res.data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.errors?.[0] ?? 'Unable to fetch buses.');
        this.loading.set(false);
      }
    });
  }
}
