using System;
using System.IO;

namespace FileClipper.UI.Services
{
    public interface IFilesSystem
    {
        bool DirectoryExists(string path);
        string? GetFileExtension(string path);
        DateTime GetFileLastModifiedDateTime(string path);
        string? GetFileName(string path);
        string[] GetFilesInDirectory(string path, SearchOption searchOption);
        long GetFileSize(string path);

        string ReadAllText(string path);
    }
}