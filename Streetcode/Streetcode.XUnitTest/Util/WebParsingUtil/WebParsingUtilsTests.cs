using Moq.Protected;
using Moq;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Streetcode.WebApi.Utils;
using System.Net;
using System.IO.Compression;
using System.Security;


namespace Streetcode.XUnitTest.Util.WebParsingUtil
{
    public class WebParsingUtilsTests
    {
        [Fact]
        public async Task DownloadAndExtractAsync_ShouldThrowArgumentException_WhenUrlIsInvalid()
        {
            // Arrange
            var invalidUrl = "invalid_url";
            var zipPath = "test.zip";
            var extractTo = "test_extract";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                WebParsingUtils.DownloadAndExtractAsync(invalidUrl, zipPath, extractTo, CancellationToken.None));
        }

        [Fact]
        public void SafeExtractToDirectory_ShouldExtractFiles_WhenArchiveIsValid()
        {
            // Arrange
            var zipPath = "test.zip";
            var extractTo = "test_extract";

            if (File.Exists(zipPath)) File.Delete(zipPath);
            if (Directory.Exists(extractTo)) Directory.Delete(extractTo, true);

            using (var zipFile = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                var demoFile = zipFile.CreateEntry("test.txt");
                using (var writer = new StreamWriter(demoFile.Open()))
                {
                    writer.Write("Test content");
                }
            }

            // Act
            WebParsingUtils.SafeExtractToDirectory(zipPath, extractTo);

            // Assert
            Assert.True(File.Exists(Path.Combine(extractTo, "test.txt")));
        }

        [Fact]
        public void SafeExtractToDirectory_ShouldThrowSecurityException_WhenDepthIsTooLarge()
        {
            // Arrange
            var zipPath = "test.zip";
            var extractTo = "test_extract";

            if (File.Exists(zipPath)) File.Delete(zipPath);
            if (Directory.Exists(extractTo)) Directory.Delete(extractTo, true);

            using (var zipFile = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                var demoFile = zipFile.CreateEntry("deep/folder/structure/test.txt");
                using (var writer = new StreamWriter(demoFile.Open()))
                {
                    writer.Write("Test content");
                }
            }
            // Act & Assert
            Assert.Throws<SecurityException>(() => WebParsingUtils.SafeExtractToDirectory(zipPath, extractTo, 1_000_000, 2)); // Встановлюємо малу глибину для перевірки
        }
    }
}
