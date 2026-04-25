import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { OperatorRequestItem } from '../../core/models';

@Component({
  selector: 'app-operator-request-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section>
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold text-white">Become an operator</h1>
          <p class="mt-1 text-sm text-slate-400">Submit a role switch request for admin approval.</p>
        </div>
        <button (click)="createRequest()" class="rounded-lg bg-cyan-600 px-4 py-2 font-semibold text-white hover:bg-cyan-500">Request now</button>
      </div>

      @if (message()) {
        <p class="mt-4 rounded bg-emerald-900/40 px-3 py-2 text-sm text-emerald-200">{{ message() }}</p>
      }
      @if (error()) {
        <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p>
      }

      <div class="mt-6 overflow-hidden rounded-xl border border-slate-800">
        <table class="min-w-full divide-y divide-slate-800 bg-slate-900">
          <thead class="bg-slate-800/80 text-left text-xs uppercase tracking-wide text-slate-400">
            <tr><th class="px-4 py-3">Request ID</th><th class="px-4 py-3">Status</th><th class="px-4 py-3">Created</th></tr>
          </thead>
          <tbody class="divide-y divide-slate-800 text-sm text-slate-200">
            @for (item of requests(); track item.requestId) {
              <tr><td class="px-4 py-3">{{ item.requestId }}</td><td class="px-4 py-3">{{ item.status }}</td><td class="px-4 py-3">{{ item.createdAt | date:'medium' }}</td></tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `
})
export class OperatorRequestPageComponent implements OnInit {
  readonly requests = signal<OperatorRequestItem[]>([]);
  readonly message = signal('');
  readonly error = signal('');

  constructor(private readonly api: ApiService) {}

  ngOnInit() {
    this.load();
  }

  createRequest() {
    this.error.set('');
    this.message.set('');
    this.api.createOperatorRequest().subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Request submitted.');
        this.load();
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to submit request.')
    });
  }

  private load() {
    this.api.getMyOperatorRequests().subscribe({
      next: (res) => this.requests.set(res.data),
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to load requests.')
    });
  }
}
