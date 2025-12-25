using DrawingMarketplace.Application.DTOs.Auth;
using FluentValidation;
using System.Text.RegularExpressions;

namespace DrawingMarketplace.Application.Features.Auth
{
    public class RegisterCommandValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(254)
                .EmailAddress()
                .Must(BeLowerCase);

            RuleFor(x => x.Username)
                .NotEmpty()
                .Length(3, 30)
                .Matches("^[a-zA-Z0-9._]+$")
                .Must(NotStartOrEndWithSpecialChar)
                .Must(NotContainConsecutiveSpecialChars);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(64)
                .Must(ContainUppercase)
                .Must(ContainLowercase)
                .Must(ContainDigit)
                .Must(ContainSpecialCharacter);
        }

        private static bool BeLowerCase(string email)
            => email == email.ToLowerInvariant();

        private static bool NotStartOrEndWithSpecialChar(string username)
        {
            return !username.StartsWith(".") &&
                   !username.StartsWith("_") &&
                   !username.EndsWith(".") &&
                   !username.EndsWith("_");
        }

        private static bool NotContainConsecutiveSpecialChars(string username)
            => !Regex.IsMatch(username, @"[._]{2,}");

        private static bool ContainUppercase(string password)
            => password.Any(char.IsUpper);

        private static bool ContainLowercase(string password)
            => password.Any(char.IsLower);

        private static bool ContainDigit(string password)
            => password.Any(char.IsDigit);

        private static bool ContainSpecialCharacter(string password)
            => password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
