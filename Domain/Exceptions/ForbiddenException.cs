using System.Net;

namespace DrawingMarketplace.Domain.Exceptions
{
    public sealed class ForbiddenException : DomainException
    {
        public ForbiddenException(string message = "Access denied.")
            : base(message, (int)HttpStatusCode.Forbidden)
        {
        }
    }
}
