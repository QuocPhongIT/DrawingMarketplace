using DrawingMarketplace.Application.DTOs.Download;
using DrawingMarketplace.Application.DTOs.MediaFile;

public interface IDownloadService
{
    Task<List<MediaFileDto>> GetDownloadFilesAsync(Guid contentId);

    Task<DownloadFileResult> DownloadFileAsync(Guid contentId, Guid fileId);

}
