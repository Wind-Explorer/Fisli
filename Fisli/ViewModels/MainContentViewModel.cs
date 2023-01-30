using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Fisli.Models;
using Fisli.Services;
using ReactiveUI;

namespace Fisli.ViewModels;

public class MainContentViewModel : ViewModelBase
{
    private const string AssembleModeName = "File Assembler";
    private const string SliceModeName = "File Slicer";
    private List<FileListLayout> _cachedFileList = new();
    private List<FileListLayout> _cachedFileList2 = new();
    private int _currentActionProgress;
    private string _currentAppStatus = "Welcome! Ready for action.";
    private int _currentFileSplitting;
    private int _currentlySelectedFile = -1;
    private string _currentModeName = null!;
    private List<FileListLayout> _fileList = new();
    private bool _isActionExecuting;
    private bool _isAppInfoButtonVisible;
    private bool _isSliceMode = true;
    private string _outputFileName = "output.jpg";
    private string _outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private int _servingsValue = 25;

    public MainContentViewModel()
    {
        CurrentModeName = IsSliceMode ? SliceModeName : AssembleModeName;
        IsAppInfoButtonVisible = OperatingSystem.IsWindows();
    }

    public bool IsAppInfoButtonVisible
    {
        get => _isAppInfoButtonVisible;
        set => this.RaiseAndSetIfChanged(ref _isAppInfoButtonVisible, value);
    }

    public bool IsActionExecuting
    {
        get => _isActionExecuting;
        private set => this.RaiseAndSetIfChanged(ref _isActionExecuting, value);
    }

    public string CurrentAppStatus
    {
        get => _currentAppStatus;
        set => this.RaiseAndSetIfChanged(ref _currentAppStatus, value);
    }

    public string OutputFileName
    {
        get => _outputFileName;
        set => this.RaiseAndSetIfChanged(ref _outputFileName, value);
    }

    public int CurrentlySelectedFile
    {
        get => _currentlySelectedFile;
        set => this.RaiseAndSetIfChanged(ref _currentlySelectedFile, value);
    }

    public List<FileListLayout> FileList
    {
        get => _fileList;
        set => this.RaiseAndSetIfChanged(ref _fileList, value);
    }

    public int CurrentActionProgress
    {
        get => _currentActionProgress;
        set => this.RaiseAndSetIfChanged(ref _currentActionProgress, value);
    }

    public string CurrentModeName
    {
        get => _currentModeName;
        set => this.RaiseAndSetIfChanged(ref _currentModeName, value);
    }

    public bool IsSliceMode
    {
        get => _isSliceMode;
        set => this.RaiseAndSetIfChanged(ref _isSliceMode, value);
    }

    public string OutputFolder
    {
        get => _outputFolder;
        set => this.RaiseAndSetIfChanged(ref _outputFolder, value);
    }

    public int ServingsValue
    {
        get => _servingsValue;
        set => this.RaiseAndSetIfChanged(ref _servingsValue, value);
    }

    public void ChangeCurrentMode()
    {
        IsSliceMode = !IsSliceMode;
        CurrentModeName = IsSliceMode ? SliceModeName : AssembleModeName;
        _cachedFileList2 = _cachedFileList;
        _cachedFileList = FileList;
        FileList = _cachedFileList2;
    }

