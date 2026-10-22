using System;
using CSVEditor.Core.HelperClasses;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CSVEditor.Core;

/// <summary>
///  Class representing the application options and settings, including paths, visual configurations, save options,
/// git options, delimiters, block identifiers, and the last CSV files structure. In general, options that are persisted
/// between application runs and can be modified by the user.
///  Opposite to the <see cref="AppSettings"/> which is used for not persisting, general settings.
/// </summary>
/// <param name="lastRootPath"></param>
/// <param name="lastOpenedDirectoryPath"></param>
/// <param name="lastSavedDirectoryPath"></param>
/// <param name="lastSelectedFilePath"></param>
/// <param name="wasEdited"></param>
/// <param name="lastSelectedCsvFile"></param>
/// <param name="visulaConfig"></param>
/// <param name="saveOptions"></param>
/// <param name="gitOptions"></param>
/// <param name="delimiters"></param>
/// <param name="blockIdentifiers"></param>
/// <param name="lastCsvFilesStructure"></param>
public class AppOptions(
    string lastRootPath,
    string lastOpenedDirectoryPath,
    string lastSavedDirectoryPath,
    string lastSelectedFilePath,
    bool wasEdited,
    CsvFile lastSelectedCsvFile,
    VisualConfig visulaConfig,
    SaveOptions saveOptions,
    GitOptions gitOptions,
    List<char> delimiters,
    List<char> blockIdentifiers,
    List<DirectoryWithCsv> lastCsvFilesStructure)
{
    public string LastRootPath { get; set; } = lastRootPath;

    public string LastOpenedDirectoryPath
    {
        get
        {
            if (string.IsNullOrEmpty(field))
            {
                field = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            return field;
        }
        set;
    } = lastOpenedDirectoryPath;

    public string LastSavedDirectoryPath
    {
        get
        {
            if (string.IsNullOrEmpty(field))
            {
                field = LastRootPath ??  Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            return field;
        }
        set;
    } = lastSavedDirectoryPath;

    public string LastSelectedFilePath { get; set; } = lastSelectedFilePath;
    public bool WasEdited { get; set; } = wasEdited;
    public CsvFile LastSelectedCsvFile { get; set; } = lastSelectedCsvFile;
    public VisualConfig VisualConfig { get; set; } = visulaConfig;
    public SaveOptions SaveOptions { get; set; } = saveOptions;
    public GitOptions GitOptions { get; set; } = gitOptions;
    public List<char> Delimiters { get; set; } = delimiters;
    public List<char> BlockIdentifiers { get; set; } = blockIdentifiers;
    public List<DirectoryWithCsv> LastCsvFilesStructure { get; set; } = lastCsvFilesStructure;

    public AppOptions() : this(
        "",
        "",
        "",
        "",
        false,
        null,
        new VisualConfig(),
        new SaveOptions(),
        new GitOptions(),
        CsvFile.Delimiters,
        CsvFile.BlockIdentifiers,
        [new DirectoryWithCsv()])
    {
    }
}