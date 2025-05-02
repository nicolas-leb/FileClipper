using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileClipper.UI.Services
{
    public interface IFileService
    {
        List<string> GetFilesInDirectory(string folderPath, IEnumerable<string> selectedExtensions, bool includeSubdirectories);
        List<string> GetFilesExtensionInDirectory(string folderPath, bool includeSubdirectories);
        string BuildClipboardText(List<string> files);
    }
}
