using MediaManager.Dtos.Application;
using MediaManager.Services2;

namespace MediaManager.Initializations;

public interface IChannelBrowsingUseCases
{
    List<ApplicationChannelDto> GetAllChannels();
    void UpdateBlacklistFlagOfChannels(IEnumerable<ApplicationChannelDto> channels);
}

public class ChannelBrowsingUseCases(IChannelServices channelServices) : IChannelBrowsingUseCases
{
    public List<ApplicationChannelDto> GetAllChannels()
    {
        return channelServices.GetAllChannels().Select(ServiceDtoAndApplicationDtoConverter.ConvertChannelToApplicationDto).ToList();
    }

    public void UpdateBlacklistFlagOfChannels(IEnumerable<ApplicationChannelDto> channels)
    {
        channelServices.UpdateBlacklistFlagOfChannels(channels.Select(ServiceDtoAndApplicationDtoConverter.ConvertChannelToServiceDto).ToList());
    }
}
