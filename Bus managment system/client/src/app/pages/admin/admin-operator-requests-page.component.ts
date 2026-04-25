import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { OperatorRequestItem } from '../../core/models';

@Component({
  selector: 'app-admin-operator-requests-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section>
      <h1 class="text-2xl font-bold text-white">Operator requests</h1>
      <p class="mt-1 text-sm text-slate-400">Review user role-upgrade requests.</p>

      @if (message()) { <p class="mt-4 rounded bg-emerald-900/40 px-3 py-2 text-sm text-emerald-200">{{ message() }}</p> }
      @if (error()) { <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p> }

      <div class="mt-6 overflow-hidden rounded-xl border border-slate-800">
        <table class="min-w-full divide-y divide-slate-800 bg-slate-900">
          <thead class="bg-slate-800/80 text-left text-xs uppercase tracking-wide text-slate-400">
            <tr><th class="px-4 py-3">User</th><th class="px-4 py-3">Status</th><th class="px-4 py-3">Created</th><th class="px-4 py-3">Actions</th></tr>
          </thead>
          <tbody class="divide-y divide-slate-800 text-sm text-slate-200">
            @for (item of items(); track item.requestId) {
              <tr>
                <td class="px-4 py-3">{{ item.username }}<div class="text-xs text-slate-400">{{ item.email }}</div></td>
                <td class="px-4 py-3">{{ item.status }}</td>
                <td class="px-4 py-3">{{ item.createdAt | date:'short' }}</td>
                <td class="px-4 py-3 space-x-2">
                  <button (click)="approve(item.requestId)" class="rounded bg-emerald-600 px-2 py-1 text-xs text-white hover:bg-emerald-500">Approve</button>
                  <button (click)="reject(item.requestId)" class="rounded bg-rose-600 px-2 py-1 text-xs text-white hover:bg-rose-500">Reject</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class AdminOperatorRequestsPageComponent implements OnInit {
  readonly items = signal<OperatorRequestItem[]>([]);
  readonly error = signal('');
  readonly message = signal('');

  constructor(private readonly api: ApiService) {}

  ngOnInit() {
    this.load();
  }

  approve(requestId: string) {
    this.api.approveOperatorRequest(requestId).subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Request approved.');
        this.load();
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to approve request.')
    });
  }

  reject(requestId: string) {
    this.api.rejectOperatorRequest(requestId).subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Request rejected.');
        this.load();
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to reject request.')
    });
  }

  private load() {
    this.error.set('');
    this.api.getOperatorRequests('PENDING').subscribe({
      next: (res) => this.items.set(res.data),
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to load operator requests.')
    });
  }
}
