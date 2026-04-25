# Bus Management System - API Documentation

## Overview

This is a comprehensive .NET Web API for a bus ticket booking system with PostgreSQL backend. The API handles user authentication, bus route management, seat booking, payments, and admin operations.

**Base URL:** `http://localhost:5017/api`

---

## Authentication

The API uses **JWT (JSON Web Token)** for authentication. Tokens are issued on login/registration and can be refreshed.

- Tokens are stored as **HttpOnly cookies** with automatic refresh token rotation
- Role-based access control (USER, BUS_OPERATOR, ADMIN)
- Public endpoints: `/auth/register`, `/auth/login`, `/auth/refresh`, `/buses/search`, `/places`

### Why refresh token exists
- Access tokens are intentionally short-lived to reduce risk if intercepted.
- Refresh tokens allow silent session continuation without forcing frequent logins.
- Refresh token rotation enables token revocation and replay protection per device/session.

### User Roles
- **USER**: Regular customer for bus bookings
- **BUS_OPERATOR**: Can manage routes and buses
- **ADMIN**: Can approve routes, manage platform fees, approve operator requests

---

## Endpoints

### Authentication

#### 1. Register User
Create a new user account (USER or BUS_OPERATOR role only)

```http
POST /auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "gender": "MALE",
  "age": 28,
  "password": "SecurePass@123",
  "role": "USER"
}
```

**Response (200 - Created):**
```json
{
  "success": true,
  "message": "Registration successful.",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com",
    "role": "USER",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAtUtc": "2026-04-23T11:16:41Z"
  }
}
```

---

#### 2. Login User
Authenticate with email and password

```http
POST /auth/login
Content-Type: application/json
Cookie: access_token=null; refresh_token=null

{
  "email": "john@example.com",
  "password": "SecurePass@123",
  "rememberMe": false
}
```

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com",
    "role": "USER",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAtUtc": "2026-04-23T11:16:41Z"
  }
}
```

**Cookies Set:**
- `access_token`: JWT token (HttpOnly, Secure, SameSite=None)
- `refresh_token`: Refresh token hash (HttpOnly, Secure, SameSite=None)

---

#### 3. Refresh Token
Get a new access token using the refresh token

```http
POST /auth/refresh
Cookie: refresh_token=<refresh_token_hash>
```

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Session refreshed.",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com",
    "role": "USER",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAtUtc": "2026-04-23T11:21:41Z"
  }
}
```

---

#### 4. Get Current User
Retrieve authenticated user profile

```http
GET /auth/me
Authorization: Bearer <token>
Cookie: access_token=<token>
```

**Response (200 - OK):**
```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com",
    "role": "USER",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAtUtc": "2026-04-23T11:21:41Z"
  }
}
```

---

#### 5. Logout
Clear session and revoke refresh token

