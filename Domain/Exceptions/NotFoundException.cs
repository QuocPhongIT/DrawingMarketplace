using System.Net;

namespace DrawingMarketplace.Domain.Exceptions
{
    public sealed class NotFoundException : DomainException
    {
        public NotFoundException(string resourceName, object key)
            : base($"{resourceName} with identifier '{key}' was not found.",
                   (int)HttpStatusCode.NotFound)
        {
        }
    }
}
