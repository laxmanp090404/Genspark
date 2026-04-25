namespace server.DTOs.Operators;

public class CreateOperatorRequestResponse
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
