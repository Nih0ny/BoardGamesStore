using BoardGamesStore.Models;
using Microsoft.AspNetCore.Identity;

namespace BoardGamesStore.Models;

public class UserRole : IdentityUserRole<int>
{
  public required User User { get; set; }
  public required Role Role { get; set; }
}
