using System.Configuration;
using System.Runtime.CompilerServices;

namespace Retroworks.RCBus.Services;

internal class Settings : ISettings
{
    private readonly Configuration _config;

    public Settings()
    {
        _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
    }

    public double SerialPanelWidth
    {
        get => double.TryParse(ConfigurationManager.AppSettings[Key()], out var value) ? value : 360;
        set => SetAppSetting(Key(), value.ToString());
    }

    public double EmulatorPanelWidth
    {
        get => double.TryParse(ConfigurationManager.AppSettings[Key()], out var value) ? value : 360;
        set => SetAppSetting(Key(), value.ToString());
    }

    private static string Key([CallerMemberName] string? propertyName = null)
    {
        return $"{propertyName}";
    }

    public string? GetString(string key)
    {
        return ConfigurationManager.AppSettings[key];
    }

    public void Reset()
    {
        var settings = _config.AppSettings.Settings;
        settings.Clear();
        _config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection(_config.AppSettings.SectionInformation.Name);
    }

    public void SetString(string key, string value)
    {
        SetAppSetting(key, value);
    }

    private void SetAppSetting(string key, string value)
    {
        var settings = _config.AppSettings.Settings;
        if (settings[key] == null)
        {
            settings.Add(key, value);
        }
        else
        {
            settings[key].Value = value;
        }

        _config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection(_config.AppSettings.SectionInformation.Name);
    }
}
