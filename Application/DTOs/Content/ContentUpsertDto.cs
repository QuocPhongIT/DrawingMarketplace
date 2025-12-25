namespace DrawingMarketplace.Application.DTOs.Content
{
    public class ContentUpsertDto
    {
        public class CreateContentDto
        {
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public Guid? CategoryId { get; set; }
            public Guid? CollaboratorId { get; set; }
        }

        public class UpdateContentDto
        {
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public Guid? CategoryId { get; set; }
        }
    }
}
