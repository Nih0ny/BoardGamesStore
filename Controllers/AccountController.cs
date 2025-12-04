using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BoardGamesStore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IAccountService accountService, ITokenService tokenService) : ControllerBase
{
  private readonly IAccountService _accountService = accountService;
  private readonly ITokenService _tokenService = tokenService;

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
  public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
  {
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
      return BadRequest("Invalid parameters for email confirmation.");

    var result = await _accountService.ConfirmEmailAsync(email, token);
    if (result.Succeeded)
    {
      return Ok(new { Message = "Email successfully confirmed." });
    }

    return BadRequest(result.Errors);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginDto loginDto)
  {

    var result = await _accountService.LoginUserAsync(loginDto);
    if (result.IsFailed) return Unauthorized(new { result.Errors[0].Message });

    var (accessToken, refreshToken) = result.Value;
    var cookieOptions = new CookieOptions
    {
      HttpOnly = true,
      Secure = false, // FIXME: true if using HTTPS
      SameSite = SameSiteMode.Strict,
      Expires = DateTime.UtcNow.AddDays(90)
    };
    Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    return Ok(new { Token = accessToken });
  }

  [HttpPost("refresh")]
  public async Task<IActionResult> RefreshToken()
  {
    var refreshToken = Request.Cookies["refreshToken"];
    if (string.IsNullOrEmpty(refreshToken))
    {
      return Unauthorized(new { Message = "Refresh token is missing." });
    }

    var result = await _tokenService.RefreshTokensAsync(refreshToken);
    if (result.IsFailed)
    {
      return Unauthorized(new { Message = "Invalid refresh token." });
    }
    var (newAccessToken, newRefreshToken) = result.Value;

    var cookieOptions = new CookieOptions
    {
      HttpOnly = true,
      Secure = false, // true if using HTTPS
      SameSite = SameSiteMode.Strict,
      Expires = DateTime.UtcNow.AddDays(90)
    };
    Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);
    return Ok(new { Token = newAccessToken });
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

    Console.WriteLine($"Change password request for user: {userId}");
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
    _ = await _accountService.ForgotPasswordAsync(forgotPasswordDto);
    return Ok(new { Message = "If an account with this email exists, a password reset link has been sent." });
  }

  [HttpPost("reset-password")]
  public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
  {
    var result = await _accountService.ResetPasswordAsync(
        resetPasswordDto.Email,
        resetPasswordDto.Token,
        resetPasswordDto.NewPassword);
    if (result.Succeeded)
    {
      return Ok(new { Message = "Password has been reset successfully." });
    }

    return BadRequest(result.Errors);
  }

  [Authorize]
  [HttpGet("me")]
  public IActionResult GetCurrentUser()
  {

    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null)
    {
      return Unauthorized();
    }

    return Ok(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
  }

  // FIXME: Implement account deletion in AccountService
  [Authorize]
  [HttpDelete]
  public async Task<IActionResult> DeleteAccount()
  {
    var email = User.FindFirstValue(ClaimTypes.Email);
    if (email == null)
    {
      return Unauthorized();
    }

    // var result = await _accountService.DeleteAccountAsync(email);
    // if (result)
    // {
    //   return Ok(new { Message = "Account deleted successfully." });
    // }

    return BadRequest(new { Message = "Failed to delete account." });
  }
}