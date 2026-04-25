import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { OperatorBus, RouteItem } from '../../core/models';

@Component({
  selector: 'app-operator-buses-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section>
      <h1 class="text-2xl font-bold text-white">Bus management</h1>
      <p class="mt-1 text-sm text-slate-400">Create buses for your approved routes.</p>

      <form [formGroup]="form" (ngSubmit)="createBus()" class="mt-6 grid grid-cols-1 gap-3 rounded-xl border border-slate-800 bg-slate-900 p-4 md:grid-cols-2">
        <select formControlName="routeId" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100">
          <option value="">Select approved route</option>
          @for (route of approvedRoutes(); track route.routeId) {
            <option [value]="route.routeId">{{ route.source }} → {{ route.destination }}</option>
          }
        </select>
        <input formControlName="registrationNumber" placeholder="Registration Number" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />
        <input type="time" formControlName="departureTime" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 time-white" placeholder="Departure Time " />
        <input type="time" formControlName="arrivalTime" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 time-white" placeholder="Arrival Time" />
        <input type="number" formControlName="seatPrice" placeholder="Seat price" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />
        <button [disabled]="form.invalid" class="rounded-lg bg-cyan-600 px-4 py-2 font-semibold text-white hover:bg-cyan-500">Create bus</button>
      </form>

      @if (message()) { <p class="mt-4 rounded bg-emerald-900/40 px-3 py-2 text-sm text-emerald-200">{{ message() }}</p> }
      @if (error()) { <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p> }

      <div class="mt-6 overflow-hidden rounded-xl border border-slate-800">
        <table class="min-w-full divide-y divide-slate-800 bg-slate-900">
          <thead class="bg-slate-800/80 text-left text-xs uppercase tracking-wide text-slate-400">
            <tr><th class="px-4 py-3">Bus</th><th class="px-4 py-3">Route</th><th class="px-4 py-3">Price</th><th class="px-4 py-3">Status</th><th class="px-4 py-3">Actions</th></tr>
          </thead>
          <tbody class="divide-y divide-slate-800 text-sm text-slate-200">
            @for (bus of buses(); track bus.busId) {
              <tr>
                <td class="px-4 py-3">{{ bus.registrationNumber }}</td>
                <td class="px-4 py-3">{{ bus.source }} → {{ bus.destination }}</td>
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
                <td class="px-4 py-3 space-x-2">
                  <button 
                    [disabled]="bus.status === 'DELETED'" 
                    (click)="toggleStatus(bus)" 
                    class="rounded bg-amber-600 px-2 py-1 text-xs text-white hover:bg-amber-500 disabled:opacity-50 disabled:cursor-not-allowed">
                    Toggle
                  </button>
                  <button 
                    [disabled]="bus.status === 'DELETED'" 
                    (click)="deleteBus(bus.busId)" 
                    class="rounded bg-rose-600 px-2 py-1 text-xs text-white hover:bg-rose-500 disabled:opacity-50 disabled:cursor-not-allowed">
                    Delete
                  </button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class OperatorBusesPageComponent implements OnInit {
  readonly buses = signal<OperatorBus[]>([]);
  readonly approvedRoutes = signal<RouteItem[]>([]);
  readonly error = signal('');
  readonly message = signal('');
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    routeId: ['', [Validators.required]],
    registrationNumber: ['', [Validators.required]],
    departureTime: ['', [Validators.required]],
    arrivalTime: ['', [Validators.required]],
    seatPrice: [1, [Validators.required, Validators.min(0.01)]]
  });

  constructor(
    private readonly api: ApiService
  ) {}

  ngOnInit() {
    this.load();
  }

  createBus() {
    if (this.form.invalid) return;
    const payload = this.form.getRawValue();
    this.api.createBus(payload).subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Bus created.');
        this.load();
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to create bus.')
    });
  }

  toggleStatus(bus: OperatorBus) {
    const status = bus.status === 'ACTIVE' ? 'OUT_OF_SERVICE' : 'ACTIVE';
    this.api.updateBusStatus(bus.busId, status).subscribe({
      next: () => this.load(),
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to update bus status.')
    });
  }

  deleteBus(busId: string) {
    this.api.deleteBus(busId).subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Bus deleted.');
        this.load();
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to delete bus.')
    });
  }

  private load() {
    this.error.set('');
    this.api.getBuses().subscribe({
      next: (res) => this.buses.set(res.data),
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to load buses.')
    });
    this.api.getRoutes('APPROVED', 1, 100).subscribe({
      next: (res) => this.approvedRoutes.set(res.data.items),
      error: () => this.approvedRoutes.set([])
    });
  }
}
