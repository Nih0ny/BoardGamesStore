using System.ComponentModel.DataAnnotations;
using BoardGamesStore.Models;

namespace BoardGamesStore.Models;

public class Role
{
  [Key]
  public int Id { get; set; }
  public required string RoleName { get; set; }

  public ICollection<User>? Users { get; set; }
}

