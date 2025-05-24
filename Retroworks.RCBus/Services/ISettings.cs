namespace Retroworks.RCBus.Services;

public interface ISettings
{
    double SerialPanelWidth { get; set; }
    double EmulatorPanelWidth { get; set; }

    void SetString(string key, string value);
    string? GetString(string key);
    void Reset();
}
