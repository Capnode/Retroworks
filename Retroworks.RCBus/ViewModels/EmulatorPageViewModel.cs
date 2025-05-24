using CommunityToolkit.Mvvm.ComponentModel;
using Retroworks.RCBus.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using Retroworks.RCBus.Services.Internals;
using System.IO;
using Retroworks.RCBus.Devices;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using VtNetCore.VirtualTerminal;
using Avalonia.Styling;
using VtNetCore.VirtualTerminal.Enums;
using VtNetCore.VirtualTerminal.Model;
using Avalonia;

namespace Retroworks.RCBus.ViewModels;

public partial class EmulatorPageViewModel : ViewModelBase
{
    private readonly ISettings _settings;
    private readonly EmulatorConnection _emulator;

    private bool _pending;

    /// <summary>
    /// Design-time only constructor
    /// Allow nullable PageFactory for now in designer... ideally get it working
    /// </summary>
#pragma warning disable CS8618
    public EmulatorPageViewModel()
    {
    }
#pragma warning restore CS8618

    public EmulatorPageViewModel(ISettings settings, IEnumerable<IConnection> connections)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _emulator = connections?.OfType<EmulatorConnection>().FirstOrDefault() ?? throw new ArgumentNullException(nameof(connections));
        _emulator.Opened += (sender, args) => { IsPower = true; };
        _emulator.Closed += (sender, args) => { IsPower = false; };
        Connection = _emulator;

