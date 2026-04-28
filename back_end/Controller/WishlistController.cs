using System.Security.Claims;
using back_end.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controller;

[ApiController]
[Route("/api/wishlists")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    
    public WishlistController(IWishlistService wishlist)
    {
        _wishlistService = wishlist;
    }

    [HttpGet("find")]
    [Authorize]
    public async Task<IActionResult> CheckExists([FromQuery] int itemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _wishlistService.ExistsByUserIdAndItemId(userId, itemId);
        return Ok(result);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetByUser([FromQuery] string? keyword, [FromQuery] int? categoryId,
        [FromQuery] string? sortBy, [FromQuery] string? sortDir)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _wishlistService.GetByUserId(userId, keyword, categoryId, sortBy, sortDir);
        return Ok(result);
    }

    [HttpPost("add")]
    [Authorize]
    public async Task<IActionResult> AddToWishlist([FromQuery] int itemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _wishlistService.Add(userId, itemId);
        return Ok();
    }

    [Authorize]
    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveFromWishlist([FromQuery] int itemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _wishlistService.Delete(userId, itemId);
        return Ok();
    }
}