```http
POST /auth/logout
Cookie: refresh_token=<refresh_token_hash>
```

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Logged out.",
  "data": null
}
```

**Cookies Deleted:**
- `access_token`
- `refresh_token`

---

### Bus Search & Discovery

#### 6. Search Buses
Find available buses by route and date

```http
GET /buses/search?source=Chennai&destination=Bangalore&date=2026-05-10
```

**Query Parameters:**
- `source` (**required**): Departure location (case-insensitive)
- `destination` (**required**): Arrival location (case-insensitive)
- `date` (**required**): Travel date in `YYYY-MM-DD` format

This endpoint now requires all 3 parameters to avoid expensive broad scans and to always return schedule-aware seat availability for a concrete travel day.

**Response (200 - OK):**
```json
{
  "success": true,
  "data": [
    {
      "busId": "650e8400-e29b-41d4-a716-446655440001",
      "scheduleId": "750e8400-e29b-41d4-a716-446655440002",
      "registrationNumber": "TN-01-AB-1234",
      "operatorName": "FastTravel Co.",
      "source": "Chennai",
      "destination": "Bangalore",
      "travelDate": "2026-05-10",
      "departureTime": "14:30:00",
      "arrivalTime": "20:45:00",
      "seatPrice": 450.00,
      "availableSeats": 8,
      "status": "ACTIVE"
    }
  ]
}
```

---

#### 7. Get Places (Autocomplete)
Get available source/destination locations

```http
GET /places?q=Che&limit=20
```

**Query Parameters:**
- `q` (optional): Search query for location names
- `limit` (optional, default: 100, max: 500): Maximum places returned

Performance guidance:
- Frontend should fetch once (`GET /places?limit=500`) and run fuzzy filtering locally while the user types.
- Backend endpoint remains the source of truth and supports `q` for fallback/server filtering.
- Endpoint is cache-friendly and returns stable normalized place names.

**Response (200 - OK):**
```json
{
  "success": true,
  "data": [
    "Chennai",
    "Chengalpattu",
    "Vellore"
  ]
}
```

---

### Schedules & Seats

#### 8. Get Schedule Seats
Retrieve all seats for a bus schedule

```http
GET /schedules/{scheduleId}/seats
Authorization: Bearer <token>
```

**Path Parameters:**
- `scheduleId`: UUID of the bus schedule

**Response (200 - OK):**
```json
{
  "success": true,
  "data": {
    "scheduleId": "750e8400-e29b-41d4-a716-446655440002",
    "busId": "650e8400-e29b-41d4-a716-446655440001",
    "registrationNumber": "TN-01-AB-1234",
    "source": "Chennai",
    "destination": "Bangalore",
    "travelDate": "2026-05-10",
    "departureTime": "14:30:00",
    "arrivalTime": "20:45:00",
    "seatPrice": 450.00,
    "seats": [
      {
        "seatId": "850e8400-e29b-41d4-a716-446655440003",
        "seatNumber": "A1",
        "status": "AVAILABLE",
        "passengerName": null,
        "passengerAge": null,
        "passengerGender": null
      },
      {
        "seatId": "850e8400-e29b-41d4-a716-446655440004",
        "seatNumber": "A2",
        "status": "BOOKED",
        "passengerName": "Raj Kumar",
        "passengerAge": 35,
        "passengerGender": "MALE"
      },
      {
        "seatId": "850e8400-e29b-41d4-a716-446655440005",
        "seatNumber": "A3",
        "status": "FROZEN",
        "passengerName": null,
        "passengerAge": null,
        "passengerGender": null
      }
    ]
  }
}
```

---

### Seat Management (Booking Flow)

#### 9. Freeze Seats
Reserve seats for 5 minutes (mandatory before booking)

```http
POST /seats/freeze
Authorization: Bearer <token>
Content-Type: application/json

{
  "scheduleId": "750e8400-e29b-41d4-a716-446655440002",
  "seatIds": [
    "850e8400-e29b-41d4-a716-446655440003",
    "850e8400-e29b-41d4-a716-446655440006"
  ]
}
```

**Response (201 - Created):**
```json
{
  "success": true,
  "message": "Seats frozen for 5 minutes.",
  "data": {
    "bookingId": "950e8400-e29b-41d4-a716-446655440007",
    "scheduleId": "750e8400-e29b-41d4-a716-446655440002",
    "expiresAtUtc": "2026-04-23T10:21:41Z",
    "seatIds": [
      "850e8400-e29b-41d4-a716-446655440003",
      "850e8400-e29b-41d4-a716-446655440006"
    ]
  }
}
```

---

#### 10. Release Frozen Seats
Cancel seat freeze (if booking cancelled before payment)

```http
DELETE /seats/freeze/{bookingId}
Authorization: Bearer <token>

```

**Path Parameters:**
- `bookingId`: UUID of the booking to release

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Seat freeze released.",
  "data": null
}
```

---

### Bookings

#### 11. Confirm Booking
Update passenger details and calculate total amount

