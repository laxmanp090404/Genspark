import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { RouteItem } from '../../core/models';

@Component({
  selector: 'app-operator-routes-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section>
      <h1 class="text-2xl font-bold text-white">Route management</h1>
      <p class="mt-1 text-sm text-slate-400">Create and monitor your routes.</p>

      <form [formGroup]="form" (ngSubmit)="createRoute()" class="mt-6 grid grid-cols-1 gap-3 rounded-xl border border-slate-800 bg-slate-900 p-4 md:grid-cols-3">
        <input formControlName="source" placeholder="Source" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />
        <input formControlName="destination" placeholder="Destination" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />
        <button [disabled]="form.invalid" class="rounded-lg bg-cyan-600 px-4 py-2 font-semibold text-white hover:bg-cyan-500">Add route</button>
      </form>

      @if (message()) { 
        <div class="mt-4 flex items-center justify-between rounded bg-emerald-900/40 px-3 py-2 text-sm text-emerald-200">
          <span>{{ message() }}</span>
          <button type="button" (click)="message.set('')" class="text-emerald-400 hover:text-emerald-300">✕</button>
        </div> 
      }
      @if (error()) { 
        <div class="mt-4 flex items-center justify-between rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">
          <span>{{ error() }}</span>
          <button type="button" (click)="error.set('')" class="text-rose-400 hover:text-rose-300">✕</button>
        </div> 
      }

      <div class="mt-6 overflow-hidden rounded-xl border border-slate-800">
        <table class="min-w-full divide-y divide-slate-800 bg-slate-900">
          <thead class="bg-slate-800/80 text-left text-xs uppercase tracking-wide text-slate-400">
            <tr><th class="px-4 py-3">Route</th><th class="px-4 py-3">Status</th><th class="px-4 py-3">Created</th><th class="px-4 py-3">Actions</th></tr>
          </thead>
          <tbody class="divide-y divide-slate-800 text-sm text-slate-200">
            @for (route of routes(); track route.routeId) {
              <tr>
                <td class="px-4 py-3">{{ route.source }} → {{ route.destination }}</td>
                <td class="px-4 py-3">
                  <div>{{ route.status }}</div>
                  @if (route.status === 'REJECTED' && route.rejectionReason) {
                    <div class="mt-1 text-xs text-rose-300">Rejection reason available</div>
                  }
                </td>
                <td class="px-4 py-3">{{ route.createdAt | date:'short' }}</td>
                <td class="px-4 py-3 flex gap-2">
                  @if (route.status === 'REJECTED') {
                    <button type="button" (click)="viewReason(route)" class="rounded bg-slate-700 px-2 py-1 text-xs text-white hover:bg-slate-600">View reject reason</button>
                  }
                  <button type="button" (click)="deleteRoute(route.routeId)" class="rounded bg-rose-600 px-2 py-1 text-xs text-white hover:bg-rose-500">Request delete</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class OperatorRoutesPageComponent implements OnInit {
  readonly routes = signal<RouteItem[]>([]);
  readonly error = signal('');
  readonly message = signal('');
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    source: ['', [Validators.required]],
    destination: ['', [Validators.required]]
  });

  constructor(
    private readonly api: ApiService
  ) {}

  ngOnInit() {
    this.load();
  }

  createRoute() {
    if (this.form.invalid) return;
    const payload = this.form.getRawValue();
    this.api.createRoute(payload).subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Route created.');
        this.form.reset({ source: '', destination: '' });
        this.load();
      },
      error: (err) => {
        let errMsg = err?.error?.errors?.[0] ?? 'Failed to create route.';
        if (errMsg.toLowerCase().includes('already exists')) {
          errMsg = errMsg + ' If it is rejected, please delete the rejected route to add it.';
        }
        this.error.set(errMsg);
      }
    });
  }

  deleteRoute(routeId: string) {
    this.api.deleteRoute(routeId).subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Deletion request submitted.');
        this.load();
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to delete route.')
    });
  }

  viewReason(route: RouteItem) {
    this.message.set(route.rejectionReason ?? 'No rejection reason was provided.');
  }

  private load() {
    this.error.set('');
    this.api.getRoutes(undefined, 1, 50).subscribe({
      next: (res) => this.routes.set(res.data.items),
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to load routes.')
    });
  }
}
