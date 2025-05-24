using Retroworks.RCBus.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
namespace Retroworks.RCBus.ViewModels;

public partial class MainViewModel : ViewModelBase, IDialogProvider
{
    private readonly SerialPageViewModel _serialVm;
    private readonly EmulatorPageViewModel _emulatorVm;

    [ObservableProperty]
    private DialogViewModel? _dialog;

    [ObservableProperty]
    private bool _serialPageIsActive;

    [ObservableProperty]
    private bool _emulatorPageIsActive;

    [ObservableProperty]
    private bool _settingsPageIsActive;

    /// <summary>
    /// Design-time only constructor
    /// Allow nullable PageFactory for now in designer... ideally get it working
    /// </summary>
#pragma warning disable CS8618, CS9264
    public MainViewModel()
    {
    }
#pragma warning restore CS8618, CS9264

    public MainViewModel(SerialPageViewModel serialVm, EmulatorPageViewModel emulatorVm)
    {
        _serialVm = serialVm ?? throw new ArgumentNullException(nameof(serialVm));
        _emulatorVm = emulatorVm ?? throw new ArgumentNullException(nameof(emulatorVm));
    }

    [RelayCommand]
    private void GoToSerial()
    {
        if (SerialPageIsActive)
        {
            _serialVm.PanelExpanded = !_serialVm.PanelExpanded;
        }

        SerialPageIsActive = true;
        EmulatorPageIsActive = false;
        SettingsPageIsActive = false;
    }

    [RelayCommand]
    private void GoToEmulator()
    {
        if (EmulatorPageIsActive)
        {
            _emulatorVm.PanelExpanded = !_emulatorVm.PanelExpanded;
        }

        SerialPageIsActive = false;
        EmulatorPageIsActive = true;
        SettingsPageIsActive = false;
    }

    [RelayCommand]
    private void GoToSettings()
    {
        SerialPageIsActive = false;
        EmulatorPageIsActive = false;
        SettingsPageIsActive = true;
    }
}