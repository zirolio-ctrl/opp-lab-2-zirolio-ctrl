using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using opp_lab_2_zirolio_ctrl.Models;
using opp_lab_2_zirolio_ctrl.Services;
using opp_lab_2_zirolio_ctrl.Services.Export;
using opp_lab_2_zirolio_ctrl.Services.SearchStrategies;

namespace opp_lab_2_zirolio_ctrl.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IDialogService _dialogs;
    private readonly GoogleDriveService _drive = new();

    public MainViewModel(IDialogService dialogs)
    {
        _dialogs = dialogs;
        SearchResults.CollectionChanged += (_, _) => ExportCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty] private string _selectedFilePath = string.Empty;
    [ObservableProperty] private string _selectedStrategy = "LINQ to XML";
    [ObservableProperty] private string? _selectedFaculty;
    [ObservableProperty] private string? _selectedCourse;
    [ObservableProperty] private string? _selectedRoom;

    [ObservableProperty] private string _exportFormat = "XML";
    [ObservableProperty] private string _exportTarget = "Локально";
    [ObservableProperty] private string _statusMessage = string.Empty;

    public ObservableCollection<string> Faculties { get; } = new();
    public ObservableCollection<string> Courses { get; } = new();
    public ObservableCollection<string> Rooms { get; } = new();
    public ObservableCollection<StudentRecord> SearchResults { get; } = new();

    public string[] Strategies { get; } = { "SAX API", "DOM API", "LINQ to XML" };
    public string[] ExportFormats { get; } = { "XML", "HTML" };
    public string[] ExportTargets { get; } = { "Локально", "Google Drive", "Локально + Google Drive" };

    [RelayCommand]
    private async Task OpenXmlAsync()
    {
        var path = await _dialogs.PickXmlFileAsync();
        if (path == null) return;

        try
        {
            LoadXmlData(path);
            StatusMessage = $"Завантажено: {Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Помилка читання файлу: {ex.Message}";
            EventLogger.Instance.Log("Файл", StatusMessage);
        }
    }

    private void LoadXmlData(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"Файл {path} не існує!", path);

        var doc = XDocument.Load(path);
        SelectedFilePath = path;

        Fill(Faculties, doc.Descendants("Faculty").Select(e => e.Value));
        Fill(Courses, doc.Descendants("Course").Select(e => e.Value));
        Fill(Rooms, doc.Descendants("Student").Attributes("room").Select(a => a.Value));

        SelectedFaculty = SelectedCourse = SelectedRoom = null;
        Search();
    }

    private static void Fill(ObservableCollection<string> target, IEnumerable<string> values)
    {
        target.Clear();
        target.Add(string.Empty);
        foreach (var v in values.Where(x => !string.IsNullOrEmpty(x)).Distinct().OrderBy(x => x))
            target.Add(v);
    }

    [RelayCommand]
    private async Task TransformToHtmlAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
        {
            StatusMessage = "Спочатку відкрийте XML-файл";
            return;
        }

        var output = await _dialogs.PickSaveFileAsync("dormitory.html", "html");
        if (output == null) return;

        try
        {
            var xsl = Path.Combine(AppContext.BaseDirectory, "dormitory.xsl");
            new XsltTransformerService().TransformToHtml(SelectedFilePath, xsl, output);
            EventLogger.Instance.Log("Трансформація", $"XSLT → {Path.GetFileName(output)}");

            try
            {
                _dialogs.OpenFile(output);
                StatusMessage = $"HTML збережено і відкрито в браузері: {output}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"HTML збережено: {output}. Не вдалося відкрити браузер: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Помилка трансформації: {ex.Message}";
            EventLogger.Instance.Log("Трансформація", StatusMessage);
        }
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrEmpty(SelectedFilePath)) return;

        IXmlSearchStrategy strategy = SelectedStrategy switch
        {
            "SAX API" => new SaxSearchStrategy(),
            "DOM API" => new DomSearchStrategy(),
            _ => new LinqSearchStrategy()
        };

        var results = strategy.Search(SelectedFilePath,
            SelectedFaculty ?? "", SelectedCourse ?? "", SelectedRoom ?? "");

        EventLogger.Instance.Log("Фільтрація",
            $"Стратегія: {SelectedStrategy}; Faculty - {SelectedFaculty ?? "All"}, Course - {SelectedCourse ?? "All"}, Room - {SelectedRoom ?? "All"}");

        SearchResults.Clear();
        foreach (var item in results) SearchResults.Add(item);
        StatusMessage = $"Знайдено записів: {SearchResults.Count}";
    }

    [RelayCommand]
    private void Clear()
    {
        SelectedFaculty = SelectedCourse = SelectedRoom = null;
        SearchResults.Clear();
        StatusMessage = string.Empty;
        Search();
    }

    private bool CanExport() => SearchResults.Count > 0;

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportAsync()
    {
        IExportFormat format;
        string content;
        try
        {
            format = ExportFormatFactory.Create(ExportFormat);
            content = format.Build(SearchResults);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Помилка формування файлу: {ex.Message}";
            EventLogger.Instance.Log("Збереження", StatusMessage);
            return;
        }

        string ext = format.Extension;
        string mime = format.MimeType;
        string fileName = $"filtered_dormitory_{DateTime.Now:yyyyMMdd_HHmm}.{ext}";

        bool toLocal = ExportTarget != "Google Drive";
        bool toDrive = ExportTarget != "Локально";

        if (toLocal)
        {
            var path = await _dialogs.PickSaveFileAsync(fileName, ext);
            if (path == null) return;

            try
            {
                await File.WriteAllTextAsync(path, content, new UTF8Encoding(false));
                StatusMessage = $"Збережено: {path}";
                EventLogger.Instance.Log("Збереження", $"Локально: {Path.GetFileName(path)}");

                if (ext == "html")
                {
                    try { _dialogs.OpenFile(path); } catch {}
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка збереження: {ex.Message}";
                EventLogger.Instance.Log("Збереження", StatusMessage);
                return;
            }
        }

        if (toDrive)
        {
            if (!GoogleDriveService.IsConfigured)
            {
                StatusMessage = "Google Drive: відсутній client_secret.json";
                EventLogger.Instance.Log("Збереження", StatusMessage);
                return;
            }

            try
            {
                var link = await _drive.UploadAsync(fileName, content, mime);
                StatusMessage = $"Завантажено на Google Drive: {link}";
                EventLogger.Instance.Log("Збереження", $"Google Drive: {fileName}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка Google Drive: {ex.Message}";
                EventLogger.Instance.Log("Збереження", StatusMessage);
            }
        }
    }

    public Task<bool> ConfirmExitAsync() =>
        _dialogs.ConfirmAsync("Вихід", "Чи дійсно ви хочете завершити роботу з програмою?");
}