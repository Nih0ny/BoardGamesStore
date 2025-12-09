namespace BoardGamesStore.Models;

public class CategoryDto
{
  public int Id { get; init; }
  public string Name { get; init; } = null!;
}

public class CreateCategoryDto
{
  public string Name { get; init; } = null!;
}

public class UpdateCategoryDto
{
  public string Name { get; init; } = null!;
}