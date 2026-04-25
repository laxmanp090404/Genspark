import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-admin-config-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section>
      <h1 class="text-2xl font-bold text-white">Platform configuration</h1>
      <p class="mt-1 text-sm text-slate-400">Manage global booking fee.</p>

      @if (message()) { <p class="mt-4 rounded bg-emerald-900/40 px-3 py-2 text-sm text-emerald-200">{{ message() }}</p> }
      @if (error()) { <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p> }

      <div class="mt-6 max-w-lg rounded-xl border border-slate-800 bg-slate-900 p-4">
        <p class="text-sm text-slate-400">Current Platform Fee</p>
        <p class="mt-1 text-3xl font-bold text-cyan-300">₹{{ currentFee() }}</p>

        <form [formGroup]="form" (ngSubmit)="save()" class="mt-4 space-y-3">
          <label class="block">
            <span class="mb-1 block text-sm text-slate-300">New platform fee</span>
            <input type="number" formControlName="platformFee" class="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100" />
          </label>
          <button [disabled]="form.invalid" class="rounded-lg bg-cyan-600 px-4 py-2 font-semibold text-white hover:bg-cyan-500">Update fee</button>
        </form>
      </div>
    </section>
  `
})
export class AdminConfigPageComponent implements OnInit {
  readonly currentFee = signal(0);
  readonly message = signal('');
  readonly error = signal('');
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    platformFee: [0, [Validators.required, Validators.min(0)]]
  });

  constructor(
    private readonly api: ApiService
  ) {}

  ngOnInit() {
    this.load();
  }

  save() {
    if (this.form.invalid) return;
    const { platformFee } = this.form.getRawValue();
    this.api.updatePlatformFee(platformFee).subscribe({
      next: (res) => {
        this.message.set(res.message ?? 'Platform fee updated.');
        this.load();
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to update platform fee.')
    });
  }

  private load() {
    this.error.set('');
    this.api.getPlatformConfig().subscribe({
      next: (res) => {
        this.currentFee.set(res.data.platformFee);
        this.form.patchValue({ platformFee: res.data.platformFee });
      },
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to load platform config.')
    });
  }
}