    public async void AddFileToList()
    {
        Debug.WriteLine("Adding file");
        var fd = new OpenFileDialog
        {
            AllowMultiple = true,
            Title = "Select file(s) for Fisli to work with."
        };
        if (!IsSliceMode)
        {
            var filteredFiles = new List<FileDialogFilter>
            {
                new()
                {
                    Name = "Fisli Piece",
                    Extensions = new List<string> { "fspiece" }
                }
            };
            fd.Filters = filteredFiles;
        }

        var selectedFile =
            await fd.ShowAsync(((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!)
                .MainWindow);
        if (selectedFile == null)
        {
            return;
        }

        foreach (var file in selectedFile)
        {
            Debug.WriteLine(file);
        }

        PopulateSelectedFilesList(selectedFile);
    }

    private void PopulateSelectedFilesList(IEnumerable<string> selectedFile)
    {
        foreach (var file in selectedFile)
        {
            if (_fileList.Any(existingFile => existingFile.FilePath == file))
            {
                continue;
            }

            var fileInfo = new FileInfo(file);
            if (fileInfo.Length >= 2147483648)
            {
                var size = fileInfo.Length / 1073741824;
                Alert.ShowAlert(
                    $"Support for files bigger than 2GB is on the way! {fileInfo.Name} is around {size}GB in size.");
                continue;
            }

            if (!IsSliceMode)
            {
                var i = false;
                for (var c = 1; c <= 1000; c++)
                {
                    if (!fileInfo.Name.EndsWith($"({c}).fspiece"))
                    {
                        continue;
                    }

                    i = true;
                }

                if (!i)
                {
                    Alert.ShowAlert(
                        "Only files with names that look like \"xxx(3).fspiece\" or \"yyy(69).fspiece\" can be assembled!");
                    return;
                }
            }

            var addedFiles = new List<FileListLayout>
            {
                new()
                {
                    FileName = fileInfo.Name,
                    FilePath = file
                }
            };
            var iData = _fileList;
            addedFiles.AddRange(iData);
            FileList = addedFiles;
        }
    }

    public async void SelectOutputFolder()
    {
        var folderPicker = new OpenFolderDialog
        {
            Title = "Select a folder for Fisli to drop off the files in.",
            Directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };
        var result = await folderPicker.ShowAsync(
            ((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!)
            .MainWindow);
        if (result == null)
        {
            return;
        }

        OutputFolder = result;
    }

    public void RemoveFileFromList()
    {
        if (CurrentlySelectedFile <= -1)
        {
            return;
        }

        var iData = new List<FileListLayout>();
        iData.AddRange(FileList);
        iData.Remove(FileList[CurrentlySelectedFile]);
        FileList = iData;
    }

    public async void StartOperation()
    {
        if (FileList.Count <= 0)
        {
            Alert.ShowAlert("You need to add at least one file for Fisli to work with!");
            return;
        }

        CurrentAppStatus = "Preparing...";
        IsActionExecuting = true;
        if (IsSliceMode)
        {
            Debug.WriteLine("Slicer operating...");
            foreach (var file in FileList)
            {
                var outputDir = Path.Combine(OutputFolder, file.FileName!) + ".o";
                Directory.CreateDirectory(outputDir);
                _currentFileSplitting += 1;
                var operation = await Task.Run(() =>
                    Slice(file.FilePath!,
                        Path.Combine(outputDir, file.FileName!) + "(#).fspiece",
                        ServingsValue));
                switch (operation)
                {
                    case 1:
                        Alert.ShowAlert($"{file.FileName} has failed to be sliced.", file.FilePath!);
                        Directory.Delete(outputDir);
                        break;
                    case 2:
                        Alert.ShowAlert("Operation failed! Something unexpected happened.");
                        Directory.Delete(outputDir);
                        break;
                }

                CurrentActionProgress += FileList.Count == 1 ? 0 : 100 / FileList.Count;
            }

            CurrentActionProgress = 0;
            _currentFileSplitting = 0;
            FileList = new List<FileListLayout>();
            CurrentAppStatus = "Slicing complete! Ready for action.";
            IsActionExecuting = false;
        }
        else
        {
            Debug.WriteLine("Assembler operating...");
            await Task.Run(Assemble);
            CurrentActionProgress = 0;
            _currentFileSplitting = 0;
            FileList = new List<FileListLayout>();
            CurrentAppStatus = "Assemble complete! Ready for action.";
            IsActionExecuting = false;
        }
    }

    private int? Slice(string input, string output, int servings = 1)
    {
        // ReSharper disable once RedundantAssignment
        var fileBytes = new List<byte>();
        try
        {
            fileBytes = File.ReadAllBytes(input).ToList();
        }
        catch (IOException ex)
        {
            Debug.WriteLine(ex.Message);
            return 1;
        }
        catch
        {
            Debug.WriteLine(ThrownExceptions.ToString());

            return 2;
        }

        if (servings == 1)
        {
            return 0;
        }

        Debug.WriteLine($"[FileSlicer] {servings} servings requested! Dumping servings...");
        var num = fileBytes.Count / servings;
        var progressValuePerRound = 100 / servings;
        for (var c = 1; c <= servings; c++)
        {
            CurrentAppStatus = FileList.Count == 1
                ? $"{c} out of {servings} servings sliced"
                : $"{c} / {servings} servings of {_currentFileSplitting} / {FileList.Count} files sliced";
            try
            {
                Debug.WriteLine("[FileSlicer] New serving!");
                List<byte> oneServing = new();
                for (var ii = 0; ii <= num; ii++)
                {
                    oneServing.Add(fileBytes[ii]);
                }

                Debug.WriteLine($"[FileSlicer] Writing {oneServing.Count} bytes into file {c}...");
                File.WriteAllBytes(output.Replace("#", c.ToString()), oneServing.ToArray());
                Debug.WriteLine($"[FileSlicer] Finished writing file {c}! Preparing for next serving...");
                fileBytes.RemoveRange(0, oneServing.Count);
                Debug.WriteLine("[FileSlicer] Preparation finished!");
            }
            catch
            {
                Debug.WriteLine($"[FileSlicer] Dumping all remaining {fileBytes.Count} bytes into file {c}...");
                File.WriteAllBytes(output.Replace("#", c.ToString()), fileBytes.ToArray());
            }

            CurrentActionProgress += FileList.Count == 1 ? progressValuePerRound : 0;
        }

        return 0;
    }

    private void Assemble()
    {
        var pieces = new List<byte>();
        var progressValuePerRound = 100 / FileList.Count;
        for (var c = 1; c <= FileList.Count; c++)
        {
            foreach (var file in FileList)
            {
                if (!file.FileName!.EndsWith($"({c}).fspiece"))
                {
                    continue;
                }

                Debug.WriteLine($"[FileSlicer] Assembling {file.FileName}...");
                CurrentAppStatus = $"Assembling {c} / {FileList.Count} files together...";
                pieces.AddRange(File.ReadAllBytes(file.FilePath!).ToList());
                CurrentActionProgress += progressValuePerRound;
            }
        }

        File.WriteAllBytes(Path.Combine(OutputFolder, OutputFileName), pieces.ToArray());
    }

#pragma warning disable CA1822
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public void ShowAboutFisli()
#pragma warning restore CA1822
    {
        About.ShowAbout();
    }
}