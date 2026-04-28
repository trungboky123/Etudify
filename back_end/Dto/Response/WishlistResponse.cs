namespace back_end.Dto.Response;

public class WishlistResponse
{
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string Slug { get; set; }
    public decimal? ListedPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime AddedAt { get; set; }

    public WishlistResponse(int itemId, string itemName, string slug, decimal? listedPrice, decimal? salePrice, string? thumbnailUrl, DateTime addedAt)
    {
        ItemId = itemId;
        ItemName = itemName;
        Slug = slug;
        ListedPrice = listedPrice;
        SalePrice = salePrice;
        ThumbnailUrl = thumbnailUrl;
        AddedAt = addedAt;
    }
}