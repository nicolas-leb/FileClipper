using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileClipper.UI.Services
{
    internal class FilesSystem : IFilesSystem
    {
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] GetFilesInDirectory(string path, SearchOption searchOption)
        {
            return Directory.GetFiles(path, "*.*", searchOption);
        }

        public string? GetFileExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public string? GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public DateTime GetFileLastModifiedDateTime(string path)
        {
            return new FileInfo(path).LastWriteTime;
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
    }
}
