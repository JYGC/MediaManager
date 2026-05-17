using System;
using OPMF.Entities;
using Xunit;

namespace OPMF.UnitTests.Entities
{
    public class OPMFLogTests
    {
        [Fact]
        public void NewLog_HasGuidId()
        {
            var log = new OPMFLog();
            Assert.False(string.IsNullOrWhiteSpace(log.Id));
            Assert.True(Guid.TryParse(log.Id, out _));
        }

        [Fact]
        public void TwoLogs_HaveDifferentIds()
        {
            var a = new OPMFLog();
            var b = new OPMFLog();
            Assert.NotEqual(a.Id, b.Id);
        }

        [Fact]
        public void NewLog_DateCreatedIsRecent()
        {
            var before = DateTime.Now.AddSeconds(-1);
            var log = new OPMFLog();
            var after = DateTime.Now.AddSeconds(1);
            Assert.InRange(log.DateCreated, before, after);
        }

        [Fact]
        public void OPMFError_FromException_CapturesMessageTypeAndDetails()
        {
            Exception cause;
            try { throw new InvalidOperationException("boom"); }
            catch (Exception ex) { cause = ex; }

            var err = new OPMFError(cause);

            Assert.Equal("boom", err.Message);
            Assert.Equal(OPMFLogType.Error, err.Type);
            Assert.Contains("InvalidOperationException", err.ExceptionObject);
            Assert.Contains("boom", err.ExceptionObject);
        }

        [Fact]
        public void OPMFError_Parameterless_HasGuidId()
        {
            var err = new OPMFError();
            Assert.True(Guid.TryParse(err.Id, out _));
        }

        [Fact]
        public void OPMFLogTypeEnum_DefinesExpectedMembers()
        {
            var values = Enum.GetValues<OPMFLogType>();
            Assert.Contains(OPMFLogType.Info, values);
            Assert.Contains(OPMFLogType.Warning, values);
            Assert.Contains(OPMFLogType.Error, values);
            Assert.Equal(3, values.Length);
        }
    }
}