```http
POST /bookings
Authorization: Bearer <token>
Content-Type: application/json

{
  "bookingId": "950e8400-e29b-41d4-a716-446655440007",
  "passengers": [
    {
      "seatId": "850e8400-e29b-41d4-a716-446655440003",
      "passengerName": "John Doe",
      "passengerAge": 28,
      "passengerGender": "MALE",
      "isPrimary": true
    },
    {
      "seatId": "850e8400-e29b-41d4-a716-446655440006",
      "passengerName": "Jane Smith",
      "passengerAge": 26,
      "passengerGender": "FEMALE",
      "isPrimary": false
    }
  ]
}
```

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Booking details captured.",
  "data": {
    "bookingId": "950e8400-e29b-41d4-a716-446655440007",
    "seatCount": 2,
    "totalAmount": 945.00,
    "platformFeeSnapshot": 45.00
  }
}
```

---

#### 12. Get Booking Details
Retrieve full booking information

```http
GET /bookings/{bookingId}
Authorization: Bearer <token>
```

**Path Parameters:**
- `bookingId`: UUID of the booking

**Response (200 - OK):**
```json
{
  "success": true,
  "data": {
    "bookingId": "950e8400-e29b-41d4-a716-446655440007",
    "status": "CONFIRMED",
    "travelDate": "2026-05-10",
    "source": "Chennai",
    "destination": "Bangalore",
    "registrationNumber": "TN-01-AB-1234",
    "departureTime": "14:30:00",
    "arrivalTime": "20:45:00",
    "seatPrice": 450.00,
    "totalAmount": 945.00,
    "platformFeeSnapshot": 45.00,
    "refundStatus": "NOT_APPLICABLE",
    "refundAmount": null,
    "seatNumbers": ["A1", "A4"],
    "passengers": [
      {
        "seatId": "850e8400-e29b-41d4-a716-446655440003",
        "seatNumber": "A1",
        "passengerName": "John Doe",
        "passengerAge": 28,
        "passengerGender": "MALE",
        "isPrimary": true
      },
      {
        "seatId": "850e8400-e29b-41d4-a716-446655440006",
        "seatNumber": "A4",
        "passengerName": "Jane Smith",
        "passengerAge": 26,
        "passengerGender": "FEMALE",
        "isPrimary": false
      }
    ]
  }
}
```

---

#### 13. List My Bookings
Get all bookings for the current user

```http
GET /bookings/my
Authorization: Bearer <token>
```

**Response (200 - OK):**
```json
{
  "success": true,
  "data": [
    {
      "bookingId": "950e8400-e29b-41d4-a716-446655440007",
      "status": "CONFIRMED",
      "travelDate": "2026-05-10",
      "source": "Chennai",
      "destination": "Bangalore",
      "totalAmount": 945.00,
      "refundStatus": "NOT_APPLICABLE",
      "refundAmount": null,
      "seatNumbers": ["A1", "A4"]
    },
    {
      "bookingId": "950e8400-e29b-41d4-a716-446655440008",
      "status": "CANCELLED_BY_USER",
      "travelDate": "2026-04-25",
      "source": "Bangalore",
      "destination": "Hyderabad",
      "totalAmount": 600.00,
      "refundStatus": "FULL",
      "refundAmount": 600.00,
      "seatNumbers": ["B2"]
    }
  ]
}
```

---

#### 14. Cancel Booking
Cancel a confirmed booking and process refund

```http
DELETE /bookings/{bookingId}
Authorization: Bearer <token>
```

**Path Parameters:**
- `bookingId`: UUID of the booking to cancel

**Refund Rules:**
- **>48 hours before departure:** Full refund
- **24-48 hours before departure:** 50% refund
- **<24 hours before departure:** No refund

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Booking cancelled.",
  "data": null
}
```

---

### Payments

#### 15. Process Payment
Complete payment for a booking

```http
POST /payments
Authorization: Bearer <token>
Content-Type: application/json

{
  "bookingId": "950e8400-e29b-41d4-a716-446655440007",
  "payerName": "John Doe",
  "payerEmail": "john@example.com",
  "voucherCode": "SUMMER2026"
}
```

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Payment successful.",
  "data": {
    "paymentId": "a50e8400-e29b-41d4-a716-446655440009",
    "bookingId": "950e8400-e29b-41d4-a716-446655440007",
    "amount": 850.00,
    "status": "SUCCESS",
    "paidAt": "2026-04-23T10:19:41Z"
  }
}
```

---

### Vouchers

#### 16. Validate Voucher
Check voucher validity before payment

```http
GET /vouchers/{code}
Authorization: Bearer <token>
```

**Path Parameters:**
- `code`: Voucher code (e.g., "SUMMER2026")

**Response (200 - OK):**
```json
{
  "success": true,
  "data": {
    "code": "SUMMER2026",
    "discountPercent": 10.00,
    "isUsed": false,
    "expiresAt": "2026-12-31T23:59:59Z"
  }
}
```

---

### Routes (Bus Operator)

#### 17. Create Route
Submit a new route for admin approval

```http
POST /routes
Authorization: Bearer <token>
Content-Type: application/json

