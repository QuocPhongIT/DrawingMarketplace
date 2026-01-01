namespace DrawingMarketplace.Api.Responses
{
    public class ViolationMessage
    {
        public string En { get; set; } = string.Empty;
        public string Vi { get; set; } = string.Empty;
    }

    public class Violation
    {
        public ViolationMessage Message { get; set; } = new();
        public string Type { get; set; } = string.Empty;
        public int Code { get; set; }
        public string? Field { get; set; }
    }
}

