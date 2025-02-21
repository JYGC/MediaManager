using OPMF.Entities;
using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace OPMF.Tests.Helpers
{
    public class TestChannelServiceHelpers
    {
        public static void AssertChannelIsEqual(Channel expectedChannel, Channel actualChannel)
        {
            Assert.Equal(expectedChannel.Name, actualChannel.Name);
            if (!string.IsNullOrWhiteSpace(expectedChannel.Description))
            {
                Assert.Equal(expectedChannel.Description, actualChannel.Description);
            }
            Assert.Equal(expectedChannel.IsAddedBySingleVideo, actualChannel.IsAddedBySingleVideo);
            Assert.Equal(expectedChannel.Blacklisted, actualChannel.Blacklisted);
            Assert.Equal(expectedChannel.LastActivityDate, actualChannel.LastActivityDate);
            Assert.Equal(expectedChannel.LastCheckedOut, actualChannel.LastCheckedOut);
            Assert.Equal(expectedChannel.NotFound, actualChannel.NotFound);
            Assert.Equal(expectedChannel.Url, actualChannel.Url);
            Assert.Equal(JsonSerializer.Serialize(expectedChannel.Thumbnail), JsonSerializer.Serialize(actualChannel.Thumbnail));
        }

        public static void ModifyChannel(Channel channel, int i)
        {
            channel.Name = $"{channel.Name}{from number in Enumerable.Range(0, i * i) select Math.Pow(i, i)}";
            channel.Description = $"{channel.Description}{from number in Enumerable.Range(0, i * i) select Math.Pow(i, i)}";
            //channel.IsAddedBySingleVideo = !channel.IsAddedBySingleVideo;
            //channel.BlackListed = !channel.BlackListed;
            //channel.LastActivityDate = channel.LastActivityDate.HasValue ? channel.LastActivityDate.Value.AddDays(i) : DateTime.UtcNow;
            //channel.LastCheckedOut = channel.LastCheckedOut.HasValue ? channel.LastCheckedOut.Value.AddDays(i) : DateTime.UtcNow;
            //channel.NotFound = !channel.NotFound;
            channel.Url = $"{channel.Url}{from number in Enumerable.Range(0, i * i) select Math.Pow(i, i)}";
            channel.Thumbnail.Width = 2 * i;
            channel.Thumbnail.Height = 3 * i * i;
        }
    }

}