{
  "source": "Chennai",
  "destination": "Bangalore"
}
```

`approvedBy` is kept even with a single-admin model to preserve immutable audit history (who approved/rejected each route) and to keep schema compatible if admin delegation is introduced later.

**Response (201 - Created):**
```json
{
  "success": true,
  "message": "Route submitted for admin approval.",
  "data": {
    "routeId": "b50e8400-e29b-41d4-a716-446655440010",
    "operatorId": "550e8400-e29b-41d4-a716-446655440000",
    "source": "Chennai",
    "destination": "Bangalore",
    "status": "PENDING_APPROVAL",
    "approvedBy": null,
    "approvedAt": null,
    "createdAt": "2026-04-23T10:19:41Z",
    "busCount": 0
  }
}
```

---

#### 18. List My Routes
Get all routes created by the operator

```http
GET /routes?status=PENDING_APPROVAL&page=1&pageSize=10
Authorization: Bearer <token>
```

**Query Parameters:**
- `status` (optional): Filter by route status (PENDING_APPROVAL, APPROVED, REJECTED)
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 10): Items per page

**Response (200 - OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "routeId": "b50e8400-e29b-41d4-a716-446655440010",
        "operatorId": "550e8400-e29b-41d4-a716-446655440000",
        "source": "Chennai",
        "destination": "Bangalore",
        "status": "PENDING_APPROVAL",
        "approvedBy": null,
        "approvedAt": null,
        "createdAt": "2026-04-23T10:19:41Z",
        "busCount": 0
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10
  }
}
```

---

#### 19. Delete Route
Remove a route (only if no confirmed bookings exist)

```http
DELETE /routes/{routeId}
Authorization: Bearer <token>
```

**Path Parameters:**
- `routeId`: UUID of the route

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Route and associated buses deleted.",
  "data": null
}
```

---

### Buses (Bus Operator)

#### 20. Create Bus
Add a bus to an approved route

```http
POST /buses
Authorization: Bearer <token>
Content-Type: application/json

{
  "routeId": "b50e8400-e29b-41d4-a716-446655440010",
  "registrationNumber": "TN-01-AB-1234",
  "departureTime": "14:30:00",
  "arrivalTime": "20:45:00",
  "seatPrice": 450.00
}
```

**Response (201 - Created):**
```json
{
  "success": true,
  "message": "Bus created successfully.",
  "data": {
    "busId": "650e8400-e29b-41d4-a716-446655440001"
  }
}
```

---

#### 21. List My Buses
Get all buses created by the operator

```http
GET /buses?status=ACTIVE
Authorization: Bearer <token>
```

**Query Parameters:**
- `status` (optional): Filter by bus status (ACTIVE, OUT_OF_SERVICE, DELETED)

**Response (200 - OK):**
```json
{
  "success": true,
  "data": [
    {
      "busId": "650e8400-e29b-41d4-a716-446655440001",
      "routeId": "b50e8400-e29b-41d4-a716-446655440010",
      "registrationNumber": "TN-01-AB-1234",
      "source": "Chennai",
      "destination": "Bangalore",
      "departureTime": "14:30:00",
      "arrivalTime": "20:45:00",
      "seatPrice": 450.00,
      "status": "ACTIVE"
    }
  ]
}
```

---

#### 22. Update Bus Status
Change bus availability status

```http
PATCH /buses/{busId}/status
Authorization: Bearer <token>
Content-Type: application/json

{
  "status": "OUT_OF_SERVICE"
}
```

**Path Parameters:**
- `busId`: UUID of the bus

**Status Options:** ACTIVE, OUT_OF_SERVICE, DELETED

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Bus status updated.",
  "data": null
}
```

---

#### 23. Delete Bus
Remove a bus from service

```http
DELETE /buses/{busId}
Authorization: Bearer <token>
```

