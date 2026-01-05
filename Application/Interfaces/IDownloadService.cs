using DrawingMarketplace.Application.DTOs.MediaFile;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IDownloadService
    {
        Task<List<MediaFileDto>> GetDownloadFilesAsync(Guid contentId);
    }
}
