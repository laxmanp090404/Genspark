# 🛣 Routes Feature — Implementation Plan

A complete **end-to-end backend + frontend implementation plan** for the **Routes module** in BusBook.

**Tech Stack:**

* .NET WebAPI
* PostgreSQL

---

## ⚠️ Critical Constraint

### Single Admin Rule

* Only **ONE Admin** exists in the system.
* Enforced at DB level using a **partial unique index**.

```sql
CREATE UNIQUE INDEX one_admin
ON users (role)
WHERE role = 'ADMIN';
```

👉 All approvals are performed by this single admin (`user_id`).

---

# 🧱 Phase 1 — Database Layer

## 1.1 Partial Unique Index (Admin + Routes)

```sql
-- Only one admin allowed
CREATE UNIQUE INDEX idx_one_admin
ON users (role)
WHERE role = 'ADMIN';

-- Prevent duplicate routes per operator
CREATE UNIQUE INDEX idx_unique_operator_route
ON routes (operator_id, source, destination);
```

---

## 1.2 Seed Admin

```sql
INSERT INTO users (username, email, password_hash, gender, age, role)
VALUES (
  'sysadmin',
  'admin@busbook.in',
  '$argon2id$v=19$...',
  'OTHER',
  30,
  'ADMIN'
)
ON CONFLICT DO NOTHING;
```

---

# 🏗 Phase 2 — .NET Project Structure

```
BusBook.API/
├── Controllers/
├── DTOs/
├── Services/
├── Repositories/
├── Models/
└── Middleware/
```

---

## 2.2 Route Entity

```csharp
public class Route
{
    public Guid RouteId { get; set; }
    public Guid OperatorId { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public RouteStatus Status { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
```

---

# 🌐 Phase 3 — API Endpoints

## 🟢 POST /api/routes

**Role:** BUS_OPERATOR

Create new route → starts as `PENDING_APPROVAL`

```json
{
  "source": "Chennai",
  "destination": "Bangalore"
}
```

### Responses

* ✅ 201 Created
* ❌ 400 Validation error
* ❌ 409 Duplicate route

---

## 🔵 GET /api/routes

**Role:** BUS_OPERATOR

Fetch operator’s routes

---

## 👑 GET /api/admin/routes

**Role:** ADMIN

Fetch all pending routes

---

## 🟠 PATCH /approve

**Endpoint:**

```
/api/admin/routes/:routeId/approve
```

* Sets:

  * `status = APPROVED`
  * `approved_by = admin`
  * `approved_at = NOW()`

---

## 🔴 PATCH /reject

```json
{
  "reason": "Invalid route"
}
```

---

## ❌ DELETE /api/routes/:id

**Role:** BUS_OPERATOR

* Allowed only if **no active bookings**

---

# 🧠 Phase 4 — Service Layer

## Create Route

```csharp
if (source == destination)
    throw ValidationException;

if (exists)
    throw ConflictException;
```

### Flow:

1. Validate input
2. Check duplicates
3. Save as `PENDING_APPROVAL`
4. Email admin

---

## Approve Route

```csharp
route.Status = Approved;
route.ApprovedBy = adminId;
route.ApprovedAt = DateTime.UtcNow;
```

---

## Reject Route

* Same as approve, but:

  * Status = Rejected
  * Sends rejection email

---

## Get Single Admin

```csharp
return await _db.Users
    .SingleAsync(u => u.Role == Admin);
```

---

# 🔐 Phase 5 — Authorization

## Role-Based Attribute

```csharp
[RequireRole(UserRole.Admin)]
```

### Behavior:

* Reads JWT claim
* Blocks unauthorized access

---

# 📧 Phase 6 — Email Notifications

| Trigger        | Recipient | Template               |
| -------------- | --------- | ---------------------- |
| Route Created  | Admin     | ROUTE_APPROVAL_REQUEST |
| Route Approved | Operator  | ROUTE_APPROVED         |
| Route Rejected | Operator  | ROUTE_REJECTED         |

---

# 🌐 Phase 7 — AngularJS Integration

## Route Service

```js
createRoute(source, destination)
getMyRoutes(status)
getPendingRoutes()
approveRoute(routeId)
rejectRoute(routeId, reason)
deleteRoute(routeId)
```

---

## Example

```js
$http.post('/api/routes', { source, destination })
```

---

# 👥 Roles

| Role         | Permissions    |
| ------------ | -------------- |
| USER         | None           |
| BUS_OPERATOR | Manage routes  |
| ADMIN        | Approve/reject |

---

# 🚀 Key Highlights

* Enforced **single admin constraint**
* Strong **DB-level validation**
* Clean **layered architecture**
* Fully **REST-compliant APIs**
* Integrated **email workflows**
* Role-secured endpoints

---

# 📊 Summary

* **7 Phases**
* **13 Steps**
* **6 Core APIs**
* **1 Admin (strictly enforced)**

-