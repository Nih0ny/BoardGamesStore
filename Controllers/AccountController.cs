// Controllers/AccountController.cs

using BoardGamesStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
  private readonly IAccountService _accountService;

  public AccountController(IAccountService accountService)
  {
    _accountService = accountService;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterDto registerDto)
  {
    var result = await _accountService.RegisterUserAsync(registerDto);
    if (result.Succeeded)
    {
      return Ok(new { Message = "Реєстрація успішна. Будь ласка, перевірте свою пошту для підтвердження." });
    }

    return BadRequest(result.Errors);
  }

  [HttpGet("confirm-email")]
  public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
  {
    if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
      return BadRequest("Неправильні параметри для підтвердження пошти.");

    var result = await _accountService.ConfirmEmailAsync(userId, token);
    if (result.Succeeded)
    {
      return Ok(new { Message = "Пошту успішно підтверджено." });
    }

    return BadRequest(result.Errors);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginDto loginDto)
  {
    var (succeeded, token) = await _accountService.LoginUserAsync(loginDto);
    if (succeeded)
    {
      return Ok(new { Token = token });
    }

    return Unauthorized(new { Message = "Неправильний логін або пароль, або пошта не підтверджена." });
  }

  [Authorize] // Тільки для авторизованих користувачів
  [HttpPost("change-password")]
  public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
  {
    // Отримуємо ID поточного користувача з його "claims" (даних токена)
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null)
    {
      return Unauthorized();
    }

    var result = await _accountService.ChangePasswordAsync(userId, changePasswordDto);
    if (result.Succeeded)
    {
      return Ok(new { Message = "Пароль успішно змінено." });
    }

    return BadRequest(result.Errors);
  }
}