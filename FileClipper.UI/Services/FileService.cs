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
        public string BuildClipboardText(List<string> files)
        {
            var sb = new StringBuilder();
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var fileSize = new FileInfo(filePath).Length;
                var fileLastModifiedDate = new FileInfo(filePath).LastWriteTime;
                sb.AppendLine($"====== Start of {fileName} ======");
                sb.AppendLine($"File path: {filePath}");
                sb.AppendLine($"File size: {fileSize} bytes");
                sb.AppendLine($"Last Modified: {fileLastModifiedDate}");
                sb.AppendLine("File content: ");
                sb.AppendLine();
                sb.AppendLine(File.ReadAllText(filePath));
                sb.AppendLine();
                sb.AppendLine($"====== End of {fileName} ======");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public List<string> GetFilesExtensionInDirectory(string folderPath, bool includeSubdirectories)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"The specified folder does not exist: {folderPath}");

            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            return Directory.GetFiles(folderPath, "*.*", searchOption)
                .Select(file => Path.GetExtension(file)?.ToLower())
                .Where(ext => !string.IsNullOrEmpty(ext))
                .Distinct()
                .ToList()!;
        }

        public List<string> GetFilesInDirectory(string folderPath, IEnumerable<string> selectedExtensions, bool includeSubdirectories)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"The specified folder does not exist: {folderPath}");

            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var extensionsList = selectedExtensions.ToList();

            return Directory.GetFiles(folderPath, "*.*", searchOption)
                .Where(file =>
                {
                    if (extensionsList.Count == 0)
                        return true;

                    var fileExt = Path.GetExtension(file)?.ToLower();
                    return !string.IsNullOrEmpty(fileExt) && extensionsList.Contains(fileExt);
                })
                .ToList();
        }
    }
}
