using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace OPMF.E2ETests
{
    public sealed class CliFixture
    {
        public string ExePath { get; }

        public CliFixture()
        {
            var testDir = Path.GetDirectoryName(typeof(CliFixture).Assembly.Location);
            var candidate = Path.Combine(testDir, "OffPeakMediaFetcher.exe");
            if (!File.Exists(candidate))
            {
                var dll = Path.Combine(testDir, "OffPeakMediaFetcher.dll");
                if (!File.Exists(dll))
                {
                    throw new FileNotFoundException(
                        "Could not find OffPeakMediaFetcher executable beside the test assembly: " + candidate);
                }
                candidate = dll;
            }
            ExePath = candidate;
        }

        public (int ExitCode, string StdOut, string StdErr) Run(params string[] args)
        {
            var psi = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (ExePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                psi.FileName = "dotnet";
                psi.ArgumentList.Add(ExePath);
            }
            else
            {
                psi.FileName = ExePath;
            }
            foreach (var a in args) psi.ArgumentList.Add(a);

            using var p = Process.Start(psi);
            string stdOut = p.StandardOutput.ReadToEnd();
            string stdErr = p.StandardError.ReadToEnd();
            if (!p.WaitForExit(60_000))
            {
                try { p.Kill(true); } catch { }
                throw new TimeoutException("CLI did not exit within 60s. StdOut=" + stdOut);
            }
            return (p.ExitCode, stdOut, stdErr);
        }
    }
}
