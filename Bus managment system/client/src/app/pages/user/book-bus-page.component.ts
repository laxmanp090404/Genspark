import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject, signal ,effect} from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { Profile } from '../../core/models';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-book-bus-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="mx-auto max-w-5xl">
      <h1 class="text-2xl font-bold text-white">Book Bus</h1>
      <p class="mt-1 text-sm text-slate-400">Select your seats and passenger details.</p>

      @if (loading()) {
        <p class="mt-4 text-cyan-300">Loading schedule...</p>
      }

      @if (error()) {
        <p class="mt-4 rounded bg-rose-900/40 px-3 py-2 text-sm text-rose-200">{{ error() }}</p>
      }

      @if (schedule()) {
        <div class="mt-6 flex flex-col md:flex-row gap-8">
          
          <!-- Seat Layout Selection -->
          @if (step() === 'SEATS') {
            <div class="flex-1 rounded-xl border border-slate-800 bg-slate-900 p-6">
              <h2 class="mb-4 text-lg font-semibold text-white">Select Seats ({{ schedule()?.registrationNumber }})</h2>
              
              <div class="mb-6 flex flex-wrap gap-4 text-xs text-slate-300">
                <div class="flex items-center gap-1.5"><div class="w-4 h-4 bg-slate-800 border border-slate-700 rounded-sm"></div> Available</div>
                @if (!isReadOnly()) {
                  <div class="flex items-center gap-1.5"><div class="w-4 h-4 bg-cyan-600 border border-cyan-500 rounded-sm"></div> Selected</div>
                }
                <div class="flex items-center gap-1.5"><div class="w-4 h-4 bg-rose-900/50 border border-rose-800 rounded-sm"></div> Booked/Frozen</div>
                <div class="flex items-center gap-1.5"><div class="w-4 h-4 border-2 border-blue-500 bg-blue-900/30 rounded-sm"></div> Male</div>
                <div class="flex items-center gap-1.5"><div class="w-4 h-4 border-2 border-pink-500 bg-pink-900/30 rounded-sm"></div> Female</div>
              </div>

              <div class="flex gap-8 justify-center">
                
                <!-- Left Division -->
                <div class="grid grid-cols-3 gap-2">
                  @for (seat of leftSeats; track seat.seatId) {
                    @if (seat.seatNumber !== 0) {
                      <button 
                        [id]="'seat-' + schedule()?.registrationNumber + '-' + seat.seatNumber"
                        (click)="!isReadOnly() && toggleSeat(seat)"
                        [disabled]="(seat.status?.toUpperCase() !== 'AVAILABLE' && !isSelected(seat)) || isReadOnly()"
                        class="w-10 h-10 rounded-t-lg rounded-b-sm border-2 font-medium flex items-center justify-center transition-all relative group"
                        [ngClass]="{
                          'border-slate-700 bg-slate-800 text-slate-400 hover:border-cyan-500 hover:text-cyan-400': seat.status === 'AVAILABLE' && !isSelected(seat),
                          'border-cyan-500 bg-cyan-600 text-white shadow-[0_0_15px_rgba(6,182,212,0.6)]': isSelected(seat),
                          'border-rose-800 bg-rose-900/60 text-rose-200 cursor-not-allowed': (seat.status === 'BOOKED' || seat.status === 'FROZEN') && !seat.passengerGender,
                          'border-pink-500 bg-pink-600 text-white shadow-[0_0_10px_rgba(236,72,153,0.4)]': seat.status === 'BOOKED' && seat.passengerGender === 'FEMALE',
                          'border-blue-500 bg-blue-600 text-white shadow-[0_0_10px_rgba(59,130,246,0.4)]': seat.status === 'BOOKED' && seat.passengerGender === 'MALE'
                        }"
                      >
                        {{ seat.seatNumber }}
                        @if ((seat.status === 'BOOKED' || seat.status === 'FROZEN') && seat.passengerName) {
                          <div class="absolute bottom-full mb-2 hidden w-max flex-col rounded bg-slate-800 px-2 py-1 text-xs text-slate-200 shadow-xl group-hover:flex z-10 border border-slate-700"
                            [ngClass]="{'border-l-4 border-pink-500': seat.passengerGender === 'FEMALE', 'border-l-4 border-blue-500': seat.passengerGender === 'MALE'}"
                          >
                            <span class="font-bold text-white">{{ seat.passengerName }}</span>
                            <span class="text-[10px] text-slate-400">{{ seat.passengerAge }} yrs, {{ seat.passengerGender }}</span>
                          </div>
                        }
                      </button>
                    } @else {
                      <div class="w-10 h-10"></div>
                    }
                  }
                </div>

                <!-- Pathway -->
                <div class="w-8 flex items-center justify-center">
                  <span class="text-[10px] font-bold text-slate-700 rotate-90 tracking-widest uppercase">Pathway</span>
                </div>

                <!-- Right Division -->
                <div class="grid grid-cols-3 gap-2">
                  @for (seat of rightSeats; track seat.seatId) {
                    @if (seat.seatNumber !== 0) {
                      <button 
                        [id]="'seat-' + schedule()?.registrationNumber + '-' + seat.seatNumber"
                        (click)="!isReadOnly() && toggleSeat(seat)"
                        [disabled]="(seat.status?.toUpperCase() !== 'AVAILABLE' && !isSelected(seat)) || isReadOnly()"
                        class="w-10 h-10 rounded-t-lg rounded-b-sm border-2 font-bold flex items-center justify-center transition-all relative group"
                        [ngClass]="{
                          'border-slate-700 bg-slate-800 text-slate-400 hover:border-cyan-500 hover:text-cyan-400': seat.status === 'AVAILABLE' && !isSelected(seat),
                          'border-cyan-500 bg-cyan-600 text-white shadow-[0_0_15px_rgba(6,182,212,0.6)]': isSelected(seat),
                          'border-rose-800 bg-rose-900/60 text-rose-200 cursor-not-allowed': (seat.status === 'BOOKED' || seat.status === 'FROZEN') && !seat.passengerGender,
                          'border-pink-500 bg-pink-600 text-white shadow-[0_0_10px_rgba(236,72,153,0.4)]': seat.status === 'BOOKED' && seat.passengerGender === 'FEMALE',
                          'border-blue-500 bg-blue-600 text-white shadow-[0_0_10px_rgba(59,130,246,0.4)]': seat.status === 'BOOKED' && seat.passengerGender === 'MALE'
                        }"
                      >
                        {{ seat.seatNumber }}
                        @if ((seat.status === 'BOOKED' || seat.status === 'FROZEN') && seat.passengerName) {
                          <div class="absolute bottom-full mb-2 hidden w-max flex-col rounded bg-slate-800 px-2 py-1 text-xs text-slate-200 shadow-xl group-hover:flex z-10 border border-slate-700"
                            [ngClass]="{'border-l-4 border-pink-500': seat.passengerGender === 'FEMALE', 'border-l-4 border-blue-500': seat.passengerGender === 'MALE'}"
                          >
                            <span class="font-bold text-white">{{ seat.passengerName }}</span>
                            <span class="text-[10px] text-slate-400">{{ seat.passengerAge }} yrs, {{ seat.passengerGender }}</span>
                          </div>
                        }
                      </button>
                    } @else {
                      <div class="w-10 h-10"></div>
                    }
                  }
                </div>

              </div>

              @if (!isReadOnly()) {
                <div class="mt-8 flex justify-between items-center border-t border-slate-800 pt-4">
                  <div>
                    <p class="text-sm text-slate-400">Selected: {{ selectedSeats().length }} seats</p>
                    <p class="text-lg font-bold text-white">Total: ₹{{ selectedSeats().length * (schedule()?.seatPrice || 0) }}</p>
                  </div>
                  <button 
                    (click)="freezeSeats()"
                    [disabled]="selectedSeats().length === 0 || freezing()"
                    class="rounded-lg bg-cyan-600 px-6 py-2 font-semibold text-white hover:bg-cyan-500 disabled:opacity-50"
                  >
                    {{ freezing() ? 'Freezing...' : 'Proceed to details' }}
                  </button>
                </div>
              }
            </div>
          }

          <!-- Passenger Details -->
          @if (step() === 'PASSENGERS') {
            <div class="flex-1 rounded-xl border border-slate-800 bg-slate-900 p-6">
              <div class="flex justify-between items-center mb-6">
                <h2 class="text-lg font-semibold text-white">Passenger Details</h2>
                <div class="text-sm font-mono text-cyan-400 flex items-center gap-2">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 animate-pulse" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                  Time left: {{ timeLeftFormatted() }}
                </div>
              </div>
              
              <form [formGroup]="passengerForm" (ngSubmit)="submitPassengers()">
                <div formArrayName="passengers" class="space-y-6">
                  @for (passenger of passengers.controls; track i; let i = $index) {
                    <div [formGroupName]="i" class="rounded-lg border border-slate-800 bg-slate-950 p-4">
                      <h3 class="mb-3 font-medium text-cyan-300">Seat {{ selectedSeats()[i].seatNumber }}</h3>
                      
                      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <label class="block">
                          <span class="mb-1 block text-sm text-slate-400">Name</span>
                          <input formControlName="passengerName" required 
                            class="w-full rounded border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100 outline-none focus:border-cyan-500 transition-colors"
                            [ngClass]="{'border-rose-500/50': passengers.at(i).get('passengerName')?.invalid && passengers.at(i).get('passengerName')?.touched}" />
                        </label>
                        <label class="block">
                          <span class="mb-1 block text-sm text-slate-400">Age</span>
                          <input type="number" formControlName="passengerAge" required min="1" max="120"
                            class="w-full rounded border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100 outline-none focus:border-cyan-500 transition-colors"
                            [ngClass]="{'border-rose-500/50': passengers.at(i).get('passengerAge')?.invalid && passengers.at(i).get('passengerAge')?.touched}" />
                        </label>
                        <label class="block">
                          <span class="mb-1 block text-sm text-slate-400">Gender</span>
                          <select formControlName="passengerGender" required
                            class="w-full rounded border border-slate-700 bg-slate-900 px-3 py-2 text-slate-100 outline-none focus:border-cyan-500 transition-colors">
                            <option value="MALE">MALE</option>
                            <option value="FEMALE">FEMALE</option>
                            <option value="OTHER">OTHER</option>
                          </select>
                        </label>
                      </div>
                    </div>
                  }
                </div>

                <div class="mt-4">
                  @if (passengerForm.invalid) {
                    <p class="text-xs text-rose-400">Please fill in all passenger names and details to proceed.</p>
                  }
                </div>

                <div class="mt-6 flex justify-between">
                  <button type="button" (click)="cancelFreeze()" class="rounded px-4 py-2 text-sm text-slate-400 hover:text-white">Cancel</button>
                  <button type="submit" [disabled]="passengerForm.invalid || capturing()" class="rounded-lg bg-cyan-600 px-6 py-2 font-semibold text-white hover:bg-cyan-500 disabled:opacity-50">
                    {{ capturing() ? 'Saving...' : 'Proceed to Payment' }}
                  </button>
                </div>
              </form>
            </div>
          }

          <!-- Payment -->
          @if (step() === 'PAYMENT') {
            <div class="flex-1 rounded-xl border border-slate-800 bg-slate-900 p-6">
              <h2 class="mb-4 text-lg font-semibold text-white">Payment Details</h2>
              <div class="mb-6 rounded-lg bg-slate-950 p-4 text-sm text-slate-300">
                <p class="flex justify-between py-1"><span>Seats ({{ selectedSeats().length }}):</span> <span>₹{{ selectedSeats().length * (schedule()?.seatPrice || 0) }}</span></p>
                <p class="flex justify-between py-1"><span>Platform Fee:</span> <span>₹{{ pricingInfo()?.platformFeeSnapshot }}</span></p>
                <div class="mt-2 border-t border-slate-800 pt-2 flex justify-between font-bold text-white text-lg">
                  <span>Total Amount:</span> <span>₹{{ pricingInfo()?.totalAmount }}</span>
                </div>
              </div>

              <form [formGroup]="paymentForm" (ngSubmit)="pay()">
                <div class="space-y-4">
                  <label class="block">
                    <span class="mb-1 block text-sm text-slate-400">Payer Name</span>
                    <input formControlName="payerName" class="w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none focus:border-cyan-500" />
                  </label>
                  <label class="block">
                    <span class="mb-1 block text-sm text-slate-400">Payer Email (for notification)</span>
                    <input type="email" formControlName="payerEmail" class="w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none focus:border-cyan-500" />
                  </label>
                  <!-- Mock Card Fields -->
                  <div class="grid grid-cols-2 gap-4">
                    <label class="block col-span-2">
                      <span class="mb-1 block text-sm text-slate-400">Card Number</span>
                      <input type="text" placeholder="0000 0000 0000 0000" class="w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none focus:border-cyan-500" />
                    </label>
                    <label class="block">
                      <span class="mb-1 block text-sm text-slate-400">Expiry</span>
                      <input type="text" placeholder="MM/YY" class="w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none focus:border-cyan-500" />
                    </label>
                    <label class="block">
                      <span class="mb-1 block text-sm text-slate-400">CVV</span>
                      <input type="password" placeholder="123" class="w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100 outline-none focus:border-cyan-500" />
                    </label>
                  </div>
                </div>

                <div class="mt-6">
                  <button type="submit" [disabled]="paymentForm.invalid || paying()" class="w-full rounded-lg bg-emerald-600 px-6 py-3 font-semibold text-white hover:bg-emerald-500 disabled:opacity-50">
                    {{ paying() ? 'Processing...' : 'Pay ₹' + pricingInfo()?.totalAmount }}
                  </button>
                </div>
              </form>
            </div>
          }

          <!-- Success -->
          @if (step() === 'SUCCESS') {
            <div class="flex-1 rounded-xl border border-emerald-900/50 bg-slate-900 p-8 text-center">
              <div class="mx-auto mb-4 flex h-20 w-20 items-center justify-center rounded-full bg-emerald-900/30 text-4xl text-emerald-400 shadow-[0_0_20px_rgba(52,211,153,0.3)]">✓</div>
              <h2 class="text-2xl font-bold text-white">Booking Confirmed!</h2>
              <p class="mt-2 text-slate-400">Your seats have been booked successfully. A confirmation email has been sent to <span class="text-cyan-400">{{ paymentForm.value.payerEmail }}</span>.</p>
              
              <div class="mt-10 flex flex-col sm:flex-row justify-center gap-4">
                <button (click)="downloadTicketAfterBooking()" class="rounded-lg bg-emerald-600 px-8 py-3 font-semibold text-white hover:bg-emerald-500 flex items-center justify-center gap-2">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" /></svg>
                  Download Ticket
                </button>
                <a routerLink="/my-bookings" class="rounded-lg bg-slate-800 border border-slate-700 px-8 py-3 font-semibold text-white hover:bg-slate-700">View My Bookings</a>
              </div>
            </div>
          }
          
          <!-- Summary Sidebar -->
          @if (step() !== 'SUCCESS' && !isReadOnly()) {
            <div class="w-full md:w-80">
              <div class="rounded-xl border border-slate-800 bg-slate-900 p-6 sticky top-6">
                <h3 class="font-semibold text-white">Journey Details</h3>
                <p class="mt-4 text-cyan-300">{{ schedule()?.source }} → {{ schedule()?.destination }}</p>
                <p class="mt-2 text-sm text-slate-300">{{ schedule()?.travelDate }}</p>
                <p class="text-sm text-slate-400">{{ schedule()?.departureTime }} - {{ schedule()?.arrivalTime }}</p>
                
                <hr class="my-4 border-slate-800" />
                
                <h4 class="text-sm font-medium text-slate-300">Selected Seats</h4>
                <div class="mt-2 flex flex-wrap gap-2">
                  @if (selectedSeats().length === 0) {
                    <span class="text-sm text-slate-500">None</span>
                  }
                  @for (seat of selectedSeats(); track seat.seatId) {
                    <span class="rounded bg-slate-800 px-2 py-1 text-xs text-slate-300">{{ seat.seatNumber }}</span>
                  }
                </div>
              </div>
            </div>
          }

        </div>
      }
    </section>
  `
})
export class BookBusPageComponent implements OnInit, OnDestroy {

  readonly loading = signal(true);
  readonly error = signal('');
  readonly scheduleId = signal<string | null>(null);
  readonly schedule = signal<any>(null);
  readonly isReadOnly = signal(false);

  readonly step = signal<'SEATS' | 'PASSENGERS' | 'PAYMENT' | 'SUCCESS'>('SEATS');
  readonly selectedSeats = signal<any[]>([]);

  readonly freezing = signal(false);
  readonly capturing = signal(false);
  readonly paying = signal(false);

  readonly bookingId = signal<string>('');
  readonly pricingInfo = signal<any>(null);

  private hubConnection?: signalR.HubConnection;

  private timerInterval: any;
  readonly timeLeft = signal<number>(0);

  private readonly fb = inject(FormBuilder);

  readonly passengerForm = this.fb.group({
    passengers: this.fb.array([])
  });

  readonly paymentForm = this.fb.nonNullable.group({
    payerName: ['', Validators.required],
    payerEmail: ['', [Validators.required, Validators.email]]
  });

  get passengers() {
    return this.passengerForm.get('passengers') as FormArray;
  }

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly api: ApiService,
    public readonly auth: AuthService
  ) {
    effect(() => {
      const p = this.auth.profile();
      if (p) {
        this.paymentForm.patchValue({
          payerName: p.username,
          payerEmail: p.email
        });
      }
    });
  }

  // ✅ FIX 1: Proper ngOnInit (your code was broken here)
  ngOnInit() {
    this.isReadOnly.set(
      this.auth.role() === 'ADMIN' || this.auth.role() === 'BUS_OPERATOR'
    );

    this.route.paramMap.subscribe(params => {
      const id = params.get('scheduleId');

      if (!id) {
        this.error.set('No schedule ID provided.');
        this.loading.set(false);
        return;
      }

      console.log('Route scheduleId:', id);

      this.scheduleId.set(id);
      this.loadSchedule();
    });
  }

  ngOnDestroy() {
    this.stopTimer();
    this.hubConnection?.stop();

    if (this.bookingId() && this.step() !== 'SUCCESS') {
      this.api.releaseSeats(this.bookingId()).subscribe();
    }
  }

  // ✅ FIX 2: SignalR safe init
  private initSignalR() {
    if (!this.scheduleId()) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiBaseUrl}/hubs/seats`)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => {
        console.log('SignalR Connected');

        if (this.scheduleId()) {
          this.hubConnection?.invoke('JoinScheduleGroup', this.scheduleId());
        }
      })
      .catch(err => console.error('SignalR Error:', err));

    this.hubConnection.on('SeatStatusChanged', () => {
      if (!this.scheduleId()) return;

      console.log('Seats updated elsewhere → refreshing...');
      this.loadSchedule();
    });

    this.hubConnection.onclose(() => {
      console.warn('SignalR disconnected');
    });
  }

  // ✅ FIX 3: Guard scheduleId everywhere
  loadSchedule() {
    const id = this.scheduleId();

    if (!id) {
      console.error('scheduleId is NULL ❌');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    console.log('Loading schedule:', id);

    this.api.getScheduleSeats(id).subscribe({
      next: (res) => {
        this.schedule.set(res.data);
        this.loading.set(false);

        if (!this.isReadOnly()) {

          const frozenSeats = res.data.seats.filter((s: any) => s.isFrozenByCurrentUser);

          if (frozenSeats.length > 0) {
            this.selectedSeats.set(frozenSeats);
            this.resumeBooking();
          }

          // init signalR only once
          if (!this.hubConnection) {
            this.initSignalR();
          }
        }
      },
      error: (err) => {
        this.error.set(err?.error?.errors?.[0] ?? 'Failed to load seats.');
        this.loading.set(false);
      }
    });
  }

  resumeBooking() {
    const id = this.scheduleId();
    if (!id) return;

    this.api.getMyBookings().subscribe({
      next: (res) => {
        const targetId = id.toLowerCase().trim();

        const pending = res.data.find((b: any) =>
          b.scheduleId?.toLowerCase().trim() === targetId &&
          (b.status === 'PENDING' || b.status === 'FROZEN')
        );

        if (pending) {
          this.bookingId.set(pending.bookingId);
          this.setupPassengerForm();
          this.step.set('PASSENGERS');
          this.startTimer(300);
        }
      }
    });
  }

  get leftSeats() {
    return this.schedule()?.seats?.slice(0, 20) || [];
  }

  get rightSeats() {
    return this.schedule()?.seats?.slice(20, 40) || [];
  }

  isSelected(seat: any) {
    return this.selectedSeats().some(s => s.seatId === seat.seatId);
  }

  toggleSeat(seat: any) {
    if (seat.status !== 'AVAILABLE') {
      // Show a user-friendly message about the frozen seat
      const statusMsg = seat.status === 'FROZEN' ? 'already frozen by another user' : seat.status.toLowerCase();
      this.error.set(`Seat ${seat.seatNumber} is ${statusMsg}. Please select a different seat.`);
      return;
    }

    const current = this.selectedSeats();
    const exists = current.findIndex(s => s.seatId === seat.seatId);

    if (exists >= 0) {
      this.selectedSeats.set(current.filter((_, i) => i !== exists));
    } else {
      this.selectedSeats.set([...current, seat]);
    }
  }

  freezeSeats() {
    if (!this.scheduleId() || this.selectedSeats().length === 0) return;

    this.freezing.set(true);

    const seatIds = this.selectedSeats().map(s => s.seatId);

    this.api.freezeSeats(this.scheduleId()!, seatIds).subscribe({
      next: (res) => {
        this.bookingId.set(res.data.bookingId);
        this.setupPassengerForm();
        this.step.set('PASSENGERS');
        this.startTimer(300);
        this.freezing.set(false);
      },
      error: (err) => {
        const errorMsg = err?.error?.errors?.[0] ?? 'Failed to freeze seats.';
        if (err.status === 409) {
          alert('Seat(s) already frozen or booked by another user. Please select different seats.');
          this.selectedSeats.set([]); // Clear selection as it's no longer valid
        }
        this.error.set(errorMsg);
        this.freezing.set(false);
        this.loadSchedule();
      }
    });
  }

  cancelFreeze() {
    if (!this.bookingId()) return;

    this.api.releaseSeats(this.bookingId()).subscribe(() => {
      this.bookingId.set('');
      this.step.set('SEATS');
      this.selectedSeats.set([]);
      this.stopTimer();
      this.loadSchedule();
    });
  }

  private setupPassengerForm() {
    this.passengers.clear();
    const profile = this.auth.profile();

    this.selectedSeats().forEach((seat, index) => {
      this.passengers.push(this.fb.group({
        seatId: [seat.seatId],
        passengerName: [index === 0 ? (profile?.username || '') : '', Validators.required],
        passengerAge: [18, [Validators.required, Validators.min(1), Validators.max(120)]],
        passengerGender: ['MALE', Validators.required],
        isPrimary: [index === 0]
      }));
    });
  }

  submitPassengers() {
    if (this.passengerForm.invalid) return;

    this.capturing.set(true);

    this.api.captureBookingDetails(this.bookingId(), this.passengers.value).subscribe({
      next: (res) => {
        this.pricingInfo.set(res.data);
        
        // Pre-fill payment form with profile info
        const profile = this.auth.profile();
        if (profile) {
          this.paymentForm.patchValue({
            payerName: profile.username,
            payerEmail: profile.email
          });
        }

        this.step.set('PAYMENT');
        this.capturing.set(false);
      },
      error: () => {
        this.capturing.set(false);
      }
    });
  }

  pay() {
    if (this.paymentForm.invalid) return;

    this.paying.set(true);

    const { payerName, payerEmail } = this.paymentForm.getRawValue();

    this.api.processPayment(this.bookingId(), payerName, payerEmail).subscribe({
      next: () => {
        this.stopTimer();
        this.step.set('SUCCESS');
        this.paying.set(false);
      },
      error: () => {
        this.paying.set(false);
      }
    });
  }

  private startTimer(seconds: number) {
    this.stopTimer();

    this.timeLeft.set(seconds);

    this.timerInterval = setInterval(() => {
      const current = this.timeLeft();

      if (current > 0) {
        this.timeLeft.set(current - 1);
      } else {
        this.stopTimer();
        this.cancelFreeze();
      }
    }, 1000);
  }

  private stopTimer() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  timeLeftFormatted() {
    const m = Math.floor(this.timeLeft() / 60);
    const s = this.timeLeft() % 60;
    return `${m}:${s < 10 ? '0' : ''}${s}`;
  }

  downloadTicketAfterBooking() {
    const doc = new jsPDF();
    const s = this.schedule();
    const p = this.pricingInfo();

    doc.setFontSize(22);
    doc.setTextColor(0, 150, 200);
    doc.text('E-Ticket', 14, 22);

    doc.setFontSize(10);
    doc.setTextColor(100, 100, 100);
    doc.text(`Booking ID: ${this.bookingId()}`, 14, 30);
    doc.text(`Date Issued: ${new Date().toLocaleString()}`, 14, 36);

    doc.setFontSize(14);
    doc.setTextColor(0, 0, 0);
    doc.text('Journey Details', 14, 48);

    autoTable(doc, {
      startY: 52,
      body: [
        ['Route', `${s.source} to ${s.destination}`],
        ['Travel Date', s.travelDate],
        ['Time', `${s.departureTime} - ${s.arrivalTime}`],
        ['Bus Registration', s.registrationNumber],
        ['Total Paid', `INR ${p.totalAmount}`]
      ],
      theme: 'striped',
      styles: { fontSize: 10 }
    });

    doc.text('Passenger Details', 14, (doc as any).lastAutoTable.finalY + 15);

    autoTable(doc, {
      startY: (doc as any).lastAutoTable.finalY + 20,
      head: [['Seat', 'Name', 'Age', 'Gender']],
      body: this.passengers.value.map((p: any) => [
        this.selectedSeats().find(s => s.seatId === p.seatId)?.seatNumber || 'N/A',
        p.passengerName,
        p.passengerAge,
        p.passengerGender
      ]),
      headStyles: { fillColor: [0, 150, 200] }
    });

    doc.save(`Ticket_${this.bookingId()}.pdf`);
  }
}
