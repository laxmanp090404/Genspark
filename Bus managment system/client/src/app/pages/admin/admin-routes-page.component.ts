import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { AdminRouteItem } from '../../core/models';

@Component({
  selector: 'app-admin-routes-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section>
      <h1 class="text-2xl font-bold text-white">Route approvals</h1>
      <p class="mt-1 text-sm text-slate-400">Approve, reject, or add routes.</p>

      <form [formGroup]="form" (ngSubmit)="createRoute()" class="mt-6 grid grid-cols-1 gap-3 rounded-xl border border-slate-800 bg-slate-900 p-4 md:grid-cols-3">
        <input formControlName="source" placeholder="Source" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />
        <input formControlName="destination" placeholder="Destination" class="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />
        <button [disabled]="form.invalid" class="rounded-lg bg-cyan-600 px-4 py-2 font-semibold text-white hover:bg-cyan-500">Add route</button>
      </form>

      @if (message()) { <p class="mt-4 rounded bg-emerald-900/40 px-3 py-2 text-sm text-emerald-200">{{ message() }}</p> }
      @if (error()) { <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p> }

      <div class="mt-6 overflow-hidden rounded-xl border border-slate-800">
        <table class="min-w-full divide-y divide-slate-800 bg-slate-900">
          <thead class="bg-slate-800/80 text-left text-xs uppercase tracking-wide text-slate-400">
            <tr><th class="px-4 py-3">Operator</th><th class="px-4 py-3">Route</th><th class="px-4 py-3">Status</th><th class="px-4 py-3">Submitted</th><th class="px-4 py-3">Actions</th></tr>
          </thead>
          <tbody class="divide-y divide-slate-800 text-sm text-slate-200">
            @for (item of items(); track item.routeId) {
              <tr>
                <td class="px-4 py-3">{{ item.operatorName }}<div class="text-xs text-slate-400">{{ item.operatorEmail }}</div></td>
                <td class="px-4 py-3">{{ item.source }} → {{ item.destination }}</td>
                <td class="px-4 py-3">
                  <div>{{ item.status }}</div>
                  @if (item.status === 'REJECTED' && item.rejectionReason) {
                    <div class="text-xs text-rose-300">Reason recorded</div>
                  }
                </td>
                <td class="px-4 py-3">{{ item.createdAt | date:'short' }}</td>
                <td class="px-4 py-3 space-x-2">
                  @if (item.status === 'PENDING_APPROVAL' || item.status === 'PENDING_DELETION') {
                    <button type="button" (click)="approve(item.routeId)" class="rounded bg-emerald-600 px-2 py-1 text-xs text-white hover:bg-emerald-500">Approve</button>
                    <button type="button" (click)="reject(item.routeId)" class="rounded bg-rose-600 px-2 py-1 text-xs text-white hover:bg-rose-500">Reject</button>
                  } @else {
                    <span class="text-xs text-slate-500">No actions</span>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class AdminRoutesPageComponent implements OnInit {
  readonly items = signal<AdminRouteItem[]>([]);
  readonly error = signal('');
  readonly message = signal('');
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    source: ['', [Validators.required]],
    destination: ['', [Validators.required]]
  });

  constructor(private readonly api: ApiService) {}

  ngOnInit() {
    this.load();
  }

  createRoute() {
    if (this.form.invalid) return;

    this.api.createAdminRoute(this.form.getRawValue()).subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Route added.');
        this.form.reset({ source: '', destination: '' });
        this.load();
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to add route.')
    });
  }

  approve(routeId: string) {
    this.api.approveRoute(routeId).subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Route approved.');
        this.load();
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to approve route.')
    });
  }

  reject(routeId: string) {
    const reason = window.prompt('Enter rejection reason (optional)') ?? '';
    this.api.rejectRoute(routeId, reason).subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Route rejected.');
        this.load();
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to reject route.')
    });
  }

  private load() {
    this.error.set('');
    this.api.getAdminRoutes('', 1, 100).subscribe({
      next: (res) => this.items.set(res.data.items),
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to load routes.')
    });
  }
}
