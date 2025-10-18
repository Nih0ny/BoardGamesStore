using System.ComponentModel.DataAnnotations;
using BoardGamesStore.Models;
using Microsoft.AspNetCore.Identity;

namespace BoardGamesStore.Models;

public class Role : IdentityRole<int>
{
  public ICollection<UserRole>? UserRoles { get; set; }
}

