using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileClipper.UI.Services
{
    public class FileService : IFileService
    {
        private readonly IFilesSystem _filesSystem;

        public FileService(IFilesSystem filesSystem)
        {
            _filesSystem = filesSystem;
        }
        public string BuildClipboardText(List<string> files)
        {
            var sb = new StringBuilder();
            foreach (var filePath in files)
            {
                var fileName = _filesSystem.GetFileName(filePath);
                var fileSize = _filesSystem.GetFileSize(filePath);
                var fileLastModifiedDate = _filesSystem.GetFileLastModifiedDateTime(filePath);
                sb.AppendLine($"====== Start of {fileName} ======");
                sb.AppendLine($"File path: {filePath}");
                sb.AppendLine($"File size: {fileSize} bytes");
                sb.AppendLine($"Last Modified: {fileLastModifiedDate}");
                sb.AppendLine("File content: ");
                sb.AppendLine();
                sb.AppendLine(_filesSystem.ReadAllText(filePath));
                sb.AppendLine();
                sb.AppendLine($"====== End of {fileName} ======");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public List<string> GetFilesExtensionInDirectory(string directoryPath, bool includeSubdirectories)
        {
            if (!_filesSystem.DirectoryExists(directoryPath))
                throw new DirectoryNotFoundException($"The specified folder does not exist: {directoryPath}");

            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            return _filesSystem.GetFilesInDirectory(directoryPath, searchOption)
                .Select(file => _filesSystem.GetFileExtension(file)?.ToLower())
                .Where(ext => !string.IsNullOrEmpty(ext))
                .Distinct()
                .ToList()!;
        }

        public List<string> GetFilesInDirectory(string directoryPath, IEnumerable<string> selectedExtensions, bool includeSubdirectories)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException($"Directory path cannot be null or whitespace.", nameof(directoryPath));
            }

            if (!_filesSystem.DirectoryExists(directoryPath))
            {
                throw new DirectoryNotFoundException($"The specified folder does not exist: {directoryPath}");
            }

            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var extensionsList = selectedExtensions.ToList();

            return _filesSystem.GetFilesInDirectory(directoryPath, searchOption)
                .Where(file =>
                {
                    if (extensionsList.Count == 0)
                        return true;

                    var fileExt = _filesSystem.GetFileExtension(file)?.ToLower();
                    return !string.IsNullOrEmpty(fileExt) && extensionsList.Contains(fileExt);
                })
                .ToList();
        }
    }
}
