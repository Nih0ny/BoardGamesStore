namespace BoardGamesStore.Models;

// public class ProductDto
// {
//   public int Id { get; set; }
//   public string Name { get; set; } = null!;
//   public string Description { get; set; } = null!;
//   public decimal Price { get; set; }
//   public bool InStock { get; set; }
//   public string Category { get; set; } = null!;
//   public string ImageUrl { get; set; } = null!;
//   public decimal BonusRate { get; set; }
//   public double Rating { get; set; }
// }

// public class CreateProductDto
// {
//   public string Name { get; set; } = null!;
//   public string Description { get; set; } = null!;
//   public decimal Price { get; set; }
//   public string Category { get; set; } = null!;
//   public string ImageUrl { get; set; } = null!;
//   public decimal BonusRate { get; set; }
//   public int Stock { get; set; }
//   public decimal MaxBonusPaymentPercent { get; set; }
// }

public class CreateProductDto
{
    public string Name { get; init; } = null!;
    public List<string> Categories { get; init; } = null!;
    public decimal Price { get; init; }
    public string? Description { get; init; }
    public int StockQuantity { get; init; }
}

public class UpdateProductDto
{
    public string? Name { get; init; }
    public List<string>? Categories { get; init; }
    public decimal? Price { get; init; }
    public string? Description { get; init; }
    public int? Stock { get; init; }
}

public class ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public decimal BonusRate { get; init; }
    public decimal MaxBonusPaymentPercent { get; init; }
    public string? Description { get; init; }
    public double? Rating { get; init; }
    public bool IsInStock { get; init; }
    public List<string> Categories { get; init; } = null!;
    public List<string>? ImageUrl { get; init; }
}

public class ProductSearchQuery
{
    public string? SearchText { get; init; }
    public string? Category { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? InStock { get; init; }
    public string? SortBy { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class CategoryCount
{
    public string Category { get; init; } = null!;
    public int Count { get; init; }
}

public class SetProductDiscountDto
{
    public decimal? DiscountPercent { get; init; }
}