using Xunit;

namespace OPMF.E2ETests
{
    public class CliArgumentsTests : IClassFixture<CliFixture>
    {
        private readonly CliFixture _cli;
        public CliArgumentsTests(CliFixture cli) { _cli = cli; }

        [Fact]
        public void NoArgs_PrintsArgumentsRequiredMessage()
        {
            var (exit, stdOut, _) = _cli.Run();
            Assert.Equal(0, exit);
            Assert.Contains("Appropriate arguments required.", stdOut);
        }

        [Fact]
        public void UnknownArg_PrintsInvalidArgumentMessage()
        {
            var (exit, stdOut, _) = _cli.Run("not-a-real-command");
            Assert.Equal(0, exit);
            Assert.Contains("Invalid argument.", stdOut);
        }
    }
}
