import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AdminRouteItem,
  AdminSummary,
  ApiResponse,
  BookingSummary,
  BusSearchResult,
  OperatorBus,
  OperatorRequestItem,
  OperatorSummary,
  PagedResult,
  PlatformConfig,
  RouteItem
} from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  searchBuses(source: string, destination: string, date: string) {
    return this.http.get<ApiResponse<BusSearchResult[]>>(`${this.base}/buses/search`, {
      params: { source, destination, date }
    });
  }

  getPlaces(query?: string, limit = 100) {
    return this.http.get<ApiResponse<string[]>>(`${this.base}/places`, {
      params: { q: query ?? '', limit }
    });
  }

  getMyBookings() {
    return this.http.get<ApiResponse<BookingSummary[]>>(`${this.base}/bookings/my`);
  }

  createOperatorRequest() {
    return this.http.post<ApiResponse<OperatorRequestItem>>(`${this.base}/operator-requests`, {});
  }

  getMyOperatorRequests() {
    return this.http.get<ApiResponse<OperatorRequestItem[]>>(`${this.base}/operator-requests/my`);
  }

  getOperatorSummary() {
    return this.http.get<ApiResponse<OperatorSummary>>(`${this.base}/operator/summary`);
  }

  getRoutes(status?: string, page = 1, pageSize = 10) {
    return this.http.get<ApiResponse<PagedResult<RouteItem>>>(`${this.base}/routes`, {
      params: { status: status ?? '', page, pageSize }
    });
  }

  createRoute(payload: { source: string; destination: string }) {
    return this.http.post<ApiResponse<RouteItem>>(`${this.base}/routes`, payload);
  }

  createAdminRoute(payload: { source: string; destination: string }) {
    return this.http.post<ApiResponse<RouteItem>>(`${this.base}/admin/routes`, payload);
  }

  deleteRoute(routeId: string) {
    return this.http.delete<ApiResponse<null>>(`${this.base}/routes/${routeId}`);
  }

  getBuses(status?: string) {
    return this.http.get<ApiResponse<OperatorBus[]>>(`${this.base}/buses`, {
      params: { status: status ?? '' }
    });
  }

  getAllBuses() {
  return this.http.get<ApiResponse<any[]>>(`${this.base}/admin/buses`);
}

  createBus(payload: {
    routeId: string;
    registrationNumber: string;
    departureTime: string;
    arrivalTime: string;
    seatPrice: number;
  }): Observable<ApiResponse<{ busId: string }>> {
    return this.http.post<ApiResponse<{ busId: string }>>(`${this.base}/buses`, payload);
  }

  updateBusStatus(busId: string, status: string) {
    return this.http.patch<ApiResponse<null>>(`${this.base}/buses/${busId}/status`, { status });
  }

  deleteBus(busId: string) {
    return this.http.delete<ApiResponse<null>>(`${this.base}/buses/${busId}`);
  }

  getAdminSummary() {
    return this.http.get<ApiResponse<AdminSummary>>(`${this.base}/admin/summary`);
  }

  getAllBookings() {
    return this.http.get<ApiResponse<any[]>>(`${this.base}/admin/bookings`);
  }

  getAdminRoutes(status = 'PENDING_APPROVAL', page = 1, pageSize = 20) {
    return this.http.get<ApiResponse<PagedResult<AdminRouteItem>>>(`${this.base}/admin/routes`, {
      params: { status, page, pageSize }
    });
  }

  approveRoute(routeId: string) {
    return this.http.patch<ApiResponse<RouteItem>>(`${this.base}/admin/routes/${routeId}/approve`, {});
  }

  rejectRoute(routeId: string, reason?: string) {
    return this.http.patch<ApiResponse<RouteItem>>(`${this.base}/admin/routes/${routeId}/reject`, { reason: reason ?? null });
  }

  getOperatorRequests(status = 'PENDING') {
    return this.http.get<ApiResponse<OperatorRequestItem[]>>(`${this.base}/admin/operator-requests`, {
      params: { status }
    });
  }

  approveOperatorRequest(requestId: string) {
    return this.http.patch<ApiResponse<null>>(`${this.base}/admin/operator-requests/${requestId}/approve`, {});
  }

  rejectOperatorRequest(requestId: string) {
    return this.http.patch<ApiResponse<null>>(`${this.base}/admin/operator-requests/${requestId}/reject`, {});
  }

  getPlatformConfig() {
    return this.http.get<ApiResponse<PlatformConfig>>(`${this.base}/admin/config`);
  }

  updatePlatformFee(platformFee: number) {
    return this.http.put<ApiResponse<unknown>>(`${this.base}/admin/config`, { platformFee });
  }

  getScheduleSeats(scheduleId: string) {
    return this.http.get<ApiResponse<any>>(`${this.base}/schedules/${scheduleId}/seats`);
  }

  freezeSeats(scheduleId: string, seatIds: string[]) {
    return this.http.post<ApiResponse<any>>(`${this.base}/seats/freeze`, { scheduleId, seatIds });
  }

  releaseSeats(bookingId: string) {
    return this.http.delete<ApiResponse<any>>(`${this.base}/seats/freeze/${bookingId}`);
  }

  captureBookingDetails(bookingId: string, passengers: any[]) {
    return this.http.post<ApiResponse<any>>(`${this.base}/bookings`, { bookingId, passengers });
  }

  processPayment(bookingId: string, payerName: string, payerEmail: string) {
    return this.http.post<ApiResponse<any>>(`${this.base}/payments`, { bookingId, payerName, payerEmail });
  }

  cancelBooking(bookingId: string) {
    return this.http.delete<ApiResponse<any>>(`${this.base}/bookings/${bookingId}`);
  }

  getBookingById(bookingId: string) {
    return this.http.get<ApiResponse<any>>(`${this.base}/bookings/${bookingId}`);
  }

  
}
