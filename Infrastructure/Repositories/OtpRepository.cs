using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class OtpRepository : IOtpRepository
{
    private readonly DrawingMarketplaceContext _context;

    public OtpRepository(DrawingMarketplaceContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Otp otp)
    {
        _context.Otps.Add(otp);
        await _context.SaveChangesAsync();
    }

    public async Task<Otp?> GetLatestOtpAsync(string email, OtpType type)
    {
        return await _context.Otps
            .Where(o =>
                o.Email == email &&
                o.Type == type)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Otp otp)
    {
        _context.Otps.Update(otp);
        await _context.SaveChangesAsync();
    }

    public async Task InvalidateAsync(string email, OtpType type)
    {
        var otps = await _context.Otps
            .Where(o =>
                o.Email == email &&
                o.Type == type &&
                !o.IsUsed)
            .ToListAsync();

        foreach (var otp in otps)
        {
            otp.IsUsed = true;
        }

        await _context.SaveChangesAsync();
    }
}