        if (Design.IsDesignMode) return;
        Devices = _emulator.Devices;
        DeviceName = _settings.GetString(nameof(DeviceName));
        Device = Devices.FirstOrDefault(x => x.Name == DeviceName);
        GetSettings(Device);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PanelWidth))]
    private bool _panelExpanded = true;

    [ObservableProperty]
    public IConnection? _connection;

    [ObservableProperty]
    private VirtualTerminalController? _terminal;

    [ObservableProperty]
    private bool _isPower;

    [ObservableProperty]
    private IEnumerable<IEmulatorDevice>? _devices;

    [ObservableProperty]
    private string? _deviceName;

    [ObservableProperty]
    private IEmulatorDevice? _device;
    partial void OnDeviceChanged(IEmulatorDevice? value)
    {
        GetSettings(value);
        if (value == default) return;
        DeviceName = _settings.GetString(nameof(DeviceName));
        if (DeviceName == value.Name) return;
        _settings.SetString(nameof(DeviceName), value.Name);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgramFile))]
    public string? _programPath;
    partial void OnProgramPathChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        if (Device == default) return;
        var key = string.Join(':', Device.Name, nameof(ProgramPath));
        var old = _settings.GetString(key);
        if (old == value) return;
        _settings.SetString(key, value);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CfFile))]
    public string? _cfPath;
    partial void OnCfPathChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        if (Device == default) return;
        var key = string.Join(':', Device.Name, nameof(CfPath));
        var old = _settings.GetString(key);
        if (old == value) return;
        _settings.SetString(key, value);
    }

    public string? ProgramFile => Path.GetFileName(ProgramPath) ?? "Select program file";

    public string? CfFile => Path.GetFileName(CfPath) ?? "Select Compact Flash file";

    [ObservableProperty]
    private bool _outPort1;

    [ObservableProperty]
    private bool _outPort2;

    [ObservableProperty]
    private bool _outPort3;

    [ObservableProperty]
    private bool _outPort4;

    [ObservableProperty]
    private bool _outPort5;

    [ObservableProperty]
    private bool _outPort6;

    [ObservableProperty]
    private bool _outPort7;

    [ObservableProperty]
    private bool _outPort8;

    [ObservableProperty]
    private bool _inPort1;

    [ObservableProperty]
    private bool _inPort2;

    [ObservableProperty]
    private bool _inPort3;

    [ObservableProperty]
    private bool _inPort4;

    [ObservableProperty]
    private bool _inPort5;

    [ObservableProperty]
    private bool _inPort6;

    [ObservableProperty]
    private bool _inPort7;

    [ObservableProperty]
    private bool _inPort8;

    /// <summary>
    /// A command used to select some files
    /// </summary>
    [RelayCommand]
    private async Task GetProgramFile(Avalonia.Visual? context)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(context) ?? throw new NullReferenceException(nameof(TopLevel));

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Program File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            ProgramPath = files[0].TryGetLocalPath();
        }
    }

    /// <summary>
    /// A command used to select some files
    /// </summary>
    [RelayCommand]
    private async Task GetCfFile(Avalonia.Visual? context)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(context) ?? throw new NullReferenceException(nameof(TopLevel));

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Compact Flash File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            CfPath = files[0].TryGetLocalPath();
        }
    }

    [RelayCommand]
    private void InPort()
    {
        if (Device == default) return;
        if (Device.DigitalIO == default) return;
        byte data = 0;
        data |= (byte)(InPort1 ? 0x01 : 0x00);
        data |= (byte)(InPort2 ? 0x02 : 0x00);
        data |= (byte)(InPort3 ? 0x04 : 0x00);
        data |= (byte)(InPort4 ? 0x08 : 0x00);
        data |= (byte)(InPort5 ? 0x10 : 0x00);
        data |= (byte)(InPort6 ? 0x20 : 0x00);
        data |= (byte)(InPort7 ? 0x40 : 0x00);
        data |= (byte)(InPort8 ? 0x80 : 0x00);
        Device.DigitalIO.InData = data;
        _settings.SetString(string.Join(':', Device.Name, nameof(InPort)), data.ToString());
    }

    [RelayCommand]
    private void Power()
    {
        if (Device == default) return;
        if (_emulator == null) throw new NullReferenceException(nameof(_emulator));
        if (_pending) return;
        _pending = true;
        if (IsPower)
        {
            _emulator.Disconnect();
            Terminal = default;
        }
        else
        {
            if (Application.Current?.ActualThemeVariant == ThemeVariant.Light)
            {
                TerminalAttribute.DefaultBackground = ETerminalColor.White;
                TerminalAttribute.DefaultForeground = ETerminalColor.Black;
            }
            else
            {
                TerminalAttribute.DefaultBackground = ETerminalColor.Black;
                TerminalAttribute.DefaultForeground = ETerminalColor.Green;
            }
            Terminal = new VirtualTerminalController();
            _emulator.Connect(Device, ProgramPath, CfPath);
        }

        _pending = false;
    }

    public double PanelWidth
    {
        get => PanelExpanded ? _settings.EmulatorPanelWidth : 0;
        set
        {
            if (!PanelExpanded) return;
            if (_settings == null) return;
            if (value == PanelWidth) return;
            _settings.EmulatorPanelWidth = value;
            OnPropertyChanged();
        }
    }

    private void GetSettings(IEmulatorDevice? device)
    {
        if (device == default) return;
        ProgramPath = _settings.GetString(string.Join(':', device.Name, nameof(ProgramPath)));
        CfPath = _settings.GetString(string.Join(':', device.Name, nameof(CfPath)));
        if (device.DigitalIO != default)
        {
            device.DigitalIO.OutData += DigitalOut;
            var inPort = _settings.GetString(string.Join(':', device.Name, nameof(InPort))) ?? "0";
            var indata = byte.Parse(inPort);
            device.DigitalIO.InData = indata;
            InPort1 = (indata & 0x01) != 0;
            InPort2 = (indata & 0x02) != 0;
            InPort3 = (indata & 0x04) != 0;
            InPort4 = (indata & 0x08) != 0;
            InPort5 = (indata & 0x10) != 0;
            InPort6 = (indata & 0x20) != 0;
            InPort7 = (indata & 0x40) != 0;
            InPort8 = (indata & 0x80) != 0;
        }
    }

    private void DigitalOut(byte data)
    {
        OutPort1 = (data & 0x01) != 0;
        OutPort2 = (data & 0x02) != 0;
        OutPort3 = (data & 0x04) != 0;
        OutPort4 = (data & 0x08) != 0;
        OutPort5 = (data & 0x10) != 0;
        OutPort6 = (data & 0x20) != 0;
        OutPort7 = (data & 0x40) != 0;
        OutPort8 = (data & 0x80) != 0;
    }
}
