using FileClipper.UI.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileClipper.Tests
{
    public class FileServiceTests
    {
        private readonly IFilesSystem _filesSystem = Substitute.For<IFilesSystem>();
        private readonly FileService _fileService;

        public FileServiceTests()
        {
            _fileService = new FileService(_filesSystem);
        }

        [Fact]
        public void GetFilesExtensionInDirectory_ShouldThrow_WhenDirectoryDoesNotExist()
        {
            var path = "invalid/path";
            _filesSystem.DirectoryExists(path).Returns(false);

            var ex = Assert.Throws<DirectoryNotFoundException>(() =>
                _fileService.GetFilesExtensionInDirectory(path, false));

            Assert.Equal($"The specified folder does not exist: {path}", ex.Message);
        }

        [Fact]
        public void GetFilesExtensionInDirectory_ShouldReturnDistinctLowercaseExtensions()
        {
            var path = "some/path";
            _filesSystem.DirectoryExists(path).Returns(true);

            var files = new[] { "file1.TXT", "file2.txt", "image.png", "doc.PDF" };
            _filesSystem.GetFilesInDirectory(path, SearchOption.TopDirectoryOnly).Returns(files);
            _filesSystem.GetFileExtension(Arg.Any<string>())
                .Returns(callInfo => Path.GetExtension(callInfo.Arg<string>()).TrimStart('.'));

            var result = _fileService.GetFilesExtensionInDirectory(path, false);

            Assert.Contains("txt", result);
            Assert.Contains("png", result);
            Assert.Contains("pdf", result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetFilesInDirectory_ShouldReturnFilesMatchingExtensions()
        {
            var path = "test/path";
            _filesSystem.DirectoryExists(path).Returns(true);
            var files = new[] { "file1.txt", "file2.png", "file3.doc" };
            _filesSystem.GetFilesInDirectory(path, SearchOption.TopDirectoryOnly).Returns(files);

            _filesSystem.GetFileExtension(Arg.Any<string>())
                .Returns(callInfo => Path.GetExtension(callInfo.Arg<string>()).TrimStart('.'));

            var result = _fileService.GetFilesInDirectory(path, new[] { "txt", "doc" }, false);

            Assert.Equal(2, result.Count);
            Assert.Contains("file1.txt", result);
            Assert.Contains("file3.doc", result);
        }

        [Fact]
        public void GetFilesInDirectory_ShouldReturnAllFiles_WhenNoExtensionsProvided()
        {
            var path = "test/path";
            _filesSystem.DirectoryExists(path).Returns(true);
            var files = new[] { "file1.txt", "file2.png" };
            _filesSystem.GetFilesInDirectory(path, SearchOption.TopDirectoryOnly).Returns(files);

            var result = _fileService.GetFilesInDirectory(path, Enumerable.Empty<string>(), false);

            Assert.Equal(files, result);
        }

        [Fact]
        public void BuildClipboardText_ShouldReturnFormattedText()
        {
            var filePath = "C:/test/file1.txt";
            var files = new List<string> { filePath };
            var fileName = "file1.txt";
            var fileSize = 123L;
            var lastModified = new DateTime(2024, 1, 1);
            var fileContent = "This is a test.";

            _filesSystem.GetFileName(files[0]).Returns(fileName);
            _filesSystem.GetFileSize(files[0]).Returns(fileSize);
            _filesSystem.GetFileLastModifiedDateTime(files[0]).Returns(lastModified);
            _filesSystem.ReadAllText(files[0]).Returns(fileContent);

            var result = _fileService.BuildClipboardText(files);

            var expected = new StringBuilder()
                .AppendLine($"====== Start of {fileName} ======")
                .AppendLine($"File path: {filePath}")
                .AppendLine($"File size: {fileSize} bytes")
                .AppendLine($"Last Modified: {lastModified}")
                .AppendLine("File content: ")
                .AppendLine()
                .AppendLine(fileContent)
                .AppendLine()
                .AppendLine($"====== End of {fileName} ======")
                .AppendLine()
                .ToString();

            Assert.Equal(expected, result);
        }
    }
}