**Path Parameters:**
- `busId`: UUID of the bus

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Bus deleted successfully.",
  "data": null
}
```

---

### Operator Requests

#### 24. Request Operator Role
Submit request to upgrade from USER to BUS_OPERATOR

```http
POST /operator-requests
Authorization: Bearer <token>
```

**Response (201 - Created):**
```json
{
  "success": true,
  "message": "Operator switch request submitted.",
  "data": {
    "requestId": "c50e8400-e29b-41d4-a716-446655440011",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "PENDING",
    "createdAt": "2026-04-23T10:19:41Z"
  }
}
```

---

#### 25. List My Operator Requests
Get all operator switch requests by current user

```http
GET /operator-requests/my
Authorization: Bearer <token>
```

**Response (200 - OK):**
```json
{
  "success": true,
  "data": [
    {
      "requestId": "c50e8400-e29b-41d4-a716-446655440011",
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "status": "PENDING",
      "createdAt": "2026-04-23T10:19:41Z"
    }
  ]
}
```

---

### Admin Operations

#### 26. Get Admin Summary
Dashboard metrics for admin

```http
GET /admin/summary
Authorization: Bearer <token>
```

**Response (200 - OK):**
```json
{
  "success": true,
  "data": {
    "pendingRoutes": 3,
    "pendingOperatorRequests": 2,
    "currentPlatformFee": 45.00
  }
}
```

---

#### 27. List Pending Routes
View all routes awaiting approval

```http
GET /admin/routes?status=PENDING_APPROVAL&page=1&pageSize=20
Authorization: Bearer <token>
```

**Query Parameters:**
- `status` (optional, default: PENDING_APPROVAL): Filter by route status
- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Items per page

**Response (200 - OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "routeId": "b50e8400-e29b-41d4-a716-446655440010",
        "operatorId": "550e8400-e29b-41d4-a716-446655440000",
        "operatorName": "FastTravel Co.",
        "operatorEmail": "operator@fasttravel.com",
        "source": "Chennai",
        "destination": "Bangalore",
        "status": "PENDING_APPROVAL",
        "createdAt": "2026-04-23T10:19:41Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20
  }
}
```

---

#### 28. Approve Route
Approve a pending route

```http
PATCH /admin/routes/{routeId}/approve
Authorization: Bearer <token>
```

**Path Parameters:**
- `routeId`: UUID of the route

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Route approved.",
  "data": {
    "routeId": "b50e8400-e29b-41d4-a716-446655440010",
    "operatorId": "550e8400-e29b-41d4-a716-446655440000",
    "source": "Chennai",
    "destination": "Bangalore",
    "status": "APPROVED",
    "approvedBy": "a50e8400-e29b-41d4-a716-446655440012",
    "approvedAt": "2026-04-23T10:20:41Z",
    "createdAt": "2026-04-23T10:19:41Z",
    "busCount": 0
  }
}
```

---

#### 29. Reject Route
Reject a pending route with reason

```http
PATCH /admin/routes/{routeId}/reject
Authorization: Bearer <token>
Content-Type: application/json

{
  "reason": "Route already exists for this operator"
}
```

**Path Parameters:**
- `routeId`: UUID of the route

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Route rejected.",
  "data": {
    "routeId": "b50e8400-e29b-41d4-a716-446655440010",
    "operatorId": "550e8400-e29b-41d4-a716-446655440000",
    "source": "Chennai",
    "destination": "Bangalore",
    "status": "REJECTED",
    "approvedBy": "a50e8400-e29b-41d4-a716-446655440012",
    "approvedAt": "2026-04-23T10:20:41Z",
    "createdAt": "2026-04-23T10:19:41Z",
    "busCount": 0
  }
}
```

---

#### 30. List Operator Requests
View pending operator switch requests

```http
GET /admin/operator-requests?status=PENDING
Authorization: Bearer <token>
```

**Query Parameters:**
- `status` (optional, default: PENDING): Filter by request status (PENDING, APPROVED, REJECTED)

**Response (200 - OK):**
```json
{
  "success": true,
  "data": [
    {
      "requestId": "c50e8400-e29b-41d4-a716-446655440011",
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "email": "john@example.com",
      "status": "PENDING",
      "createdAt": "2026-04-23T10:19:41Z",
      "reviewedBy": null,
      "reviewedAt": null
    }
  ]
}
```

---

#### 31. Approve Operator Request
Upgrade a user to BUS_OPERATOR role

```http
PATCH /admin/operator-requests/{requestId}/approve
Authorization: Bearer <token>
```

**Path Parameters:**
- `requestId`: UUID of the request

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Operator request approved.",
  "data": null
}
```

---

#### 32. Reject Operator Request
Deny a user's operator role request

```http
PATCH /admin/operator-requests/{requestId}/reject
Authorization: Bearer <token>
```

**Path Parameters:**
- `requestId`: UUID of the request

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Operator request rejected.",
  "data": null
}
```

