using MediaManager.Dtos.Services;
using MediaManager.Repositories;

namespace MediaManager.Services2
{
    public interface IMetadataServices
    {
        void UpdateDownloadStatusOfVideos(IEnumerable<ServiceVideoMetadataDto> videos);
        ServiceVideoMetadataDto ClearBeingDownloadedFlagOfVideo(ServiceVideoMetadataDto video);
        ServiceVideoMetadataDto MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(ServiceVideoMetadataDto video);
    }
    public class MetadataServices(
        ITaskRunnerServices taskRunnerServices,
        IVideoMetadataRepository videoMetadataRepository) : IMetadataServices
    {
        public void UpdateDownloadStatusOfVideos(IEnumerable<ServiceVideoMetadataDto> videos)
        {
            videoMetadataRepository.UpdateDownloadStatusOfVideos(videos.Select(RepositoryDtoAndServiceDtoConverter.ConvertVideoToRepositoryDto).ToList());
        }

        public ServiceVideoMetadataDto ClearBeingDownloadedFlagOfVideo(ServiceVideoMetadataDto video)
        {
            video.IsBeingDownloaded = false;
            videoMetadataRepository.SetBeingDownloadedFlagOfVideos([RepositoryDtoAndServiceDtoConverter.ConvertVideoToRepositoryDto(video)], false);
            return video;
        }

        public ServiceVideoMetadataDto MarkVideoAsBeingDownloadedAndStartTaskRunnerProcessThatDownloadsIt(ServiceVideoMetadataDto video)
        {
            video.IsBeingDownloaded = true;
            videoMetadataRepository.SetBeingDownloadedFlagOfVideos([RepositoryDtoAndServiceDtoConverter.ConvertVideoToRepositoryDto(video)], true);
            taskRunnerServices.StartTaskRunnerProcessThatDownloadsVideoWithSiteId(video.SiteId);
            return video;
        }
    }
}
