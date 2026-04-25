import { useState } from "react";

// ─── Data ────────────────────────────────────────────────────────────────────

const STACK = [
  { label: "AngularJS 1.x", color: "#e53935", desc: "MVC framework" },
  { label: "UI-Router", color: "#7b61ff", desc: "State-based routing" },
  { label: "Bootstrap 3", color: "#563d7c", desc: "Grid + base components" },
  { label: "$http / Interceptors", color: "#0288d1", desc: "API communication" },
  { label: "angular-toastr", color: "#f57c00", desc: "Notifications" },
  { label: "jsPDF", color: "#2e7d32", desc: "Client PDF download" },
];

const ROLES = {
  USER:         { color: "#38bdf8", pill: "bg-sky-900/50 text-sky-300 border-sky-700/50" },
  BUS_OPERATOR: { color: "#34d399", pill: "bg-emerald-900/50 text-emerald-300 border-emerald-700/50" },
  ADMIN:        { color: "#f59e0b", pill: "bg-amber-900/50 text-amber-300 border-amber-700/50" },
  PUBLIC:       { color: "#94a3b8", pill: "bg-slate-700/50 text-slate-300 border-slate-600/50" },
};

const phases = [
  // ── 1. Project scaffold ──────────────────────────────────────────────────
  {
    id: 1, title: "Project Scaffold & Module Setup", color: "#6366f1",
    icon: "🏗",
    summary: "Bootstrap the AngularJS app, configure modules, routing, HTTP interceptor for JWT, and shared services.",
    items: [
      {
        id: "1.1", title: "Folder Structure",
        roles: [],
        type: "STRUCTURE",
        desc: "Feature-first layout. Each role gets its own folder. Shared utilities live in /common.",
        code: `busbook-ui/
├── app/
│   ├── app.module.js          ← angular.module('busbook', [...])
│   ├── app.config.js          ← UI-Router state definitions
│   ├── app.run.js             ← Auth guard on $stateChangeStart
│   │
│   ├── common/
│   │   ├── services/
│   │   │   ├── auth.service.js
│   │   │   ├── route.service.js
│   │   │   ├── bus.service.js
│   │   │   ├── booking.service.js
│   │   │   ├── payment.service.js
│   │   │   └── notification.service.js
│   │   ├── interceptors/
│   │   │   └── jwt.interceptor.js
│   │   ├── directives/
│   │   │   ├── seat-map.directive.js
│   │   │   └── role-guard.directive.js
│   │   └── filters/
│   │       └── status-label.filter.js
│   │
│   ├── auth/
│   │   ├── login/
│   │   └── signup/
│   ├── user/
│   │   ├── search/
│   │   ├── bus-detail/
│   │   ├── booking/
│   │   ├── payment/
│   │   └── my-bookings/
│   ├── operator/
│   │   ├── dashboard/
│   │   ├── routes/
│   │   └── buses/
│   └── admin/
│       ├── dashboard/
│       ├── routes/
│       ├── operators/
│       └── config/
└── index.html`,
      },
      {
        id: "1.2", title: "JWT HTTP Interceptor",
        roles: [],
        type: "JS",
        desc: "Attaches Bearer token to every request. Redirects to /login on 401.",
        code: `angular.module('busbook')
.factory('JwtInterceptor', ['$q', '$injector', 'AuthService',
function($q, $injector, AuthService) {
  return {
    request: function(config) {
      var token = AuthService.getToken();
      if (token) config.headers['Authorization'] = 'Bearer ' + token;
      return config;
    },
    responseError: function(rejection) {
      if (rejection.status === 401) {
        $injector.get('$state').go('auth.login');
      }
      return $q.reject(rejection);
    }
  };
}])
.config(['$httpProvider', function($httpProvider) {
  $httpProvider.interceptors.push('JwtInterceptor');
}]);`,
      },
      {
        id: "1.3", title: "Auth Guard on State Change",
        roles: [],
        type: "JS",
        desc: "Reads state.data.roles; redirects if user's role doesn't match.",
        code: `// app.run.js
angular.module('busbook')
.run(['$rootScope', '$state', 'AuthService',
function($rootScope, $state, AuthService) {
  $rootScope.$on('$stateChangeStart', function(e, toState) {
    var required = toState.data && toState.data.roles;
    if (!required) return;                        // public route

    if (!AuthService.isLoggedIn()) {
      e.preventDefault();
      $state.go('auth.login');
      return;
    }
    var userRole = AuthService.getRole();
    if (required.indexOf(userRole) === -1) {
      e.preventDefault();
      $state.go('error.forbidden');
    }
  });
}]);

// State definition example:
// $stateProvider.state('admin.routes', {
//   url: '/admin/routes',
//   templateUrl: '...',
//   controller: '...',
//   data: { roles: ['ADMIN'] }
// });`,
      },
    ],
  },

  // ── 2. Auth screens ──────────────────────────────────────────────────────
  {
    id: 2, title: "Auth — Login & Signup", color: "#8b5cf6",
    icon: "🔐",
    summary: "Two public screens. Signup collects username, email, gender, age, password. Login returns JWT + role; stored in sessionStorage.",
    items: [
      {
        id: "2.1", title: "Signup Screen",
        roles: ["PUBLIC"],
        type: "SCREEN",
        route: "/signup",
        fields: [
          { name: "username",  type: "text",     validation: "required, minlength:3" },
          { name: "email",     type: "email",    validation: "required, email format" },
          { name: "gender",    type: "select",   options: ["MALE","FEMALE","OTHER"], validation: "required" },
          { name: "age",       type: "number",   validation: "required, min:1, max:120" },
          { name: "password",  type: "password", validation: "required, minlength:8" },
          { name: "confirm",   type: "password", validation: "must match password" },
        ],
        onSuccess: "Redirect → /search",
        onError: "Show inline field errors via ng-messages",
        apiCall: "POST /api/auth/signup",
      },
      {
        id: "2.2", title: "Login Screen",
        roles: ["PUBLIC"],
        type: "SCREEN",
        route: "/login",
        fields: [
          { name: "email",    type: "email",    validation: "required" },
          { name: "password", type: "password", validation: "required" },
        ],
        onSuccess: "Store JWT + role in AuthService. Role-based redirect: USER→/search, OPERATOR→/operator/dashboard, ADMIN→/admin/dashboard",
        onError: "Toast: 'Invalid credentials'",
        apiCall: "POST /api/auth/login",
        note: "AuthService.setSession(token, role, userId) — wraps sessionStorage.",
      },
    ],
  },

  // ── 3. User — Search & Results ───────────────────────────────────────────
  {
    id: 3, title: "User — Bus Search & Results", color: "#0891b2",
    icon: "🔍",
    summary: "The primary landing screen for authenticated users. Fuzzy search powered by pg_trgm on the backend. Results show live seat counts.",
    items: [
      {
        id: "3.1", title: "Search Form",
        roles: ["USER"],
        type: "SCREEN",
        route: "/search",
        fields: [
          { name: "source",      type: "text", validation: "required — typeahead from known places" },
          { name: "destination", type: "text", validation: "required — typeahead from known places" },
          { name: "date",        type: "date", validation: "required, not in past" },
        ],
        onSuccess: "Render results list below form (no page nav)",
        apiCall: "GET /api/buses/search?source=&destination=&date=",
        note: "Typeahead uses GET /api/places?q= which returns operator-provided source locations.",
      },
      {
        id: "3.2", title: "Search Results List",
        roles: ["USER"],
        type: "COMPONENT",
        desc: "Each bus card shows: operator name, registration number, source → destination, departure/arrival time, seats available (color-coded: green ≥10, amber 1–9, red 0), price per seat. Clicking navigates to seat selection.",
        stateBindings: [
          "busId, scheduleId passed as UI-Router params to next state",
          "Sold-out buses shown greyed-out, Book button disabled",
        ],
      },
    ],
  },

  // ── 4. User — Seat Selection ─────────────────────────────────────────────
  {
    id: 4, title: "User — Seat Layout & Selection", color: "#059669",
    icon: "💺",
    summary: "The seat-map is a custom AngularJS directive rendering the fixed 40-seat grid. Seats are color-coded by gender of occupant. On selection, a 5-minute freeze timer starts.",
    items: [
      {
        id: "4.1", title: "Seat Map Directive  (seat-map)",
        roles: ["USER"],
        type: "DIRECTIVE",
        desc: "Renders a 2+2 column bus layout. Each seat is a button with color state.",
        bindings: [
          { prop: "seats",       direction: "=", desc: "Array of seat objects from API" },
          { prop: "onSelect",    direction: "&", desc: "Callback when user clicks available seat" },
          { prop: "onDeselect",  direction: "&", desc: "Callback to unfreeze" },
        ],
        colorCoding: [
          { state: "AVAILABLE",  color: "#e2e8f0", label: "White/Grey — free" },
          { state: "FROZEN",     color: "#fbbf24", label: "Amber — frozen by someone" },
          { state: "BOOKED (M)", color: "#60a5fa", label: "Blue — male occupant" },
          { state: "BOOKED (F)", color: "#f472b6", label: "Pink — female occupant" },
          { state: "BOOKED (O)", color: "#a78bfa", label: "Purple — other" },
          { state: "SELECTED",   color: "#34d399", label: "Green — your selection" },
        ],
        code: `// Tooltip on hover: shows name, age, gender if booked
// Directive template snippet (pseudo):
// <button ng-repeat="seat in seats"
//   ng-class="seatClass(seat)"
//   ng-disabled="seat.status === 'FROZEN' || seat.status === 'BOOKED'"
//   ng-click="ctrl.selectSeat(seat)"
//   uib-tooltip="{{seat.passengerName}} · {{seat.passengerAge}} · {{seat.passengerGender}}"
//   tooltip-enable="seat.status === 'BOOKED'">
//   {{seat.seatNumber}}
// </button>`,
      },
      {
        id: "4.2", title: "Freeze Timer + Passenger Details Form",
        roles: ["USER"],
        type: "COMPONENT",
        desc: "After selecting seats, a countdown timer (5:00) appears. For each selected seat a passenger form appears: primary seat auto-fills from user profile, additional seats show name/age/gender fields.",
        logic: [
          "POST /api/seats/freeze  { scheduleId, seatIds[] } → starts 5-min freeze server-side",
          "$interval polls GET /api/schedules/:id/seats every 30s to refresh seat states for other users",
          "On timer expiry: auto-cancel freeze via DELETE /api/seats/freeze/:bookingId, show toast, reset selection",
          "Proceed to payment only if all seats still FROZEN and form valid",
        ],
      },
    ],
  },

  // ── 5. User — Payment & Confirmation ────────────────────────────────────
  {
    id: 5, title: "User — Payment & Booking Confirmation", color: "#dc2626",
    icon: "💳",
    summary: "Mock payment form. On success: booking confirmed, email triggered server-side, PDF downloadable in-app via jsPDF.",
    items: [
      {
        id: "5.1", title: "Payment Screen",
        roles: ["USER"],
        type: "SCREEN",
        route: "/booking/:bookingId/payment",
        fields: [
          { name: "cardholderName", type: "text",   validation: "required" },
          { name: "cardNumber",     type: "text",   validation: "16 digits, masked display" },
          { name: "expiry",         type: "text",   validation: "MM/YY format" },
          { name: "cvv",            type: "text",   validation: "3 digits" },
          { name: "voucherCode",    type: "text",   validation: "optional — validated on blur via GET /api/vouchers/:code" },
        ],
        breakdown: "Shows: seat subtotal, platform fee, voucher discount (if any), total. All read from bookings.total_amount + platform_fee_snapshot.",
        apiCall: "POST /api/payments  { bookingId, payerName, payerEmail }",
        onSuccess: "Navigate to /booking/:id/confirmation",
        onFailure: "Toast error, seats unfrozen (server handles), redirect to /search",
      },
      {
        id: "5.2", title: "Booking Confirmation Screen",
        roles: ["USER"],
        type: "SCREEN",
        route: "/booking/:bookingId/confirmation",
        desc: "Displays booking summary: booking ID, bus details, seat numbers, passenger list, total paid. Two CTAs: 'Download PDF' (jsPDF, client-side generation) and 'View My Bookings'.",
        code: `// jsPDF generation (confirmation.controller.js)
$scope.downloadPdf = function() {
  var doc = new jsPDF();
  doc.setFontSize(18);
  doc.text('BusBook — Booking Confirmation', 14, 22);
  doc.setFontSize(11);
  doc.text('Booking ID: ' + $scope.booking.bookingId, 14, 35);
  doc.text('Route: ' + $scope.booking.source + ' → ' + $scope.booking.destination, 14, 42);
  doc.text('Date: ' + $scope.booking.travelDate, 14, 49);
  doc.text('Seats: ' + $scope.booking.seatNumbers.join(', '), 14, 56);
  doc.text('Total Paid: ₹' + $scope.booking.totalAmount, 14, 63);
  // Passenger table via autoTable plugin
  doc.autoTable({ head: [['Seat','Name','Age','Gender']], body: $scope.passengerRows, startY: 72 });
  doc.save('booking-' + $scope.booking.bookingId + '.pdf');
};`,
      },
    ],
  },

  // ── 6. User — My Bookings & Cancellation ────────────────────────────────
  {
    id: 6, title: "User — My Bookings & Cancellation", color: "#f59e0b",
    icon: "📋",
    summary: "Lists all bookings for the logged-in user with status badges. Cancellation shows refund preview based on departure proximity.",
    items: [
      {
        id: "6.1", title: "My Bookings List",
        roles: ["USER"],
        type: "SCREEN",
        route: "/my-bookings",
        desc: "Tabbed by status: Active, Cancelled, Completed. Each card: route, travel date, seat numbers, amount, status badge. Active bookings show a 'Cancel' button if departure is in the future.",
        apiCall: "GET /api/bookings/my",
      },
      {
        id: "6.2", title: "Cancellation Confirmation Modal",
        roles: ["USER"],
        type: "COMPONENT",
        desc: "Before confirming cancel, compute and display refund preview client-side. User confirms → API call.",
        logic: [
          ">48h before departure → show 'Full refund: ₹X'",
          "24–48h → show '50% refund: ₹X'",
          "<24h → show 'No refund applicable'",
          "On confirm: DELETE /api/bookings/:bookingId → email sent by server to user + operator",
        ],
      },
    ],
  },

  // ── 7. Operator — Dashboard & Routes ────────────────────────────────────
  {
    id: 7, title: "Bus Operator — Dashboard & Routes", color: "#0d9488",
    icon: "🚌",
    summary: "Operator's home after login. Manages routes (submit/view/delete) and buses. Route status cards reflect PENDING/APPROVED/REJECTED from admin.",
    items: [
      {
        id: "7.1", title: "Operator Dashboard",
        roles: ["BUS_OPERATOR"],
        type: "SCREEN",
        route: "/operator/dashboard",
        desc: "Summary cards: Total Routes, Pending Approval, Total Buses, Active Buses. Quick links to manage routes and buses.",
      },
      {
        id: "7.2", title: "Routes Management Screen",
        roles: ["BUS_OPERATOR"],
        type: "SCREEN",
        route: "/operator/routes",
        desc: "Table of own routes with status badges (PENDING_APPROVAL in amber, APPROVED in green, REJECTED in red). Actions: Add New Route (modal), Delete (disabled if active bookings).",
        modalFields: [
          { name: "source",      type: "text", validation: "required" },
          { name: "destination", type: "text", validation: "required, different from source" },
        ],
        apiCalls: [
          "GET /api/routes",
          "POST /api/routes",
          "DELETE /api/routes/:id",
        ],
        note: "On successful route creation, show toast: 'Route submitted for admin approval.'",
      },
      {
        id: "7.3", title: "Buses Management Screen",
        roles: ["BUS_OPERATOR"],
        type: "SCREEN",
        route: "/operator/buses",
        desc: "Lists buses grouped by route (only APPROVED routes shown in dropdown). Can add bus to an approved route, toggle OUT_OF_SERVICE, or delete bus. Registration number must be unique — validated on blur.",
        apiCalls: [
          "GET /api/buses (own)",
          "POST /api/buses",
          "PATCH /api/buses/:id/status",
          "DELETE /api/buses/:id",
        ],
      },
    ],
  },

  // ── 8. Admin ─────────────────────────────────────────────────────────────
  {
    id: 8, title: "Admin — Approvals & Config", color: "#f59e0b",
    icon: "👑",
    summary: "Single admin panel. Approves/rejects routes and operator switch requests. Can edit the global platform fee.",
    items: [
      {
        id: "8.1", title: "Admin Dashboard",
        roles: ["ADMIN"],
        type: "SCREEN",
        route: "/admin/dashboard",
        desc: "Badge-count cards: Pending Routes, Pending Operator Requests, Current Platform Fee. Links to each queue.",
      },
      {
        id: "8.2", title: "Route Approval Queue",
        roles: ["ADMIN"],
        type: "SCREEN",
        route: "/admin/routes",
        desc: "Table of PENDING_APPROVAL routes. Columns: operator name, source, destination, submitted date. Row actions: Approve (green button) → immediate, Reject (red button) → modal asks for rejection reason. After action, row updates in-place with new status badge.",
        apiCalls: [
          "GET /api/admin/routes?status=PENDING_APPROVAL",
          "PATCH /api/admin/routes/:id/approve",
          "PATCH /api/admin/routes/:id/reject  { reason }",
        ],
      },
      {
        id: "8.3", title: "Operator Switch Request Queue",
        roles: ["ADMIN"],
        type: "SCREEN",
        route: "/admin/operator-requests",
        desc: "Lists users who requested BUS_OPERATOR role. Shows username, email, requested date. Approve upgrades users.role, Reject sends email with no role change.",
        apiCalls: [
          "GET /api/admin/operator-requests?status=PENDING",
          "PATCH /api/admin/operator-requests/:id/approve",
          "PATCH /api/admin/operator-requests/:id/reject",
        ],
      },
      {
        id: "8.4", title: "Platform Fee Config",
        roles: ["ADMIN"],
        type: "SCREEN",
        route: "/admin/config",
        desc: "Single editable field showing current platform fee. On save, new record inserted in platform_config (audit trail). Toast: 'Platform fee updated to ₹X. Applies to new bookings only.'",
        apiCalls: [
          "GET /api/admin/config",
          "PUT /api/admin/config  { platformFee }",
        ],
      },
    ],
  },

  // ── 9. Shared Components ─────────────────────────────────────────────────
  {
    id: 9, title: "Shared Components & Services", color: "#7c3aed",
    icon: "🧩",
    summary: "Reusable directives, filters, and services used across all roles.",
    items: [
      {
        id: "9.1", title: "Status Label Filter",
        roles: [],
        type: "JS",
        desc: "Converts enum values to human-readable labels and Bootstrap badge classes.",
        code: `angular.module('busbook')
.filter('statusLabel', function() {
  var map = {
    PENDING_APPROVAL: { label: 'Pending',  cls: 'warning' },
    APPROVED:         { label: 'Approved', cls: 'success' },
    REJECTED:         { label: 'Rejected', cls: 'danger'  },
    ACTIVE:           { label: 'Active',   cls: 'success' },
    OUT_OF_SERVICE:   { label: 'Inactive', cls: 'default' },
    CONFIRMED:        { label: 'Confirmed',cls: 'success' },
    CANCELLED_BY_USER:    { label: 'Cancelled', cls: 'danger' },
    CANCELLED_BY_OPERATOR:{ label: 'Op. Cancelled', cls: 'danger' },
  };
  return function(value, prop) {
    var entry = map[value];
    if (!entry) return value;
    return prop === 'cls' ? entry.cls : entry.label;
  };
});
// Usage: <span class="label label-{{booking.status | statusLabel:'cls'}}">
//          {{booking.status | statusLabel}}
//        </span>`,
      },
      {
        id: "9.2", title: "Notification Toast Service",
        roles: [],
        type: "JS",
        desc: "Wraps angular-toastr with preset configs for success/error/info/warning.",
        code: `angular.module('busbook')
.factory('Notify', ['toastr', function(toastr) {
  return {
    success: function(msg) { toastr.success(msg, '', { timeOut: 3000 }); },
    error:   function(msg) { toastr.error(msg,   '', { timeOut: 5000 }); },
    info:    function(msg) { toastr.info(msg,    '', { timeOut: 3000 }); },
    warn:    function(msg) { toastr.warning(msg, '', { timeOut: 4000 }); },
  };
}]);`,
      },
      {
        id: "9.3", title: "Shared Navbar",
        roles: [],
        type: "COMPONENT",
        desc: "Role-aware navigation. Uses ng-show to reveal role-specific links. USER: Search, My Bookings. OPERATOR: Dashboard, Routes, Buses, Request Admin. ADMIN: Approvals, Requests, Config. All: Logout.",
        code: `<!-- navbar.html (partial) -->
<nav class="navbar navbar-default">
  <div class="container">
    <a class="navbar-brand" ui-sref="home">🚌 BusBook</a>
    <ul class="nav navbar-nav" ng-if="auth.isLoggedIn()">
      <!-- USER -->
      <li ng-if="auth.isRole('USER')"><a ui-sref="user.search">Search Buses</a></li>
      <li ng-if="auth.isRole('USER')"><a ui-sref="user.bookings">My Bookings</a></li>
      <!-- OPERATOR -->
      <li ng-if="auth.isRole('BUS_OPERATOR')"><a ui-sref="operator.routes">Routes</a></li>
      <li ng-if="auth.isRole('BUS_OPERATOR')"><a ui-sref="operator.buses">Buses</a></li>
      <!-- ADMIN -->
      <li ng-if="auth.isRole('ADMIN')"><a ui-sref="admin.routes">Approvals</a></li>
      <li ng-if="auth.isRole('ADMIN')"><a ui-sref="admin.config">Config</a></li>
      <!-- ALL -->
      <li><a ng-click="auth.logout()">Logout</a></li>
    </ul>
  </div>
</nav>`,
      },
    ],
  },

  // ── 10. UI State map ─────────────────────────────────────────────────────
  {
    id: 10, title: "UI-Router State Map", color: "#0891b2",
    icon: "🗺",
    summary: "Complete list of named states registered in app.config.js, their URLs, required roles, and which API endpoints they consume.",
    items: [
      {
        id: "10.1", title: "Full State Table",
        roles: [],
        type: "STATE_TABLE",
        states: [
          { state: "auth.login",               url: "/login",                       roles: "PUBLIC",       apis: "POST /api/auth/login" },
          { state: "auth.signup",              url: "/signup",                      roles: "PUBLIC",       apis: "POST /api/auth/signup" },
          { state: "user.search",              url: "/search",                      roles: "USER",         apis: "GET /api/buses/search, GET /api/places" },
          { state: "user.bus-detail",          url: "/bus/:scheduleId",             roles: "USER",         apis: "GET /api/schedules/:id/seats" },
          { state: "user.booking",             url: "/booking/new/:scheduleId",     roles: "USER",         apis: "POST /api/seats/freeze, POST /api/bookings" },
          { state: "user.payment",             url: "/booking/:bookingId/payment",  roles: "USER",         apis: "POST /api/payments, GET /api/vouchers/:code" },
          { state: "user.confirmation",        url: "/booking/:bookingId/confirm",  roles: "USER",         apis: "GET /api/bookings/:id" },
          { state: "user.my-bookings",         url: "/my-bookings",                 roles: "USER",         apis: "GET /api/bookings/my" },
          { state: "operator.dashboard",       url: "/operator",                    roles: "BUS_OPERATOR", apis: "GET /api/operator/summary" },
          { state: "operator.routes",          url: "/operator/routes",             roles: "BUS_OPERATOR", apis: "GET/POST/DELETE /api/routes" },
          { state: "operator.buses",           url: "/operator/buses",              roles: "BUS_OPERATOR", apis: "GET/POST/PATCH/DELETE /api/buses" },
          { state: "admin.dashboard",          url: "/admin",                       roles: "ADMIN",        apis: "GET /api/admin/summary" },
          { state: "admin.routes",             url: "/admin/routes",                roles: "ADMIN",        apis: "GET/PATCH /api/admin/routes" },
          { state: "admin.operator-requests",  url: "/admin/operators",             roles: "ADMIN",        apis: "GET/PATCH /api/admin/operator-requests" },
          { state: "admin.config",             url: "/admin/config",                roles: "ADMIN",        apis: "GET/PUT /api/admin/config" },
          { state: "error.forbidden",          url: "/403",                         roles: "PUBLIC",       apis: "—" },
        ],
      },
    ],
  },
];

