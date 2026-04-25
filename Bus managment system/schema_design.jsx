import { useState } from "react";

const schema = {
  enums: [
    {
      name: "gender",
      values: ["MALE", "FEMALE", "OTHER"],
    },
    {
      name: "user_role",
      values: ["USER", "BUS_OPERATOR", "ADMIN"],
    },
    {
      name: "bus_status",
      values: ["ACTIVE", "OUT_OF_SERVICE", "DELETED"],
    },
    {
      name: "seat_status",
      values: ["AVAILABLE", "FROZEN", "BOOKED"],
    },
    {
      name: "booking_status",
      values: ["PENDING", "CONFIRMED", "CANCELLED_BY_USER", "CANCELLED_BY_OPERATOR"],
    },
    {
      name: "route_status",
      values: ["PENDING_APPROVAL", "APPROVED", "REJECTED"],
    },
    {
      name: "refund_status",
      values: ["NOT_APPLICABLE", "FULL", "PARTIAL", "NONE"],
    },
    {
      name: "payment_status",
      values: ["PENDING", "SUCCESS", "FAILED", "REFUNDED"],
    },
    {
      name: "operator_request_status",
      values: ["PENDING", "APPROVED", "REJECTED"],
    },
  ],
  tables: [
    {
      name: "users",
      color: "#4f46e5",
      description: "All system actors — passengers, operators, admins",
      columns: [
        { name: "user_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "username", type: "VARCHAR(100)", constraints: ["NOT NULL", "UNIQUE"] },
        { name: "email", type: "VARCHAR(255)", constraints: ["NOT NULL", "UNIQUE"] },
        { name: "password_hash", type: "TEXT", constraints: ["NOT NULL"] },
        { name: "gender", type: "gender (enum)", constraints: ["NOT NULL"] },
        { name: "age", type: "INT", constraints: ["NOT NULL", "CHECK (age >= 1 AND age <= 120)"] },
        { name: "role", type: "user_role (enum)", constraints: ["NOT NULL", "DEFAULT 'USER'"] },
        { name: "is_active", type: "BOOLEAN", constraints: ["DEFAULT TRUE"] },
        { name: "created_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
        { name: "updated_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
      ],
      notes: "Passwords must be hashed (bcrypt/Argon2) before insert. Role-based access enforced at API layer.",
    },
    {
      name: "platform_config",
      color: "#0891b2",
      description: "Admin-controlled global settings like platform fee",
      columns: [
        { name: "config_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "platform_fee", type: "NUMERIC(10,2)", constraints: ["NOT NULL", "CHECK (platform_fee >= 0)"] },
        { name: "updated_by", type: "UUID", constraints: ["FK → users.user_id"] },
        { name: "updated_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
      ],
      notes: "Only admins can write to this table. Single active row; new changes insert a new row for audit trail.",
    },
    {
      name: "operator_switch_requests",
      color: "#0891b2",
      description: "User requests to become a bus operator",
      columns: [
        { name: "request_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "user_id", type: "UUID", constraints: ["FK → users.user_id", "NOT NULL"] },
        { name: "status", type: "operator_request_status", constraints: ["DEFAULT 'PENDING'"] },
        { name: "reviewed_by", type: "UUID", constraints: ["FK → users.user_id", "NULLABLE"] },
        { name: "reviewed_at", type: "TIMESTAMPTZ", constraints: ["NULLABLE"] },
        { name: "created_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
      ],
      notes: "Admin approves/rejects. On approval, users.role is updated to BUS_OPERATOR and email sent.",
    },
    {
      name: "routes",
      color: "#059669",
      description: "Routes defined by operators, approved by admin",
      columns: [
        { name: "route_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "operator_id", type: "UUID", constraints: ["FK → users.user_id", "NOT NULL"] },
        { name: "source", type: "VARCHAR(150)", constraints: ["NOT NULL"] },
        { name: "destination", type: "VARCHAR(150)", constraints: ["NOT NULL"] },
        { name: "status", type: "route_status (enum)", constraints: ["DEFAULT 'PENDING_APPROVAL'"] },
        { name: "approved_by", type: "UUID", constraints: ["FK → users.user_id", "NULLABLE"] },
        { name: "approved_at", type: "TIMESTAMPTZ", constraints: ["NULLABLE"] },
        { name: "created_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
        { name: "updated_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
      ],
      notes: "UNIQUE(operator_id, source, destination). Admin must approve before buses can be assigned.",
    },
    {
      name: "buses",
      color: "#d97706",
      description: "Bus assets owned by operators, linked to approved routes",
      columns: [
        { name: "bus_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "operator_id", type: "UUID", constraints: ["FK → users.user_id", "NOT NULL"] },
        { name: "route_id", type: "UUID", constraints: ["FK → routes.route_id", "NOT NULL"] },
        { name: "registration_number", type: "VARCHAR(20)", constraints: ["NOT NULL", "UNIQUE"] },
        { name: "departure_time", type: "TIMETZ", constraints: ["NOT NULL"] },
        { name: "arrival_time", type: "TIMETZ", constraints: ["NOT NULL"] },
        { name: "seat_price", type: "NUMERIC(10,2)", constraints: ["NOT NULL", "CHECK (seat_price > 0)"] },
        { name: "status", type: "bus_status (enum)", constraints: ["DEFAULT 'ACTIVE'"] },
        { name: "created_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
        { name: "updated_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
      ],
      notes: "Operator can set OUT_OF_SERVICE or DELETED without admin approval. Route must be APPROVED before bus creation.",
    },
    {
      name: "bus_schedules",
      color: "#d97706",
      description: "Daily travel instances of a bus on its route",
      columns: [
        { name: "schedule_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "bus_id", type: "UUID", constraints: ["FK → buses.bus_id", "NOT NULL"] },
        { name: "travel_date", type: "DATE", constraints: ["NOT NULL"] },
        { name: "created_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
      ],
      notes: "UNIQUE(bus_id, travel_date). Enables seat availability per day. Schedules auto-generate or are created on demand.",
    },
    {
      name: "seats",
      color: "#7c3aed",
      description: "Fixed 40-seat layout per bus; instantiated per schedule",
      columns: [
        { name: "seat_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "schedule_id", type: "UUID", constraints: ["FK → bus_schedules.schedule_id", "NOT NULL"] },
        { name: "seat_number", type: "VARCHAR(5)", constraints: ["NOT NULL"] },
        { name: "status", type: "seat_status (enum)", constraints: ["DEFAULT 'AVAILABLE'"] },
        { name: "freeze_expires_at", type: "TIMESTAMPTZ", constraints: ["NULLABLE"] },
        { name: "booked_by_user_id", type: "UUID", constraints: ["FK → users.user_id", "NULLABLE"] },
      ],
      notes: "UNIQUE(schedule_id, seat_number). freeze_expires_at set to NOW()+5min on freeze. Cron/trigger reverts FROZEN → AVAILABLE on expiry.",
    },
    {
      name: "bookings",
      color: "#dc2626",
      description: "A single booking transaction covering one or more seats",
      columns: [
        { name: "booking_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "user_id", type: "UUID", constraints: ["FK → users.user_id", "NOT NULL"] },
        { name: "schedule_id", type: "UUID", constraints: ["FK → bus_schedules.schedule_id", "NOT NULL"] },
        { name: "total_amount", type: "NUMERIC(10,2)", constraints: ["NOT NULL"] },
        { name: "platform_fee_snapshot", type: "NUMERIC(10,2)", constraints: ["NOT NULL"] },
        { name: "status", type: "booking_status (enum)", constraints: ["DEFAULT 'PENDING'"] },
        { name: "refund_status", type: "refund_status (enum)", constraints: ["DEFAULT 'NOT_APPLICABLE'"] },
        { name: "refund_amount", type: "NUMERIC(10,2)", constraints: ["NULLABLE"] },
        { name: "cancelled_at", type: "TIMESTAMPTZ", constraints: ["NULLABLE"] },
        { name: "created_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
        { name: "updated_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
      ],
      notes: "total_amount = (seat_price × seat_count) + platform_fee_snapshot. Refund rules: >48h full, 24–48h 50%, <24h 0%.",
    },
    {
      name: "booking_seats",
      color: "#dc2626",
      description: "Junction: which seats belong to a booking, with passenger info",
      columns: [
        { name: "booking_seat_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "booking_id", type: "UUID", constraints: ["FK → bookings.booking_id", "NOT NULL"] },
        { name: "seat_id", type: "UUID", constraints: ["FK → seats.seat_id", "NOT NULL"] },
        { name: "passenger_name", type: "VARCHAR(150)", constraints: ["NOT NULL"] },
        { name: "passenger_age", type: "INT", constraints: ["NOT NULL", "CHECK (passenger_age >= 1)"] },
        { name: "passenger_gender", type: "gender (enum)", constraints: ["NOT NULL"] },
        { name: "is_primary", type: "BOOLEAN", constraints: ["DEFAULT FALSE"] },
      ],
      notes: "UNIQUE(seat_id). is_primary = TRUE for the booking user's own seat. Additional passengers entered manually.",
    },
    {
      name: "payments",
      color: "#be185d",
      description: "Mock payment records per booking",
      columns: [
        { name: "payment_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "booking_id", type: "UUID", constraints: ["FK → bookings.booking_id", "UNIQUE", "NOT NULL"] },
        { name: "amount", type: "NUMERIC(10,2)", constraints: ["NOT NULL"] },
        { name: "status", type: "payment_status (enum)", constraints: ["DEFAULT 'PENDING'"] },
        { name: "payer_name", type: "VARCHAR(150)", constraints: ["NOT NULL"] },
        { name: "payer_email", type: "VARCHAR(255)", constraints: ["NOT NULL"] },
        { name: "transaction_ref", type: "VARCHAR(100)", constraints: ["UNIQUE", "NULLABLE"] },
        { name: "paid_at", type: "TIMESTAMPTZ", constraints: ["NULLABLE"] },
        { name: "created_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
      ],
      notes: "Mock: no real gateway. transaction_ref = generated UUID on success. Email sent on SUCCESS and REFUNDED.",
    },
    {
      name: "notifications",
      color: "#475569",
      description: "Audit log of all outbound emails",
      columns: [
        { name: "notification_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "recipient_user_id", type: "UUID", constraints: ["FK → users.user_id", "NOT NULL"] },
        { name: "type", type: "VARCHAR(60)", constraints: ["NOT NULL"] },
        { name: "subject", type: "TEXT", constraints: ["NOT NULL"] },
        { name: "body", type: "TEXT", constraints: ["NOT NULL"] },
        { name: "sent_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
        { name: "related_booking_id", type: "UUID", constraints: ["FK → bookings.booking_id", "NULLABLE"] },
      ],
      notes: "Types: BOOKING_CONFIRM, BOOKING_CANCEL_USER, BOOKING_CANCEL_OPERATOR, PAYMENT_SUCCESS, REFUND_ISSUED, OPERATOR_APPROVED, DISCOUNT_VOUCHER.",
    },
    {
      name: "discount_vouchers",
      color: "#475569",
      description: "10% discount coupons issued when operator cancels a trip",
      columns: [
        { name: "voucher_id", type: "UUID", constraints: ["PK", "DEFAULT gen_random_uuid()"] },
        { name: "user_id", type: "UUID", constraints: ["FK → users.user_id", "NOT NULL"] },
        { name: "code", type: "VARCHAR(20)", constraints: ["UNIQUE", "NOT NULL"] },
        { name: "discount_percent", type: "NUMERIC(5,2)", constraints: ["DEFAULT 10.00"] },
        { name: "is_used", type: "BOOLEAN", constraints: ["DEFAULT FALSE"] },
        { name: "expires_at", type: "TIMESTAMPTZ", constraints: ["NOT NULL"] },
        { name: "created_at", type: "TIMESTAMPTZ", constraints: ["DEFAULT NOW()"] },
      ],
      notes: "Issued automatically when booking status = CANCELLED_BY_OPERATOR. Applied at payment calculation.",
    },
  ],
  relationships: [
    { from: "users", to: "routes", label: "operator creates (1:N)", color: "#059669" },
    { from: "users", to: "buses", label: "operator owns (1:N)", color: "#d97706" },
    { from: "routes", to: "buses", label: "route hosts (1:N)", color: "#d97706" },
    { from: "buses", to: "bus_schedules", label: "bus has schedules (1:N)", color: "#d97706" },
    { from: "bus_schedules", to: "seats", label: "schedule has 40 seats (1:N)", color: "#7c3aed" },
    { from: "users", to: "bookings", label: "user makes (1:N)", color: "#dc2626" },
    { from: "bus_schedules", to: "bookings", label: "schedule has bookings (1:N)", color: "#dc2626" },
    { from: "bookings", to: "booking_seats", label: "booking has seats (1:N)", color: "#dc2626" },
    { from: "seats", to: "booking_seats", label: "seat in booking (1:1)", color: "#dc2626" },
    { from: "bookings", to: "payments", label: "booking has payment (1:1)", color: "#be185d" },
    { from: "users", to: "operator_switch_requests", label: "user requests role (1:N)", color: "#0891b2" },
    { from: "users", to: "notifications", label: "user receives (1:N)", color: "#475569" },
    { from: "users", to: "discount_vouchers", label: "user holds (1:N)", color: "#475569" },
    { from: "users", to: "platform_config", label: "admin manages (1:N)", color: "#0891b2" },
  ],
};

const colorMap = {
  "PK": "bg-yellow-900/40 text-yellow-300 border border-yellow-700/50",
  "FK": "bg-blue-900/40 text-blue-300 border border-blue-700/50",
  "NOT NULL": "bg-red-900/30 text-red-300 border border-red-700/40",
  "UNIQUE": "bg-purple-900/40 text-purple-300 border border-purple-700/50",
  "DEFAULT": "bg-green-900/30 text-green-300 border border-green-700/40",
  "CHECK": "bg-orange-900/30 text-orange-300 border border-orange-700/40",
  "NULLABLE": "bg-slate-700/40 text-slate-400 border border-slate-600/40",
};

function getConstraintStyle(c) {
  if (c.startsWith("PK")) return colorMap["PK"];
  if (c.startsWith("FK")) return colorMap["FK"];
  if (c === "NOT NULL") return colorMap["NOT NULL"];
  if (c === "UNIQUE") return colorMap["UNIQUE"];
  if (c.startsWith("DEFAULT")) return colorMap["DEFAULT"];
  if (c.startsWith("CHECK")) return colorMap["CHECK"];
  if (c === "NULLABLE") return colorMap["NULLABLE"];
  return "bg-slate-700/30 text-slate-400 border border-slate-600/30";
}

function TableCard({ table, isOpen, onToggle }) {
  return (
    <div
      className="rounded-xl overflow-hidden border border-slate-700/60 shadow-lg shadow-black/30"
      style={{ borderTopColor: table.color, borderTopWidth: 3 }}
    >
      <button
        onClick={onToggle}
        className="w-full flex items-center justify-between px-5 py-4 bg-slate-800/80 hover:bg-slate-800 transition-colors"
      >
        <div className="flex items-center gap-3">
          <span
            className="w-3 h-3 rounded-full flex-shrink-0"
            style={{ backgroundColor: table.color }}
          />
          <span className="font-mono font-bold text-white text-sm tracking-wide">
            {table.name}
          </span>
          <span className="text-xs text-slate-500 font-normal hidden sm:block">
            — {table.description}
          </span>
        </div>
        <span className="text-slate-400 text-lg">{isOpen ? "▲" : "▼"}</span>
      </button>

      {isOpen && (
        <div className="bg-slate-900/60">
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-slate-700/60">
                  <th className="text-left px-4 py-2 text-slate-400 font-semibold uppercase tracking-wider w-40">Column</th>
                  <th className="text-left px-4 py-2 text-slate-400 font-semibold uppercase tracking-wider w-36">Type</th>
                  <th className="text-left px-4 py-2 text-slate-400 font-semibold uppercase tracking-wider">Constraints</th>
                </tr>
              </thead>
              <tbody>
                {table.columns.map((col, i) => (
                  <tr
                    key={i}
                    className="border-b border-slate-800/60 hover:bg-slate-800/30 transition-colors"
                  >
                    <td className="px-4 py-2 font-mono text-white font-medium">{col.name}</td>
                    <td className="px-4 py-2 font-mono text-cyan-300/80">{col.type}</td>
                    <td className="px-4 py-2">
                      <div className="flex flex-wrap gap-1">
                        {col.constraints.map((c, j) => (
                          <span
                            key={j}
                            className={`px-1.5 py-0.5 rounded text-[10px] font-mono ${getConstraintStyle(c)}`}
                          >
                            {c}
                          </span>
                        ))}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="px-4 py-3 border-t border-slate-700/40 bg-slate-800/20">
            <p className="text-slate-400 text-xs">
              <span className="text-slate-500 font-semibold">📌 Note: </span>
              {table.notes}
            </p>
          </div>
        </div>
      )}
    </div>
  );
}

export default function SchemaDesign() {
  const [openTables, setOpenTables] = useState(
    Object.fromEntries(schema.tables.map((t) => [t.name, true]))
  );
  const [activeTab, setActiveTab] = useState("tables");

  const toggleTable = (name) =>
    setOpenTables((prev) => ({ ...prev, [name]: !prev[name] }));

  const expandAll = () =>
    setOpenTables(Object.fromEntries(schema.tables.map((t) => [t.name, true])));
  const collapseAll = () =>
    setOpenTables(Object.fromEntries(schema.tables.map((t) => [t.name, false])));

  return (
    <div
      className="min-h-screen bg-slate-950 text-white"
      style={{
        fontFamily: "'IBM Plex Mono', 'Fira Code', monospace",
        backgroundImage:
          "radial-gradient(ellipse at 20% 10%, rgba(79,70,229,0.07) 0%, transparent 60%), radial-gradient(ellipse at 80% 80%, rgba(8,145,178,0.07) 0%, transparent 60%)",
      }}
    >
      <link
        href="https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:wght@300;400;500;600;700&display=swap"
        rel="stylesheet"
      />

      {/* Header */}
      <div className="border-b border-slate-800 px-6 py-5 bg-slate-900/50 backdrop-blur sticky top-0 z-20">
        <div className="max-w-5xl mx-auto flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
          <div>
            <div className="flex items-center gap-2 mb-1">
              <span className="text-indigo-400 text-lg">⬡</span>
              <h1 className="text-lg font-bold text-white tracking-tight">
                BusBook — PostgreSQL Schema
              </h1>
              <span className="px-2 py-0.5 rounded-full text-[10px] bg-indigo-900/60 text-indigo-300 border border-indigo-700/50 font-semibold">
                v1.0
              </span>
            </div>
            <p className="text-slate-500 text-xs">
              AngularJS · .NET WebAPI · PostgreSQL 16
            </p>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={expandAll}
              className="px-3 py-1.5 text-xs rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300 border border-slate-700 transition"
            >
              Expand All
            </button>
            <button
              onClick={collapseAll}
              className="px-3 py-1.5 text-xs rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300 border border-slate-700 transition"
            >
              Collapse All
            </button>
          </div>
        </div>
      </div>

      <div className="max-w-5xl mx-auto px-4 py-8">
        {/* Tabs */}
        <div className="flex gap-1 mb-6 bg-slate-900/50 p-1 rounded-xl w-fit border border-slate-800">
          {["tables", "enums", "relationships", "logic"].map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`px-4 py-1.5 rounded-lg text-xs font-semibold uppercase tracking-widest transition-all ${
                activeTab === tab
                  ? "bg-indigo-600 text-white shadow"
                  : "text-slate-500 hover:text-slate-300"
              }`}
            >
              {tab}
            </button>
          ))}
        </div>

        {/* Tables Tab */}
        {activeTab === "tables" && (
          <div className="space-y-3">
            {/* Legend */}
            <div className="flex flex-wrap gap-2 mb-4 p-3 bg-slate-900/50 rounded-xl border border-slate-800">
              <span className="text-slate-500 text-xs mr-1 self-center">Legend:</span>
              {Object.entries(colorMap).map(([k, v]) => (
                <span key={k} className={`px-2 py-0.5 rounded text-[10px] font-mono ${v}`}>
                  {k}
                </span>
              ))}
            </div>
            {schema.tables.map((table) => (
              <TableCard
                key={table.name}
                table={table}
                isOpen={openTables[table.name]}
                onToggle={() => toggleTable(table.name)}
              />
            ))}
          </div>
        )}

        {/* Enums Tab */}
        {activeTab === "enums" && (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {schema.enums.map((e) => (
              <div
                key={e.name}
                className="rounded-xl border border-slate-700/60 bg-slate-900/50 overflow-hidden"
              >
                <div className="px-4 py-3 bg-slate-800/60 border-b border-slate-700/40">
                  <span className="font-mono font-bold text-cyan-300 text-sm">
                    TYPE {e.name}
                  </span>
                </div>
                <div className="px-4 py-3 flex flex-wrap gap-2">
                  {e.values.map((v) => (
                    <span
                      key={v}
                      className="px-2 py-1 rounded-lg text-xs font-mono bg-slate-800 text-emerald-300 border border-emerald-900/50"
                    >
                      '{v}'
                    </span>
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Relationships Tab */}
        {activeTab === "relationships" && (
          <div className="space-y-2">
            <p className="text-slate-500 text-xs mb-4">
              All foreign keys enforce referential integrity. ON DELETE behavior should be set to RESTRICT by default unless stated otherwise.
            </p>
            {schema.relationships.map((r, i) => (
              <div
                key={i}
                className="flex items-center gap-3 px-4 py-3 rounded-xl bg-slate-900/50 border border-slate-800 hover:border-slate-700 transition"
              >
                <span
                  className="font-mono text-sm font-bold"
                  style={{ color: r.color }}
                >
                  {r.from}
                </span>
                <span className="text-slate-600 text-xs">──</span>
                <span className="text-slate-400 text-xs italic">{r.label}</span>
                <span className="text-slate-600 text-xs">──▶</span>
                <span
                  className="font-mono text-sm font-bold"
                  style={{ color: r.color }}
                >
                  {r.to}
                </span>
              </div>
            ))}
          </div>
        )}

        {/* Logic Tab */}
        {activeTab === "logic" && (
          <div className="space-y-4">
            {[
              {
                title: "🔐 Authentication & Security",
                color: "#4f46e5",
                items: [
                  "JWT-based authentication; role stored in token claims (USER, BUS_OPERATOR, ADMIN).",
                  "passwords stored as bcrypt/Argon2 hashes — never plaintext.",
                  "All non-public API endpoints verify JWT and check role against required permission.",
                ],
              },
              {
                title: "🧊 Seat Freeze Logic",
                color: "#7c3aed",
                items: [
                  "On seat selection: UPDATE seats SET status='FROZEN', freeze_expires_at=NOW()+interval'5 minutes' WHERE seat_id=X AND status='AVAILABLE'.",
                  "Background job (or pg_cron) runs every minute: UPDATE seats SET status='AVAILABLE', freeze_expires_at=NULL WHERE status='FROZEN' AND freeze_expires_at < NOW().",
                  "Front-end polls seat status; frozen seats shown as disabled for all other users.",
                  "On payment failure: seats belonging to that booking_id are reverted to AVAILABLE immediately.",
                ],
              },
              {
                title: "🔍 Fuzzy Bus Search",
                color: "#059669",
                items: [
                  "Use PostgreSQL pg_trgm extension: CREATE INDEX ON routes USING gin(source gin_trgm_ops).",
                  "Query: SELECT ... FROM buses b JOIN routes r ON ... WHERE similarity(r.source, $input) > 0.3 ORDER BY similarity DESC.",
                  "Filter by travel_date matching bus_schedules.travel_date.",
                  "Return: bus_id, registration_number, source, destination, departure_time, arrival_time, available_seat_count.",
                ],
              },
              {
                title: "💳 Payment Flow",
                color: "#be185d",
                items: [
                  "Amount = (seat_price × seat_count) + platform_fee_snapshot (read from latest platform_config at booking time).",
                  "payment_status transitions: PENDING → SUCCESS (email + PDF triggered) or FAILED (seats unfrozen).",
                  "PDF generation triggered server-side on SUCCESS; emailed as attachment.",
                  "Discount voucher code accepted at payment: reduce total by voucher.discount_percent, mark voucher is_used=TRUE.",
                ],
              },
              {
                title: "❌ Cancellation & Refund Logic",
                color: "#dc2626",
                items: [
                  "User cancel >48h before departure: refund_status=FULL, refund_amount=total_amount.",
                  "User cancel 24–48h before departure: refund_status=PARTIAL, refund_amount=total_amount × 0.5.",
                  "User cancel <24h before departure: refund_status=NONE, refund_amount=0.",
                  "Operator cancels: refund_status=FULL for all affected bookings; generate discount_voucher (10%) for each affected user; send apology email to user + confirmation to operator.",
                  "All cancellations: email sent to both user and operator.",
                ],
              },
              {
                title: "📧 Notification Triggers",
                color: "#475569",
                items: [
                  "BOOKING_CONFIRM — on payment SUCCESS.",
                  "PAYMENT_SUCCESS — on payment SUCCESS (includes PDF attachment).",
                  "BOOKING_CANCEL_USER — on user-initiated cancellation.",
                  "BOOKING_CANCEL_OPERATOR — on operator-initiated cancellation (to user).",
                  "REFUND_ISSUED — when refund_amount > 0.",
                  "DISCOUNT_VOUCHER — when operator cancels (10% code emailed to affected users).",
                  "OPERATOR_APPROVED / OPERATOR_REJECTED — on admin action on operator_switch_requests.",
                  "ROUTE_APPROVED / ROUTE_REJECTED — on admin action on routes.",
                ],
              },
            ].map((section) => (
              <div
                key={section.title}
                className="rounded-xl border border-slate-800 bg-slate-900/50 overflow-hidden"
                style={{ borderLeftColor: section.color, borderLeftWidth: 3 }}
              >
                <div className="px-5 py-3 bg-slate-800/40 border-b border-slate-800">
                  <h3 className="font-bold text-white text-sm">{section.title}</h3>
                </div>
                <ul className="px-5 py-3 space-y-2">
                  {section.items.map((item, i) => (
                    <li key={i} className="flex gap-2 text-xs text-slate-300">
                      <span style={{ color: section.color }} className="flex-shrink-0 mt-0.5">▸</span>
                      <span className="font-mono leading-relaxed">{item}</span>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        )}

        {/* Footer */}
        <div className="mt-10 pt-6 border-t border-slate-800 flex flex-wrap gap-4 justify-between text-[10px] text-slate-600 font-mono">
          <span>Tables: {schema.tables.length} · Enums: {schema.enums.length} · Relationships: {schema.relationships.length}</span>
          <span>Stack: AngularJS · .NET WebAPI · PostgreSQL 16 · pg_trgm · pg_cron</span>
        </div>
      </div>
    </div>
  );
}
