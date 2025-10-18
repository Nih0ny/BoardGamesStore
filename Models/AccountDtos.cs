using System.ComponentModel.DataAnnotations;

namespace BoardGamesStore.Models;

public class RegisterDto
{
  [Required]
  [EmailAddress]
  public string Email { get; set; }

  [Required]
  [DataType(DataType.Text)]
  public string Name { get; set; }

  [Required]
  [DataType(DataType.Password)]
  public string Password { get; set; }

  [DataType(DataType.Password)]
  [Compare("Password", ErrorMessage = "Паролі не співпадають.")]
  public string ConfirmPassword { get; set; }
}

public class LoginDto
{
  [Required]
  [EmailAddress]
  public string Email { get; set; }

  [Required]
  [DataType(DataType.Password)]
  public string Password { get; set; }
}

public class ChangePasswordDto
{
  [Required]
  [DataType(DataType.Password)]
  public string CurrentPassword { get; set; }

  [Required]
  [DataType(DataType.Password)]
  public string NewPassword { get; set; }

  [DataType(DataType.Password)]
  [Compare("NewPassword", ErrorMessage = "Нові паролі не співпадають.")]
  public string ConfirmNewPassword { get; set; }
}