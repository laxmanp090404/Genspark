import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { BookingSummary } from '../../core/models';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-my-bookings-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section>
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-white">My bookings</h1>
          <p class="mt-1 text-sm text-slate-400">Track your trips and refunds.</p>
        </div>
      </div>

      <div class="mt-6 flex gap-2 border-b border-slate-800">
        @for (tab of ['Upcoming', 'Completed', 'Cancelled']; track tab) {
          <button 
            (click)="activeTab.set(tab)"
            class="px-4 py-2 text-sm font-medium transition-colors border-b-2"
            [ngClass]="activeTab() === tab ? 'text-cyan-400 border-cyan-400' : 'text-slate-400 border-transparent hover:text-slate-200'"
          >
            {{ tab }}
          </button>
        }
      </div>

      @if (error()) {
        <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p>
      }

      <div class="mt-6 grid gap-4 md:grid-cols-2">
        @for (item of filteredBookings(); track item.bookingId) {
          <article class="rounded-xl border border-slate-800 bg-slate-900 p-4 transition-all hover:border-slate-700">
            <div class="flex items-start justify-between">
              <div>
                <h2 class="font-semibold text-cyan-300">{{ item.source }} → {{ item.destination }}</h2>
                <p class="text-xs text-slate-500 mt-0.5">Booking ID: {{ item.bookingId.substring(0, 8) }}</p>
              </div>
              <span class="rounded px-2 py-0.5 text-xs" 
                [ngClass]="{
                  'bg-emerald-900/50 text-emerald-300': item.status === 'CONFIRMED',
                  'bg-rose-900/50 text-rose-300': item.status.includes('CANCELLED'),
                  'bg-slate-800 text-slate-400': item.status === 'PENDING'
                }">
                {{ item.status }}
              </span>
            </div>
            
            <div class="mt-4 grid grid-cols-2 gap-4 text-sm">
              <div>
                <p class="text-slate-500 text-xs uppercase font-semibold">Travel Date</p>
                <p class="text-slate-200">{{ item.travelDate }}</p>
              </div>
              <div>
                <p class="text-slate-500 text-xs uppercase font-semibold">Seats</p>
                <p class="text-slate-200">{{ item.seatNumbers.join(', ') }}</p>
              </div>
              <div>
                <p class="text-slate-500 text-xs uppercase font-semibold">Total Amount</p>
                <p class="text-slate-200 font-bold text-white">₹{{ item.totalAmount }}</p>
              </div>
              @if (item.refundAmount !== null) {
                <div>
                  <p class="text-slate-500 text-xs uppercase font-semibold">Refund ({{ item.refundStatus }})</p>
                  <p class="text-amber-400 font-bold">₹{{ item.refundAmount }}</p>
                </div>
              }
            </div>

            <div class="mt-6 flex flex-wrap gap-3">
              @if (item.status === 'CONFIRMED') {
                <button (click)="downloadTicket(item)" class="flex-1 rounded-lg bg-slate-800 border border-slate-700 px-4 py-2 text-xs font-semibold text-white hover:bg-slate-750">
                  Download Ticket
                </button>
                @if (canCancel(item)) {
                  <button (click)="cancelTrip(item)" class="flex-1 rounded-lg bg-rose-900/40 border border-rose-800 px-4 py-2 text-xs font-semibold text-rose-300 hover:bg-rose-900/60">
                    Cancel Trip
                  </button>
                }
              }
            </div>
          </article>
        } @empty {
          <div class="col-span-full py-12 text-center">
            <p class="text-slate-500">No {{ activeTab().toLowerCase() }} bookings found.</p>
          </div>
        }
      </div>
    </section>
  `
})
export class MyBookingsPageComponent implements OnInit {
  readonly bookings = signal<BookingSummary[]>([]);
  readonly activeTab = signal('Upcoming');
  readonly error = signal('');

  constructor(private readonly api: ApiService) {}

  ngOnInit() {
    this.loadBookings();
  }

  loadBookings() {
    this.api.getMyBookings().subscribe({
      next: (res) => this.bookings.set(res.data),
      error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to load bookings.')
    });
  }

  filteredBookings() {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return this.bookings().filter(b => {
      const travelDate = new Date(b.travelDate);
      
      if (this.activeTab() === 'Upcoming') {
        return travelDate >= today && (b.status === 'CONFIRMED' || b.status === 'PENDING');
      } else if (this.activeTab() === 'Completed') {
        return travelDate < today && b.status === 'CONFIRMED';
      } else {
        return b.status.includes('CANCELLED');
      }
    });
  }

  canCancel(booking: BookingSummary): boolean {
    const travelDate = new Date(booking.travelDate);
    const now = new Date();
    return travelDate > now;
  }

  cancelTrip(booking: BookingSummary) {
    const refundInfo = this.calculateRefundEstimate(booking);
    if (confirm(`Are you sure you want to cancel? Estimated refund (${refundInfo.status}): ₹${refundInfo.amount}`)) {
      this.api.cancelBooking(booking.bookingId).subscribe({
        next: () => {
          this.loadBookings();
        },
        error: (err) => this.error.set(err?.error?.errors?.[0] ?? 'Failed to cancel booking.')
      });
    }
  }

  calculateRefundEstimate(booking: BookingSummary) {
    // This is just a UI estimate. The server does the actual calculation.
    const travelDate = new Date(booking.travelDate);
    const now = new Date();
    const hours = (travelDate.getTime() - now.getTime()) / (1000 * 60 * 60);

    if (hours > 48) return { status: 'Full', amount: booking.totalAmount };
    if (hours > 24) return { status: '50%', amount: booking.totalAmount * 0.5 };
    return { status: 'No', amount: 0 };
  }

  downloadTicket(booking: BookingSummary) {
    const doc = new jsPDF();

    doc.setFontSize(20);
    doc.setTextColor(0, 150, 200);
    doc.text('Bus Ticket', 14, 22);

    doc.setFontSize(12);
    doc.setTextColor(50, 50, 50);
    doc.text(`Booking ID: ${booking.bookingId}`, 14, 32);
    doc.text(`Status: ${booking.status}`, 14, 40);
    doc.text(`Route: ${booking.source} to ${booking.destination}`, 14, 48);
    doc.text(`Travel Date: ${booking.travelDate}`, 14, 56);

    autoTable(doc, {
      startY: 64,
      head: [['Detail', 'Value']],
      body: [
        ['Seats', booking.seatNumbers.join(', ')],
        ['Total Amount', `INR ${booking.totalAmount}`]
      ],
      theme: 'grid',
      headStyles: { fillColor: [0, 150, 200] }
    });

    doc.save(`Ticket_${booking.bookingId}.pdf`);
  }
}
