import { useState } from "react";

const CLARIFICATION = {
  note: "Single Admin Constraint",
  detail: "Only ONE admin exists in the system. The users table enforces this via a partial unique index: CREATE UNIQUE INDEX one_admin ON users (role) WHERE role = 'ADMIN'. All approval actions are tied to this fixed admin user_id."
};

const ROLES = {
  USER: { color: "#38bdf8", bg: "bg-sky-900/40 border-sky-700/50 text-sky-300" },
  BUS_OPERATOR: { color: "#34d399", bg: "bg-emerald-900/40 border-emerald-700/50 text-emerald-300" },
  ADMIN: { color: "#f59e0b", bg: "bg-amber-900/40 border-amber-700/50 text-amber-300" },
};

const HTTP = {
  GET: "bg-blue-900/50 text-blue-300 border-blue-700/50",
  POST: "bg-green-900/50 text-green-300 border-green-700/50",
  PUT: "bg-yellow-900/50 text-yellow-300 border-yellow-700/50",
  PATCH: "bg-orange-900/50 text-orange-300 border-orange-700/50",
  DELETE: "bg-red-900/50 text-red-300 border-red-700/50",
};

const plan = [
  // ─── PHASE 1 ───
  {
    phase: 1,
    title: "Database Layer",
    color: "#6366f1",
    steps: [
      {
        id: "1.1",
        title: "Partial unique index for single admin",
        type: "SQL",
        description: "Enforce exactly one ADMIN role in the system at the DB level.",
        code: `-- Enforces only one admin can ever exist
CREATE UNIQUE INDEX idx_one_admin
  ON users (role)
  WHERE role = 'ADMIN';

-- Routes table (from schema design)
-- routes: route_id, operator_id, source, destination,
--         status (PENDING_APPROVAL|APPROVED|REJECTED),
--         approved_by (FK → users.user_id, NULLABLE),
--         approved_at (TIMESTAMPTZ, NULLABLE)

-- Ensure operator cannot duplicate same route
CREATE UNIQUE INDEX idx_unique_operator_route
  ON routes (operator_id, source, destination);`,
      },
      {
        id: "1.2",
        title: "Seed single admin user",
        type: "SQL",
        description: "Insert the one-and-only admin on first deployment.",
        code: `INSERT INTO users (username, email, password_hash, gender, age, role)
VALUES (
  'sysadmin',
  'admin@busbook.in',
  '$argon2id$v=19$...', -- pre-hashed
  'OTHER',
  30,
  'ADMIN'
)
ON CONFLICT DO NOTHING;
-- The partial unique index prevents a second ADMIN row.`,
      },
    ],
  },

  // ─── PHASE 2 ───
  {
    phase: 2,
    title: ".NET WebAPI — Project Structure",
    color: "#8b5cf6",
    steps: [
      {
        id: "2.1",
        title: "Folder layout",
        type: "STRUCTURE",
        description: "Recommended project layout for the Routes feature.",
        code: `BusBook.API/
├── Controllers/
│   └── RoutesController.cs
├── DTOs/
│   └── Routes/
│       ├── CreateRouteRequest.cs
│       ├── RouteResponse.cs
│       ├── RouteListResponse.cs
│       └── ApproveRejectRouteRequest.cs
├── Services/
│   ├── Interfaces/IRouteService.cs
│   └── RouteService.cs
├── Repositories/
│   ├── Interfaces/IRouteRepository.cs
│   └── RouteRepository.cs
├── Models/
│   └── Route.cs          ← EF Core entity
└── Middleware/
    └── RoleAuthAttribute.cs`,
      },
      {
        id: "2.2",
        title: "EF Core entity",
        type: "C#",
        description: "Route model mapped to the routes table.",
        code: `public class Route
{
    public Guid RouteId { get; set; }
    public Guid OperatorId { get; set; }
    public string Source { get; set; } = null!;
    public string Destination { get; set; } = null!;
    public RouteStatus Status { get; set; } = RouteStatus.PendingApproval;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public User Operator { get; set; } = null!;
    public User? Approver { get; set; }
    public ICollection<Bus> Buses { get; set; } = [];
}

public enum RouteStatus
{
    PendingApproval,
    Approved,
    Rejected
}`,
      },
    ],
  },

  // ─── PHASE 3 ───
  {
    phase: 3,
    title: "API Endpoints",
    color: "#059669",
    steps: [
      {
        id: "3.1",
        title: "POST /api/routes — Operator submits new route",
        type: "ENDPOINT",
        method: "POST",
        url: "/api/routes",
        auth: "BUS_OPERATOR",
        description: "Operator creates a route. Starts in PENDING_APPROVAL. Admin is notified by email.",
        request: {
          headers: { Authorization: "Bearer <jwt_token>" },
          body: {
            source: "Chennai",
            destination: "Bangalore"
          }
        },
        response201: {
          success: true,
          data: {
            routeId: "uuid-xxxx",
            operatorId: "uuid-operator",
            source: "Chennai",
            destination: "Bangalore",
            status: "PENDING_APPROVAL",
            approvedBy: null,
            approvedAt: null,
            createdAt: "2026-04-22T10:00:00Z"
          },
          message: "Route submitted for admin approval."
        },
        response400: {
          success: false,
          errors: ["Source and destination cannot be the same."]
        },
        response409: {
          success: false,
          errors: ["Route Chennai → Bangalore already exists for this operator."]
        },
      },
      {
        id: "3.2",
        title: "GET /api/routes — Operator views own routes",
        type: "ENDPOINT",
        method: "GET",
        url: "/api/routes",
        auth: "BUS_OPERATOR",
        description: "Returns routes belonging to the authenticated operator. Filterable by status.",
        request: {
          headers: { Authorization: "Bearer <jwt_token>" },
          query: { status: "PENDING_APPROVAL | APPROVED | REJECTED (optional)", page: 1, pageSize: 10 }
        },
        response200: {
          success: true,
          data: {
            items: [
              {
                routeId: "uuid-xxxx",
                source: "Chennai",
                destination: "Bangalore",
                status: "APPROVED",
                approvedAt: "2026-04-21T09:00:00Z",
                busCount: 3
              }
            ],
            totalCount: 1,
            page: 1,
            pageSize: 10
          }
        },
      },
      {
        id: "3.3",
        title: "GET /api/admin/routes — Admin views all pending routes",
        type: "ENDPOINT",
        method: "GET",
        url: "/api/admin/routes",
        auth: "ADMIN",
        description: "Admin sees all routes across operators. Default filter: PENDING_APPROVAL.",
        request: {
          headers: { Authorization: "Bearer <admin_jwt>" },
          query: { status: "PENDING_APPROVAL", page: 1, pageSize: 20 }
        },
        response200: {
          success: true,
          data: {
            items: [
              {
                routeId: "uuid-xxxx",
                operatorId: "uuid-op",
                operatorName: "FastTravel Co.",
                operatorEmail: "ops@fasttravel.in",
                source: "Chennai",
                destination: "Bangalore",
                status: "PENDING_APPROVAL",
                createdAt: "2026-04-22T10:00:00Z"
              }
            ],
            totalCount: 1,
            page: 1,
            pageSize: 20
          }
        },
      },
      {
        id: "3.4",
        title: "PATCH /api/admin/routes/:routeId/approve — Admin approves",
        type: "ENDPOINT",
        method: "PATCH",
        url: "/api/admin/routes/:routeId/approve",
        auth: "ADMIN",
        description: "Admin approves a PENDING_APPROVAL route. Sets approved_by = admin user_id, approved_at = NOW(). Email sent to operator.",
        request: {
          headers: { Authorization: "Bearer <admin_jwt>" },
          body: {}
        },
        response200: {
          success: true,
          data: {
            routeId: "uuid-xxxx",
            status: "APPROVED",
            approvedBy: "uuid-admin",
            approvedAt: "2026-04-22T11:00:00Z"
          },
          message: "Route approved. Operator notified via email."
        },
        response400: {
          success: false,
          errors: ["Route is not in PENDING_APPROVAL state."]
        },
        response404: {
          success: false,
          errors: ["Route not found."]
        },
      },
      {
        id: "3.5",
        title: "PATCH /api/admin/routes/:routeId/reject — Admin rejects",
        type: "ENDPOINT",
        method: "PATCH",
        url: "/api/admin/routes/:routeId/reject",
        auth: "ADMIN",
        description: "Admin rejects a route with an optional reason. Operator receives email with reason.",
        request: {
          headers: { Authorization: "Bearer <admin_jwt>" },
          body: { reason: "Route overlaps with a restricted zone." }
        },
        response200: {
          success: true,
          data: {
            routeId: "uuid-xxxx",
            status: "REJECTED",
            approvedBy: "uuid-admin",
            approvedAt: "2026-04-22T11:05:00Z"
          },
          message: "Route rejected. Operator notified via email."
        },
      },
      {
        id: "3.6",
        title: "DELETE /api/routes/:routeId — Operator deletes a route",
        type: "ENDPOINT",
        method: "DELETE",
        url: "/api/routes/:routeId",
        auth: "BUS_OPERATOR",
        description: "Operator deletes own route. Only allowed if no CONFIRMED bookings exist on associated buses. Cascades: buses on this route are soft-deleted.",
        request: {
          headers: { Authorization: "Bearer <jwt_token>" }
        },
        response200: {
          success: true,
          message: "Route and associated buses deleted."
        },
        response409: {
          success: false,
          errors: ["Cannot delete route with active or confirmed bookings."]
        },
        response403: {
          success: false,
          errors: ["You do not own this route."]
        },
      },
    ],
  },

  // ─── PHASE 4 ───
  {
    phase: 4,
    title: "Service Layer Logic",
    color: "#f59e0b",
    steps: [
      {
        id: "4.1",
        title: "CreateRouteAsync",
        type: "C#",
        description: "Validates, checks for duplicates, persists, notifies admin.",
        code: `public async Task<RouteResponse> CreateRouteAsync(
    Guid operatorId, CreateRouteRequest req)
{
    // 1. Guard: source != destination
    if (req.Source.Trim().Equals(req.Destination.Trim(),
        StringComparison.OrdinalIgnoreCase))
        throw new ValidationException("Source and destination cannot be the same.");

    // 2. Guard: duplicate route for this operator
    bool exists = await _routeRepo.ExistsAsync(operatorId, req.Source, req.Destination);
    if (exists)
        throw new ConflictException($"Route {req.Source} → {req.Destination} already exists.");

    // 3. Persist with status = PENDING_APPROVAL
    var route = new Route {
        RouteId     = Guid.NewGuid(),
        OperatorId  = operatorId,
        Source      = req.Source.Trim(),
        Destination = req.Destination.Trim(),
        Status      = RouteStatus.PendingApproval,
        CreatedAt   = DateTime.UtcNow,
        UpdatedAt   = DateTime.UtcNow
    };
    await _routeRepo.AddAsync(route);

    // 4. Notify the single admin via email
    var admin = await _userRepo.GetSingleAdminAsync(); // returns the one ADMIN
    await _emailService.SendAsync(admin.Email,
        subject: $"New route approval request: {route.Source} → {route.Destination}",
        templateId: "ROUTE_APPROVAL_REQUEST",
        data: new { route, operatorId });

    return _mapper.Map<RouteResponse>(route);
}`,
      },
      {
        id: "4.2",
        title: "ApproveRouteAsync & RejectRouteAsync",
        type: "C#",
        description: "Admin-only. Updates status, stamps approved_by, emails operator.",
        code: `public async Task<RouteResponse> ApproveRouteAsync(Guid adminId, Guid routeId)
{
    var route = await _routeRepo.GetByIdAsync(routeId)
        ?? throw new NotFoundException("Route not found.");

    if (route.Status != RouteStatus.PendingApproval)
        throw new ValidationException("Route is not in PENDING_APPROVAL state.");

    route.Status     = RouteStatus.Approved;
    route.ApprovedBy = adminId;   // Always the single admin's user_id
    route.ApprovedAt = DateTime.UtcNow;
    route.UpdatedAt  = DateTime.UtcNow;
    await _routeRepo.UpdateAsync(route);

    // Notify operator
    var op = await _userRepo.GetByIdAsync(route.OperatorId);
    await _emailService.SendAsync(op.Email,
        subject: $"Route Approved: {route.Source} → {route.Destination}",
        templateId: "ROUTE_APPROVED");

    return _mapper.Map<RouteResponse>(route);
}

public async Task<RouteResponse> RejectRouteAsync(
    Guid adminId, Guid routeId, string? reason)
{
    var route = await _routeRepo.GetByIdAsync(routeId)
        ?? throw new NotFoundException("Route not found.");

    if (route.Status != RouteStatus.PendingApproval)
        throw new ValidationException("Route is not in PENDING_APPROVAL state.");

    route.Status     = RouteStatus.Rejected;
    route.ApprovedBy = adminId;
    route.ApprovedAt = DateTime.UtcNow;
    route.UpdatedAt  = DateTime.UtcNow;
    await _routeRepo.UpdateAsync(route);

    var op = await _userRepo.GetByIdAsync(route.OperatorId);
    await _emailService.SendAsync(op.Email,
        subject: $"Route Rejected: {route.Source} → {route.Destination}",
        templateId: "ROUTE_REJECTED",
        data: new { reason });

    return _mapper.Map<RouteResponse>(route);
}`,
      },
      {
        id: "4.3",
        title: "GetSingleAdminAsync — Repository helper",
        type: "C#",
        description: "Utility method to fetch the one admin. Throws if somehow missing.",
        code: `// IUserRepository.cs
Task<User> GetSingleAdminAsync();

// UserRepository.cs
public async Task<User> GetSingleAdminAsync()
{
    var admin = await _db.Users
        .SingleOrDefaultAsync(u => u.Role == UserRole.Admin);

    return admin ?? throw new InvalidOperationException(
        "No admin configured in the system. Run seed migration.");
}`,
      },
    ],
  },

  // ─── PHASE 5 ───
  {
    phase: 5,
    title: "Authorization Middleware",
    color: "#dc2626",
    steps: [
      {
        id: "5.1",
        title: "Role-based attribute",
        type: "C#",
        description: "Protects endpoints based on JWT role claim.",
        code: `// Usage: [RequireRole(UserRole.Admin)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole[] _roles;
    public RequireRoleAttribute(params UserRole[] roles) => _roles = roles;

    public void OnAuthorization(AuthorizationFilterContext ctx)
    {
        var roleClaim = ctx.HttpContext.User
            .FindFirst(ClaimTypes.Role)?.Value;

        if (roleClaim == null || !_roles.Any(r =>
            r.ToString().Equals(roleClaim, StringComparison.OrdinalIgnoreCase)))
        {
            ctx.Result = new ForbidResult();
        }
    }
}

// RoutesController.cs usage:
[HttpPost]
[RequireRole(UserRole.BusOperator)]
public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequest req) { ... }

[HttpPatch("{routeId:guid}/approve")]
[RequireRole(UserRole.Admin)]
public async Task<IActionResult> ApproveRoute(Guid routeId) { ... }`,
      },
    ],
  },

  // ─── PHASE 6 ───
  {
    phase: 6,
    title: "Email Notifications for Routes",
    color: "#7c3aed",
    steps: [
      {
        id: "6.1",
        title: "Email trigger matrix",
        type: "TABLE",
        rows: [
          { trigger: "Operator creates route", recipient: "Admin (single)", template: "ROUTE_APPROVAL_REQUEST", content: "New route pending: Chennai → Bangalore by FastTravel Co." },
          { trigger: "Admin approves route", recipient: "Bus Operator", template: "ROUTE_APPROVED", content: "Your route Chennai → Bangalore has been approved. You may now add buses." },
          { trigger: "Admin rejects route", recipient: "Bus Operator", template: "ROUTE_REJECTED", content: "Your route was rejected. Reason: [reason]. You may resubmit." },
        ]
      },
    ],
  },

  // ─── PHASE 7 ───
  {
    phase: 7,
    title: "AngularJS — Frontend Integration",
    color: "#0891b2",
    steps: [
      {
        id: "7.1",
        title: "Route service (AngularJS factory)",
        type: "JS",
        description: "HTTP service layer for route operations.",
        code: `angular.module('busbook').factory('RouteService', ['$http', 'AuthService',
function($http, AuthService) {
  var BASE = '/api';

  function headers() {
    return { Authorization: 'Bearer ' + AuthService.getToken() };
  }

  return {
    // Operator: submit new route
    createRoute: function(source, destination) {
      return $http.post(BASE + '/routes',
        { source, destination },
        { headers: headers() });
    },

    // Operator: view own routes
    getMyRoutes: function(status, page) {
      return $http.get(BASE + '/routes', {
        headers: headers(),
        params: { status, page, pageSize: 10 }
      });
    },

    // Admin: view all pending routes
    getPendingRoutes: function() {
      return $http.get(BASE + '/admin/routes', {
        headers: headers(),
        params: { status: 'PENDING_APPROVAL' }
      });
    },

    // Admin: approve
    approveRoute: function(routeId) {
      return $http.patch(BASE + '/admin/routes/' + routeId + '/approve',
        {}, { headers: headers() });
    },

    // Admin: reject
    rejectRoute: function(routeId, reason) {
      return $http.patch(BASE + '/admin/routes/' + routeId + '/reject',
        { reason }, { headers: headers() });
    },

    // Operator: delete
    deleteRoute: function(routeId) {
      return $http.delete(BASE + '/routes/' + routeId,
        { headers: headers() });
    }
  };
}]);`,
      },
    ],
  },
];

