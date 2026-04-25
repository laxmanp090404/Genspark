export type Role = 'USER' | 'BUS_OPERATOR' | 'ADMIN';

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export interface AuthPayload {
  userId: string;
  username: string;
  email: string;
  role: Role;
  token: string;
  expiresAtUtc: string;
}

export interface Profile {
  userId: string;
  username: string;
  email: string;
  role: Role;
}

export interface BusSearchResult {
  busId: string;
  scheduleId: string;
  registrationNumber: string;
  operatorName: string;
  source: string;
  destination: string;
  travelDate: string;
  departureTime: string;
  arrivalTime: string;
  seatPrice: number;
  availableSeats: number;
  status: string;
}

export interface OperatorSummary {
  totalRoutes: number;
  pendingRoutes: number;
  approvedRoutes: number;
  rejectedRoutes: number;
  totalBuses: number;
  activeBuses: number;
}

export interface AdminSummary {
  pendingRoutes: number;
  pendingOperatorRequests: number;
  currentPlatformFee: number;
  totalBookings: number;
  cancelledBookings: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface RouteItem {
  routeId: string;
  operatorId: string;
  source: string;
  destination: string;
  status: string;
  approvedBy: string | null;
  approvedAt: string | null;
  rejectionReason?: string | null;
  createdAt: string;
  busCount: number;
}

export interface AdminRouteItem {
  routeId: string;
  operatorId: string;
  operatorName: string;
  operatorEmail: string;
  source: string;
  destination: string;
  status: string;
  rejectionReason?: string | null;
  createdAt: string;
}

export interface OperatorBus {
  busId: string;
  routeId: string;
  registrationNumber: string;
  source: string;
  destination: string;
  departureTime: string;
  arrivalTime: string;
  seatPrice: number;
  status: string;
}

export interface OperatorRequestItem {
  requestId: string;
  userId: string;
  username?: string;
  email?: string;
  status: string;
  createdAt: string;
  reviewedBy?: string | null;
  reviewedAt?: string | null;
}

export interface PlatformConfig {
  platformFee: number;
  updatedAt: string;
  updatedBy: string | null;
}

export interface BookingSummary {
  bookingId: string;
  scheduleId: string;
  status: string;
  travelDate: string;
  departureTime: string;
  registrationNumber: string;
  source: string;
  destination: string;
  totalAmount: number;
  refundStatus: string;
  refundAmount: number | null;
  seatNumbers: string[];
}
