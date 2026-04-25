using System.ComponentModel.DataAnnotations;
using server.Models;

namespace server.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public Gender Gender { get; set; }

    [Range(1, 120)]
    public int Age { get; set; }

    public UserRole Role { get; set; } = UserRole.USER;
}
