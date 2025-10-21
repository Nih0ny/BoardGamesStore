// Controllers/AccountController.cs

using BoardGamesStore.Models;
using BoardGamesStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
  private readonly IAccountService _accountService;
  private readonly ITokenService _tokenService;

  public AccountController(IAccountService accountService, ITokenService tokenService)
  {
    _accountService = accountService;
    _tokenService = tokenService;
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
    try
    {
      var (accessToken, refreshToken) = await _accountService.LoginUserAsync(loginDto);
      var cookieOptions = new CookieOptions
      {
        HttpOnly = true,
        Secure = false, // true if using HTTPS
        SameSite = SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddDays(90)
      };
      Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
      return Ok(new { Token = accessToken });
    }
    catch (UnauthorizedAccessException e)
    {
      return Unauthorized(new { e.Message });
    }
  }

  [HttpPost("refresh")]
  public async Task<IActionResult> RefreshToken()
  {
    var refreshToken = Request.Cookies["refreshToken"];
    if (string.IsNullOrEmpty(refreshToken))
    {
      return Unauthorized(new { Message = "Refresh token is missing." });
    }
    var (newAccessToken, newRefreshToken) = await _tokenService.RefreshTokensAsync(refreshToken);
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
    var email = User.FindFirstValue(ClaimTypes.Email);
    if (email == null)
    {
      return Unauthorized();
    }

    Console.WriteLine($"Change password request for user: {email}");

    var result = await _accountService.ChangePasswordAsync(email, changePasswordDto);
    if (result.Succeeded)
    {
      return Ok(new { Message = "Password changed successfully." });
    }

    return BadRequest(result.Errors);
  }

  [HttpPost("forgot-password")]
  public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
  {
    _ = await _accountService.ForgotPasswordAsync(forgotPasswordDto.Email);
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
}