using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FileClipper.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string? Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    [ObservableProperty]
    private string _directoryToScanPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _extensions = [];

    public List<string> GetFilesExtensionInDirectory(string folderPath, bool includeSubdirectories)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Le dossier spécifié n'existe pas : {folderPath}");

        SearchOption searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        var files = Directory.GetFiles(folderPath, "*.*", searchOption).ToList();

        var extensions = files
            .Select(file => Path.GetExtension(file)?.ToLower())
            .Where(ext => !string.IsNullOrEmpty(ext))
            .Distinct()
            .ToList();

        return extensions!;
    }
}
