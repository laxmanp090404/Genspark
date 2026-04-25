using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    gender = table.Column<string>(type: "text", nullable: false),
                    age = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "discount_vouchers",
                schema: "public",
                columns: table => new
                {
                    voucher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    discount_percent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discount_vouchers", x => x.voucher_id);
                    table.ForeignKey(
                        name: "FK_discount_vouchers_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "operator_switch_requests",
                schema: "public",
                columns: table => new
                {
                    request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operator_switch_requests", x => x.request_id);
                    table.ForeignKey(
                        name: "FK_operator_switch_requests_users_reviewed_by",
                        column: x => x.reviewed_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_operator_switch_requests_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "platform_config",
                schema: "public",
                columns: table => new
                {
                    config_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_fee = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_config", x => x.config_id);
                    table.ForeignKey(
                        name: "FK_platform_config_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "routes",
                schema: "public",
                columns: table => new
                {
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    destination = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routes", x => x.route_id);
                    table.ForeignKey(
                        name: "FK_routes_users_approved_by",
                        column: x => x.approved_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_routes_users_operator_id",
                        column: x => x.operator_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "buses",
                schema: "public",
                columns: table => new
                {
                    bus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    departure_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    arrival_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    seat_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buses", x => x.bus_id);
                    table.ForeignKey(
                        name: "FK_buses_routes_route_id",
                        column: x => x.route_id,
                        principalSchema: "public",
                        principalTable: "routes",
                        principalColumn: "route_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_buses_users_operator_id",
                        column: x => x.operator_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bus_schedules",
                schema: "public",
                columns: table => new
                {
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bus_id = table.Column<Guid>(type: "uuid", nullable: false),
                    travel_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bus_schedules", x => x.schedule_id);
                    table.ForeignKey(
                        name: "FK_bus_schedules_buses_bus_id",
                        column: x => x.bus_id,
                        principalSchema: "public",
                        principalTable: "buses",
                        principalColumn: "bus_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "public",
                columns: table => new
                {
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    platform_fee_snapshot = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    refund_status = table.Column<string>(type: "text", nullable: false),
                    refund_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK_bookings_bus_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalSchema: "public",
                        principalTable: "bus_schedules",
                        principalColumn: "schedule_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bookings_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "seats",
                schema: "public",
                columns: table => new
                {
                    seat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_number = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    freeze_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    booked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seats", x => x.seat_id);
                    table.ForeignKey(
                        name: "FK_seats_bus_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalSchema: "public",
                        principalTable: "bus_schedules",
                        principalColumn: "schedule_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_seats_users_booked_by_user_id",
                        column: x => x.booked_by_user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "public",
                columns: table => new
                {
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    subject = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    related_booking_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_notifications_bookings_related_booking_id",
                        column: x => x.related_booking_id,
                        principalSchema: "public",
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_users_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                schema: "public",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    payer_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    payer_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    transaction_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_payments_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "public",
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "booking_seats",
                schema: "public",
                columns: table => new
                {
                    booking_seat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    passenger_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    passenger_age = table.Column<int>(type: "integer", nullable: false),
                    passenger_gender = table.Column<string>(type: "text", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_seats", x => x.booking_seat_id);
                    table.ForeignKey(
                        name: "FK_booking_seats_bookings_booking_id",
                        column: x => x.booking_id,
                        principalSchema: "public",
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_booking_seats_seats_seat_id",
                        column: x => x.seat_id,
                        principalSchema: "public",
                        principalTable: "seats",
                        principalColumn: "seat_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_booking_seats_booking_id",
                schema: "public",
                table: "booking_seats",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_seats_seat_id",
                schema: "public",
                table: "booking_seats",
                column: "seat_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bookings_schedule_id",
                schema: "public",
                table: "bookings",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_user_id",
                schema: "public",
                table: "bookings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_bus_schedules_bus_id_travel_date",
                schema: "public",
                table: "bus_schedules",
                columns: new[] { "bus_id", "travel_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_buses_operator_id",
                schema: "public",
                table: "buses",
                column: "operator_id");

            migrationBuilder.CreateIndex(
                name: "IX_buses_registration_number",
                schema: "public",
                table: "buses",
                column: "registration_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_buses_route_id",
                schema: "public",
                table: "buses",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_discount_vouchers_code",
                schema: "public",
                table: "discount_vouchers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_discount_vouchers_user_id",
                schema: "public",
                table: "discount_vouchers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_user_id",
                schema: "public",
                table: "notifications",
                column: "recipient_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_related_booking_id",
                schema: "public",
                table: "notifications",
                column: "related_booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_operator_switch_requests_reviewed_by",
                schema: "public",
                table: "operator_switch_requests",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_operator_switch_requests_user_id",
                schema: "public",
                table: "operator_switch_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_booking_id",
                schema: "public",
                table: "payments",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_transaction_ref",
                schema: "public",
                table: "payments",
                column: "transaction_ref",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_config_updated_by",
                schema: "public",
                table: "platform_config",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "idx_unique_operator_route",
                schema: "public",
                table: "routes",
                columns: new[] { "operator_id", "source", "destination" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routes_approved_by",
                schema: "public",
                table: "routes",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_seats_booked_by_user_id",
                schema: "public",
                table: "seats",
                column: "booked_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_seats_schedule_id_seat_number",
                schema: "public",
                table: "seats",
                columns: new[] { "schedule_id", "seat_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_one_admin",
                schema: "public",
                table: "users",
                column: "role",
                unique: true,
                filter: "role = 'ADMIN'");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "public",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                schema: "public",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_seats",
                schema: "public");

            migrationBuilder.DropTable(
                name: "discount_vouchers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "operator_switch_requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "payments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "platform_config",
                schema: "public");

            migrationBuilder.DropTable(
                name: "seats",
                schema: "public");

            migrationBuilder.DropTable(
                name: "bookings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "bus_schedules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "buses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "routes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}
