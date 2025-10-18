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
      return Ok(new { Message = "Registration successful. Please check your email to confirm." });
    }

    return BadRequest(result.Errors);
  }

  [HttpGet("confirm-email")]
  public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
  {
    if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
      return BadRequest("Invalid parameters for email confirmation.");

    var result = await _accountService.ConfirmEmailAsync(userId, token);
    if (result.Succeeded)
    {
      return Ok(new { Message = "Email successfully confirmed." });
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

    return Unauthorized(new { Message = "Incorrect username or password, or email not confirmed." });
  }

  [Authorize]
  [HttpPost("change-password")]
  public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null)
    {
      return Unauthorized();
    }

    var result = await _accountService.ChangePasswordAsync(userId, changePasswordDto);
    if (result.Succeeded)
    {
      return Ok(new { Message = "Password changed successfully." });
    }

    return BadRequest(result.Errors);
  }

  [HttpPost("forgot-password")]
  public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
  {
    var result = await _accountService.ForgotPasswordAsync(forgotPasswordDto.Email);
    if (result.Succeeded)
    {
      return Ok(new { Message = "If an account with this email exists, a password reset link has been sent." });
    }

    // To prevent user enumeration, we can return a success message even if the user doesn't exist.
    // The decision depends on the security policy. For this example, we'll return a generic success message.
    return Ok(new { Message = "If an account with this email exists, a password reset link has been sent." });
  }

  [HttpPost("reset-password")]
  public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
  {
    var result = await _accountService.ResetPasswordAsync(
        resetPasswordDto.UserId,
        resetPasswordDto.Token,
        resetPasswordDto.NewPassword);
    if (result.Succeeded)
    {
      return Ok(new { Message = "Password has been reset successfully." });
    }

    return BadRequest(result.Errors);
  }
}