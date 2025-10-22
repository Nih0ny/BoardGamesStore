using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class RegisterDto
{
  [Required]
  [EmailAddress]
  public required string Email { get; set; }

  [Required]
  [DataType(DataType.Text)]
  public required string Name { get; set; }

  [Required]
  [DataType(DataType.Password)]
  public required string Password { get; set; }

  [DataType(DataType.Password)]
  [Compare("Password", ErrorMessage = "Passwords do not match.")]
  public required string ConfirmPassword { get; set; }

  [Required]
  [DataType(DataType.Text)]
  public required string ClientConfirmationUrl { get; set; }
}

public class LoginDto
{
  [Required]
  [EmailAddress]
  public required string Email { get; set; }

  [Required]
  [DataType(DataType.Password)]
  public required string Password { get; set; }
}

public class ChangePasswordDto
{
  [Required]
  [DataType(DataType.Password)]
  public required string CurrentPassword { get; set; }

  [Required]
  [DataType(DataType.Password)]
  public required string NewPassword { get; set; }

  [DataType(DataType.Password)]
  [Compare("NewPassword", ErrorMessage = "New passwords do not match.")]
  public required string ConfirmNewPassword { get; set; }
}

public class ForgotPasswordDto
{
  [Required]
  [EmailAddress]
  public required string Email { get; set; }

  [Required]
  [DataType(DataType.Text)]
  public required string ClientResetPasswordUrl { get; set; }
}

public class ResetPasswordDto
{
  public required string Email { get; set; }
  public required string Token { get; set; }
  public required string NewPassword { get; set; }

  [DataType(DataType.Password)]
  [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
  public required string ConfirmNewPassword { get; set; }
}