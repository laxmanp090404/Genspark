# 🚌 BusBook — AngularJS UI Implementation Plan

A full frontend architecture and implementation blueprint for a **Bus Booking System** built using:

* AngularJS 1.x
* UI-Router
* Bootstrap 3
* jsPDF
* angular-toastr

---

## 📦 Tech Stack

| Technology           | Purpose                |
| -------------------- | ---------------------- |
| Angular              | MVC Framework          |
| UI-Router            | State-based routing    |
| $http + Interceptors | API communication      |
| angular-toastr       | Notifications          |
| jsPDF                | PDF generation         |

---

## 🧭 Overview

* **10 Phases**
* **40+ UI Components / Screens**
* **Role-Based Access (USER / OPERATOR / ADMIN / PUBLIC)**
* **JWT Auth + Route Guards**
* **Real-time Seat Booking Flow**

---

# 🏗 Phase 1 — Project Scaffold & Module Setup

## 📁 Folder Structure

```
busbook-ui/
├── app/
│   ├── app.module.js
│   ├── app.config.js
│   ├── app.run.js
│
│   ├── common/
│   │   ├── services/
│   │   ├── interceptors/
│   │   ├── directives/
│   │   └── filters/
│
│   ├── auth/
│   ├── user/
│   ├── operator/
│   └── admin/
└── index.html
```

---

## 🔐 JWT HTTP Interceptor

* Attaches token to every request
* Redirects to login on `401`

```js
$httpProvider.interceptors.push('JwtInterceptor');
```

---

## 🛡 Auth Guard

* Uses `state.data.roles`
* Blocks unauthorized navigation

---

# 🔐 Phase 2 — Authentication

## Signup

* Fields:

  * username, email, gender, age, password
* API: `POST /api/auth/signup`
* Redirect → `/search`

## Login

* API: `POST /api/auth/login`
* Stores:

  * JWT
  * Role
* Redirects based on role:

  * USER → `/search`
  * OPERATOR → `/operator/dashboard`
  * ADMIN → `/admin/dashboard`

---

# 🔍 Phase 3 — Bus Search

## Search Form

* Fields:

  * source
  * destination
  * date

API:

```
GET /api/buses/search
```

## Results

* Shows:

  * Operator
  * Route
  * Time
  * Seat availability
  * Price

---

# 💺 Phase 4 — Seat Selection

## Seat Map Directive

* 40-seat layout (2+2)
* Color-coded:

| State          | Color            |
| -------------- | ---------------- |
| Available      | Grey             |
| Frozen         | Amber            |
| Booked (M/F/O) | Blue/Pink/Purple |
| Selected       | Green            |

---

## Freeze Logic

* API:

```
POST /api/seats/freeze
```

* Timer: **5 minutes**
* Polling every 30s

---

# 💳 Phase 5 — Payment

## Payment Form

* Card details
* Optional voucher

API:

```
POST /api/payments
```

---

## Confirmation

* Shows booking details
* Download PDF via jsPDF

```js
doc.text('Booking ID: ' + bookingId);
```

---

# 📋 Phase 6 — My Bookings

## Features

* View all bookings
* Status tabs:

  * Active
  * Cancelled
  * Completed

---

## Cancellation Rules

| Time   | Refund |
| ------ | ------ |
| >48h   | 100%   |
| 24–48h | 50%    |
| <24h   | 0%     |

---

# 🚌 Phase 7 — Operator Panel

## Dashboard

* Total Routes
* Pending Approval
* Buses count

---

## Routes Management

* Add / Delete routes
* Status:

  * Pending
  * Approved
  * Rejected

---

## Bus Management

* Add buses
* Toggle availability
* Assign routes

---

# 👑 Phase 8 — Admin Panel

## Features

* Approve routes
* Manage operator requests
* Configure platform fee

---

## APIs

```
GET /api/admin/routes
PATCH /api/admin/routes/:id/approve
PATCH /api/admin/routes/:id/reject
```

---

# 🧩 Phase 9 — Shared Components

## Status Filter

```js
{{ status | statusLabel }}
```

---

## Notification Service

```js
Notify.success("Done");
Notify.error("Error");
```

---

## Navbar

* Dynamic links based on role

---

# 🗺 Phase 10 — UI Router States

| State              | URL       | Role     |
| ------------------ | --------- | -------- |
| auth.login         | /login    | PUBLIC   |
| user.search        | /search   | USER     |
| operator.dashboard | /operator | OPERATOR |
| admin.dashboard    | /admin    | ADMIN    |

---

# 🔐 Roles

| Role         | Description     |
| ------------ | --------------- |
| PUBLIC       | Unauthenticated |
| USER         | Booking user    |
| BUS_OPERATOR | Manages buses   |
| ADMIN        | System control  |

---

# 🚀 Key Highlights

* Role-based UI rendering
* Real-time seat locking system
* Modular AngularJS architecture
* Clean separation of concerns
* Fully API-driven UI

