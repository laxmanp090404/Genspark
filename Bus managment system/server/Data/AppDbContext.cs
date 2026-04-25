using Microsoft.EntityFrameworkCore;
using server.Models;
using Route = server.Models.Route;

namespace server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PlatformConfig> PlatformConfigs => Set<PlatformConfig>();
    public DbSet<OperatorSwitchRequest> OperatorSwitchRequests => Set<OperatorSwitchRequest>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<BusSchedule> BusSchedules => Set<BusSchedule>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DiscountVoucher> DiscountVouchers => Set<DiscountVoucher>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        ConfigureUsers(modelBuilder);
        ConfigureRoutes(modelBuilder);
        ConfigureBuses(modelBuilder);
        ConfigureSchedulesAndSeats(modelBuilder);
        ConfigureBookingsAndPayments(modelBuilder);
        ConfigureAuxiliary(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.Gender).HasColumnName("gender").HasConversion<string>().IsRequired();
            entity.Property(e => e.Age).HasColumnName("age").IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>().IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Role)
                .HasDatabaseName("idx_one_admin")
                .IsUnique()
                .HasFilter("role = 'ADMIN'");
        });

        modelBuilder.Entity<PlatformConfig>(entity =>
        {
            entity.ToTable("platform_config");
            entity.HasKey(e => e.ConfigId);
            entity.Property(e => e.ConfigId).HasColumnName("config_id");
            entity.Property(e => e.PlatformFee).HasColumnName("platform_fee").HasColumnType("numeric(10,2)");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy);
        });

        modelBuilder.Entity<OperatorSwitchRequest>(entity =>
        {
            entity.ToTable("operator_switch_requests");
            entity.HasKey(e => e.RequestId);
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reviewer)
                .WithMany()
                .HasForeignKey(e => e.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(e => e.TokenId);
                entity.Property(e => e.TokenId).HasColumnName("token_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.TokenHash).HasColumnName("token_hash").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
                entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
                entity.Property(e => e.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash");
                entity.Property(e => e.IsRememberMe).HasColumnName("is_remember_me").HasDefaultValue(false);

                entity.HasIndex(e => e.TokenHash).IsUnique();
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });
    }

    private static void ConfigureRoutes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Route>(entity =>
        {
            entity.ToTable("routes");
            entity.HasKey(e => e.RouteId);
            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.Source).HasColumnName("source").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Destination).HasColumnName("destination").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Operator)
                .WithMany(u => u.Routes)
                .HasForeignKey(e => e.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Approver)
                .WithMany()
                .HasForeignKey(e => e.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.OperatorId, e.Source, e.Destination })
                .IsUnique()
                .HasDatabaseName("idx_unique_operator_route");
        });
    }

    private static void ConfigureBuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bus>(entity =>
        {
            entity.ToTable("buses");
            entity.HasKey(e => e.BusId);
            entity.Property(e => e.BusId).HasColumnName("bus_id");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.RegistrationNumber).HasColumnName("registration_number").HasMaxLength(20);
            entity.Property(e => e.DepartureTime).HasColumnName("departure_time");
            entity.Property(e => e.ArrivalTime).HasColumnName("arrival_time");
            entity.Property(e => e.SeatPrice).HasColumnName("seat_price").HasColumnType("numeric(10,2)");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.RegistrationNumber).IsUnique();

            entity.HasOne(e => e.Operator)
                .WithMany(u => u.Buses)
                .HasForeignKey(e => e.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Route)
                .WithMany(r => r.Buses)
                .HasForeignKey(e => e.RouteId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSchedulesAndSeats(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusSchedule>(entity =>
        {
            entity.ToTable("bus_schedules");
            entity.HasKey(e => e.ScheduleId);
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.BusId).HasColumnName("bus_id");
            entity.Property(e => e.TravelDate).HasColumnName("travel_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => new { e.BusId, e.TravelDate }).IsUnique();

            entity.HasOne(e => e.Bus)
                .WithMany(b => b.Schedules)
                .HasForeignKey(e => e.BusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.ToTable("seats");
            entity.HasKey(e => e.SeatId);
            entity.Property(e => e.SeatId).HasColumnName("seat_id");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.SeatNumber).HasColumnName("seat_number").HasMaxLength(5);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.FreezeExpiresAt).HasColumnName("freeze_expires_at");
            entity.Property(e => e.BookedByUserId).HasColumnName("booked_by_user_id");

            entity.HasIndex(e => new { e.ScheduleId, e.SeatNumber }).IsUnique();

            entity.HasOne(e => e.Schedule)
                .WithMany(s => s.Seats)
                .HasForeignKey(e => e.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BookedByUser)
                .WithMany()
                .HasForeignKey(e => e.BookedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBookingsAndPayments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("bookings");
            entity.HasKey(e => e.BookingId);
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(10,2)");
            entity.Property(e => e.PlatformFeeSnapshot).HasColumnName("platform_fee_snapshot").HasColumnType("numeric(10,2)");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.RefundStatus).HasColumnName("refund_status").HasConversion<string>();
            entity.Property(e => e.RefundAmount).HasColumnName("refund_amount").HasColumnType("numeric(10,2)");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Schedule)
                .WithMany(s => s.Bookings)
                .HasForeignKey(e => e.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingSeat>(entity =>
        {
            entity.ToTable("booking_seats");
            entity.HasKey(e => e.BookingSeatId);
            entity.Property(e => e.BookingSeatId).HasColumnName("booking_seat_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.SeatId).HasColumnName("seat_id");
            entity.Property(e => e.PassengerName).HasColumnName("passenger_name").HasMaxLength(150);
            entity.Property(e => e.PassengerAge).HasColumnName("passenger_age");
            entity.Property(e => e.PassengerGender).HasColumnName("passenger_gender").HasConversion<string>();
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false);

            entity.HasIndex(e => e.SeatId);

            entity.HasOne(e => e.Booking)
                .WithMany(b => b.BookingSeats)
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Seat)
                .WithMany(s => s.BookingSeats)
                .HasForeignKey(e => e.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("numeric(10,2)");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.PayerName).HasColumnName("payer_name").HasMaxLength(150);
            entity.Property(e => e.PayerEmail).HasColumnName("payer_email").HasMaxLength(255);
            entity.Property(e => e.TransactionRef).HasColumnName("transaction_ref").HasMaxLength(100);
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.BookingId).IsUnique();
            entity.HasIndex(e => e.TransactionRef).IsUnique();

            entity.HasOne(e => e.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(e => e.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAuxiliary(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.RecipientUserId).HasColumnName("recipient_user_id");
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(60);
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.SentAt).HasColumnName("sent_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.RelatedBookingId).HasColumnName("related_booking_id");

            entity.HasOne(e => e.RecipientUser)
                .WithMany()
                .HasForeignKey(e => e.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RelatedBooking)
                .WithMany()
                .HasForeignKey(e => e.RelatedBookingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DiscountVoucher>(entity =>
        {
            entity.ToTable("discount_vouchers");
            entity.HasKey(e => e.VoucherId);
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(20);
            entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent").HasColumnType("numeric(5,2)");
            entity.Property(e => e.IsUsed).HasColumnName("is_used").HasDefaultValue(false);
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Code).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
