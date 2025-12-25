namespace DrawingMarketplace.Application.DTOs.Catogory
{
    public class CategoryUpsertDto
    {
        public class CreateCategoryDto
        {
            public string Name { get; set; } = null!;
        }

        public class UpdateCategoryDto
        {
            public string Name { get; set; } = null!;
        }
    }
}
