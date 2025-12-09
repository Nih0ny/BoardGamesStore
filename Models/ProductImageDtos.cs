namespace BoardGamesStore.Models;

public class ProductImageDto
{
    public int Id { get; init; }
    public string RelativePath { get; init; } = null!;
    public bool IsMainImage { get; init; }
    public int DisplayOrder { get; init; }
}

public class ProductImageUploadResult
{
    public int ImageId { get; init; }
    public string RelativePath { get; init; } = null!;
    public bool IsMainImage { get; init; }
}
