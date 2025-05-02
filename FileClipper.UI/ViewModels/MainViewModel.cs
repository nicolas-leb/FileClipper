using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileClipper.UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileClipper.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IFileService _fileService;

    public string? Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExtensionsScanned))]
    private string _directoryToScanPath = string.Empty;

    partial void OnDirectoryToScanPathChanged(string value)
    {
        // Clear extensions if directory is empty or invalid
        if (string.IsNullOrWhiteSpace(value) || !Directory.Exists(value))
        {
            ExtensionsScanned.Clear();
            SelectedExtensions.Clear();
            return;
        }
    }

    [ObservableProperty]
    private ObservableCollection<string> _extensionsScanned = [];

    [ObservableProperty]
    private ObservableCollection<string> _selectedExtensions = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExtensionsScanned))]
    private bool _includeSubdirectories = false;

    partial void OnIncludeSubdirectoriesChanged(bool value)
    {
        // Refresh extensions list when the include subdirectories option changes
        if (!string.IsNullOrWhiteSpace(DirectoryToScanPath) && Directory.Exists(DirectoryToScanPath))
        {
            RefreshExtensionsCommand.ExecuteAsync(null);
        }
    }

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public MainViewModel(IFileService fileService)
    {
        _fileService = fileService;
    }

    [RelayCommand]
    private async Task BrowseDirectory()
    {
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel == null)
                return;

            var folderDialog = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder",
                AllowMultiple = false
            });

            if (folderDialog != null && folderDialog.Count > 0)
            {
                DirectoryToScanPath = folderDialog[0].Path.LocalPath;
                await RefreshExtensionsCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error browsing directory: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshExtensions()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(DirectoryToScanPath) || !Directory.Exists(DirectoryToScanPath))
            {
                StatusMessage = "Please select a valid directory.";
                return;
            }

            var extensionsList = _fileService.GetFilesExtensionInDirectory(DirectoryToScanPath, IncludeSubdirectories);
            ExtensionsScanned.Clear();
            foreach (var ext in extensionsList)
            {
                ExtensionsScanned.Add(ext);
            }

            SelectedExtensions.Clear();
            StatusMessage = $"Found {ExtensionsScanned.Count} file extensions.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing extensions: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveToClipboard()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(DirectoryToScanPath) || !Directory.Exists(DirectoryToScanPath))
            {
                StatusMessage = "Please select a valid directory.";
                return;
            }

            var files = _fileService.GetFilesInDirectory(DirectoryToScanPath, SelectedExtensions, IncludeSubdirectories);

            if (files.Count == 0)
            {
                StatusMessage = "No files found matching the selected criteria.";
                return;
            }

            var clipboardText = _fileService.BuildClipboardText(files);
            var clipboard = GetTopLevel()?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(clipboardText);
                StatusMessage = $"Copied {files.Count} file paths to clipboard.";
            }
            else
            {
                StatusMessage = "Could not access clipboard.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving to clipboard: {ex.Message}";
        }
    }

    private TopLevel? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    private void CopyFilesToClipboard(List<string> files)
    {
        var sb = new StringBuilder();
        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);
            sb.AppendLine($"====== Start of {fileName} ======");
            sb.AppendLine($"Full path : {filePath}");
            var fileContent = File.ReadAllText(filePath);
            sb.AppendLine(fileContent);
            sb.AppendLine($"====== End of {fileName} ======");
            sb.AppendLine();
        }

        try
        {
            // Use Avalonia's clipboard service
            var clipboard = GetTopLevel()?.Clipboard;
            if (clipboard != null)
            {
                clipboard.SetTextAsync(sb.ToString());
                return;
            }
            
            StatusMessage = "Could not access clipboard.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error copying to clipboard: {ex.Message}";
        }
    }

    public List<string> GetFilesInDirectory(string folderPath, IEnumerable<string> selectedExtensions, bool includeSubdirectories)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"The specified folder does not exist: {folderPath}");

        SearchOption searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        // Convert to list for better performance in multiple operations
        var extensionsList = selectedExtensions.ToList();
        
        var files = Directory.GetFiles(folderPath, "*.*", searchOption)
            .Where(file => 
            {
                // If no extensions selected, include all files
                if (extensionsList.Count == 0)
                    return true;
                
                var fileExt = Path.GetExtension(file)?.ToLower();
                return !string.IsNullOrEmpty(fileExt) && extensionsList.Contains(fileExt);
            })
            .ToList();

        return files;
    }

    public List<string> GetFilesExtensionInDirectory(string folderPath, bool includeSubdirectories)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"The specified folder does not exist: {folderPath}");

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
