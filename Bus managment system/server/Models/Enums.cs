namespace server.Models;

public enum Gender
{
    MALE,
    FEMALE,
    OTHER
}

public enum UserRole
{
    USER,
    BUS_OPERATOR,
    ADMIN
}

public enum BusStatus
{
    ACTIVE,
    OUT_OF_SERVICE,
    DELETED
}

public enum SeatStatus
{
    AVAILABLE,
    FROZEN,
    BOOKED
}

public enum BookingStatus
{
    PENDING,
    CONFIRMED,
    CANCELLED_BY_USER,
    CANCELLED_BY_OPERATOR,
    CANCELLED_BY_SYSTEM
}

public enum RouteStatus
{
    PENDING_APPROVAL,
    PENDING_DELETION,
    APPROVED,
    REJECTED
}

public enum RefundStatus
{
    NOT_APPLICABLE,
    FULL,
    PARTIAL,
    NONE
}

public enum PaymentStatus
{
    PENDING,
    SUCCESS,
    FAILED,
    REFUNDED
}

public enum OperatorRequestStatus
{
    PENDING,
    APPROVED,
    REJECTED
}
