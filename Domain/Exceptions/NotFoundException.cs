using System.Net;

namespace DrawingMarketplace.Domain.Exceptions
{
    public sealed class NotFoundException : DomainException
    {
        public string ResourceName { get; }
        public object Key { get; }

        public NotFoundException(string resourceName, object key)
            : base($"{resourceName} with identifier '{key}' was not found.",
                   (int)HttpStatusCode.NotFound)
        {
            ResourceName = resourceName;
            Key = key;
        }
    }
}
