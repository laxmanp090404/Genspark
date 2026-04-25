using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Common;
using server.DTOs.Vouchers;
using server.Extensions;

namespace server.Controllers;

[ApiController]
[Route("api/vouchers")]
[Authorize(Roles = "USER")]
public class VouchersController(AppDbContext db) : ControllerBase
{
    [HttpGet("{code}")]
    public async Task<IActionResult> Get(string code, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(ApiResponse<object>.Fail("Voucher code is required."));
        }

        var userId = User.GetUserId();
        var normalized = code.Trim();
        var voucher = await db.DiscountVouchers
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Code.ToLower() == normalized.ToLower() && v.UserId == userId, ct);

        if (voucher is null)
        {
            return NotFound(ApiResponse<object>.Fail("Voucher not found."));
        }

        if (voucher.IsUsed)
        {
            return BadRequest(ApiResponse<object>.Fail("Voucher has already been used."));
        }

        if (voucher.ExpiresAt <= DateTime.UtcNow)
        {
            return BadRequest(ApiResponse<object>.Fail("Voucher has expired."));
        }

        var response = new VoucherResponse
        {
            Code = voucher.Code,
            DiscountPercent = voucher.DiscountPercent,
            IsUsed = voucher.IsUsed,
            ExpiresAt = voucher.ExpiresAt
        };

        return Ok(ApiResponse<VoucherResponse>.Ok(response));
    }
}
