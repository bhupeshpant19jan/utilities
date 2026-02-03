using System.Text.Json;
using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Data;

/// <summary>
/// Local database for persisting tab states and session history.
/// Uses file-based JSON storage as a simple implementation.
/// In production, this would use SQLite.
/// </summary>
public sealed class LocalDatabase
{
    private readonly string _dataDirectory;
    private readonly string _tabsFilePath;
    private readonly string _settingsFilePath;
    private readonly object _fileLock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LocalDatabase(string? dataDirectory = null)
    {
        _dataDirectory = dataDirectory ??
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MultiLLMApp");

        Directory.CreateDirectory(_dataDirectory);

        _tabsFilePath = Path.Combine(_dataDirectory, "tabs.json");
        _settingsFilePath = Path.Combine(_dataDirectory, "settings.json");
    }

    #region Tab State Persistence

    public async Task SaveTabStatesAsync(IEnumerable<TabState> states)
    {
        var data = new TabsData
        {
            Tabs = states.ToList(),
            SavedAt = DateTimeOffset.UtcNow
        };

        var json = JsonSerializer.Serialize(data, JsonOptions);

        await WriteFileAsync(_tabsFilePath, json);
    }

    public async Task<IReadOnlyList<TabState>> LoadTabStatesAsync()
    {
        try
        {
            var json = await ReadFileAsync(_tabsFilePath);
            if (string.IsNullOrEmpty(json))
            {
                return [];
            }

            var data = JsonSerializer.Deserialize<TabsData>(json, JsonOptions);
            return data?.Tabs?.AsReadOnly() ?? (IReadOnlyList<TabState>)[];
        }
        catch (Exception)
        {
            // Return empty list if file doesn't exist or is corrupted
            return [];
        }
    }

    public async Task DeleteTabStateAsync(Guid tabId)
    {
        var states = await LoadTabStatesAsync();
        var filtered = states.Where(s => s.TabId != tabId).ToList();
        await SaveTabStatesAsync(filtered);
    }

    #endregion

    #region Settings Persistence

    public async Task SaveSettingAsync(string key, string value)
    {
        var settings = await LoadAllSettingsAsync();
        settings[key] = value;
        await SaveAllSettingsAsync(settings);
    }

    public async Task<string?> LoadSettingAsync(string key)
    {
        var settings = await LoadAllSettingsAsync();
        return settings.TryGetValue(key, out var value) ? value : null;
    }

    public async Task<Dictionary<string, string>> LoadAllSettingsAsync()
    {
        try
        {
            var json = await ReadFileAsync(_settingsFilePath);
            if (string.IsNullOrEmpty(json))
            {
                return new Dictionary<string, string>();
            }

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
                ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public async Task SaveAllSettingsAsync(Dictionary<string, string> settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        await WriteFileAsync(_settingsFilePath, json);
    }

    #endregion

    #region Export/Import

    public async Task<string> ExportDataAsync()
    {
        var exportData = new ExportData
        {
            Tabs = (await LoadTabStatesAsync()).ToList(),
            Settings = await LoadAllSettingsAsync(),
            ExportedAt = DateTimeOffset.UtcNow,
            Version = "1.0"
        };

        return JsonSerializer.Serialize(exportData, JsonOptions);
    }

    public async Task ImportDataAsync(string json)
    {
        var data = JsonSerializer.Deserialize<ExportData>(json, JsonOptions);
        if (data == null)
        {
            throw new InvalidOperationException("Invalid import data format");
        }

        if (data.Tabs != null)
        {
            await SaveTabStatesAsync(data.Tabs);
        }

        if (data.Settings != null)
        {
            await SaveAllSettingsAsync(data.Settings);
        }
    }

    #endregion

    #region File Operations

    private async Task WriteFileAsync(string path, string content)
    {
        var tempPath = path + ".tmp";

        // Write to temp file first for atomic operation
        await File.WriteAllTextAsync(tempPath, content);

        lock (_fileLock)
        {
            // Atomic rename
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tempPath, path);
        }
    }

    private async Task<string> ReadFileAsync(string path)
    {
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        return await File.ReadAllTextAsync(path);
    }

    #endregion

    #region Data Classes

    private sealed class TabsData
    {
        public List<TabState>? Tabs { get; set; }
        public DateTimeOffset SavedAt { get; set; }
    }

    private sealed class ExportData
    {
        public string? Version { get; set; }
        public List<TabState>? Tabs { get; set; }
        public Dictionary<string, string>? Settings { get; set; }
        public DateTimeOffset ExportedAt { get; set; }
    }

    #endregion
}
