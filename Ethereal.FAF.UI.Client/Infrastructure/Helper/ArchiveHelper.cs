using Ethereal.FAF.UI.Client.Models.Progress;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Ethereal.FAF.UI.Client.Infrastructure.Helper
{
    public record struct ArchiveInfo(ulong Size, ulong CompressedSize);
    public static class ArchiveHelper
    {
        private static ILogger _logger;
        public static string SevenZipFileName = "7z.exe";
        // HomeDir is set by ISettingsManager.TryFindLibrary()
        public static string HomeDir { get; set; } = string.Empty;
        public static string SevenZipPath => Path.Combine(HomeDir, "Assets", SevenZipFileName);

        public static void SetupLogger(ILogger logger) => _logger = logger;

        private static Regex Regex7ZOutput() => new(@"(?<=Size:\s*)\d+|(?<=Compressed:\s*)\d+");
        private static Regex Regex7ZProgressDigits() => new(@"(?<=\s*)\d+(?=%)");
        private static Regex Regex7ZProgressFull() => new(@"(\d+)%.*- (.*)");

        public static async Task<ArchiveInfo> TestArchive(string archivePath)
        {
            var process = Process.Start(SevenZipPath, new[] { "t", archivePath });
            await process.WaitForExitAsync().ConfigureAwait(false);
            var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            var matches = Regex7ZOutput().Matches(output);
            var size = ulong.Parse(matches[0].Value);
            var compressed = ulong.Parse(matches[1].Value);
            return new ArchiveInfo(size, compressed);
        }
        public static async Task<ArchiveInfo> GetRarAchiveInfo(string archivePath)
        {
            IArchive rar = SharpCompress.Archives.Rar.RarArchive.Open(archivePath, new()
            {
                LeaveStreamOpen = false,
                LookForHeader = true,
            });
            // Calculate the total extraction size.
            var totalSize = rar.Entries.Where(e => !e.IsDirectory).Sum(e => e.Size);
            var compressedSize = rar.Entries.Where(e => !e.IsDirectory).Sum(e => e.CompressedSize);
            return new((ulong)totalSize, (ulong)compressedSize);
        }
        /// <summary>
        /// Extract an archive to the output directory.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="archivePath"></param>
        /// <param name="outputDirectory">Output directory, created if does not exist.</param>
        public static async Task Extract(
            string archivePath,
            string outputDirectory,
            IProgress<ProgressReport>? progress = default
        )
        {
            Directory.CreateDirectory(outputDirectory);
            progress?.Report(new ProgressReport(-1, isIndeterminate: true, message: "Extracting..."));

            var count = 0ul;

            // Get true size
            var (total, _) = Path.GetExtension(archivePath) switch
            {
                ".rar" => await GetRarAchiveInfo(archivePath).ConfigureAwait(false),
                //".7z" => await TestArchive(archivePath).ConfigureAwait(false),
                _ => throw new NotImplementedException("Unknown archive extension"),
            };

            // If not available, use the size of the archive file
            if (total == 0)
            {
                total = (ulong)new FileInfo(archivePath).Length;
            }

            // Create an DispatchTimer that monitors the progress of the extraction
            //var progressMonitor = progress switch
            //{
            //    null => null,
            //    _ => new Timer(TimeSpan.FromMilliseconds(36).Milliseconds)
            //};

            //if (progressMonitor != null)
            //{
            //    progressMonitor.Elapsed += (_, _) =>
            //    {
            //        if (count == 0)
            //            return;
            //        progress!.Report(new ProgressReport(count, total, message: "Extracting..."));
            //    };
            //}

            await Task.Factory
                .StartNew(() =>
                {
                    var extractOptions = new ExtractionOptions
                    {
                        Overwrite = true,
                        ExtractFullPath = true,
                    };
                    using var stream = File.OpenRead(archivePath);
                    using var archive = ReaderFactory.Open(stream);

                    // Start the progress reporting timer
                    //progressMonitor?.Start();

                    archive.EntryExtractionProgress += (s, arg) =>
                    {
                        progress?.Report(new(
                            progress: arg.ReaderProgress?.PercentageReadExact * 0.01 ?? 1,
                            message: $"Extracting \"{arg.Item.Key}\"",
                            isIndeterminate: arg.ReaderProgress == null));
                    };

                    while (archive.MoveToNextEntry())
                    {
                        var entry = archive.Entry;
                        if (!entry.IsDirectory)
                        {
                            count += (ulong)entry.CompressedSize;
                        }
                        archive.WriteEntryToDirectory(outputDirectory, extractOptions);
                    }
                },TaskCreationOptions.LongRunning)
                .ConfigureAwait(false);

            progress?.Report(new ProgressReport(progress: 1, message: "Done extracting"));
            //progressMonitor?.Stop();
            _logger.LogInformation("Finished extracting archive {path}", archivePath);
        }
    }
}
