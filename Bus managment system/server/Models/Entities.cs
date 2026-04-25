namespace server.Models;

public class User
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public UserRole Role { get; set; } = UserRole.USER;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Route> Routes { get; set; } = new List<Route>();
    public ICollection<Bus> Buses { get; set; } = new List<Bus>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class RefreshToken
{
    public Guid TokenId { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public bool IsRememberMe { get; set; }

    public User User { get; set; } = null!;
}

public class PlatformConfig
{
    public Guid ConfigId { get; set; }
    public decimal PlatformFee { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? UpdatedByUser { get; set; }
}

public class OperatorSwitchRequest
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public OperatorRequestStatus Status { get; set; } = OperatorRequestStatus.PENDING;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public User? Reviewer { get; set; }
}

public class Route
{
    public Guid RouteId { get; set; }
    public Guid OperatorId { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public RouteStatus Status { get; set; } = RouteStatus.PENDING_APPROVAL;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User Operator { get; set; } = null!;
    public User? Approver { get; set; }
    public ICollection<Bus> Buses { get; set; } = new List<Bus>();
}

public class Bus
{
    public Guid BusId { get; set; }
    public Guid OperatorId { get; set; }
    public Guid RouteId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public decimal SeatPrice { get; set; }
    public BusStatus Status { get; set; } = BusStatus.ACTIVE;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User Operator { get; set; } = null!;
    public Route Route { get; set; } = null!;
    public ICollection<BusSchedule> Schedules { get; set; } = new List<BusSchedule>();
}

public class BusSchedule
{
    public Guid ScheduleId { get; set; }
    public Guid BusId { get; set; }
    public DateOnly TravelDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Bus Bus { get; set; } = null!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class Seat
{
    public Guid SeatId { get; set; }
    public Guid ScheduleId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public SeatStatus Status { get; set; } = SeatStatus.AVAILABLE;
    public DateTime? FreezeExpiresAt { get; set; }
    public Guid? BookedByUserId { get; set; }

    public BusSchedule Schedule { get; set; } = null!;
    public User? BookedByUser { get; set; }
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
}

public class Booking
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid ScheduleId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PlatformFeeSnapshot { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.PENDING;
    public RefundStatus RefundStatus { get; set; } = RefundStatus.NOT_APPLICABLE;
    public decimal? RefundAmount { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public BusSchedule Schedule { get; set; } = null!;
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    public Payment? Payment { get; set; }
}

public class BookingSeat
{
    public Guid BookingSeatId { get; set; }
    public Guid BookingId { get; set; }
    public Guid SeatId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public int PassengerAge { get; set; }
    public Gender PassengerGender { get; set; }
    public bool IsPrimary { get; set; }

    public Booking Booking { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
}

public class Payment
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;
    public string PayerName { get; set; } = string.Empty;
    public string PayerEmail { get; set; } = string.Empty;
    public string? TransactionRef { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Booking Booking { get; set; } = null!;
}

public class Notification
{
    public Guid NotificationId { get; set; }
    public Guid RecipientUserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public Guid? RelatedBookingId { get; set; }

    public User RecipientUser { get; set; } = null!;
    public Booking? RelatedBooking { get; set; }
}

public class DiscountVoucher
{
    public Guid VoucherId { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; } = 10.00m;
    public bool IsUsed { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