// ─── Components ──────────────────────────────────────────────────────────────

function RolePill({ role }) {
  if (!role) return null;
  const r = ROLES[role] || ROLES.PUBLIC;
  return (
    <span className={`px-2 py-0.5 text-[10px] font-bold rounded border font-mono ${r.pill}`}>
      {role}
    </span>
  );
}

function FieldsTable({ fields }) {
  return (
    <table className="w-full text-xs mt-2 rounded-lg overflow-hidden">
      <thead>
        <tr className="bg-slate-800/60">
          <th className="text-left px-3 py-2 text-slate-400 font-semibold">Field</th>
          <th className="text-left px-3 py-2 text-slate-400 font-semibold">Type</th>
          {fields[0]?.options && <th className="text-left px-3 py-2 text-slate-400 font-semibold">Options</th>}
          <th className="text-left px-3 py-2 text-slate-400 font-semibold">Validation</th>
        </tr>
      </thead>
      <tbody>
        {fields.map((f, i) => (
          <tr key={i} className="border-t border-slate-800/50 hover:bg-slate-800/20">
            <td className="px-3 py-2 font-mono text-cyan-300">{f.name}</td>
            <td className="px-3 py-2 text-slate-400">{f.type}</td>
            {fields[0]?.options && <td className="px-3 py-2 text-slate-500 text-[10px]">{(f.options || []).join(", ")}</td>}
            <td className="px-3 py-2 text-slate-400">{f.validation}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function StateTable({ states }) {
  return (
    <div className="overflow-x-auto mt-2 rounded-lg border border-slate-800">
      <table className="w-full text-xs">
        <thead>
          <tr className="bg-slate-800/60">
            <th className="text-left px-3 py-2 text-slate-400 font-semibold">State</th>
            <th className="text-left px-3 py-2 text-slate-400 font-semibold">URL</th>
            <th className="text-left px-3 py-2 text-slate-400 font-semibold">Role</th>
            <th className="text-left px-3 py-2 text-slate-400 font-semibold">APIs</th>
          </tr>
        </thead>
        <tbody>
          {states.map((s, i) => (
            <tr key={i} className="border-t border-slate-800/50 hover:bg-slate-800/20">
              <td className="px-3 py-2 font-mono text-indigo-300 text-[11px]">{s.state}</td>
              <td className="px-3 py-2 font-mono text-slate-400 text-[11px]">{s.url}</td>
              <td className="px-3 py-2"><RolePill role={s.roles} /></td>
              <td className="px-3 py-2 text-slate-500 text-[10px] font-mono">{s.apis}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function ColorCodingTable({ rows }) {
  return (
    <div className="mt-2 grid grid-cols-2 gap-2">
      {rows.map((r, i) => (
        <div key={i} className="flex items-center gap-2 bg-slate-900 rounded px-3 py-2 border border-slate-800">
          <span className="w-4 h-4 rounded flex-shrink-0 border border-slate-600" style={{ backgroundColor: r.color }} />
          <span className="text-[11px] text-slate-300">{r.label}</span>
        </div>
      ))}
    </div>
  );
}

function ItemCard({ item }) {
  const [open, setOpen] = useState(true);

  return (
    <div className="rounded-xl border border-slate-800 bg-slate-900/30 overflow-hidden mb-3">
      <button
        onClick={() => setOpen(o => !o)}
        className="w-full flex items-center gap-3 px-5 py-3 hover:bg-slate-800/30 transition text-left"
      >
        <span className="text-slate-600 font-mono text-xs w-8 flex-shrink-0">{item.id}</span>
        <span className="flex-1 text-white text-sm font-semibold">{item.title}</span>
        <div className="flex items-center gap-1.5">
          {item.roles?.map(r => <RolePill key={r} role={r} />)}
          <span className={`px-2 py-0.5 text-[10px] font-mono rounded border ${
            item.type === "SCREEN"    ? "bg-indigo-900/50 text-indigo-300 border-indigo-700/50" :
            item.type === "COMPONENT" ? "bg-teal-900/50 text-teal-300 border-teal-700/50" :
            item.type === "DIRECTIVE" ? "bg-pink-900/50 text-pink-300 border-pink-700/50" :
            item.type === "JS"        ? "bg-yellow-900/50 text-yellow-300 border-yellow-700/50" :
            item.type === "STRUCTURE" ? "bg-slate-700/50 text-slate-300 border-slate-600" :
            item.type === "STATE_TABLE" ? "bg-cyan-900/50 text-cyan-300 border-cyan-700/50" :
            "bg-slate-700/50 text-slate-400 border-slate-600"
          }`}>
            {item.type}
          </span>
        </div>
        <span className="text-slate-600 ml-1">{open ? "▲" : "▼"}</span>
      </button>

      {open && (
        <div className="px-5 pb-4 border-t border-slate-800/50 space-y-3">
          {item.route && (
            <div className="flex items-center gap-2 mt-3">
              <span className="text-slate-500 text-xs">Route:</span>
              <code className="text-cyan-300 text-xs bg-slate-900 px-2 py-0.5 rounded border border-slate-800">{item.route}</code>
            </div>
          )}

          {(item.desc || item.description || item.summary) && (
            <p className="text-slate-400 text-xs leading-relaxed mt-2">
              {item.desc || item.description || item.summary}
            </p>
          )}

          {item.fields && <FieldsTable fields={item.fields} />}

          {item.colorCoding && <ColorCodingTable rows={item.colorCoding} />}

          {item.bindings && (
            <div className="mt-2">
              <p className="text-slate-500 text-[10px] uppercase tracking-wider mb-1">Directive Bindings</p>
              <table className="w-full text-xs">
                <thead><tr className="bg-slate-800/40"><th className="text-left px-3 py-1.5 text-slate-400">Prop</th><th className="text-left px-3 py-1.5 text-slate-400">Dir</th><th className="text-left px-3 py-1.5 text-slate-400">Description</th></tr></thead>
                <tbody>
                  {item.bindings.map((b, i) => (
                    <tr key={i} className="border-t border-slate-800/40">
                      <td className="px-3 py-1.5 font-mono text-emerald-300">{b.prop}</td>
                      <td className="px-3 py-1.5 text-amber-300 font-mono">{b.direction}</td>
                      <td className="px-3 py-1.5 text-slate-400">{b.desc}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {item.stateBindings && (
            <ul className="mt-2 space-y-1">
              {item.stateBindings.map((s, i) => (
                <li key={i} className="flex gap-2 text-xs text-slate-400">
                  <span className="text-teal-400 flex-shrink-0">▸</span>{s}
                </li>
              ))}
            </ul>
          )}

          {item.logic && (
            <ul className="mt-2 space-y-1">
              {item.logic.map((l, i) => (
                <li key={i} className="flex gap-2 text-xs text-slate-400">
                  <span className="text-indigo-400 flex-shrink-0">▸</span>
                  <span className="font-mono">{l}</span>
                </li>
              ))}
            </ul>
          )}

          {item.apiCalls && (
            <div className="mt-2 flex flex-wrap gap-1">
              {item.apiCalls.map((a, i) => (
                <code key={i} className="text-[10px] font-mono bg-slate-900 text-cyan-200 px-2 py-0.5 rounded border border-slate-800">{a}</code>
              ))}
            </div>
          )}

          {item.apiCall && (
            <div className="mt-2">
              <code className="text-[10px] font-mono bg-slate-900 text-cyan-200 px-2 py-0.5 rounded border border-slate-800">{item.apiCall}</code>
            </div>
          )}

          {item.breakdown && (
            <div className="mt-2 p-3 bg-slate-900 rounded-lg border border-slate-800">
              <p className="text-slate-500 text-[10px] uppercase tracking-wider mb-1">Amount Breakdown</p>
              <p className="text-slate-300 text-xs">{item.breakdown}</p>
            </div>
          )}

          {item.onSuccess && (
            <div className="flex gap-2 items-start mt-1">
              <span className="text-[10px] text-green-400 font-semibold flex-shrink-0 mt-0.5">✓ SUCCESS</span>
              <span className="text-xs text-slate-400">{item.onSuccess}</span>
            </div>
          )}
          {item.onError && (
            <div className="flex gap-2 items-start">
              <span className="text-[10px] text-red-400 font-semibold flex-shrink-0 mt-0.5">✗ ERROR</span>
              <span className="text-xs text-slate-400">{item.onError}</span>
            </div>
          )}
          {item.onFailure && (
            <div className="flex gap-2 items-start">
              <span className="text-[10px] text-red-400 font-semibold flex-shrink-0 mt-0.5">✗ FAIL</span>
              <span className="text-xs text-slate-400">{item.onFailure}</span>
            </div>
          )}

          {item.note && (
            <div className="mt-2 p-2.5 bg-amber-950/20 border border-amber-800/30 rounded-lg">
              <p className="text-amber-300/80 text-[11px]">📌 {item.note}</p>
            </div>
          )}

          {item.modalFields && (
            <div className="mt-2">
              <p className="text-slate-500 text-[10px] uppercase tracking-wider mb-1">Modal Fields</p>
              <FieldsTable fields={item.modalFields} />
            </div>
          )}

          {item.code && (
            <pre className="mt-3 bg-slate-950 rounded-lg p-4 text-xs text-emerald-200 font-mono overflow-x-auto leading-relaxed border border-slate-800">
              {item.code}
            </pre>
          )}

          {item.states && <StateTable states={item.states} />}
        </div>
      )}
    </div>
  );
}

export default function UIImplPlan() {
  const [activePhase, setActivePhase] = useState(null);
  const [allOpen, setAllOpen] = useState(true);

  const filtered = activePhase === null ? phases : phases.filter(p => p.id === activePhase);

  return (
    <div
      className="min-h-screen bg-[#0a0c12] text-white"
      style={{
        fontFamily: "'JetBrains Mono', 'Fira Code', monospace",
        backgroundImage: "radial-gradient(ellipse at 15% 15%, rgba(99,102,241,0.07) 0%, transparent 50%), radial-gradient(ellipse at 85% 85%, rgba(8,145,178,0.06) 0%, transparent 50%)",
      }}
    >
      <link href="https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@300;400;500;600;700&display=swap" rel="stylesheet" />

      {/* Header */}
      <div className="sticky top-0 z-30 bg-[#0a0c12]/95 backdrop-blur border-b border-slate-800 px-5 py-4">
        <div className="max-w-5xl mx-auto flex flex-col sm:flex-row sm:items-center gap-3 justify-between">
          <div>
            <div className="flex items-center gap-2">
              <span className="text-indigo-400">⬡</span>
              <h1 className="text-sm font-bold text-white tracking-tight">BusBook — AngularJS UI Implementation Plan</h1>
            </div>
            <div className="flex flex-wrap gap-1.5 mt-1.5">
              {STACK.map(s => (
                <span key={s.label} className="px-2 py-0.5 text-[10px] rounded border border-slate-700/60 bg-slate-900/60"
                  style={{ color: s.color }}>
                  {s.label}
                </span>
              ))}
            </div>
          </div>
          <div className="text-[10px] text-slate-600 font-mono text-right">
            {phases.length} phases · {phases.reduce((a, p) => a + p.items.length, 0)} items · 15 states
          </div>
        </div>
      </div>

      <div className="max-w-5xl mx-auto px-4 py-8">

        {/* Phase Nav */}
        <div className="flex flex-wrap gap-1.5 mb-6 p-2 bg-slate-900/40 rounded-xl border border-slate-800">
          <button
            onClick={() => setActivePhase(null)}
            className={`px-3 py-1.5 rounded-lg text-[11px] font-bold transition ${activePhase === null ? "bg-indigo-600 text-white" : "text-slate-500 hover:text-white"}`}
          >
            All
          </button>
          {phases.map(p => (
            <button
              key={p.id}
              onClick={() => setActivePhase(p.id === activePhase ? null : p.id)}
              className="px-3 py-1.5 rounded-lg text-[11px] font-bold transition flex items-center gap-1.5"
              style={{
                backgroundColor: activePhase === p.id ? p.color : "transparent",
                color: activePhase === p.id ? "#fff" : "#64748b",
                border: `1px solid ${activePhase === p.id ? p.color : "transparent"}`,
              }}
            >
              <span>{p.icon}</span>
              <span className="hidden sm:inline">{p.title.split("—")[0].trim()}</span>
              <span className="sm:hidden">{p.id}</span>
            </button>
          ))}
        </div>

        {/* Phases */}
        {filtered.map(phase => (
          <div key={phase.id} className="mb-10">
            {/* Phase header */}
            <div
              className="rounded-xl p-4 mb-4 border"
              style={{
                borderColor: phase.color + "40",
                background: `linear-gradient(135deg, ${phase.color}10 0%, transparent 60%)`,
              }}
            >
              <div className="flex items-center gap-3">
                <div
                  className="w-8 h-8 rounded-lg flex items-center justify-center text-base font-bold text-white flex-shrink-0"
                  style={{ backgroundColor: phase.color }}
                >
                  {phase.icon}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="text-slate-500 font-mono text-xs">Phase {phase.id}</span>
                    <h2 className="text-sm font-bold text-white">{phase.title}</h2>
                  </div>
                  <p className="text-slate-400 text-xs mt-0.5 leading-relaxed">{phase.summary}</p>
                </div>
              </div>
            </div>

            {/* Items */}
            {phase.items.map(item => <ItemCard key={item.id} item={item} />)}
          </div>
        ))}

        {/* Footer */}
        <div className="mt-6 pt-5 border-t border-slate-800 flex flex-wrap justify-between text-[10px] text-slate-700 font-mono">
          <span>AngularJS 1.x · UI-Router · Bootstrap 3 · jsPDF · angular-toastr</span>
          <span>BusBook UI Plan v1.0 — {new Date().toLocaleDateString()}</span>
        </div>
      </div>
    </div>
  );
}