// ── Components ────────────────────────────────────────────────

function Badge({ text, style }) {
  return (
    <span className={`px-2 py-0.5 rounded text-[10px] font-mono font-semibold border ${style}`}>
      {text}
    </span>
  );
}

function CodeBlock({ code, lang }) {
  return (
    <pre className="bg-slate-950 rounded-lg p-4 text-xs text-emerald-200 font-mono overflow-x-auto leading-relaxed border border-slate-800 mt-3">
      <code>{code}</code>
    </pre>
  );
}

function EndpointCard({ step }) {
  const [tab, setTab] = useState("request");
  const responses = [];
  if (step.response200) responses.push({ label: "200", data: step.response200, style: "bg-green-900/40 text-green-300 border-green-700/50" });
  if (step.response201) responses.push({ label: "201", data: step.response201, style: "bg-green-900/40 text-green-300 border-green-700/50" });
  if (step.response400) responses.push({ label: "400", data: step.response400, style: "bg-yellow-900/40 text-yellow-300 border-yellow-700/50" });
  if (step.response403) responses.push({ label: "403", data: step.response403, style: "bg-red-900/40 text-red-300 border-red-700/50" });
  if (step.response404) responses.push({ label: "404", data: step.response404, style: "bg-red-900/40 text-red-300 border-red-700/50" });
  if (step.response409) responses.push({ label: "409", data: step.response409, style: "bg-orange-900/40 text-orange-300 border-orange-700/50" });

  const tabs = ["request", ...responses.map(r => r.label)];

  return (
    <div className="mt-3 rounded-lg bg-slate-950 border border-slate-800 overflow-hidden">
      {/* URL Bar */}
      <div className="flex items-center gap-2 px-4 py-2 bg-slate-900/70 border-b border-slate-800">
        <span className={`px-2 py-0.5 rounded text-[10px] font-mono font-bold border ${HTTP[step.method]}`}>
          {step.method}
        </span>
        <span className="font-mono text-xs text-slate-300">{step.url}</span>
        <div className="ml-auto flex items-center gap-1">
          {Object.entries(ROLES).map(([role, { bg }]) =>
            step.auth === role ? (
              <span key={role} className={`px-2 py-0.5 rounded text-[10px] font-semibold border ${bg}`}>
                {role}
              </span>
            ) : null
          )}
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-0 border-b border-slate-800">
        {tabs.map(t => (
          <button
            key={t}
            onClick={() => setTab(t)}
            className={`px-4 py-1.5 text-[10px] font-mono font-semibold uppercase transition-all ${
              tab === t
                ? "bg-slate-800 text-white border-b-2 border-indigo-500"
                : "text-slate-500 hover:text-slate-300"
            }`}
          >
            {t === "request" ? "Request" : `${t}`}
          </button>
        ))}
      </div>

      <div className="p-4">
        {tab === "request" && (
          <pre className="text-xs text-cyan-200 font-mono leading-relaxed whitespace-pre-wrap">
            {JSON.stringify(step.request, null, 2)}
          </pre>
        )}
        {responses.map(r =>
          tab === r.label ? (
            <pre key={r.label} className="text-xs font-mono leading-relaxed whitespace-pre-wrap text-slate-200">
              {JSON.stringify(r.data, null, 2)}
            </pre>
          ) : null
        )}
      </div>
    </div>
  );
}

