using System.Net;

namespace DrawingMarketplace.Domain.Exceptions
{
    public sealed class ConflictException : DomainException
    {
        public ConflictException(string message)
            : base(message, (int)HttpStatusCode.Conflict)
        {
        }
    }
}
