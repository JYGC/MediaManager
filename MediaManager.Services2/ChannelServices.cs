using MediaManager.Dtos.Services;
using MediaManager.Repositories;

namespace MediaManager.Services2
{
    public interface IChannelServices
    {
        List<ServiceChannelDto> GetAllChannels();
        void UpdateBlacklistFlagOfChannels(IEnumerable<ServiceChannelDto> channels);
    }
    public class ChannelServices(IChannelRepository channelRepository) : IChannelServices
    {
        public List<ServiceChannelDto> GetAllChannels()
        {
            return channelRepository.GetAllChannels().Select(RepositoryDtoAndServiceDtoConverter.ConvertChannelToServiceDto).ToList();
        }

        public void UpdateBlacklistFlagOfChannels(IEnumerable<ServiceChannelDto> channels)
        {
            channelRepository.UpdateBlacklistFlagOfChannels(channels.Select(RepositoryDtoAndServiceDtoConverter.ConvertChannelToRepositoryDto).ToList());
        }
    }
}
