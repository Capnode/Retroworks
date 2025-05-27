using System;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;

namespace Retroworks.RCBus.Services;

internal class Settings : ISettings
{
    private readonly Configuration _config;

    public Settings()
    {
        string configPath = "user.config";

        // If config file doesn't exist, copy from default
        if (!File.Exists(configPath))
        {
            string exeConfigPath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".config";
            if (File.Exists(exeConfigPath))
            {
                File.Copy(exeConfigPath, configPath, overwrite: false);
            }
        }

        var configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
        _config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
    }

    public void Reset()
    {
        var settings = _config.AppSettings.Settings;
        settings.Clear();
        _config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection(_config.AppSettings.SectionInformation.Name);
    }

    public string? GetString(string key)
    {
        var settings = _config.AppSettings.Settings;
        return settings[key]?.Value;
    }

    public void SetString(string key, string value)
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

    public double SerialPanelWidth
    {
        get => double.TryParse(GetString(Key()), out var value) ? value : 360;
        set => SetString(Key(), value.ToString());
    }

    public double EmulatorPanelWidth
    {
        get => double.TryParse(GetString(Key()), out var value) ? value : 360;
        set => SetString(Key(), value.ToString());
    }

    private static string Key([CallerMemberName] string? propertyName = null)
    {
        return $"{propertyName}";
    }
}
