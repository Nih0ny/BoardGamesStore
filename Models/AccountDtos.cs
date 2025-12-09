using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class RegisterDto
{
  [Required]
  [EmailAddress]
  public string Email { get; init; } = null!;

  [Required]
  [DataType(DataType.Text)]
  public string Name { get; init; } = null!;

  [Required]
  [DataType(DataType.Password)]
  public string Password { get; init; } = null!;

  [Required]
  [DataType(DataType.Password)]
  public string ConfirmPassword { get; init; } = null!;

  [Required]
  [DataType(DataType.Text)]
  public string ClientConfirmationUrl { get; init; } = null!;
}

public class LoginDto
{
  [Required]
  [EmailAddress]
  public string Email { get; init; } = null!;

  [Required]
  [DataType(DataType.Password)]
  public string Password { get; init; } = null!;
}

public class ChangePasswordDto
{
  [Required]
  [DataType(DataType.Password)]
  public string CurrentPassword { get; init; } = null!;

  [Required]
  [DataType(DataType.Password)]
  public string NewPassword { get; init; } = null!;

  [Required]
  [DataType(DataType.Password)]
  public string ConfirmNewPassword { get; init; } = null!;
}

public class ForgotPasswordDto
{
  [Required]
  [EmailAddress]
  public string Email { get; init; } = null!;

  [Required]
  [DataType(DataType.Text)]
  public string ClientResetPasswordUrl { get; init; } = null!;
}

public class ResetPasswordDto
{
  public string Email { get; init; } = null!;
  public string Token { get; init; } = null!;
  public string NewPassword { get; init; } = null!;
  public string ConfirmNewPassword { get; init; } = null!;
}