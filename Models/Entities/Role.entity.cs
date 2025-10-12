using System.ComponentModel.DataAnnotations;
using BoardGamesStore.Models;
using Microsoft.AspNetCore.Identity;

namespace BoardGamesStore.Models;

public class Role : IdentityRole<int>
{
  public required string RoleName { get; set; }

  public ICollection<User>? Users { get; set; }
}

