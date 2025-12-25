using System.Net;

namespace DrawingMarketplace.Domain.Exceptions
{
    public sealed class BadRequestException : DomainException
    {
        public BadRequestException(string message)
            : base(message, (int)HttpStatusCode.BadRequest)
        {
        }
    }
}