---

### Platform Configuration

#### 33. Get Platform Fee
Retrieve current platform fee

```http
GET /admin/config
Authorization: Bearer <token>
```

**Response (200 - OK):**
```json
{
  "success": true,
  "data": {
    "platformFee": 45.00,
    "updatedAt": "2026-04-23T09:00:00Z",
    "updatedBy": "a50e8400-e29b-41d4-a716-446655440012"
  }
}
```

---

#### 34. Update Platform Fee
Set new platform fee per booking

```http
PUT /admin/config
Authorization: Bearer <token>
Content-Type: application/json

{
  "platformFee": 50.00
}
```

**Response (200 - OK):**
```json
{
  "success": true,
  "message": "Platform fee updated to 50.",
  "data": null
}
```

---

### Operator Dashboard

#### 35. Get Operator Summary
Dashboard metrics for bus operator

```http
GET /operator/summary
Authorization: Bearer <token>
```

**Response (200 - OK):**
```json
{
  "success": true,
  "data": {
    "totalRoutes": 5,
    "pendingRoutes": 1,
    "approvedRoutes": 4,
    "rejectedRoutes": 0,
    "totalBuses": 8,
    "activeBuses": 7
  }
}
```

---

## Error Responses

All errors follow this format:

```json
{
  "success": false,
  "errors": ["Error message"],
  "data": null
}
```

### Common Error Codes

| Status | Error | Description |
|--------|-------|-------------|
| 400 | Bad Request | Invalid payload or validation error |
| 401 | Unauthorized | Missing or invalid token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource already exists or state conflict |
| 500 | Internal Server Error | Server error |

---

## Environment Setup

### Required Environment Variables

```bash
# Database
DATABASE_URL=postgresql://user:password@localhost:5432/bus_management

# JWT
JWT_KEY=your-secret-key-at-least-32-bytes-long

# Optional: Admin seeding (first-time only)
ADMIN_SEED_USERNAME=sysadmin
ADMIN_SEED_EMAIL=admin@example.com
ADMIN_SEED_PASSWORD=secure-password
```

### Running the Server

```bash
# Install dependencies
dotnet restore

# Apply migrations
dotnet ef database update

# Run the server
dotnet run

# Server will be available at http://localhost:5017
```

---

## Database Schema

### Core Tables
- `users` - User accounts (USER, BUS_OPERATOR, ADMIN)
- `routes` - Bus routes (pending admin approval)
- `buses` - Bus vehicles linked to approved routes
- `bus_schedules` - Daily schedule instances (auto-created on search)
- `seats` - Fixed 40-seat layout per schedule (auto-created)
- `bookings` - Booking transactions
- `booking_seats` - Passenger details for each seat
- `payments` - Payment records
- `refresh_tokens` - JWT refresh token management
- `discount_vouchers` - Discount codes
- `operator_switch_requests` - Role upgrade requests
- `platform_config` - Platform fee configuration
- `notifications` - Notification records

---

## Booking Flow

```
1. Search buses → GET /buses/search
   └─ Creates schedule & seats if not exist

2. View seats → GET /schedules/{scheduleId}/seats

3. Freeze seats → POST /seats/freeze
   └─ Seats locked for 5 minutes
   └─ Returns temporary bookingId

4. Enter passenger details → POST /bookings
   └─ Calculates total with platform fee

5. Validate voucher (optional) → GET /vouchers/{code}

6. Process payment → POST /payments
   └─ Seats marked BOOKED
   └─ Booking confirmed

7. View booking → GET /bookings/{bookingId}

8. Cancel booking (if within policy) → DELETE /bookings/{bookingId}
   └─ Refund calculated based on departure time
```

---

## Notes

- All timestamps are in UTC (ISO 8601 format)
- Seat numbering: A1-A10, B1-B10, C1-C10, D1-D10 (40 seats total, generated per schedule)
- Seat freeze timeout: 5 minutes
- Platform fee is added to all bookings
- Bus price is stored once at `buses.seat_price` (uniform for all seats in that bus schedule)
- Individual seat identity is stored in `seats.seat_number`; passenger-wise mapping is stored in `booking_seats`
- Passwords are hashed using bcrypt (never stored plaintext)
- Admin role is unique (only one admin per system), but `approvedBy` is retained for route-level audit traceability
