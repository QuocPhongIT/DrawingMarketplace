using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Domain.Exceptions;

namespace DrawingMarketplace.Application.Features.Auth;

public sealed class ResetPasswordHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public ResetPasswordHandler(
        IUserRepository users,
        IPasswordHasher hasher,
        ITokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task ExecuteAsync(string email, string newPassword)
    {
        var user = await _users.GetByEmailAsync(email)
            ?? throw new NotFoundException("User", email);

        user.PasswordHash = _hasher.Hash(newPassword);

        await _tokens.LogoutAllDevicesAsync(user.Id);

        await _users.UpdateAsync(user);
    }
}
