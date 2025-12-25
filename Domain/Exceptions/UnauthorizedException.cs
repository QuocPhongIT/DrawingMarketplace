using System.Net;

namespace DrawingMarketplace.Domain.Exceptions
{
    public sealed class UnauthorizedException : DomainException
    {
        public UnauthorizedException(string message = "Unauthorized access.")
            : base(message, (int)HttpStatusCode.Unauthorized)
        {
        }
    }
}
