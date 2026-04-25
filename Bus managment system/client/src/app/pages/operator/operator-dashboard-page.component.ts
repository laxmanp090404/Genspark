import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { OperatorSummary } from '../../core/models';

@Component({
  selector: 'app-operator-dashboard-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section>
      <h1 class="text-2xl font-bold text-white">Operator dashboard</h1>
      <p class="mt-1 text-sm text-slate-400">Overview of routes and buses.</p>

      @if (error()) {
        <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p>
      }

      <div class="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        @for (card of cards(); track card.label) {
          <article class="rounded-xl border border-slate-800 bg-slate-900 p-4">
            <p class="text-sm text-slate-400">{{ card.label }}</p>
            <p class="mt-1 text-3xl font-bold text-cyan-300">{{ card.value }}</p>
          </article>
        }
      </div>
    </section>
  `
})
export class OperatorDashboardPageComponent implements OnInit {
  readonly summary = signal<OperatorSummary | null>(null);
  readonly error = signal('');

  readonly cards = signal<{ label: string; value: number }[]>([]);

  constructor(private readonly api: ApiService) {}

  ngOnInit() {
    this.api.getOperatorSummary().subscribe({
      next: (res) => {
        this.summary.set(res.data);
        this.cards.set([
          { label: 'Total Routes', value: res.data.totalRoutes },
          { label: 'Pending Routes', value: res.data.pendingRoutes },
          { label: 'Approved Routes', value: res.data.approvedRoutes },
          { label: 'Rejected Routes', value: res.data.rejectedRoutes },
          { label: 'Total Buses', value: res.data.totalBuses },
          { label: 'Active Buses', value: res.data.activeBuses }
        ]);
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to load summary.')
    });
  }
}