function EmailTable({ rows }) {
  return (
    <div className="mt-3 overflow-x-auto rounded-lg border border-slate-800">
      <table className="w-full text-xs">
        <thead>
          <tr className="bg-slate-800/60">
            <th className="text-left px-4 py-2 text-slate-400 font-semibold">Trigger</th>
            <th className="text-left px-4 py-2 text-slate-400 font-semibold">Recipient</th>
            <th className="text-left px-4 py-2 text-slate-400 font-semibold">Template</th>
            <th className="text-left px-4 py-2 text-slate-400 font-semibold">Content Preview</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row, i) => (
            <tr key={i} className="border-t border-slate-800 hover:bg-slate-800/20">
              <td className="px-4 py-2 text-slate-300 font-medium">{row.trigger}</td>
              <td className="px-4 py-2">
                <span className={`px-2 py-0.5 rounded text-[10px] font-semibold border ${
                  row.recipient.includes("Admin") ? ROLES.ADMIN.bg : ROLES.BUS_OPERATOR.bg
                }`}>{row.recipient}</span>
              </td>
              <td className="px-4 py-2 font-mono text-purple-300 text-[11px]">{row.template}</td>
              <td className="px-4 py-2 text-slate-400 italic">{row.content}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function StepCard({ step }) {
  const [open, setOpen] = useState(true);
  return (
    <div className="rounded-xl border border-slate-800 bg-slate-900/40 overflow-hidden mb-3">
      <button
        onClick={() => setOpen(o => !o)}
        className="w-full flex items-center gap-3 px-5 py-3 text-left hover:bg-slate-800/30 transition"
      >
        <span className="text-slate-600 font-mono text-xs w-6 flex-shrink-0">{step.id}</span>
        <span className="text-white text-sm font-semibold flex-1">{step.title}</span>
        <span className={`px-2 py-0.5 rounded text-[10px] font-mono border ${
          step.type === "ENDPOINT" ? `${HTTP[step.method] || "bg-slate-700 text-slate-300 border-slate-600"}` :
          step.type === "SQL" ? "bg-blue-950 text-blue-300 border-blue-800" :
          step.type === "C#" ? "bg-violet-950 text-violet-300 border-violet-800" :
          step.type === "JS" ? "bg-yellow-950 text-yellow-300 border-yellow-800" :
          step.type === "TABLE" ? "bg-teal-950 text-teal-300 border-teal-800" :
          "bg-slate-800 text-slate-300 border-slate-700"
        }`}>{step.method || step.type}</span>
        <span className="text-slate-600">{open ? "▲" : "▼"}</span>
      </button>
      {open && (
        <div className="px-5 pb-4 border-t border-slate-800/60">
          <p className="text-slate-400 text-xs mt-3 mb-1">{step.description}</p>
          {step.type === "ENDPOINT" && <EndpointCard step={step} />}
          {(step.type === "SQL" || step.type === "C#" || step.type === "JS" || step.type === "STRUCTURE") && (
            <CodeBlock code={step.code} lang={step.type} />
          )}
          {step.type === "TABLE" && <EmailTable rows={step.rows} />}
        </div>
      )}
    </div>
  );
}

export default function RoutesImplPlan() {
  const [activePhase, setActivePhase] = useState(null);

  return (
    <div
      className="min-h-screen bg-slate-950 text-white"
      style={{
        fontFamily: "'JetBrains Mono', 'Fira Code', monospace",
        backgroundImage: "radial-gradient(ellipse at 10% 20%, rgba(99,102,241,0.06) 0%, transparent 55%), radial-gradient(ellipse at 90% 80%, rgba(5,150,105,0.06) 0%, transparent 55%)"
      }}
    >
      <link href="https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@300;400;500;600;700&display=swap" rel="stylesheet" />

      {/* Header */}
      <div className="sticky top-0 z-20 bg-slate-950/90 backdrop-blur border-b border-slate-800 px-6 py-4">
        <div className="max-w-4xl mx-auto flex flex-col sm:flex-row sm:items-center gap-3 justify-between">
          <div>
            <div className="flex items-center gap-2">
              <span className="text-emerald-400 text-base">⬡</span>
              <h1 className="text-base font-bold text-white tracking-tight">Routes — Implementation Plan</h1>
              <span className="px-2 py-0.5 rounded-full text-[10px] bg-emerald-900/50 text-emerald-300 border border-emerald-700/50 font-semibold">Phase-by-Phase</span>
            </div>
            <p className="text-slate-500 text-[11px] mt-0.5">.NET WebAPI · PostgreSQL · AngularJS</p>
          </div>
          <div className="flex flex-wrap gap-1">
            {Object.entries(ROLES).map(([role, { bg }]) => (
              <span key={role} className={`px-2 py-0.5 rounded text-[10px] font-semibold border ${bg}`}>{role}</span>
            ))}
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 py-8">

        {/* Clarification Banner */}
        <div className="mb-6 p-4 rounded-xl bg-amber-950/30 border border-amber-700/40 flex gap-3 items-start">
          <span className="text-amber-400 text-lg flex-shrink-0">⚠</span>
          <div>
            <p className="text-amber-300 font-bold text-sm">{CLARIFICATION.note}</p>
            <p className="text-amber-200/70 text-xs mt-1">{CLARIFICATION.detail}</p>
          </div>
        </div>

        {/* Phase nav */}
        <div className="flex flex-wrap gap-2 mb-6">
          <button
            onClick={() => setActivePhase(null)}
            className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition ${activePhase === null ? "bg-indigo-600 text-white" : "bg-slate-800 text-slate-400 hover:text-white"}`}
          >
            All Phases
          </button>
          {plan.map(p => (
            <button
              key={p.phase}
              onClick={() => setActivePhase(activePhase === p.phase ? null : p.phase)}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition border ${
                activePhase === p.phase
                  ? "text-white border-transparent"
                  : "bg-slate-900 text-slate-400 border-slate-800 hover:text-white"
              }`}
              style={activePhase === p.phase ? { backgroundColor: p.color, borderColor: p.color } : {}}
            >
              {p.phase}. {p.title}
            </button>
          ))}
        </div>

        {/* Phases */}
        {plan
          .filter(p => activePhase === null || p.phase === activePhase)
          .map(phase => (
            <div key={phase.phase} className="mb-8">
              <div className="flex items-center gap-3 mb-4">
                <div
                  className="w-7 h-7 rounded-lg flex items-center justify-center text-xs font-bold text-white flex-shrink-0"
                  style={{ backgroundColor: phase.color }}
                >
                  {phase.phase}
                </div>
                <h2 className="text-base font-bold text-white">{phase.title}</h2>
                <div className="flex-1 h-px bg-slate-800" />
              </div>
              {phase.steps.map(step => <StepCard key={step.id} step={step} />)}
            </div>
          ))}

        <div className="mt-8 pt-6 border-t border-slate-800 text-[10px] text-slate-600 flex justify-between font-mono">
          <span>7 Phases · 13 Steps · 6 Endpoints</span>
          <span>Routes Feature — BusBook v1.0</span>
        </div>
      </div>
    </div>
  );
}
